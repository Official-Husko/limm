using System.Collections.Generic;
using BepInEx.Configuration;
using UnityEngine;

namespace limm.Pages;

public class VisualsPage : IMenuPage
{
    public string Id => "visuals";
    public string Title => "Visuals";
    public int Order => 2;

    private ConfigEntry<bool> _crosshair;
    private ConfigEntry<float> _uiScale;
    private ConfigEntry<string> _theme;

    public void Build(RectTransform content, MenuContext ctx)
    {
        _crosshair = ctx.Bind("Visuals", "Crosshair", true, "Show a center crosshair overlay.");
        _uiScale = ctx.Bind("Visuals", "UiScale", 1.0f, "Scale the mod menu UI.");
        _theme = ctx.Bind("Visuals", "Theme", "Midnight", "Menu accent theme.");

        ctx.AddHeader("Visual Settings");
        ctx.AddToggle("Crosshair", _crosshair);
        ctx.AddSlider("UI Scale", _uiScale, 0.8f, 1.4f, 0.05f, v => ApplyUiScale(v));
        ctx.AddDropdown("Theme", _theme, new List<string> { "Midnight", "Ocean", "Sunset" }, s => ApplyTheme(s));
    }

    private void ApplyUiScale(float scale)
    {
        // Placeholder: hook into menu scaler or game UI when available.
    }

    private void ApplyTheme(string theme)
    {
        // Placeholder: adjust palette if desired. Kept no-op for now.
    }

    public void OnOpen(MenuContext ctx) { }

    public void OnClose(MenuContext ctx) { }
}
