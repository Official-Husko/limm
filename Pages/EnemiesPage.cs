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

    public void Build(RectTransform content, MenuContext ctx)
    {
        _disableAi = ctx.Bind("Enemies", "DisableAI", false, "Freeze enemy AI.");
        _spawnRate = ctx.Bind("Enemies", "SpawnRate", 1.0f, "Scale enemy spawn rate.");
        _showHealthBars = ctx.Bind("Enemies", "ShowHealthBars", true, "Display enemy health bars.");

        ctx.AddHeader("Enemy Controls");
        ctx.AddToggle("Disable AI", _disableAi);
        ctx.AddToggle("Show Health Bars", _showHealthBars);
        ctx.AddSlider("Spawn Rate", _spawnRate, 0.0f, 3.0f, 0.05f);
    }

    public void OnOpen(MenuContext ctx) { }
    public void OnClose(MenuContext ctx) { }
}
