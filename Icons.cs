using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace NisiAssist;

/// <summary>Loads the small PNG icons shipped in the "icons" folder next to the DLL.</summary>
public sealed class Icons
{
    private readonly ITextureProvider _tex;
    private readonly string _dir;
    private readonly Dictionary<string, string> _paths = new();

    public static readonly string[] NisiNames = { "alpha", "beta", "gamma", "delta" };

    public Icons(ITextureProvider tex, IDalamudPluginInterface pi)
    {
        _tex = tex;
        _dir = Path.Combine(pi.AssemblyLocation.DirectoryName ?? "", "icons");
    }

    /// <summary>Draws the icon at the cursor, sized to the current text line. Returns true if drawn (caller then uses SameLine).</summary>
    public bool Draw(string name, float size)
    {
        try
        {
            if (!_paths.TryGetValue(name, out var path))
                _paths[name] = path = Path.Combine(_dir, name + ".png");
            if (!File.Exists(path))
                return false;

            var wrap = _tex.GetFromFile(path).GetWrapOrEmpty();
            ImGui.Image(wrap.Handle, new Vector2(size, size));
            return true;
        }
        catch
        {
            return false;
        }
    }
}
