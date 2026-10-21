using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace NisiAssist;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IFramework Framework { get; private set; } = null!;
    [PluginService] internal static IObjectTable ObjectTable { get; private set; } = null!;
    [PluginService] internal static IPartyList PartyList { get; private set; } = null!;
    [PluginService] internal static ICondition Condition { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;

    private const string CommandName = "/nisi";

    public Configuration Configuration { get; init; }

    private readonly WindowSystem _windows = new("NisiAssist");
    private readonly NisiTracker _tracker;
    private readonly Fonts _fonts;
    private readonly ConfigWindow _configWindow;
    private readonly OverlayWindow _overlay;
    private readonly CountdownWindow _countdown;
    private readonly WormholeWindow _wormhole;

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        _tracker = new NisiTracker(ObjectTable, PartyList, Condition, Configuration);
        Loc.Thai = Configuration.Language == 1;
        _fonts = new Fonts(PluginInterface);
        var icons = new Icons(TextureProvider, PluginInterface);
        _configWindow = new ConfigWindow(Configuration, _tracker, _fonts);
        _overlay = new OverlayWindow(Configuration, _tracker, icons, _fonts);

        _windows.AddWindow(_configWindow);
        _windows.AddWindow(_overlay);
        _countdown = new CountdownWindow(Configuration, _tracker, icons, _fonts);
        _windows.AddWindow(_countdown);
        _wormhole = new WormholeWindow(Configuration, _tracker, _fonts);
        _windows.AddWindow(_wormhole);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "/nisi = settings, /nisi preview = toggle preview overlay, /nisi lock = lock/unlock overlay, /nisi wh [1-8] = wormhole guide",
        });

        Framework.Update += OnUpdate;
        PluginInterface.UiBuilder.Draw += _windows.Draw;
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi += ToggleConfigUi;
    }

    public void Dispose()
    {
        Framework.Update -= OnUpdate;
        PluginInterface.UiBuilder.Draw -= _windows.Draw;
        PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi -= ToggleConfigUi;

        CommandManager.RemoveHandler(CommandName);

        _windows.RemoveAllWindows();
        _configWindow.Dispose();
        _overlay.Dispose();
        _countdown.Dispose();
        _wormhole.Dispose();
        _fonts.Dispose();
    }

    private void OnUpdate(IFramework framework)
    {
        try
        {
            _tracker.Update();
        }
        catch (System.Exception ex)
        {
            // Never let an exception escape into the game's framework loop.
            Log.Error(ex, "NisiAssist update failed");
        }
    }

    private void OnCommand(string command, string args)
    {
        var arg = args.Trim().ToLowerInvariant();
        if (arg == "wh" || arg.StartsWith("wh "))
        {
            var rest = arg.Length > 2 ? arg.Substring(3).Trim() : "";
            if (int.TryParse(rest, out var num) && num >= 1 && num <= 8)
            {
                Configuration.WormholeNumber = num;
                Configuration.Save();
                _tracker.WormholeManual = true;
            }
            else
            {
                _tracker.WormholeManual = !_tracker.WormholeManual;
            }
            return;
        }

        switch (arg)
        {
            case "preview":
                _tracker.Preview = !_tracker.Preview;
                break;
            case "lock":
                Configuration.OverlayLocked = !Configuration.OverlayLocked;
                Configuration.Save();
                break;
            default:
                ToggleConfigUi();
                break;
        }
    }

    public void ToggleConfigUi() => _configWindow.Toggle();
}
