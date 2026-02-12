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
    private ConfigEntry<bool> _noFallDamage;
    private ConfigEntry<bool> _silentFootsteps;
    private ConfigEntry<float> _jumpHeight;

    public void Build(RectTransform content, MenuContext ctx)
    {
        _godMode = ctx.Bind("Player", "GodMode", false, "Prevent player damage.");
        _speedMultiplier = ctx.Bind("Player", "SpeedMultiplier", 1.0f, "Adjust player speed.");
        _infiniteStamina = ctx.Bind("Player", "InfiniteStamina", false, "Never run out of stamina.");
        _noFallDamage = ctx.Bind("Player", "NoFallDamage", true, "Disable fall damage.");
        _silentFootsteps = ctx.Bind("Player", "SilentFootsteps", false, "Mute footstep sounds.");
        _jumpHeight = ctx.Bind("Player", "JumpHeight", 1.0f, "Scale jump height.");

        ctx.AddHeader("Vitals");
        ctx.AddSeparator();
        var vitalsRow = ctx.AddRow();
        ctx.AddToggleCard("God Mode", _godMode, vitalsRow[0]);
        ctx.AddToggleCard("Infinite Stamina", _infiniteStamina, vitalsRow[1]);
        ctx.AddToggleCard("No Fall Damage", _noFallDamage, vitalsRow[2]);

        ctx.AddHeader("Movement");
        ctx.AddSeparator();
        var moveRow = ctx.AddRow();
        ctx.AddSliderCard("Speed Multiplier", _speedMultiplier, 0.5f, 3.0f, 0.05f, moveRow[0]);
        ctx.AddSliderCard("Jump Height", _jumpHeight, 0.5f, 3.0f, 0.05f, moveRow[1]);
        ctx.AddToggleCard("Silent Footsteps", _silentFootsteps, moveRow[2]);
    }

    public void OnOpen(MenuContext ctx) { }
    public void OnClose(MenuContext ctx) { }
}
