using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace NisiAssist;

/// <summary>
/// Phase 3 part 2 "Wormhole Formation": shows what YOUR number (1-8) has to do.
/// Pick your number with the buttons here, in /nisi, or with "/nisi wh 3".
/// </summary>
public sealed class WormholeWindow : Window, IDisposable
{
    private readonly Configuration _cfg;
    private readonly NisiTracker _tracker;
    private readonly Fonts _fonts;

    private const ImGuiWindowFlags BaseFlags =
        ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoCollapse |
        ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoNav;

    private static readonly Vector4 White = new(1f, 1f, 1f, 1f);
    private static readonly Vector4 Grey = new(0.7f, 0.7f, 0.7f, 1f);
    private static readonly Vector4 Yellow = new(1f, 0.9f, 0.3f, 1f);
    private static readonly Vector4 Green = new(0.45f, 0.95f, 0.5f, 1f);
    private static readonly Vector4 Cyan = new(0.4f, 0.85f, 1f, 1f);

    public WormholeWindow(Configuration cfg, NisiTracker tracker, Fonts fonts)
        : base("Nisi Assist wormhole##wormhole", BaseFlags)
    {
        _cfg = cfg;
        _tracker = tracker;
        _fonts = fonts;
        RespectCloseHotkey = false;
        IsOpen = true;
        Position = new Vector2(80, 420);
        PositionCondition = ImGuiCond.FirstUseEver;
    }

    public void Dispose() { }

    public override bool DrawConditions() => _cfg.ShowWormhole && _tracker.WormholeVisible;

    public override void PreDraw()
    {
        Flags = BaseFlags;
        if (_cfg.OverlayLocked && !_tracker.Preview)
            Flags |= ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoInputs;
        BgAlpha = _cfg.BackgroundAlpha;
    }

    public override void Draw()
    {
        using var font = _fonts.PushText(Fonts.TextPx(_cfg.FontScale, _cfg.UiScale), out var fontScale);
        ImGui.SetWindowFontScale(fontScale);

        var n = _cfg.WormholeNumber;
        var now = _tracker.WormholePuddle;   // puddle being cast right now (0 = none)

        ImGui.TextColored(White, "WORMHOLE");
        ImGui.TextColored(Grey, "Puddles: #1 = 5,6  |  #2 = 7,8  |  #3 = 1,2  |  3,4 none");

        if (n < 1 || n > 8)
        {
            ImGui.TextColored(Yellow, "Pick your number:");
        }
        else
        {
            ImGui.TextColored(Cyan, $"#{n}  {Line(_cfg.WormholeEnter, n)}");
            ImGui.TextColored(White, $"After: {Line(_cfg.WormholeAfter, n)}");

            var mine = NisiTracker.PuddleFor(n);
            if (now != 0)
            {
                if (now == mine)
                    ImGui.TextColored(Green, $">> SOAK PUDDLE #{now} NOW <<");
                else
                    ImGui.TextColored(Grey, $"Puddle #{now} now (not yours)");
            }
        }

        // Quick number picker (only while the overlay is unlocked so clicks work).
        if (!_cfg.OverlayLocked || _tracker.Preview)
        {
            for (var i = 1; i <= 8; i++)
            {
                if (i > 1) ImGui.SameLine();
                if (ImGui.SmallButton($"{i}##wh{i}"))
                {
                    _cfg.WormholeNumber = i;
                    _cfg.Save();
                }
            }
        }
    }

    private static string Line(string[] arr, int n) =>
        arr != null && n >= 1 && n <= arr.Length ? arr[n - 1] : "";
}
