using BepInEx.Configuration;
using UnityEngine;

namespace limm.Pages;

public class WorldPage : IMenuPage
{
    public string Id => "world";
    public string Title => "World";
    public int Order => 4;

    private ConfigEntry<bool> _freezeTime;
    private ConfigEntry<bool> _lockWeather;
    private ConfigEntry<float> _timeOfDay;

    public void Build(RectTransform content, MenuContext ctx)
    {
        _freezeTime = ctx.Bind("World", "FreezeTime", false, "Pause the day/night cycle.");
        _lockWeather = ctx.Bind("World", "LockWeather", false, "Prevent weather changes.");
        _timeOfDay = ctx.Bind("World", "TimeOfDay", 12f, "Set fixed time of day (0-24).");

        ctx.AddHeader("World");
        ctx.AddToggle("Freeze Time", _freezeTime);
        ctx.AddToggle("Lock Weather", _lockWeather);
        ctx.AddSlider("Time of Day", _timeOfDay, 0f, 24f, 0.25f);
    }

    public void OnOpen(MenuContext ctx) { }
    public void OnClose(MenuContext ctx) { }
}
