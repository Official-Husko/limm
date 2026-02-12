using limm.Pages;

namespace limm.Cheats;

public class EnemiesCheats : IPageRegistrar
{
    public void Register()
    {
        ModMenu.RegisterPage(new DynamicPage(new PageDefinition
        {
            Id = "enemies",
            Title = "Enemies",
            Order = 3,
            Categories =
            {
                new CategoryDefinition
                {
                    Name = "Enemy Controls",
                    Cheats =
                    {
                        CheatBuilders.Toggle("Disable AI", ctx => ctx.Bind("Enemies", "DisableAI", false, "Freeze enemy AI.")),
                        CheatBuilders.Toggle("Show Health Bars", ctx => ctx.Bind("Enemies", "ShowHealthBars", true, "Display enemy health bars.")),
                        CheatBuilders.Slider("Spawn Rate", ctx => ctx.Bind("Enemies", "SpawnRate", 1.0f, "Scale enemy spawn rate."), 0.0f, 3.0f, 0.05f),
                        CheatBuilders.Toggle("One-Hit Kill", ctx => ctx.Bind("Enemies", "OneHitKill", false, "Enemies die in one hit.")),
                        CheatBuilders.Toggle("Freeze Enemies", ctx => ctx.Bind("Enemies", "FreezeEnemies", false, "Stop all enemy movement."))
                    }
                }
            }
        }));
    }
}
