using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.UI;

namespace limm;

/// <summary>
/// Supplies UI factories and config helpers to pages.
/// </summary>
public sealed class MenuContext
{
    private readonly RectTransform _contentRoot;
    private readonly ConfigFile _config;
    public Color32 AccentColor => Style.Accent;
    public Color32 TextColor => Style.Text;
    public Color32 MutedColor => Style.Muted;
    public Font DefaultFont => Style.DefaultFont;

    public MenuContext(RectTransform contentRoot, ConfigFile config)
    {
        _contentRoot = contentRoot;
        _config = config;
    }

    public ConfigEntry<T> Bind<T>(string section, string key, T defaultValue, string desc = "")
    {
        return _config.Bind(section, key, defaultValue, desc);
    }

    public void AddHeader(string text)
    {
        var go = CreateText(text, Style.HeaderSize, FontStyle.Bold, Style.Text);
        go.transform.SetParent(_contentRoot, false);
        var layout = go.AddComponent<LayoutElement>();
        layout.minHeight = 26;
    }

    public void AddSeparator()
    {
        var line = new GameObject("Separator", typeof(RectTransform), typeof(Image));
        line.transform.SetParent(_contentRoot, false);
        var img = line.GetComponent<Image>();
        img.sprite = Style.SolidSprite;
        img.color = new Color32(60, 60, 70, 180);
        var rt = line.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0.5f);
        rt.anchorMax = new Vector2(1, 0.5f);
        rt.sizeDelta = new Vector2(0, 2);
        var le = line.AddComponent<LayoutElement>();
        le.minHeight = 2;
        le.preferredHeight = 2;
    }

    /// <summary>
    /// Creates a horizontal row with evenly split columns.
    /// </summary>
    public RectTransform[] AddRow(int columns = 3)
    {
        var row = new GameObject("RowGroup", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        row.transform.SetParent(_contentRoot, false);
        var hlg = row.GetComponent<HorizontalLayoutGroup>();
        hlg.spacing = Style.Spacing;
        hlg.childAlignment = TextAnchor.UpperLeft;
        hlg.childControlWidth = true;
        hlg.childForceExpandWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandHeight = false;
        var le = row.AddComponent<LayoutElement>();
        le.preferredHeight = 80;
        le.flexibleWidth = 1;

        var cols = new RectTransform[columns];
        for (int i = 0; i < columns; i++)
        {
            var col = new GameObject($"Col{i + 1}", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            col.transform.SetParent(row.transform, false);
            var v = col.GetComponent<VerticalLayoutGroup>();
            v.spacing = 4;
            v.childControlWidth = true;
            v.childForceExpandWidth = true;
            v.childControlHeight = true;
            v.childForceExpandHeight = false;
            var cle = col.GetComponent<LayoutElement>();
            cle.flexibleWidth = 1;
            cle.minHeight = 70;
            cols[i] = col.GetComponent<RectTransform>();
        }

        return cols;
    }

    public Toggle AddToggleCard(string label, ConfigEntry<bool> binding, RectTransform parent, Action<bool> onChanged = null)
    {
        var block = new GameObject(label + "_Card", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
        block.transform.SetParent(parent, false);
        var v = block.GetComponent<VerticalLayoutGroup>();
        v.spacing = 4;
        v.padding = new RectOffset(4, 4, 4, 4);
        v.childControlWidth = true;
        v.childForceExpandWidth = true;
        v.childControlHeight = true;
        v.childForceExpandHeight = false;

        var cardLe = block.GetComponent<LayoutElement>();
        cardLe.minHeight = 48;
        cardLe.preferredHeight = 54;

        var row = new GameObject("CardRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        row.transform.SetParent(block.transform, false);
        var rowLayout = row.GetComponent<HorizontalLayoutGroup>();
        rowLayout.spacing = 8;
        rowLayout.childControlWidth = true;
        rowLayout.childForceExpandWidth = false;
        rowLayout.childControlHeight = true;
        rowLayout.childForceExpandHeight = false;
        rowLayout.childAlignment = TextAnchor.MiddleLeft;

        var labelGo = CreateText(label, Style.FontSize, FontStyle.Normal, Style.Text);
        labelGo.transform.SetParent(row.transform, false);
        var labelLe = labelGo.AddComponent<LayoutElement>();
        labelLe.flexibleWidth = 1;
        labelLe.minWidth = 0;

        return CreateToggleControl(row.transform, binding, onChanged);
    }

    public Slider AddSliderCard(string label, ConfigEntry<float> binding, float min, float max, float step, RectTransform parent, Action<float> onChanged = null)
    {
        var block = new GameObject(label + "_Card", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
        block.transform.SetParent(parent, false);
        var v = block.GetComponent<VerticalLayoutGroup>();
        v.spacing = 4;
        v.padding = new RectOffset(4, 4, 4, 4);
        v.childControlWidth = true;
        v.childForceExpandWidth = true;
        v.childControlHeight = true;
        v.childForceExpandHeight = false;
        var cardLe = block.GetComponent<LayoutElement>();
        cardLe.minHeight = 48;
        cardLe.preferredHeight = 54;

        var topRow = new GameObject("TopRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        topRow.transform.SetParent(block.transform, false);
        var topLayout = topRow.GetComponent<HorizontalLayoutGroup>();
        topLayout.spacing = 8;
        topLayout.childControlWidth = true;
        topLayout.childForceExpandWidth = false;
        topLayout.childControlHeight = true;
        topLayout.childForceExpandHeight = false;
        topLayout.childAlignment = TextAnchor.MiddleLeft;

        var labelGo = CreateText(label, Style.FontSize, FontStyle.Normal, Style.Text);
        labelGo.transform.SetParent(topRow.transform, false);
        var labelLe = labelGo.AddComponent<LayoutElement>();
        labelLe.flexibleWidth = 1;
        labelLe.minWidth = 0;

        var valueGo = CreateText("0.00", Style.FontSize, FontStyle.Normal, Style.Muted);
        valueGo.transform.SetParent(topRow.transform, false);
        var valueText = valueGo.GetComponent<Text>();
        valueText.alignment = TextAnchor.MiddleRight;
        var valueLe = valueGo.AddComponent<LayoutElement>();
        valueLe.preferredWidth = 56;
        valueLe.flexibleWidth = 0;

        var controlArea = new GameObject("Control", typeof(RectTransform), typeof(LayoutElement));
        controlArea.transform.SetParent(block.transform, false);
        var controlLe = controlArea.GetComponent<LayoutElement>();
        controlLe.flexibleWidth = 1;
        controlLe.minWidth = 0;

        return CreateSliderControl(controlArea.GetComponent<RectTransform>(), binding, min, max, step, valueText, onChanged);
    }

    public void AddParagraph(string text)
    {
        var go = CreateText(text, Style.FontSize, FontStyle.Normal, Style.Muted);
        go.transform.SetParent(_contentRoot, false);
        var le = go.AddComponent<LayoutElement>();
        le.minHeight = 24;
    }

    public Button AddButton(string label, Action onClick)
    {
        var row = CreateRow(label, out var _, out var controlArea);
        var button = CreateButton("Button", controlArea, onClick);
        SetButtonLabel(button, "GO");
        return button;
    }

    public Toggle AddToggle(string label, ConfigEntry<bool> binding, Action<bool> onChanged = null)
        => AddToggle(label, binding, onChanged, _contentRoot);

    private Toggle AddToggle(string label, ConfigEntry<bool> binding, Action<bool> onChanged, Transform parent)
    {
        var row = CreateRow(label, parent, out var _, out var controlArea);
        return CreateToggleControl(controlArea, binding, onChanged);
    }

    public Slider AddSlider(string label, ConfigEntry<float> binding, float min, float max, float step = 0f, Action<float> onChanged = null)
        => AddSliderInternal(binding, min, max, step, onChanged, null, null);

    private Slider AddSliderInternal(ConfigEntry<float> binding, float min, float max, float step, Action<float> onChanged, Transform parentOverride, Text valueTextOverride)
    {
        var row = CreateRow(valueTextOverride != null ? string.Empty : null, parentOverride ?? _contentRoot, out var valueLabelObj, out var controlArea);
        var valueText = valueTextOverride ?? valueLabelObj.GetComponent<Text>();
        return CreateSliderControl(controlArea, binding, min, max, step, valueText, onChanged);
    }

    private Slider CreateSliderControl(RectTransform controlArea, ConfigEntry<float> binding, float min, float max, float step, Text valueText, Action<float> onChanged)
    {
        var sliderObj = new GameObject("Slider", typeof(RectTransform), typeof(Slider));
        sliderObj.transform.SetParent(controlArea, false);
        var slider = sliderObj.GetComponent<Slider>();
        slider.minValue = min;
        slider.maxValue = max;
        slider.value = Mathf.Clamp(binding.Value, min, max);
        var sliderRt = sliderObj.GetComponent<RectTransform>();
        sliderRt.sizeDelta = new Vector2(220, 22);
        var sliderLe = sliderObj.AddComponent<LayoutElement>();
        sliderLe.preferredHeight = 22;
        sliderLe.minHeight = 20;

        var background = new GameObject("Background", typeof(RectTransform), typeof(Image));
        background.transform.SetParent(sliderObj.transform, false);
        var bgImg = background.GetComponent<Image>();
        bgImg.sprite = Style.SolidSprite;
        bgImg.type = Image.Type.Simple;
        bgImg.color = Style.Panel;
        var bgRt = background.GetComponent<RectTransform>();
        bgRt.anchorMin = new Vector2(0, 0.35f);
        bgRt.anchorMax = new Vector2(1, 0.65f);
        bgRt.offsetMin = bgRt.offsetMax = Vector2.zero;

        var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fill.transform.SetParent(background.transform, false);
        var fillImg = fill.GetComponent<Image>();
        fillImg.sprite = Style.SolidSprite;
        fillImg.type = Image.Type.Simple;
        fillImg.color = Style.Accent;
        var fillRt = fill.GetComponent<RectTransform>();
        fillRt.anchorMin = new Vector2(0, 0);
        fillRt.anchorMax = new Vector2(1, 1);
        fillRt.offsetMin = new Vector2(1, 1);
        fillRt.offsetMax = new Vector2(-1, -1);

        var handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
        handle.transform.SetParent(background.transform, false);
        var handleImg = handle.GetComponent<Image>();
        handleImg.sprite = Style.SolidSprite;
        handleImg.color = Style.Text;
        var handleRt = handle.GetComponent<RectTransform>();
        handleRt.sizeDelta = new Vector2(12, 16);
        handleRt.anchorMin = new Vector2(0, 0.5f);
        handleRt.anchorMax = new Vector2(0, 0.5f);
        handleRt.pivot = new Vector2(0.5f, 0.5f);

        slider.fillRect = fillRt;
        slider.handleRect = handleRt;
        slider.targetGraphic = handleImg;
        slider.direction = Slider.Direction.LeftToRight;

        slider.onValueChanged.AddListener(v =>
        {
            var snapped = step > 0f ? Mathf.Round(v / step) * step : v;
            binding.Value = Mathf.Clamp(snapped, min, max);
            if (valueText != null)
                valueText.text = binding.Value.ToString("0.00");
            onChanged?.Invoke(binding.Value);
        });

        if (valueText != null)
            valueText.text = binding.Value.ToString("0.00");
        return slider;
    }

    private Toggle CreateToggleControl(Transform parent, ConfigEntry<bool> binding, Action<bool> onChanged)
    {
        var toggleObj = new GameObject("Toggle", typeof(RectTransform), typeof(Toggle), typeof(Image));
        toggleObj.transform.SetParent(parent, false);

        var bg = toggleObj.GetComponent<Image>();
        bg.sprite = Style.SolidSprite;
        bg.type = Image.Type.Simple;
        bg.color = Style.Panel;

        var toggle = toggleObj.GetComponent<Toggle>();

        var check = new GameObject("Checkmark", typeof(RectTransform), typeof(Image));
        check.transform.SetParent(toggleObj.transform, false);
        var checkImg = check.GetComponent<Image>();
        checkImg.sprite = Style.SolidSprite;
        checkImg.color = Style.Accent;

        var rt = toggleObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(24, 24);
        var toggleLe = toggleObj.AddComponent<LayoutElement>();
        toggleLe.preferredWidth = 24;
        toggleLe.preferredHeight = 24;
        toggleLe.flexibleWidth = 0;

        var crt = check.GetComponent<RectTransform>();
        crt.anchorMin = new Vector2(0.2f, 0.2f);
        crt.anchorMax = new Vector2(0.8f, 0.8f);
        crt.offsetMin = crt.offsetMax = Vector2.zero;

        toggle.graphic = checkImg;
        toggle.targetGraphic = bg;
        toggle.isOn = binding.Value;
        toggle.onValueChanged.AddListener(v =>
        {
            binding.Value = v;
            onChanged?.Invoke(v);
        });

        return toggle;
    }

    public Dropdown AddDropdown(string label, ConfigEntry<string> binding, List<string> options, Action<string> onChanged = null)
    {
        var row = CreateRow(label, out var _, out var controlArea);

        var dropdownObj = new GameObject("Dropdown", typeof(RectTransform), typeof(Image), typeof(Dropdown));
        dropdownObj.transform.SetParent(controlArea, false);
        var image = dropdownObj.GetComponent<Image>();
        image.sprite = Style.SolidSprite;
        image.type = Image.Type.Simple;
        image.color = Style.Panel;

        var dropdown = dropdownObj.GetComponent<Dropdown>();
        dropdown.targetGraphic = image;

        // Caption label
        var caption = CreateText(binding.Value ?? (options.Count > 0 ? options[0] : string.Empty), Style.FontSize, FontStyle.Normal, Style.Text);
        caption.name = "Caption";
        caption.transform.SetParent(dropdownObj.transform, false);
        var capText = caption.GetComponent<Text>();
        var capRt = caption.GetComponent<RectTransform>();
        capRt.anchorMin = new Vector2(0, 0);
        capRt.anchorMax = new Vector2(1, 1);
        capRt.offsetMin = new Vector2(10, 6);
        capRt.offsetMax = new Vector2(-26, -6);

        // Arrow
        var arrow = new GameObject("Arrow", typeof(RectTransform), typeof(Text));
        arrow.transform.SetParent(dropdownObj.transform, false);
        var arrowTxt = arrow.GetComponent<Text>();
        arrowTxt.font = Style.DefaultFont;
        arrowTxt.fontSize = Style.FontSize;
        arrowTxt.color = Style.Muted;
        arrowTxt.alignment = TextAnchor.MiddleCenter;
        arrowTxt.text = "▼";
        var art = arrow.GetComponent<RectTransform>();
        art.anchorMin = new Vector2(1, 0.5f);
        art.anchorMax = new Vector2(1, 0.5f);
        art.anchoredPosition = new Vector2(-12, 0);
        art.sizeDelta = new Vector2(20, 20);

        // Template
        var template = new GameObject("Template", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
        template.transform.SetParent(dropdownObj.transform, false);
        var templateRect = template.GetComponent<RectTransform>();
        templateRect.pivot = new Vector2(0.5f, 1);
        templateRect.anchorMin = new Vector2(0, 0);
        templateRect.anchorMax = new Vector2(1, 0);
        templateRect.anchoredPosition = new Vector2(0, -templateRect.sizeDelta.y);
        templateRect.sizeDelta = new Vector2(0, 150);
        var templateImage = template.GetComponent<Image>();
        templateImage.sprite = Style.SolidSprite;
        templateImage.type = Image.Type.Simple;
        templateImage.color = new Color32(40, 40, 52, 240);
        template.SetActive(false);

        var scroll = template.GetComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;

        var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Mask), typeof(Image));
        viewport.transform.SetParent(template.transform, false);
        var viewportRect = viewport.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = viewportRect.offsetMax = Vector2.zero;
        var viewportImage = viewport.GetComponent<Image>();
        viewportImage.sprite = Style.SolidSprite;
        viewportImage.type = Image.Type.Simple;
        viewportImage.color = new Color32(0, 0, 0, 0);
        var viewportMask = viewport.GetComponent<Mask>();
        viewportMask.showMaskGraphic = false;

        var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        content.transform.SetParent(viewport.transform, false);
        var contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.offsetMin = contentRect.offsetMax = Vector2.zero;
        var vlg = content.GetComponent<VerticalLayoutGroup>();
        vlg.childForceExpandHeight = false;
        vlg.childControlHeight = true;
        vlg.spacing = 2;
        vlg.padding = new RectOffset(6, 6, 6, 6);
        var csf = content.GetComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Item prototype
        var item = new GameObject("Item", typeof(RectTransform), typeof(Toggle));
        item.transform.SetParent(content.transform, false);
        var itemRect = item.GetComponent<RectTransform>();
        itemRect.sizeDelta = new Vector2(0, 28);
        var itemBg = item.AddComponent<Image>();
        itemBg.sprite = Style.SolidSprite;
        itemBg.type = Image.Type.Simple;
        itemBg.color = new Color32(52, 52, 64, 255);

        var itemToggle = item.GetComponent<Toggle>();
        itemToggle.targetGraphic = itemBg;

        var itemCheck = new GameObject("Item Checkmark", typeof(RectTransform), typeof(Image));
        itemCheck.transform.SetParent(item.transform, false);
        var checkImg = itemCheck.GetComponent<Image>();
        checkImg.sprite = Style.SolidSprite;
        checkImg.color = Style.Accent;
        var checkRt = itemCheck.GetComponent<RectTransform>();
        checkRt.anchorMin = new Vector2(0, 0.5f);
        checkRt.anchorMax = new Vector2(0, 0.5f);
        checkRt.anchoredPosition = new Vector2(12, 0);
        checkRt.sizeDelta = new Vector2(16, 16);
        itemToggle.graphic = checkImg;

        var itemLabel = CreateText("Option", Style.FontSize, FontStyle.Normal, Style.Text);
        itemLabel.name = "Item Label";
        itemLabel.transform.SetParent(item.transform, false);
        var itemLabelTxt = itemLabel.GetComponent<Text>();
        var itemLabelRt = itemLabel.GetComponent<RectTransform>();
        itemLabelRt.anchorMin = new Vector2(0, 0);
        itemLabelRt.anchorMax = new Vector2(1, 1);
        itemLabelRt.offsetMin = new Vector2(32, 4);
        itemLabelRt.offsetMax = new Vector2(-6, -4);
        itemLabelTxt.alignment = TextAnchor.MiddleLeft;

        scroll.content = contentRect;
        scroll.viewport = viewportRect;

        dropdown.template = templateRect;
        dropdown.captionText = capText;
        dropdown.itemText = itemLabelTxt;
        dropdown.options.Clear();
        foreach (var opt in options)
        {
            dropdown.options.Add(new Dropdown.OptionData(opt));
        }

        var startIndex = Mathf.Max(0, options.IndexOf(binding.Value ?? (options.Count > 0 ? options[0] : string.Empty)));
        dropdown.value = startIndex;
        dropdown.RefreshShownValue();
        dropdown.onValueChanged.AddListener(i =>
        {
            var chosen = options[Mathf.Clamp(i, 0, options.Count - 1)];
            binding.Value = chosen;
            onChanged?.Invoke(chosen);
        });

        // Layout for dropdown visuals
        var rt = dropdownObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(220, 32);

        return dropdown;
    }

    private GameObject CreateText(string text, int size, FontStyle style, Color color)
    {
        var go = new GameObject("Text", typeof(RectTransform), typeof(Text));
        var txt = go.GetComponent<Text>();
        txt.text = text;
        txt.font = Style.DefaultFont;
        txt.fontSize = size;
        txt.fontStyle = style;
        txt.color = color;
        txt.horizontalOverflow = HorizontalWrapMode.Wrap;
        txt.verticalOverflow = VerticalWrapMode.Truncate;
        return go;
    }

    private GameObject CreateRow(string label, Transform parent, out GameObject valueLabel, out RectTransform controlArea)
    {
        var row = new GameObject("Row", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        row.transform.SetParent(parent, false);
        var layout = row.GetComponent<HorizontalLayoutGroup>();
        layout.spacing = Style.Spacing;
        layout.padding = new RectOffset(0, 0, 4, 4);
        layout.childControlHeight = true;
        layout.childForceExpandHeight = false;
        layout.childControlWidth = false;
        layout.childForceExpandWidth = false;

        GameObject labelGo = null;
        if (!string.IsNullOrEmpty(label))
        {
            labelGo = CreateText(label, Style.FontSize, FontStyle.Normal, Style.Text);
            labelGo.name = "Label";
            labelGo.transform.SetParent(row.transform, false);
            var labelLayout = labelGo.AddComponent<LayoutElement>();
            labelLayout.minWidth = 100;
            labelLayout.flexibleWidth = 0;
        }

        valueLabel = CreateText(string.Empty, Style.FontSize, FontStyle.Normal, Style.Muted);
        valueLabel.name = "Value";
        valueLabel.transform.SetParent(row.transform, false);
        var valueLayout = valueLabel.AddComponent<LayoutElement>();
        valueLayout.preferredWidth = 48;
        valueLayout.flexibleWidth = 0;

        var control = new GameObject("Control", typeof(RectTransform), typeof(LayoutElement));
        control.transform.SetParent(row.transform, false);
        var le = control.GetComponent<LayoutElement>();
        le.flexibleWidth = 1;
        le.minWidth = 120;
        controlArea = control.GetComponent<RectTransform>();

        return row;
    }

    private GameObject CreateRow(string label, out GameObject valueLabel, out RectTransform controlArea) =>
        CreateRow(label, _contentRoot, out valueLabel, out controlArea);

    private Button CreateButton(string name, RectTransform parent, Action onClick)
    {
        var buttonObj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObj.transform.SetParent(parent, false);
        var image = buttonObj.GetComponent<Image>();
        image.sprite = Style.SolidSprite;
        image.type = Image.Type.Simple;
        image.color = Style.Accent;

        var button = buttonObj.GetComponent<Button>();
        button.onClick.AddListener(() => onClick?.Invoke());

        var textObj = CreateText(name, Style.FontSize, FontStyle.Bold, Color.white);
        textObj.name = "Label";
        textObj.transform.SetParent(buttonObj.transform, false);
        var txt = textObj.GetComponent<Text>();
        txt.alignment = TextAnchor.MiddleCenter;
        var rt = textObj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        var brt = buttonObj.GetComponent<RectTransform>();
        brt.sizeDelta = new Vector2(140, 32);

        return button;
    }

    private void SetButtonLabel(Button btn, string text)
    {
        var label = btn.transform.Find("Label");
        if (label != null && label.TryGetComponent<Text>(out var txt))
        {
            txt.text = text;
        }
    }
}
