using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace NisiAssist;

public sealed class ConfigWindow : Window, IDisposable
{
    private readonly Configuration _cfg;
    private readonly NisiTracker _tracker;

    public ConfigWindow(Configuration cfg, NisiTracker tracker)
        : base("Nisi Assist settings##config")
    {
        _cfg = cfg;
        _tracker = tracker;
        Size = new Vector2(380, 420);
        SizeCondition = ImGuiCond.FirstUseEver;
    }

    public void Dispose() { }

    public override void OnClose() => _tracker.Preview = false;

    public override void Draw()
    {
        var changed = false;

        var preview = _tracker.Preview;
        if (ImGui.Checkbox("Preview overlay (drag it where you want it)", ref preview))
            _tracker.Preview = preview;

        ImGui.Separator();
        ImGui.TextUnformatted("Display");
        changed |= Check("Lock overlay position", _cfg.OverlayLocked, v => _cfg.OverlayLocked = v);
        changed |= Check("Show icons", _cfg.ShowIcons, v => _cfg.ShowIcons = v);
        changed |= Check("Only me (hide party table)", _cfg.ShowOnlyMe, v => _cfg.ShowOnlyMe = v);
        changed |= Check("Show pass-window countdown", _cfg.ShowCountdown, v => _cfg.ShowCountdown = v);
        changed |= Check("Show remaining time of my Nisi", _cfg.ShowMyNisiTimer, v => _cfg.ShowMyNisiTimer = v);
        changed |= Check("Show next pass a few seconds early", _cfg.ShowUpcoming, v => _cfg.ShowUpcoming = v);

        changed |= Slider("Overall scale (4K screen: try 2.0)", _cfg.UiScale, 0.5f, 4.0f, v => _cfg.UiScale = v);

        var scale = _cfg.FontScale;
        if (ImGui.SliderFloat("Text size", ref scale, 0.8f, 4.0f)) { _cfg.FontScale = scale; changed = true; }

        var alpha = _cfg.BackgroundAlpha;
        if (ImGui.SliderFloat("Background opacity", ref alpha, 0.0f, 1.0f)) { _cfg.BackgroundAlpha = alpha; changed = true; }

        ImGui.Separator();
        ImGui.TextUnformatted("Water / Lightning countdown");
        changed |= Check("Show big countdown for my water/lightning debuff", _cfg.ShowDebuffCountdown, v => _cfg.ShowDebuffCountdown = v);
        changed |= Slider("Start at (seconds left)", _cfg.DebuffCountdownSeconds, 1f, 10f, v => _cfg.DebuffCountdownSeconds = v);
        changed |= Slider("Number size (pixels)", _cfg.CountdownSize, 40f, 400f, v => _cfg.CountdownSize = v);
        changed |= Check("Play sound", _cfg.DebuffSoundEnabled, v => _cfg.DebuffSoundEnabled = v);

        var tick = _cfg.DebuffTickSound;
        if (ImGui.SliderInt("Tick sound (<se.N>)", ref tick, 1, 16)) { _cfg.DebuffTickSound = tick; changed = true; }
        var fin = _cfg.DebuffFinalSound;
        if (ImGui.SliderInt("Last-second sound (<se.N>)", ref fin, 1, 16)) { _cfg.DebuffFinalSound = fin; changed = true; }

        ImGui.Separator();
        ImGui.TextUnformatted("Timing (seconds, from Cactbot nominal values)");

        changed |= Slider("Early notice lead", _cfg.UpcomingLead, 0f, 15f, v => _cfg.UpcomingLead = v);
        changed |= Slider("Pass 3: start after Flarethrower cast begins", _cfg.Pass3DelayAfterCastStart, 0f, 20f, v => _cfg.Pass3DelayAfterCastStart = v);
        changed |= Slider("Pass 3: window length", _cfg.Pass3Duration, 3f, 20f, v => _cfg.Pass3Duration = v);
        changed |= Slider("Pass 4: start before Propeller Wind", _cfg.Pass4LeadBeforePropeller, 5f, 25f, v => _cfg.Pass4LeadBeforePropeller = v);
        changed |= Slider("Pass 4: end before Propeller Wind", _cfg.Pass4EndBeforePropeller, 0f, 8f, v => _cfg.Pass4EndBeforePropeller = v);
        changed |= Slider("Flarethrower -> Propeller Wind gap (fallback)", _cfg.PropellerAfterFlarethrower, 25f, 50f, v => _cfg.PropellerAfterFlarethrower = v);

        if (changed)
            _cfg.Save();
    }

    private static bool Check(string label, bool value, Action<bool> set)
    {
        if (!ImGui.Checkbox(label, ref value))
            return false;
        set(value);
        return true;
    }

    private static bool Slider(string label, float value, float min, float max, Action<float> set)
    {
        if (!ImGui.SliderFloat(label, ref value, min, max, "%.1f"))
            return false;
        set(value);
        return true;
    }
}
