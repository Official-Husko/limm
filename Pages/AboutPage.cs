using UnityEngine;

namespace limm.Pages;

public class AboutPage : IMenuPage
{
    public string Id => "about";
    public string Title => "About";
    public int Order => 99;

    public void Build(RectTransform content, MenuContext ctx)
    {
        ctx.AddHeader("About");
        ctx.AddParagraph("Len's Island Mod Menu (sample). Hotkey configurable in BepInEx config. Tabs are extensible via ModMenu.RegisterPage.");
        ctx.AddParagraph("Author: You. Feel free to customize branding, colors, and behaviors in Style.cs and MenuBootstrap.cs.");
    }

    public void OnOpen(MenuContext ctx) { }
    public void OnClose(MenuContext ctx) { }
}
