using BepInEx.Configuration;
using UnityEngine;

namespace limm.Pages;

public class ItemsPage : IMenuPage
{
    public string Id => "items";
    public string Title => "Items";
    public int Order => 2;

    private ConfigEntry<bool> _infiniteResources;
    private ConfigEntry<bool> _autoPickup;
    private ConfigEntry<float> _lootMultiplier;

    public void Build(RectTransform content, MenuContext ctx)
    {
        _infiniteResources = ctx.Bind("Items", "InfiniteResources", false, "Never consume building resources.");
        _autoPickup = ctx.Bind("Items", "AutoPickup", true, "Automatically pick up nearby loot.");
        _lootMultiplier = ctx.Bind("Items", "LootMultiplier", 1.0f, "Multiply loot quantity.");

        ctx.AddHeader("Items & Loot");
        ctx.AddToggle("Infinite Resources", _infiniteResources);
        ctx.AddToggle("Auto Pickup", _autoPickup);
        ctx.AddSlider("Loot Multiplier", _lootMultiplier, 0.5f, 5.0f, 0.1f);
    }

    public void OnOpen(MenuContext ctx) { }
    public void OnClose(MenuContext ctx) { }
}
