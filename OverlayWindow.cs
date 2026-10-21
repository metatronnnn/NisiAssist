using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace NisiAssist;

public sealed class OverlayWindow : Window, IDisposable
{
    private readonly Configuration _cfg;
    private readonly NisiTracker _tracker;
    private readonly Icons _icons;
    private readonly Fonts _fonts;

    private static readonly Vector4 White = new(1f, 1f, 1f, 1f);
    private static readonly Vector4 Grey = new(0.7f, 0.7f, 0.7f, 1f);
    private static readonly Vector4 Red = new(1f, 0.35f, 0.35f, 1f);

    private const ImGuiWindowFlags BaseFlags =
        ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoCollapse |
        ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoNav;

    public OverlayWindow(Configuration cfg, NisiTracker tracker, Icons icons, Fonts fonts)
        : base("Nisi Assist##overlay", BaseFlags)
    {
        _cfg = cfg;
        _tracker = tracker;
        _icons = icons;
        _fonts = fonts;
        RespectCloseHotkey = false;
        IsOpen = true;
    }

    public void Dispose() { }

    // Colours follow Cactbot: blue alpha, orange beta, purple gamma, green delta.
    private static Vector4 NisiColor(int i) => i switch
    {
        0 => new Vector4(0.40f, 0.65f, 1.00f, 1f),
        1 => new Vector4(1.00f, 0.65f, 0.20f, 1f),
        2 => new Vector4(0.78f, 0.50f, 1.00f, 1f),
        3 => new Vector4(0.45f, 0.92f, 0.50f, 1f),
        _ => White,
    };

    // NOTE: the only ImGui call here that has changed between Dalamud/ImGui versions is font scaling.
    // If it fails to compile on your Dalamud version, replace this one line.
    private void ApplyFontScale() => ImGui.SetWindowFontScale(_cfg.FontScale);

    // Small icon in front of a line; the text follows on the same line.
    private void NisiIcon(int index)
    {
        if (!_cfg.ShowIcons || index < 0 || index >= Icons.NisiNames.Length)
            return;
        if (_icons.Draw(Icons.NisiNames[index], ImGui.GetTextLineHeight()))
            ImGui.SameLine();
    }

    public override bool DrawConditions() => _tracker.Current != null;

    public override void PreDraw()
    {
        Flags = BaseFlags;
        if (_cfg.OverlayLocked && !_tracker.Preview)
            Flags |= ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoInputs;

        BgAlpha = _cfg.BackgroundAlpha;
    }

    public override void Draw()
    {
        var g = _tracker.Current;
        if (g == null)
            return;

        using var font = _fonts.PushText(Fonts.TextPx(_cfg.FontScale, _cfg.UiScale), out var fontScale);
        ImGui.SetWindowFontScale(fontScale);

        // Header: which pass + countdown of the window
        var header = g.Phase == PassPhase.Active ? $"NISI PASS {g.Pass}" : $"NISI PASS {g.Pass} - next";
        if (_cfg.ShowCountdown)
        {
            header += g.Phase == PassPhase.Active
                ? $"   {g.SecondsLeft:0.0}s"
                : $"   in {g.SecondsToStart:0.0}s";
        }
        ImGui.TextColored(g.Phase == PassPhase.Active ? White : Grey, header);

        // Main instruction
        switch (g.Action)
        {
            case "PASS":
            {
                var col = NisiColor(g.NisiIndex);
                NisiIcon(g.NisiIndex);
                var who = g.Partners.Count > 0 ? string.Join(", ", g.Partners) : "(partner not found)";
                ImGui.TextColored(col, $"PASS {NisiTracker.NisiName(g.NisiIndex)} -> {who}");
                break;
            }
            case "GET":
            {
                var col = NisiColor(g.NisiIndex);
                NisiIcon(g.NisiIndex);
                var who = g.Partners.Count > 0 ? string.Join(", ", g.Partners) : "(holder not found)";
                ImGui.TextColored(col, $"GET {NisiTracker.NisiName(g.NisiIndex)} <- {who}");
                break;
            }
            default:
                ImGui.TextColored(Red, "No verdict debuff on you (?)");
                break;
        }

        // Remaining time on the Nisi you hold
        if (_cfg.ShowMyNisiTimer && g.MyHeld >= 0)
        {
            NisiIcon(g.MyHeld);
            ImGui.TextColored(NisiColor(g.MyHeld),
                $"Holding {NisiTracker.NisiName(g.MyHeld)}: {g.MyHeldRemaining:0.0}s left");
        }

        // Optional party table (off by default: "only me" mode)
        if (!_cfg.ShowOnlyMe)
        {
            ImGui.Separator();
            foreach (var p in _tracker.Players.Values)
            {
                var held = p.Held >= 0 ? NisiTracker.NisiName(p.Held) : "-";
                var verdict = _tracker.Verdicts.TryGetValue(p.Id, out var v) ? NisiTracker.NisiName(v) : "-";
                ImGui.TextColored(p.Held >= 0 ? NisiColor(p.Held) : Grey,
                    $"{p.Name}  hold: {held}  need: {verdict}");
            }
        }
    }
}
