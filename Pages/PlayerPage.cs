using BepInEx.Configuration;
using UnityEngine;

namespace limm.Pages;

public class PlayerPage : IMenuPage
{
    public string Id => "player";
    public string Title => "Player";
    public int Order => 0;

    private ConfigEntry<bool> _godMode;
    private ConfigEntry<float> _speedMultiplier;
    private ConfigEntry<bool> _infiniteStamina;

    public void Build(RectTransform content, MenuContext ctx)
    {
        _godMode = ctx.Bind("Player", "GodMode", false, "Prevent player damage.");
        _speedMultiplier = ctx.Bind("Player", "SpeedMultiplier", 1.0f, "Adjust player speed.");
        _infiniteStamina = ctx.Bind("Player", "InfiniteStamina", false, "Never run out of stamina.");

        ctx.AddHeader("Player Controls");
        ctx.AddToggle("God Mode", _godMode);
        ctx.AddToggle("Infinite Stamina", _infiniteStamina);
        ctx.AddSlider("Speed Multiplier", _speedMultiplier, 0.5f, 3.0f, 0.05f);
    }

    public void OnOpen(MenuContext ctx) { }
    public void OnClose(MenuContext ctx) { }
}
