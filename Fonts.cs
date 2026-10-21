using System;
using Dalamud.Interface.GameFonts;
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
    // Native pixel sizes of the game fonts used below.
    private const float AxisSmallPx = 36f;     // Axis36
    private const float AxisBigPx = 96f;       // Axis96
    private const float DigitsPx = 184f;       // TrumpGothic184 (the game's big-number font)
    private const float DefaultImGuiPx = 17f;  // Dalamud's default UI font size, used for the "Text size" slider

    private readonly IFontHandle _small;
    private readonly IFontHandle _big;
    private readonly IFontHandle _digits;
    private readonly IFontHandle? _thaiSmall;
    private readonly IFontHandle? _thaiBig;
    private readonly IFontHandle? _thaiUi;

    public Fonts(IDalamudPluginInterface pi)
    {
        var atlas = pi.UiBuilder.FontAtlas;
        _small = atlas.NewGameFontHandle(new GameFontStyle(GameFontFamilyAndSize.Axis36));
        _big = atlas.NewGameFontHandle(new GameFontStyle(GameFontFamilyAndSize.Axis96));
        _digits = atlas.NewGameFontHandle(new GameFontStyle(GameFontFamilyAndSize.TrumpGothic184));

        // Thai-capable font (Sarabun, SIL OFL), embedded in the DLL (ThaiFontData.cs).
        {
            _thaiSmall = MakeThai(atlas, AxisSmallPx);
            _thaiBig = MakeThai(atlas, AxisBigPx);
            _thaiUi = MakeThai(atlas, DefaultImGuiPx);
            Loc.ThaiReady = () => _thaiSmall is { Available: true } && _thaiBig is { Available: true };
        }
    }

    // Basic Latin + Latin-1 + Thai block. Range list is pairs of code points, terminated by 0.
    private static readonly ushort[] ThaiRanges = { 0x0020, 0x00FF, 0x0E01, 0x0E5B, 0 };

    private static IFontHandle MakeThai(IFontAtlas atlas, float sizePx) =>
        atlas.NewDelegateFontHandle(e => e.OnPreBuild(tk =>
            tk.AddFontFromMemory(ThaiFontData.Bytes, new SafeFontConfig { SizePx = sizePx, GlyphRanges = ThaiRanges }, "NisiAssist Thai")));

    public bool ThaiAvailable => _thaiSmall != null;

    /// <summary>Default-size Thai font for normal ImGui widgets (settings window). Null when unavailable.</summary>
    public IDisposable? PushThaiUi() => _thaiUi is { Available: true } ? _thaiUi.Push() : null;

    /// <summary>Latin text at roughly <paramref name="px"/> pixels tall. Dispose the result (null is fine) when done.</summary>
    public IDisposable? PushText(float px, out float scale)
    {
        var useBig = px > AxisSmallPx;
        var handle = useBig ? _big : _small;
        if (Loc.Thai && (useBig ? _thaiBig : _thaiSmall) is { Available: true } thai)
            handle = thai;
        if (!handle.Available)
        {
            scale = px / DefaultImGuiPx;   // fallback: default font, scaled
            return null;
        }
        scale = px / (useBig ? AxisBigPx : AxisSmallPx);
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
        _thaiSmall?.Dispose();
        _thaiBig?.Dispose();
        _thaiUi?.Dispose();
    }
}
