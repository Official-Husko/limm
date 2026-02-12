using limm.Pages;

namespace limm.Cheats;

public class ItemsCheats : IPageRegistrar
{
    public void Register()
    {
        ModMenu.RegisterPage(new DynamicPage(new PageDefinition
        {
            Id = "items",
            Title = "Items",
            Order = 2,
            Categories =
            {
                new CategoryDefinition
                {
                    Name = "Items & Loot",
                    Cheats =
                    {
                        CheatBuilders.Toggle("Infinite Resources", ctx => ctx.Bind("Items", "InfiniteResources", false, "Never consume building resources.")),
                        CheatBuilders.Toggle("Auto Pickup", ctx => ctx.Bind("Items", "AutoPickup", true, "Automatically pick up nearby loot.")),
                        CheatBuilders.Slider("Loot Multiplier", ctx => ctx.Bind("Items", "LootMultiplier", 1.0f, "Multiply loot quantity."), 0.5f, 5.0f, 0.1f),
                        CheatBuilders.Toggle("Instant Crafting", ctx => ctx.Bind("Items", "InstantCraft", true, "Crafting completes immediately.")),
                        CheatBuilders.Toggle("Duplication (Pickup)", ctx => ctx.Bind("Items", "Duplication", false, "Duplicate picked up items."))
                    }
                }
            }
        }));
    }
}
