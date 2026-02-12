using limm.Pages;

namespace limm.Cheats;

public class OnlineCheats : IPageRegistrar
{
    public void Register()
    {
        ModMenu.RegisterPage(new DynamicPage(new PageDefinition
        {
            Id = "online",
            Title = "Online",
            Order = 1,
            Categories =
            {
                new CategoryDefinition
                {
                    Name = "Online",
                    Cheats =
                    {
                        CheatBuilders.Toggle("Show Player Names", ctx => ctx.Bind("Online", "ShowPlayerNames", true, "Render player nameplates.")),
                        CheatBuilders.Toggle("Allow Invites", ctx => ctx.Bind("Online", "AllowInvites", true, "Allow friend join/invite.")),
                        CheatBuilders.Toggle("Voice Chat Overlay", ctx => ctx.Bind("Online", "EnableVoiceChat", true, "Enable in-game voice chat overlay.")),
                        CheatBuilders.Toggle("Lag Switch (visual)", ctx => ctx.Bind("Online", "LagSwitch", false, "Introduce latency for other players (visual only).")),
                        CheatBuilders.Toggle("Ghost Mode (presence)", ctx => ctx.Bind("Online", "GhostMode", false, "Hide presence indicators."))
                    }
                }
            }
        }));
    }
}
