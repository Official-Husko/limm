using BepInEx.Configuration;
using UnityEngine;

namespace limm.Pages;

public class OnlinePage : IMenuPage
{
    public string Id => "online";
    public string Title => "Online";
    public int Order => 1;

    private ConfigEntry<bool> _showNames;
    private ConfigEntry<bool> _allowInvites;
    private ConfigEntry<bool> _voiceChat;

    public void Build(RectTransform content, MenuContext ctx)
    {
        _showNames = ctx.Bind("Online", "ShowPlayerNames", true, "Render player nameplates.");
        _allowInvites = ctx.Bind("Online", "AllowInvites", true, "Allow friend join/invite.");
        _voiceChat = ctx.Bind("Online", "EnableVoiceChat", true, "Enable in-game voice chat overlay.");

        ctx.AddHeader("Online");
        ctx.AddToggle("Show Player Names", _showNames);
        ctx.AddToggle("Allow Invites", _allowInvites);
        ctx.AddToggle("Voice Chat Overlay", _voiceChat);
    }

    public void OnOpen(MenuContext ctx) { }
    public void OnClose(MenuContext ctx) { }
}
