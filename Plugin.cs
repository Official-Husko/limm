using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using UnityEngine;

namespace limm;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

    internal static ConfigEntry<KeyboardShortcut> MenuHotkey;
    internal static ConfigEntry<bool> PauseGameWhenOpen;
    internal static ConfigEntry<bool> UnlockCursorWhenOpen;
    internal static ConfigEntry<float> MenuOpacity;
    internal static ConfigEntry<float> AnimationSpeed;

    private void Awake()
    {
        Logger = base.Logger;

        // Core config
        MenuHotkey = Config.Bind("General", "MenuHotkey", new KeyboardShortcut(KeyCode.Insert), "Hotkey to open/close the mod menu.");
        PauseGameWhenOpen = Config.Bind("General", "PauseGameWhenOpen", false, "Pause Time.timeScale while the menu is open.");
        UnlockCursorWhenOpen = Config.Bind("General", "UnlockCursorWhenOpen", true, "Unlock and show the cursor while the menu is open.");
        MenuOpacity = Config.Bind("General", "MenuOpacity", 0.9f, "Backdrop opacity for the menu (0-1).");
        AnimationSpeed = Config.Bind("General", "AnimationSpeed", 10f, "Fade/scale animation speed.");

        // Create persistent host object (remove previous if hot-reloaded)
        var host = Utils.HostGuard.CreateFresh("ModMenuHost");

        var bootstrap = host.AddComponent<MenuBootstrap>();
        bootstrap.Hotkey = MenuHotkey;
        bootstrap.PauseWhenOpen = PauseGameWhenOpen;
        bootstrap.UnlockCursorWhenOpen = UnlockCursorWhenOpen;
        bootstrap.MenuOpacity = MenuOpacity;
        bootstrap.AnimationSpeed = AnimationSpeed;
        bootstrap.Config = Config;

        // Auto-register all dynamic page registrars (Cheats folder)
        foreach (var type in typeof(Plugin).Assembly.GetTypes())
        {
            if (typeof(IPageRegistrar).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
            {
                if (System.Activator.CreateInstance(type) is IPageRegistrar registrar)
                {
                    registrar.Register();
                }
            }
        }

        Logger.LogInfo($"{MyPluginInfo.PLUGIN_NAME} ({MyPluginInfo.PLUGIN_VERSION}) initialized.");
    }
}
