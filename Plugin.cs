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
    private readonly ConfigWindow _configWindow;
    private readonly OverlayWindow _overlay;
    private readonly CountdownWindow _countdown;

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        _tracker = new NisiTracker(ObjectTable, PartyList, Condition, Configuration);
        var icons = new Icons(TextureProvider, PluginInterface);
        _configWindow = new ConfigWindow(Configuration, _tracker);
        _overlay = new OverlayWindow(Configuration, _tracker, icons);

        _windows.AddWindow(_configWindow);
        _windows.AddWindow(_overlay);
        _countdown = new CountdownWindow(Configuration, _tracker, icons);
        _windows.AddWindow(_countdown);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "/nisi = settings, /nisi preview = toggle preview overlay, /nisi lock = lock/unlock overlay",
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
        switch (args.Trim().ToLowerInvariant())
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
