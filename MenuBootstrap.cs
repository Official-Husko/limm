using System.Collections.Generic;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace limm;

/// <summary>
/// Sets up the canvas, handles input, and drives the menu UI.
/// </summary>
public class MenuBootstrap : MonoBehaviour
{
    public ConfigEntry<KeyboardShortcut> Hotkey;
    public ConfigEntry<bool> PauseWhenOpen;
    public ConfigEntry<bool> UnlockCursorWhenOpen;
    public ConfigEntry<float> MenuOpacity;
    public ConfigEntry<float> AnimationSpeed;
    public ConfigFile Config;

    private MenuUI _ui;
    private bool _isOpen;
    private float _previousTimeScale = 1f;

    private void Start()
    {
        EnsureEventSystem();
        _ui = new MenuUI(gameObject, Config, MenuOpacity, AnimationSpeed);
        _ui.Build();
        _ui.HideInstant();
    }

    private void Update()
    {
        if (Hotkey != null && Hotkey.Value.IsDown())
        {
            Toggle();
        }

        if (_isOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            Toggle(false);
        }

        _ui?.Tick(Time.unscaledDeltaTime);
    }

    private void Toggle(bool? state = null)
    {
        var target = state ?? !_isOpen;
        if (_isOpen == target || _ui == null)
            return;

        _isOpen = target;

        if (_isOpen)
        {
            _previousTimeScale = Time.timeScale;
            if (PauseWhenOpen != null && PauseWhenOpen.Value)
                Time.timeScale = 0f;

            if (UnlockCursorWhenOpen != null && UnlockCursorWhenOpen.Value)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            _ui.Show();
            ModMenu.RaiseToggled(true);
        }
        else
        {
            if (PauseWhenOpen != null && PauseWhenOpen.Value)
                Time.timeScale = _previousTimeScale;

            if (UnlockCursorWhenOpen != null && UnlockCursorWhenOpen.Value)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            _ui.Hide();
            ModMenu.RaiseToggled(false);
        }
    }

    private static void EnsureEventSystem()
    {
        if (EventSystem.current != null)
            return;

        var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        DontDestroyOnLoad(es);
    }
}

internal sealed class MenuUI
{
    private readonly GameObject _host;
    private readonly ConfigFile _config;
    private readonly ConfigEntry<float> _opacity;
    private readonly ConfigEntry<float> _animSpeed;

    private GameObject _canvasRoot;
    private CanvasGroup _canvasGroup;
    private Image _backdrop;

    private readonly Dictionary<string, RectTransform> _pageRoots = new();
    private readonly List<TabEntry> _tabs = new();
    private IMenuPage _activePage;
    private MenuContext _context;

    private float _animTarget;
    private float _animCurrent;

    public MenuUI(GameObject host, ConfigFile config, ConfigEntry<float> opacity, ConfigEntry<float> animSpeed)
    {
        _host = host;
        _config = config;
        _opacity = opacity;
        _animSpeed = animSpeed;
    }

    public void Build()
    {
        _canvasRoot = new GameObject("ModMenuCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup));
        _canvasRoot.transform.SetParent(_host.transform, false);

        var canvas = _canvasRoot.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 5000;

        var scaler = _canvasRoot.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        _canvasGroup = _canvasRoot.GetComponent<CanvasGroup>();

        // Backdrop
        var backdropObj = new GameObject("Backdrop", typeof(RectTransform), typeof(Image));
        backdropObj.transform.SetParent(_canvasRoot.transform, false);
        var backdropRT = backdropObj.GetComponent<RectTransform>();
        backdropRT.anchorMin = Vector2.zero;
        backdropRT.anchorMax = Vector2.one;
        backdropRT.offsetMin = backdropRT.offsetMax = Vector2.zero;
        _backdrop = backdropObj.GetComponent<Image>();
        _backdrop.sprite = Style.SolidSprite;
        _backdrop.type = Image.Type.Simple;
        _backdrop.color = new Color(Style.Backdrop.r / 255f, Style.Backdrop.g / 255f, Style.Backdrop.b / 255f, (_opacity?.Value ?? 0.9f));

        // Card
        var cardObj = new GameObject("Card", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
        cardObj.transform.SetParent(_canvasRoot.transform, false);
        var card = cardObj.GetComponent<RectTransform>();
        card.anchorMin = Vector2.zero;
        card.anchorMax = Vector2.one;
        card.offsetMin = Vector2.zero;
        card.offsetMax = Vector2.zero;
        card.pivot = new Vector2(0.5f, 0.5f);

        var cardImage = cardObj.GetComponent<Image>();
        cardImage.color = Style.Panel;
        cardImage.type = Image.Type.Simple;
        cardImage.sprite = Style.SolidSprite;

        var layout = cardObj.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(Style.Padding, Style.Padding, Style.Padding, Style.Padding);
        layout.spacing = Style.Spacing;
        layout.childControlWidth = true;
        layout.childForceExpandWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandHeight = false;

        // Header (icon + title)
        var header = new GameObject("Header", typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup));
        header.transform.SetParent(cardObj.transform, false);
        var headerBg = header.GetComponent<Image>();
        headerBg.sprite = Style.SolidSprite;
        headerBg.type = Image.Type.Simple;
        headerBg.color = new Color32(22, 22, 30, 255);

        var headerHLG = header.GetComponent<HorizontalLayoutGroup>();
        headerHLG.spacing = 0;
        headerHLG.padding = new RectOffset(12, 12, 8, 8);
        headerHLG.childAlignment = TextAnchor.MiddleCenter;
        headerHLG.childForceExpandHeight = false;
        headerHLG.childControlHeight = true;
        headerHLG.childForceExpandWidth = true;
        headerHLG.childControlWidth = true;

        var headerLE = header.AddComponent<LayoutElement>();
        headerLE.minHeight = 44;
        headerLE.preferredHeight = 48;
        headerLE.flexibleHeight = 0;
        headerLE.flexibleWidth = 1;

        var title = new GameObject("Title", typeof(RectTransform), typeof(Text));
        title.transform.SetParent(header.transform, false);
        var titleText = title.GetComponent<Text>();
        titleText.font = Style.DefaultFont;
        titleText.fontSize = 28;
        titleText.fontStyle = FontStyle.Bold;
        titleText.color = Style.Text;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.horizontalOverflow = HorizontalWrapMode.Overflow;
        titleText.text = MyPluginInfo.PLUGIN_NAME;
        var titleLE = title.AddComponent<LayoutElement>();
        titleLE.flexibleWidth = 1;
        titleLE.minWidth = 200;

        // Body area
        var body = new GameObject("Body", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        body.transform.SetParent(cardObj.transform, false);
        var bodyHLG = body.GetComponent<HorizontalLayoutGroup>();
        bodyHLG.spacing = 4;
        bodyHLG.childAlignment = TextAnchor.UpperLeft;
        bodyHLG.childControlHeight = true;
        bodyHLG.childForceExpandHeight = true;
        bodyHLG.childControlWidth = true;
        // Keep nav fixed and let content consume remaining width.
        bodyHLG.childForceExpandWidth = false;
        var bodyLE = body.AddComponent<LayoutElement>();
        bodyLE.flexibleHeight = 1;

        var nav = new GameObject("Nav", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
        nav.transform.SetParent(body.transform, false);
        var navImg = nav.GetComponent<Image>();
        navImg.sprite = Style.SolidSprite;
        navImg.type = Image.Type.Simple;
        navImg.color = new Color32(22, 22, 30, 255);
        var navVLG = nav.GetComponent<VerticalLayoutGroup>();
        navVLG.padding = new RectOffset(6, 6, 6, 6);
        navVLG.spacing = 6;
        navVLG.childForceExpandWidth = true;
        navVLG.childControlWidth = true;
        navVLG.childForceExpandHeight = false;
        navVLG.childControlHeight = true;
        var navLE = nav.AddComponent<LayoutElement>();
        navLE.minWidth = 90;
        navLE.preferredWidth = 90;
        navLE.flexibleWidth = 0;
        navLE.flexibleHeight = 1;

        var contentHolder = new GameObject("ContentHolder", typeof(RectTransform), typeof(LayoutElement));
        contentHolder.transform.SetParent(body.transform, false);
        var contentLE = contentHolder.GetComponent<LayoutElement>();
        contentLE.flexibleWidth = 1;
        contentLE.minWidth = 0;
        contentLE.preferredWidth = 0;

        BuildTabs(nav.transform);
        AutoFitNavWidth(navLE);
        BuildContent(contentHolder.transform);

        // Version footer bottom-right aligned via layout
        var version = new GameObject("Version", typeof(RectTransform), typeof(Text));
        version.transform.SetParent(cardObj.transform, false);
        var verText = version.GetComponent<Text>();
        verText.font = Style.DefaultFont;
        verText.fontSize = 12;
        verText.color = Style.Muted;
        verText.alignment = TextAnchor.LowerRight;
        verText.text = $"v{MyPluginInfo.PLUGIN_VERSION}";
        var verRT = version.GetComponent<RectTransform>();
        verRT.sizeDelta = new Vector2(0, 20);
        var verLE = version.AddComponent<LayoutElement>();
        verLE.minHeight = 20;
        verLE.preferredHeight = 20;
        verLE.flexibleWidth = 1;
    }

    private void BuildTabs(Transform parent)
    {
        foreach (var page in ModMenu.Pages)
        {
            var btnObj = new GameObject($"Tab_{page.Id}", typeof(RectTransform), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(parent, false);
            var img = btnObj.GetComponent<Image>();
            img.sprite = Style.SolidSprite;
            img.type = Image.Type.Simple;
            img.color = Style.Panel;

            var btn = btnObj.GetComponent<Button>();
            btn.transition = Selectable.Transition.ColorTint;
            var colors = btn.colors;
            colors.normalColor = Style.Panel;
            colors.highlightedColor = new Color32((byte)Mathf.Min(255, Style.Accent.r + 20), (byte)Mathf.Min(255, Style.Accent.g + 20), (byte)Mathf.Min(255, Style.Accent.b + 20), 255);
            colors.pressedColor = Style.Accent;
            colors.selectedColor = Style.Accent;
            colors.colorMultiplier = 1f;
            btn.colors = colors;

            // Layout size
            var brt = btnObj.GetComponent<RectTransform>();
            brt.sizeDelta = new Vector2(0, 36);

            // Icon
            var iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconObj.transform.SetParent(btnObj.transform, false);
            var iconImg = iconObj.GetComponent<Image>();
            iconImg.sprite = Style.SolidSprite;
            iconImg.color = Style.Muted;
            var iconRT = iconObj.GetComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0, 0.5f);
            iconRT.anchorMax = new Vector2(0, 0.5f);
            iconRT.pivot = new Vector2(0.5f, 0.5f);
            iconRT.anchoredPosition = new Vector2(14, 0);
            iconRT.sizeDelta = new Vector2(18, 18);

            // Icon letter
            var iconLabel = new GameObject("IconLabel", typeof(RectTransform), typeof(Text));
            iconLabel.transform.SetParent(iconObj.transform, false);
            var iconText = iconLabel.GetComponent<Text>();
            iconText.font = Style.DefaultFont;
            iconText.fontSize = 14;
            iconText.alignment = TextAnchor.MiddleCenter;
            iconText.color = Style.Panel;
            iconText.text = page.Title.Length > 0 ? page.Title[0].ToString().ToUpperInvariant() : "?";
            var iconLabelRT = iconLabel.GetComponent<RectTransform>();
            iconLabelRT.anchorMin = Vector2.zero;
            iconLabelRT.anchorMax = Vector2.one;
            iconLabelRT.offsetMin = iconLabelRT.offsetMax = Vector2.zero;

            // Text
            var label = new GameObject("Label", typeof(RectTransform), typeof(Text));
            label.transform.SetParent(btnObj.transform, false);
            var txt = label.GetComponent<Text>();
            txt.font = Style.DefaultFont;
            txt.fontSize = Style.FontSize;
            txt.color = Style.Text;
            txt.alignment = TextAnchor.MiddleLeft;
            txt.horizontalOverflow = HorizontalWrapMode.Overflow;
            txt.verticalOverflow = VerticalWrapMode.Truncate;
            txt.text = page.Title;
            var lrt = label.GetComponent<RectTransform>();
            lrt.anchorMin = new Vector2(0, 0);
            lrt.anchorMax = new Vector2(1, 1);
            lrt.offsetMin = new Vector2(34, 4);
            lrt.offsetMax = new Vector2(-8, -4);

            // Add padding via LayoutElement
            var le = btnObj.AddComponent<LayoutElement>();
            le.minHeight = 34;
            le.preferredHeight = 36;
            le.flexibleWidth = 1;

            var entry = new TabEntry
            {
                Page = page,
                Button = btn,
                Label = txt,
                Icon = iconImg
            };
            _tabs.Add(entry);

            btn.onClick.AddListener(() => SwitchPage(page, entry));
        }
    }

    private void AutoFitNavWidth(LayoutElement navLayout)
    {
        if (navLayout == null || _tabs.Count == 0)
            return;

        Canvas.ForceUpdateCanvases();

        var maxLabelWidth = 0f;
        foreach (var tab in _tabs)
        {
            if (tab.Label != null)
                maxLabelWidth = Mathf.Max(maxLabelWidth, tab.Label.preferredWidth);
        }

        // Button internals: left icon block + label + right breathing room.
        var targetWidth = Mathf.Clamp(Mathf.CeilToInt(34f + maxLabelWidth + 20f), 96, 220);
        navLayout.minWidth = targetWidth;
        navLayout.preferredWidth = targetWidth;
        navLayout.flexibleWidth = 0f;
    }

    private void BuildContent(Transform parent)
    {
        // Frame to give the content area a readable background and padding
        var frame = new GameObject("ContentFrame", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        frame.transform.SetParent(parent, false);
        var frameImg = frame.GetComponent<Image>();
        frameImg.sprite = Style.SolidSprite;
        frameImg.type = Image.Type.Simple;
        frameImg.color = new Color32(16, 16, 22, 200);
        var frameRT = frame.GetComponent<RectTransform>();
        frameRT.anchorMin = Vector2.zero;
        frameRT.anchorMax = Vector2.one;
        frameRT.offsetMin = Vector2.zero;
        frameRT.offsetMax = Vector2.zero;
        var frameLE = frame.GetComponent<LayoutElement>();
        frameLE.minWidth = 0;
        frameLE.preferredWidth = 0;
        frameLE.flexibleWidth = 1;
        frameLE.flexibleHeight = 1;

        // Scroll rect inside the frame with inner padding
        var scrollObj = new GameObject("Content", typeof(RectTransform), typeof(ScrollRect));
        scrollObj.transform.SetParent(frame.transform, false);
        var srt = scrollObj.GetComponent<RectTransform>();
        srt.anchorMin = new Vector2(0, 0);
        srt.anchorMax = new Vector2(1, 1);
        srt.offsetMin = new Vector2(8, 8);
        srt.offsetMax = new Vector2(-8, -8);

        var scroll = scrollObj.GetComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;

        var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Mask), typeof(Image));
        viewport.transform.SetParent(scrollObj.transform, false);
        var vpImage = viewport.GetComponent<Image>();
        vpImage.sprite = Style.SolidSprite;
        vpImage.type = Image.Type.Simple;
        vpImage.color = new Color32(0, 0, 0, 30);
        var mask = viewport.GetComponent<Mask>();
        mask.showMaskGraphic = false;
        var vpRT = viewport.GetComponent<RectTransform>();
        vpRT.anchorMin = Vector2.zero;
        vpRT.anchorMax = Vector2.one;
        vpRT.offsetMin = vpRT.offsetMax = Vector2.zero;

        var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        content.transform.SetParent(viewport.transform, false);
        var contentRT = content.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0f, 1f);
        contentRT.anchorMax = new Vector2(1f, 1f);
        contentRT.pivot = new Vector2(0.5f, 1f);
        contentRT.offsetMin = Vector2.zero;
        contentRT.offsetMax = Vector2.zero;
        var vlg = content.GetComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(Style.Padding, Style.Padding, Style.Padding, Style.Padding);
        vlg.spacing = Style.Spacing;
        vlg.childControlWidth = true;
        vlg.childForceExpandWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandHeight = false;
        var csf = content.GetComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scroll.content = content.GetComponent<RectTransform>();
        scroll.viewport = viewport.GetComponent<RectTransform>();

        foreach (var page in ModMenu.Pages)
        {
            var pageRoot = new GameObject(page.Id, typeof(RectTransform));
            pageRoot.transform.SetParent(content.transform, false);
            var prt = pageRoot.GetComponent<RectTransform>();
            prt.anchorMin = new Vector2(0, 1);
            prt.anchorMax = new Vector2(1, 1);
            prt.pivot = new Vector2(0.5f, 1);
            prt.offsetMin = Vector2.zero;
            prt.offsetMax = Vector2.zero;

            var pageLayoutElement = pageRoot.AddComponent<LayoutElement>();
            pageLayoutElement.minWidth = 0;
            pageLayoutElement.preferredWidth = 0;
            pageLayoutElement.flexibleWidth = 1;

            var layout = pageRoot.AddComponent<VerticalLayoutGroup>();
            layout.spacing = Style.Spacing;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;

            var fitter = pageRoot.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var ctx = new MenuContext(prt, _config);
            page.Build(prt, ctx);
            _pageRoots[page.Id] = prt;
        }

        if (ModMenu.Pages.Count > 0 && _tabs.Count > 0)
        {
            SwitchPage(ModMenu.Pages[0], _tabs[0]);
        }
    }

    private void SwitchPage(IMenuPage page, TabEntry entry)
    {
        if (page == _activePage)
            return;

        foreach (var kv in _pageRoots)
        {
            kv.Value.gameObject.SetActive(kv.Key == page.Id);
        }

        foreach (var tab in _tabs)
        {
            var selected = tab.Page == page;
            var img = tab.Button.targetGraphic as Image;
            if (img != null)
                img.color = selected ? new Color32(Style.Accent.r, Style.Accent.g, Style.Accent.b, 180) : Style.Panel;

            tab.Label.color = selected ? Style.Accent : Style.Text;
            tab.Icon.color = selected ? Style.Accent : Style.Muted;
        }

        _activePage?.OnClose(_context);
        _context = new MenuContext(_pageRoots[page.Id], _config);
        _activePage = page;
        _activePage.OnOpen(_context);
    }

    public void Show()
    {
        _animTarget = 1f;
        if (_canvasRoot != null && !_canvasRoot.activeSelf)
            _canvasRoot.SetActive(true);
    }

    public void Hide() => _animTarget = 0f;

    public void HideInstant()
    {
        _animCurrent = 0f;
        _animTarget = 0f;
        ApplyAnimation();
        if (_canvasRoot != null)
            _canvasRoot.SetActive(false);
    }

    public void Tick(float dt)
    {
        if (_canvasRoot == null)
            return;

        if (Mathf.Approximately(_animCurrent, _animTarget))
        {
            if (_animTarget <= 0f && _canvasRoot.activeSelf)
                _canvasRoot.SetActive(false);
            return;
        }

        var speed = _animSpeed?.Value ?? 10f;
        _animCurrent = Mathf.MoveTowards(_animCurrent, _animTarget, dt * speed);
        ApplyAnimation();
    }

    private void ApplyAnimation()
    {
        if (_canvasGroup != null)
            _canvasGroup.alpha = _animCurrent;

        // Subtle scale on all children
        if (_canvasRoot != null)
            _canvasRoot.transform.localScale = Vector3.Lerp(new Vector3(0.99f, 0.99f, 1f), Vector3.one, _animCurrent);
    }

    private class TabEntry
    {
        public IMenuPage Page;
        public Button Button;
        public Text Label;
        public Image Icon;
    }
}
