using BepInEx.Configuration;
using limm.Pages;

namespace limm.Cheats;

public class PlayerCheats : IPageRegistrar
{
    public void Register()
    {
        ModMenu.RegisterPage(new DynamicPage(new PageDefinition
        {
            Id = "player",
            Title = "Player",
            Order = 0,
            Categories =
            {
                new CategoryDefinition
                {
                    Name = "Vitals",
                    Order = 0,
                    Cheats =
                    {
                        CheatBuilders.Toggle("God Mode", ctx => ctx.Bind("Player", "GodMode", false, "Prevent player damage.")),
                        CheatBuilders.Toggle("Infinite Stamina", ctx => ctx.Bind("Player", "InfiniteStamina", false, "Never run out of stamina.")),
                        CheatBuilders.Toggle("No Fall Damage", ctx => ctx.Bind("Player", "NoFallDamage", true, "Disable fall damage."))
                    }
                },
                new CategoryDefinition
                {
                    Name = "Movement",
                    Order = 1,
                    Cheats =
                    {
                        CheatBuilders.Slider("Speed Multiplier", ctx => ctx.Bind("Player", "SpeedMultiplier", 1.15f, "Adjust player speed."), 0.5f, 3.0f, 0.05f),
                        CheatBuilders.Slider("Jump Height", ctx => ctx.Bind("Player", "JumpHeight", 1.0f, "Scale jump height."), 0.5f, 3.0f, 0.05f),
                        CheatBuilders.Toggle("Silent Footsteps", ctx => ctx.Bind("Player", "SilentFootsteps", false, "Mute footstep sounds."))
                    }
                }
            }
        }));
    }
}
