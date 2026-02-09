using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace limm;

/// <summary>
/// Central registry for menu pages and events.
/// </summary>
public static class ModMenu
{
    private static readonly List<IMenuPage> PagesInternal = new();

    /// <summary>
    /// Fired whenever the menu is shown or hidden.
    /// </summary>
    public static event Action<bool> OnMenuToggled;

    internal static IReadOnlyList<IMenuPage> Pages => PagesInternal;

    public static void RegisterPage(IMenuPage page)
    {
        if (page == null)
            return;

        // Prevent duplicate IDs.
        if (PagesInternal.Any(p => p.Id == page.Id))
            return;

        PagesInternal.Add(page);
        PagesInternal.Sort((a, b) => a.Order.CompareTo(b.Order));
    }

    internal static void RaiseToggled(bool isOpen) => OnMenuToggled?.Invoke(isOpen);
}

public interface IMenuPage
{
    string Id { get; }
    string Title { get; }
    int Order { get; }
    void Build(RectTransform content, MenuContext ctx);
    void OnOpen(MenuContext ctx);
    void OnClose(MenuContext ctx);
}
