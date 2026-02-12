using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace limm.Pages;

public delegate void CheatBuilder(MenuContext ctx, RectTransform column);

public sealed class PageDefinition
{
    public string Id { get; set; }
    public string Title { get; set; }
    public int Order { get; set; } = 0;
    public List<CategoryDefinition> Categories { get; set; } = new();
}

public sealed class CategoryDefinition
{
    public string Name { get; set; }
    public int Order { get; set; } = 0;
    public List<CheatBuilder> Cheats { get; set; } = new();
}

/// <summary>
/// Generic page that renders categories and cheats defined in data.
/// </summary>
public sealed class DynamicPage : IMenuPage
{
    private readonly PageDefinition _def;

    public DynamicPage(PageDefinition def)
    {
        _def = def ?? throw new ArgumentNullException(nameof(def));
    }

    public string Id => _def.Id;
    public string Title => _def.Title;
    public int Order => _def.Order;

    public void Build(RectTransform content, MenuContext ctx)
    {
        foreach (var cat in _def.Categories.OrderBy(c => c.Order))
        {
            ctx.AddHeader(cat.Name);
            ctx.AddSeparator();

            var columnsPerRow = 3;
            RectTransform[] row = null;
            var colIndex = 0;
            foreach (var cheat in cat.Cheats)
            {
                if (colIndex % columnsPerRow == 0)
                    row = ctx.AddRow(columnsPerRow);

                cheat?.Invoke(ctx, row[colIndex % columnsPerRow]);
                colIndex++;
            }

            // Spacer between categories
            ctx.AddParagraph(string.Empty);
        }
    }

    public void OnOpen(MenuContext ctx) { }
    public void OnClose(MenuContext ctx) { }
}
