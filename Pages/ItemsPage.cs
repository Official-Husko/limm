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
    private ConfigEntry<bool> _instantCraft;
    private ConfigEntry<bool> _duplication;

    public void Build(RectTransform content, MenuContext ctx)
    {
        _infiniteResources = ctx.Bind("Items", "InfiniteResources", false, "Never consume building resources.");
        _autoPickup = ctx.Bind("Items", "AutoPickup", true, "Automatically pick up nearby loot.");
        _lootMultiplier = ctx.Bind("Items", "LootMultiplier", 1.0f, "Multiply loot quantity.");
        _instantCraft = ctx.Bind("Items", "InstantCraft", true, "Crafting completes immediately.");
        _duplication = ctx.Bind("Items", "Duplication", false, "Duplicate picked up items.");

        ctx.AddHeader("Items & Loot");
        ctx.AddSeparator();
        var row1 = ctx.AddRow();
        ctx.AddToggleCard("Infinite Resources", _infiniteResources, row1[0]);
        ctx.AddToggleCard("Auto Pickup", _autoPickup, row1[1]);
        ctx.AddSliderCard("Loot Multiplier", _lootMultiplier, 0.5f, 5.0f, 0.1f, row1[2]);

        var row2 = ctx.AddRow();
        ctx.AddToggleCard("Instant Crafting", _instantCraft, row2[0]);
        ctx.AddToggleCard("Duplication (Pickup)", _duplication, row2[1]);
    }

    public void OnOpen(MenuContext ctx) { }
    public void OnClose(MenuContext ctx) { }
}
