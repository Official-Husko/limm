using limm.Pages;

namespace limm.Cheats;

public class WorldCheats : IPageRegistrar
{
    public void Register()
    {
        ModMenu.RegisterPage(new DynamicPage(new PageDefinition
        {
            Id = "world",
            Title = "World",
            Order = 4,
            Categories =
            {
                new CategoryDefinition
                {
                    Name = "World",
                    Cheats =
                    {
                        CheatBuilders.Toggle("Freeze Time", ctx => ctx.Bind("World", "FreezeTime", false, "Pause the day/night cycle.")),
                        CheatBuilders.Toggle("Lock Weather", ctx => ctx.Bind("World", "LockWeather", false, "Prevent weather changes.")),
                        CheatBuilders.Slider("Time of Day", ctx => ctx.Bind("World", "TimeOfDay", 12f, "Set fixed time of day (0-24)."), 0f, 24f, 0.25f),
                        CheatBuilders.Toggle("NoClip", ctx => ctx.Bind("World", "NoClip", false, "Allow moving through colliders.")),
                        CheatBuilders.Slider("Fog Density", ctx => ctx.Bind("World", "FogDensity", 1.0f, "Adjust fog density."), 0f, 3f, 0.05f)
                    }
                }
            }
        }));
    }
}
