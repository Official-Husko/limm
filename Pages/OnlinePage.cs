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
    private ConfigEntry<bool> _lagSwitch;
    private ConfigEntry<bool> _ghostMode;

    public void Build(RectTransform content, MenuContext ctx)
    {
        _showNames = ctx.Bind("Online", "ShowPlayerNames", true, "Render player nameplates.");
        _allowInvites = ctx.Bind("Online", "AllowInvites", true, "Allow friend join/invite.");
        _voiceChat = ctx.Bind("Online", "EnableVoiceChat", true, "Enable in-game voice chat overlay.");
        _lagSwitch = ctx.Bind("Online", "LagSwitch", false, "Introduce latency for other players (visual only).");
        _ghostMode = ctx.Bind("Online", "GhostMode", false, "Hide presence indicators.");

        ctx.AddHeader("Online");
        ctx.AddSeparator();
        var row1 = ctx.AddRow();
        ctx.AddToggleCard("Show Player Names", _showNames, row1[0]);
        ctx.AddToggleCard("Allow Invites", _allowInvites, row1[1]);
        ctx.AddToggleCard("Voice Chat Overlay", _voiceChat, row1[2]);

        var row2 = ctx.AddRow();
        ctx.AddToggleCard("Lag Switch (visual)", _lagSwitch, row2[0]);
        ctx.AddToggleCard("Ghost Mode (presence)", _ghostMode, row2[1]);
    }

    public void OnOpen(MenuContext ctx) { }
    public void OnClose(MenuContext ctx) { }
}
