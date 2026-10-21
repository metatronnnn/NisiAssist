using System;
using System.IO;
using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Interface.Utility;
using Dalamud.Plugin;

namespace NisiAssist;

/// <summary>
/// Crisp text at any size. Instead of stretching the small default font (which gets blurry/pixelated),
/// we push a game font that was rasterised at a large size and only scale it DOWN a little.
/// </summary>
public sealed class Fonts : IDisposable
{
    // Native pixel sizes the embedded font is rasterised at (explicit, so scaling maths is exact).
    private const float SmallPx = 36f;
    private const float BigPx = 96f;
    private const float DigitsPx = 192f;
    private const float DefaultImGuiPx = 17f;  // Dalamud's default UI font size, used for the "Text size" slider

    private readonly IFontHandle _small;
    private readonly IFontHandle _big;
    private readonly IFontHandle _digits;
    private readonly IFontHandle _ui;

    public Fonts(IDalamudPluginInterface pi)
    {
        var atlas = pi.UiBuilder.FontAtlas;
        // One embedded font (Sarabun, SIL OFL) for Latin + Thai, at three big sizes so scaling stays sharp.
        _small = Make(atlas, SmallPx, TextRanges);
        _big = Make(atlas, BigPx, TextRanges);
        _digits = Make(atlas, DigitsPx, DigitRanges);
        _ui = Make(atlas, DefaultImGuiPx, TextRanges);
        Loc.ThaiReady = () => _small.Available && _big.Available;
    }

    // Basic Latin + Latin-1 + Thai block. Range list is pairs of code points, terminated by 0.
    private static readonly ushort[] TextRanges = { 0x0020, 0x00FF, 0x0E01, 0x0E5B, 0 };
    private static readonly ushort[] DigitRanges = { 0x002E, 0x003A, 0 };   // . / 0-9 :

    private static IFontHandle Make(IFontAtlas atlas, float sizePx, ushort[] ranges) =>
        atlas.NewDelegateFontHandle(e => e.OnPreBuild(tk =>
            tk.AddFontFromMemory(ThaiFontData.Bytes, new SafeFontConfig { SizePx = sizePx, GlyphRanges = ranges }, "NisiAssist font")));

    public bool ThaiAvailable => _small.Available;

    /// <summary>Default-size font for normal ImGui widgets (settings window). Null when unavailable.</summary>
    public IDisposable? PushThaiUi() => _ui.Available ? _ui.Push() : null;

    /// <summary>Text at roughly <paramref name="px"/> pixels tall. Dispose the result (null is fine) when done.</summary>
    public IDisposable? PushText(float px, out float scale)
    {
        var useBig = px > SmallPx;
        var handle = useBig ? _big : _small;
        if (!handle.Available)
        {
            scale = px / DefaultImGuiPx;   // fallback: default font, scaled
            return null;
        }
        scale = px / (useBig ? BigPx : SmallPx);
        return handle.Push();
    }

    /// <summary>Big countdown digits at roughly <paramref name="px"/> pixels tall.</summary>
    public IDisposable? PushDigits(float px, out float scale)
    {
        if (!_digits.Available)
        {
            scale = px / DefaultImGuiPx;
            return null;
        }
        scale = px / DigitsPx;
        return _digits.Push();
    }

    /// <summary>Overall multiplier = your "Overall scale" setting x Dalamud's global UI scale (DPI).</summary>
    public static float Mul(float uiScale) => Math.Max(0.1f, uiScale) * ImGuiHelpers.GlobalScale;

    public static float TextPx(float fontScale, float uiScale) => fontScale * DefaultImGuiPx * Mul(uiScale);

    public void Dispose()
    {
        _small.Dispose();
        _big.Dispose();
        _digits.Dispose();
        _ui.Dispose();
    }
}
