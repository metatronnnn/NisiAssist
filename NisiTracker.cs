using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace NisiAssist;

public enum PassPhase { Upcoming, Active }

public sealed record PlayerState(uint Id, string Name, int Held, float HeldRemaining);

public enum DebuffKind { Water, Lightning }

public sealed record DebuffCountdown(DebuffKind Kind, float Remaining);

public sealed class Guidance
{
    public int Pass;                         // 3 or 4
    public PassPhase Phase;
    public double SecondsToStart;            // while Upcoming
    public double SecondsLeft;               // time until the pass window closes
    public string Action = "UNKNOWN";        // PASS / GET / UNKNOWN
    public int NisiIndex = -1;               // 0..3 = alpha, beta, gamma, delta
    public List<string> Partners = new();
    public int MyHeld = -1;                  // Nisi you currently hold, -1 = none
    public float MyHeldRemaining;
}

/// <summary>
/// Game-state logic only (no ImGui). Mirrors Cactbot's TEA phase-2 Nisi handling:
///  - "held" Nisi alpha..delta   = status ids 0x8AE, 0x8AF, 0x859, 0x85A   (Cactbot: kDecreeNisi)
///  - "verdict" Nisi alpha..delta = status ids 0x8B0, 0x8B1, 0x85B, 0x85C  (Cactbot: kFinalJudgementNisi)
///  - if you hold a Nisi: pass it to the player whose verdict matches it (preferably one not holding a Nisi)
///  - if you hold none:   collect the Nisi matching your verdict from whoever holds it
/// Pass 3 = 8s after Brute Justice starts Flarethrower (0x4845), 9s long.
/// Pass 4 = 15s before Propeller Wind (0x4832), 14s long.
/// </summary>
public sealed class NisiTracker
{
    // Status ids (hex, from Cactbot the_epic_of_alexander.ts)
    private static readonly uint[] HeldNisi = { 0x8AE, 0x8AF, 0x859, 0x85A };
    private static readonly uint[] VerdictNisi = { 0x8B0, 0x8B1, 0x85B, 0x85C };
    private const uint StatusCompressedWater = 0x85E;     // Cactbot: 'Compressed Water'
    private const uint StatusCompressedLightning = 0x85F; // Cactbot: 'Compressed Lightning'

    // Action ids (hex, from Cactbot triggers / timeline)
    private const uint ActJudgmentNisi = 0x483E;  // Brute Justice - applies the Nisi, marks the start of the sequence
    private const uint ActFlarethrower = 0x4845;  // Brute Justice - pass 3 anchor
    private const uint ActPropellerWind = 0x4832; // Cruise Chaser - pass 4 anchor
    private const uint ActWormholeFormation = 0x486E; // Alexander Prime - phase 3 part 2
    private const uint ActVoidOfRepentance = 0x4866;
    private const uint ActRepentance1 = 0x4869;
    private const uint ActRepentance2 = 0x4868;
    private const uint ActRepentance3 = 0x4867;
    private const uint ActGavel = 0x483C;         // Brute Justice - after this, Cactbot stops the water/lightning callouts

    private readonly IObjectTable _objects;
    private readonly IPartyList _party;
    private readonly ICondition _condition;
    private readonly Configuration _cfg;

    private readonly Dictionary<uint, uint> _activeCasts = new();
    private readonly Dictionary<uint, uint> _scratchCasts = new();
    private readonly Dictionary<uint, int> _verdicts = new();

    private bool _sequenceActive;
    private bool _nisiSeen;
    private bool _gavelSeen;
    private float _myWater, _myLightning;
    private readonly Dictionary<DebuffKind, int> _lastTick = new();
    private double _flareResolve = double.NaN;
    private double _propResolve = double.NaN;
    private double _p3Start = double.NaN, _p3End = double.NaN;
    private double _p4Start = double.NaN, _p4End = double.NaN;

    public Dictionary<uint, PlayerState> Players { get; } = new();
    public IReadOnlyDictionary<uint, int> Verdicts => _verdicts;
    public Guidance? Current { get; private set; }
    public List<DebuffCountdown> Countdowns { get; } = new();
    public bool Preview { get; set; }

    // ---- wormhole ----
    private double _whUntil = double.NaN;
    private double _puddleUntil = double.NaN;
    public bool WormholeManual { get; set; }
    public int WormholePuddle { get; private set; }   // 1..3 while that Repentance puddle is being cast, else 0
    public bool WormholeVisible => WormholeManual || Preview || Now() < _whUntil;

    /// <summary>Which Repentance puddle a Limit Cut number soaks (Cactbot wormhole strat): 5,6 -> 1; 7,8 -> 2; 1,2 -> 3; 3,4 -> none.</summary>
    public static int PuddleFor(int n) => n switch { 5 or 6 => 1, 7 or 8 => 2, 1 or 2 => 3, _ => 0 };

    public NisiTracker(IObjectTable objects, IPartyList party, ICondition condition, Configuration cfg)
    {
        _objects = objects;
        _party = party;
        _condition = condition;
        _cfg = cfg;
    }

    public static string NisiName(int i) => i switch { 0 => "ALPHA", 1 => "BETA", 2 => "GAMMA", 3 => "DELTA", _ => "?" };

    private static double Now() => Stopwatch.GetTimestamp() / (double)Stopwatch.Frequency;

    public void Reset()
    {
        _sequenceActive = false;
        _nisiSeen = false;
        _gavelSeen = false;
        _myWater = _myLightning = 0f;
        _lastTick.Clear();
        Countdowns.Clear();
        _flareResolve = _propResolve = double.NaN;
        _p3Start = _p3End = _p4Start = _p4End = double.NaN;
        _verdicts.Clear();
        _activeCasts.Clear();
        Players.Clear();
        Current = null;
    }

    /// <summary>Call once per framework update (game thread).</summary>
    public void Update()
    {
        if (Preview)
        {
            Current = BuildPreview();
            Countdowns.Clear();
            Countdowns.Add(new DebuffCountdown(DebuffKind.Water, 4.2f));
            return;
        }

        if (!_condition[ConditionFlag.InCombat])
        {
            if (_sequenceActive || _nisiSeen || _gavelSeen || _verdicts.Count > 0)
                Reset();
            Current = null;
            Countdowns.Clear();
            _whUntil = double.NaN;
            WormholePuddle = 0;
            return;
        }

        var t = Now();
        ScanCasts(t);
        if (WormholePuddle != 0 && t > _puddleUntil)
            WormholePuddle = 0;
        ScanStatuses();
        UpdateCountdowns();
        Current = BuildCurrent(t);
    }

    // ------------------------------------------------------------------ casts

    private void ScanCasts(double t)
    {
        _scratchCasts.Clear();
        foreach (var obj in _objects)
        {
            if (obj is not IBattleNpc npc || !npc.IsCasting)
                continue;

            var action = npc.CastActionId;
            _scratchCasts[npc.EntityId] = action;

            if (_activeCasts.TryGetValue(npc.EntityId, out var prev) && prev == action)
                continue; // same cast we already handled

            OnCastStart(action, npc.CurrentCastTime, npc.TotalCastTime, t);
        }

        _activeCasts.Clear();
        foreach (var kv in _scratchCasts)
            _activeCasts[kv.Key] = kv.Value;
    }

    private void OnCastStart(uint action, float currentCast, float totalCast, double t)
    {
        var castStart = t - currentCast;
        var resolve = castStart + totalCast;

        switch (action)
        {
            case ActJudgmentNisi:
                // New Nisi sequence: drop any stale state from an earlier attempt.
                Reset();
                _sequenceActive = true;
                break;

            case ActFlarethrower when _sequenceActive || _nisiSeen:
                _sequenceActive = true;
                _flareResolve = resolve;
                _p3Start = castStart + _cfg.Pass3DelayAfterCastStart;
                _p3End = _p3Start + _cfg.Pass3Duration;
                _propResolve = double.NaN;
                RecomputePass4(_flareResolve + _cfg.PropellerAfterFlarethrower);
                break;

            case ActWormholeFormation:
            case ActVoidOfRepentance:
                _whUntil = t + 45;
                break;

            case ActRepentance1:
            case ActRepentance2:
            case ActRepentance3:
                _whUntil = t + 30;
                WormholePuddle = action == ActRepentance1 ? 1 : action == ActRepentance2 ? 2 : 3;
                _puddleUntil = resolve + 1.0;
                break;

            case ActGavel when _sequenceActive || _nisiSeen:
                _gavelSeen = true;
                break;

            case ActPropellerWind when _sequenceActive || _nisiSeen:
                _propResolve = resolve;
                RecomputePass4(_propResolve);
                break;
        }
    }

    private void RecomputePass4(double propellerTime)
    {
        _p4Start = propellerTime - _cfg.Pass4LeadBeforePropeller;
        _p4End = propellerTime - _cfg.Pass4EndBeforePropeller;
    }

    // --------------------------------------------------------------- statuses

    private IEnumerable<IBattleChara> PartyCharacters()
    {
        var seen = new HashSet<uint>();
        foreach (var member in _party)
        {
            if (member.GameObject is IBattleChara bc && seen.Add(bc.EntityId))
                yield return bc;
        }

        var lp = _objects.LocalPlayer; // also covers solo / duty-support testing
        if (lp != null && seen.Add(lp.EntityId))
            yield return lp;
    }

    private void ScanStatuses()
    {
        Players.Clear();
        _myWater = _myLightning = 0f;
        var localId = _objects.LocalPlayer?.EntityId ?? 0u;
        foreach (var bc in PartyCharacters())
        {
            var held = -1;
            var remaining = 0f;
            var verdict = -1;

            foreach (var status in bc.StatusList)
            {
                var id = status.StatusId;

                if (bc.EntityId == localId)
                {
                    if (id == StatusCompressedWater) _myWater = status.RemainingTime;
                    else if (id == StatusCompressedLightning) _myLightning = status.RemainingTime;
                }

                var h = Array.IndexOf(HeldNisi, id);
                if (h >= 0)
                {
                    held = h;
                    remaining = status.RemainingTime;
                }

                var v = Array.IndexOf(VerdictNisi, id);
                if (v >= 0)
                    verdict = v;
            }

            if (held >= 0)
                _nisiSeen = true;

            // Latched like Cactbot's finalNisiMap: the verdict debuff is remembered for the rest of the sequence.
            if (verdict >= 0)
                _verdicts[bc.EntityId] = verdict;

            Players[bc.EntityId] = new PlayerState(bc.EntityId, bc.Name.TextValue, held, remaining);
        }
    }

    // ------------------------------------------------- water / lightning timers

    private void UpdateCountdowns()
    {
        Countdowns.Clear();
        if (!_cfg.ShowDebuffCountdown || _gavelSeen)
        {
            _lastTick.Clear();
            return;
        }

        Consider(DebuffKind.Water, _myWater);
        Consider(DebuffKind.Lightning, _myLightning);
    }

    private void Consider(DebuffKind kind, float remaining)
    {
        if (remaining <= 0f || remaining > _cfg.DebuffCountdownSeconds)
        {
            _lastTick.Remove(kind);
            return;
        }

        Countdowns.Add(new DebuffCountdown(kind, remaining));

        var n = (int)Math.Ceiling(remaining);
        if (_lastTick.TryGetValue(kind, out var last) && last == n)
            return;

        _lastTick[kind] = n;
        if (_cfg.DebuffSoundEnabled)
            PlayTick(n);
    }

    private void PlayTick(int secondsLeft)
    {
        try
        {
            var se = secondsLeft <= 1 ? _cfg.DebuffFinalSound : _cfg.DebuffTickSound;
            UIGlobals.PlayChatSoundEffect((uint)se);
        }
        catch
        {
            // sound is best-effort, never break the update loop
        }
    }

    // --------------------------------------------------------------- guidance

    private Guidance? BuildCurrent(double t)
    {
        var me = _objects.LocalPlayer;
        if (me == null)
            return null;

        var lead = _cfg.ShowUpcoming ? _cfg.UpcomingLead : 0.0;

        foreach (var (pass, start, end) in new[] { (3, _p3Start, _p3End), (4, _p4Start, _p4End) })
        {
            if (double.IsNaN(start) || double.IsNaN(end) || t > end)
                continue;

            if (t < start - lead)
                return null;

            var phase = t < start ? PassPhase.Upcoming : PassPhase.Active;
            return Resolve(pass, phase, start - t, end - t, me.EntityId);
        }

        return null;
    }

    private Guidance Resolve(int pass, PassPhase phase, double toStart, double left, uint meId)
    {
        var g = new Guidance
        {
            Pass = pass,
            Phase = phase,
            SecondsToStart = Math.Max(0, toStart),
            SecondsLeft = Math.Max(0, left),
        };

        if (Players.TryGetValue(meId, out var me))
        {
            g.MyHeld = me.Held;
            g.MyHeldRemaining = me.HeldRemaining;
        }

        // Same precondition as Cactbot: without our own verdict debuff we can't tell what to do.
        if (!_verdicts.TryGetValue(meId, out var myVerdict))
            return g; // Action stays UNKNOWN

        if (g.MyHeld >= 0)
        {
            g.Action = "PASS";
            g.NisiIndex = g.MyHeld;

            var targets = Players.Values
                .Where(p => p.Id != meId && _verdicts.TryGetValue(p.Id, out var v) && v == g.MyHeld)
                .ToList();
            var free = targets.Where(p => p.Held < 0).ToList();
            g.Partners = (free.Count > 0 ? free : targets).Select(p => p.Name).ToList();
        }
        else
        {
            g.Action = "GET";
            g.NisiIndex = myVerdict;
            g.Partners = Players.Values.Where(p => p.Held == myVerdict).Select(p => p.Name).ToList();
        }

        return g;
    }

    private static Guidance BuildPreview() => new()
    {
        Pass = 3,
        Phase = PassPhase.Active,
        SecondsLeft = 6.4,
        Action = "PASS",
        NisiIndex = 0,
        Partners = new List<string> { "Sample Player" },
        MyHeld = 0,
        MyHeldRemaining = 18.2f,
    };
}
