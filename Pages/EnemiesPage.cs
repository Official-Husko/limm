using BepInEx.Configuration;
using UnityEngine;

namespace limm.Pages;

public class EnemiesPage : IMenuPage
{
    public string Id => "enemies";
    public string Title => "Enemies";
    public int Order => 3;

    private ConfigEntry<bool> _disableAi;
    private ConfigEntry<float> _spawnRate;
    private ConfigEntry<bool> _showHealthBars;
    private ConfigEntry<bool> _oneHitKill;
    private ConfigEntry<bool> _freezeEnemies;

    public void Build(RectTransform content, MenuContext ctx)
    {
        _disableAi = ctx.Bind("Enemies", "DisableAI", false, "Freeze enemy AI.");
        _spawnRate = ctx.Bind("Enemies", "SpawnRate", 1.0f, "Scale enemy spawn rate.");
        _showHealthBars = ctx.Bind("Enemies", "ShowHealthBars", true, "Display enemy health bars.");
        _oneHitKill = ctx.Bind("Enemies", "OneHitKill", false, "Enemies die in one hit.");
        _freezeEnemies = ctx.Bind("Enemies", "FreezeEnemies", false, "Stop all enemy movement.");

        ctx.AddHeader("Enemy Controls");
        ctx.AddSeparator();
        var row1 = ctx.AddRow();
        ctx.AddToggleCard("Disable AI", _disableAi, row1[0]);
        ctx.AddToggleCard("Show Health Bars", _showHealthBars, row1[1]);
        ctx.AddSliderCard("Spawn Rate", _spawnRate, 0.0f, 3.0f, 0.05f, row1[2]);

        var row2 = ctx.AddRow();
        ctx.AddToggleCard("One-Hit Kill", _oneHitKill, row2[0]);
        ctx.AddToggleCard("Freeze Enemies", _freezeEnemies, row2[1]);
    }

    public void OnOpen(MenuContext ctx) { }
    public void OnClose(MenuContext ctx) { }
}
