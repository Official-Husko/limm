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
        ctx.AddParagraph("Len's Island Mod Menu example. Hotkey can be changed in BepInEx config. Extend by implementing IMenuPage and calling ModMenu.RegisterPage.");
    }

    public void OnOpen(MenuContext ctx) { }

    public void OnClose(MenuContext ctx) { }
}
