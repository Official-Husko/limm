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

        // Create persistent host object
        var host = new GameObject("ModMenuHost");
        DontDestroyOnLoad(host);

        var bootstrap = host.AddComponent<MenuBootstrap>();
        bootstrap.Hotkey = MenuHotkey;
        bootstrap.PauseWhenOpen = PauseGameWhenOpen;
        bootstrap.UnlockCursorWhenOpen = UnlockCursorWhenOpen;
        bootstrap.MenuOpacity = MenuOpacity;
        bootstrap.AnimationSpeed = AnimationSpeed;
        bootstrap.Config = Config;

        // Register built-in pages once; external mods can also call ModMenu.RegisterPage
        // Register built-in pages matching requested tabs
        ModMenu.RegisterPage(new Pages.PlayerPage());
        ModMenu.RegisterPage(new Pages.OnlinePage());
        ModMenu.RegisterPage(new Pages.ItemsPage());
        ModMenu.RegisterPage(new Pages.EnemiesPage());
        ModMenu.RegisterPage(new Pages.WorldPage());

        Logger.LogInfo($"{MyPluginInfo.PLUGIN_NAME} ({MyPluginInfo.PLUGIN_VERSION}) initialized.");
    }
}
