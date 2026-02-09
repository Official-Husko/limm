using BepInEx.Configuration;
using UnityEngine;

namespace limm.Pages;

public class GameplayPage : IMenuPage
{
    public string Id => "gameplay";
    public string Title => "Gameplay";
    public int Order => 1;

    private ConfigEntry<bool> _godMode;
    private ConfigEntry<float> _speedMultiplier;
    private ConfigEntry<bool> _infiniteStamina;

    public void Build(RectTransform content, MenuContext ctx)
    {
        _godMode = ctx.Bind("Gameplay", "GodMode", false, "Prevent the player from taking damage.");
        _speedMultiplier = ctx.Bind("Gameplay", "SpeedMultiplier", 1.0f, "Adjust player movement speed.");
        _infiniteStamina = ctx.Bind("Gameplay", "InfiniteStamina", false, "Never run out of stamina.");

        ctx.AddHeader("Gameplay Tweaks");
        ctx.AddToggle("God Mode", _godMode);
        ctx.AddToggle("Infinite Stamina", _infiniteStamina);
        ctx.AddSlider("Speed Multiplier", _speedMultiplier, 0.5f, 3.0f, 0.05f);
    }

    public void OnOpen(MenuContext ctx) { }

    public void OnClose(MenuContext ctx) { }
}
