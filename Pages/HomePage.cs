using BepInEx.Configuration;
using UnityEngine;

namespace limm.Pages;

public class HomePage : IMenuPage
{
    public string Id => "home";
    public string Title => "Home";
    public int Order => 0;

    private ConfigEntry<bool> _showHints;

    public void Build(RectTransform content, MenuContext ctx)
    {
        _showHints = ctx.Bind("Home", "ShowHints", true, "Show helper text in the menu.");

        ctx.AddHeader("Welcome");
        ctx.AddParagraph("Mod menu is active. Use tabs to explore sample settings and extend with your own pages via ModMenu.RegisterPage.");
        ctx.AddToggle("Show helper hints", _showHints);
    }

    public void OnOpen(MenuContext ctx) { }

    public void OnClose(MenuContext ctx) { }
}
