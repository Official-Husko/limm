using System;
using BepInEx.Configuration;
using UnityEngine;

namespace limm.Pages;

public static class CheatBuilders
{
    public static CheatBuilder Toggle(string label, Func<MenuContext, ConfigEntry<bool>> bind, Action<bool> onChanged = null)
    {
        return (ctx, col) =>
        {
            var entry = bind(ctx);
            ctx.AddToggleCard(label, entry, col, onChanged);
        };
    }

    public static CheatBuilder Slider(string label, Func<MenuContext, ConfigEntry<float>> bind, float min, float max, float step, Action<float> onChanged = null)
    {
        return (ctx, col) =>
        {
            var entry = bind(ctx);
            ctx.AddSliderCard(label, entry, min, max, step, col, onChanged);
        };
    }
}
