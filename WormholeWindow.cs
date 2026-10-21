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

        var pos = _cfg.WormholePosition;
        var myPuddle = _cfg.PuddleOfPosition(pos);

        ImGui.TextColored(White, Loc.WormholeTitle);

        // ---- Glance line: my position and my puddle, BIG ----
        if (pos >= 1 && pos <= 8)
        {
            ImGui.SetWindowFontScale(fontScale * 2.2f);
            if (now != 0)
            {
                if (myPuddle == now)
                    ImGui.TextColored(Green, Loc.GoNow(now));
                else
                    ImGui.TextColored(Grey, Loc.NotYou(now, _cfg.WhoIn(now)));
            }
            else
            {
                ImGui.TextColored(myPuddle > 0 ? Cyan : Grey, Loc.MyRoleLine(Configuration.PositionNames[pos - 1], myPuddle));
            }
            ImGui.SetWindowFontScale(fontScale);
        }
        else
        {
            ImGui.TextColored(Yellow, Loc.PickRole);
        }

        // Who goes where (only shown once you have ticked something in /nisi)
        var w1 = _cfg.WhoIn(1); var w2 = _cfg.WhoIn(2); var w3 = _cfg.WhoIn(3);
        if (w1 != "" || w2 != "" || w3 != "")
            ImGui.TextColored(Grey, Loc.PuddleWho(w1, w2, w3));

        // Your Limit Cut number text (unchanged from before)
        if (n >= 1 && n <= 8)
        {
            ImGui.TextColored(Cyan, $"#{n}  {Line(Loc.Thai ? _cfg.WormholeEnterTh : _cfg.WormholeEnter, n)}");
            ImGui.TextColored(White, Loc.After(Line(Loc.Thai ? _cfg.WormholeAfterTh : _cfg.WormholeAfter, n)));
            ImGui.TextColored(Grey, Loc.Then(Loc.Thai ? _cfg.WormholeFooterTh : _cfg.WormholeFooter));
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
            ImGui.TextColored(Grey, "Position:");
            for (var i = 0; i < 8; i++)
            {
                ImGui.SameLine();
                if (ImGui.SmallButton($"{Configuration.PositionNames[i]}##wp{i}"))
                    _cfg.SetPosition(i + 1);
            }
        }
    }

    private static string Line(string[] arr, int n) =>
        arr != null && n >= 1 && n <= arr.Length ? arr[n - 1] : "";
}
