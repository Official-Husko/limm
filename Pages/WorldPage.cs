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
    private ConfigEntry<bool> _noclip;
    private ConfigEntry<float> _fogDensity;

    public void Build(RectTransform content, MenuContext ctx)
    {
        _freezeTime = ctx.Bind("World", "FreezeTime", false, "Pause the day/night cycle.");
        _lockWeather = ctx.Bind("World", "LockWeather", false, "Prevent weather changes.");
        _timeOfDay = ctx.Bind("World", "TimeOfDay", 12f, "Set fixed time of day (0-24).");
        _noclip = ctx.Bind("World", "NoClip", false, "Allow moving through colliders.");
        _fogDensity = ctx.Bind("World", "FogDensity", 1.0f, "Adjust fog density.");

        ctx.AddHeader("World");
        ctx.AddSeparator();
        var row1 = ctx.AddRow();
        ctx.AddToggleCard("Freeze Time", _freezeTime, row1[0]);
        ctx.AddToggleCard("Lock Weather", _lockWeather, row1[1]);
        ctx.AddSliderCard("Time of Day", _timeOfDay, 0f, 24f, 0.25f, row1[2]);

        var row2 = ctx.AddRow();
        ctx.AddToggleCard("NoClip", _noclip, row2[0]);
        ctx.AddSliderCard("Fog Density", _fogDensity, 0f, 3f, 0.05f, row2[1]);
    }

    public void OnOpen(MenuContext ctx) { }
    public void OnClose(MenuContext ctx) { }
}
