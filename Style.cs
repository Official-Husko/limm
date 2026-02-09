using UnityEngine;

namespace limm;

/// <summary>
/// Palette and sizing tokens for the menu.
/// </summary>
public static class Style
{
    public static readonly Color32 Backdrop = new Color32(12, 12, 18, 200);
    public static readonly Color32 Panel = new Color32(28, 28, 38, 240);
    public static readonly Color32 Accent = new Color32(88, 130, 255, 255);
    public static readonly Color32 Text = new Color32(230, 234, 242, 255);
    public static readonly Color32 Muted = new Color32(170, 176, 189, 255);
    public const int CornerRadius = 10;
    public const int Padding = 14;
    public const int Spacing = 10;
    public const int FontSize = 16;
    public const int HeaderSize = 20;

    public static Font DefaultFont => Resources.GetBuiltinResource<Font>("Arial.ttf");

    private static Sprite _solid;
    public static Sprite SolidSprite => _solid ??= Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
}
