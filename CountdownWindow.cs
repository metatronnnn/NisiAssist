using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace NisiAssist;

/// <summary>
/// Big, see-through countdown number for the water / lightning debuff on you.
/// Appears only for the last N seconds (default 5). Drag it into place with "/nisi preview".
/// </summary>
public sealed class CountdownWindow : Window, IDisposable
{
    private readonly Configuration _cfg;
    private readonly NisiTracker _tracker;
    private readonly Icons _icons;

    private const ImGuiWindowFlags BaseFlags =
        ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoCollapse |
        ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoNav |
        ImGuiWindowFlags.NoBackground;

    private static readonly Vector4 WaterColor = new(0.30f, 0.72f, 1.00f, 0.95f);
    private static readonly Vector4 LightningColor = new(1.00f, 0.92f, 0.25f, 0.95f);

    public CountdownWindow(Configuration cfg, NisiTracker tracker, Icons icons)
        : base("Nisi Assist countdown##countdown", BaseFlags)
    {
        _cfg = cfg;
        _tracker = tracker;
        _icons = icons;
        RespectCloseHotkey = false;
        IsOpen = true;
        Position = new Vector2(720, 260);
        PositionCondition = ImGuiCond.FirstUseEver;
    }

    public void Dispose() { }

    public override bool DrawConditions() => _tracker.Countdowns.Count > 0;

    public override void PreDraw()
    {
        Flags = BaseFlags;
        if (_cfg.OverlayLocked && !_tracker.Preview)
            Flags |= ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoInputs;
    }

    public override void Draw()
    {
        foreach (var c in _tracker.Countdowns)
        {
            var color = c.Kind == DebuffKind.Water ? WaterColor : LightningColor;
            var label = c.Kind == DebuffKind.Water ? "WATER" : "LIGHTNING";
            var number = Math.Max(1, (int)Math.Ceiling(c.Remaining)).ToString();

            // Same font-scale call as the main overlay; swap it here if your Dalamud version changes it.
            ImGui.SetWindowFontScale(_cfg.CountdownFontScale);
            ImGui.TextColored(color, number);

            ImGui.SetWindowFontScale(Math.Max(1.2f, _cfg.CountdownFontScale * 0.25f));
            if (_cfg.ShowIcons && _icons.Draw(c.Kind == DebuffKind.Water ? "water" : "lightning", ImGui.GetTextLineHeight()))
                ImGui.SameLine();
            ImGui.TextColored(color, label);
        }
    }
}
