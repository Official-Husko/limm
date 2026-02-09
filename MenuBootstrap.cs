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
    private RectTransform _card;
    private Image _backdrop;

    private readonly Dictionary<string, RectTransform> _pageRoots = new();
    private readonly List<Button> _tabButtons = new();
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
        var cardObj = new GameObject("Card", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        cardObj.transform.SetParent(_canvasRoot.transform, false);
        _card = cardObj.GetComponent<RectTransform>();
        _card.sizeDelta = new Vector2(900, 620);
        _card.anchorMin = new Vector2(0.5f, 0.5f);
        _card.anchorMax = new Vector2(0.5f, 0.5f);
        _card.pivot = new Vector2(0.5f, 0.5f);
        _card.anchoredPosition = Vector2.zero;

        var cardImage = cardObj.GetComponent<Image>();
        cardImage.color = Style.Panel;
        cardImage.type = Image.Type.Simple;
        cardImage.sprite = Style.SolidSprite;

        var vlg = cardObj.GetComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(Style.Padding, Style.Padding, Style.Padding, Style.Padding);
        vlg.spacing = Style.Spacing;
        vlg.childControlHeight = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth = true;
        vlg.childForceExpandWidth = true;

        var csf = cardObj.GetComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

        BuildHeader(cardObj.transform);
        BuildTabs(cardObj.transform);
        BuildContent(cardObj.transform);
    }

    private void BuildHeader(Transform parent)
    {
        var header = new GameObject("Header", typeof(RectTransform), typeof(Text));
        header.transform.SetParent(parent, false);
        var text = header.GetComponent<Text>();
        text.font = Style.DefaultFont;
        text.fontSize = Style.HeaderSize;
        text.fontStyle = FontStyle.Bold;
        text.color = Style.Text;
        text.text = $"{MyPluginInfo.PLUGIN_NAME} v{MyPluginInfo.PLUGIN_VERSION}";
        text.alignment = TextAnchor.MiddleLeft;
        var rt = header.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0, 32);
    }

    private void BuildTabs(Transform parent)
    {
        var tabs = new GameObject("Tabs", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        tabs.transform.SetParent(parent, false);
        var hlg = tabs.GetComponent<HorizontalLayoutGroup>();
        hlg.spacing = Style.Spacing;
        hlg.childForceExpandWidth = false;
        hlg.childControlWidth = true;

        foreach (var page in ModMenu.Pages)
        {
            var btnObj = new GameObject($"Tab_{page.Id}", typeof(RectTransform), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(tabs.transform, false);
            var img = btnObj.GetComponent<Image>();
            img.sprite = Style.SolidSprite;
            img.type = Image.Type.Simple;
            img.color = Style.Panel;
            var btn = btnObj.GetComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = Style.Panel;
            colors.highlightedColor = Style.Accent;
            colors.selectedColor = Style.Accent;
            colors.pressedColor = Style.Accent;
            btn.colors = colors;
            _tabButtons.Add(btn);

            var label = new GameObject("Label", typeof(RectTransform), typeof(Text));
            label.transform.SetParent(btnObj.transform, false);
            var txt = label.GetComponent<Text>();
            txt.font = Style.DefaultFont;
            txt.fontSize = Style.FontSize;
            txt.color = Style.Text;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.text = page.Title;
            var lrt = label.GetComponent<RectTransform>();
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.offsetMin = lrt.offsetMax = Vector2.zero;

            var brt = btnObj.GetComponent<RectTransform>();
            brt.sizeDelta = new Vector2(120, 36);

            btn.onClick.AddListener(() => SwitchPage(page, btn));
        }
    }

    private void BuildContent(Transform parent)
    {
        var scrollObj = new GameObject("Content", typeof(RectTransform), typeof(ScrollRect));
        scrollObj.transform.SetParent(parent, false);
        var scroll = scrollObj.GetComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;

        var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Mask), typeof(Image));
        viewport.transform.SetParent(scrollObj.transform, false);
        var vpImage = viewport.GetComponent<Image>();
            vpImage.sprite = Style.SolidSprite;
            vpImage.type = Image.Type.Simple;
            vpImage.color = new Color32(0, 0, 0, 0);
        var mask = viewport.GetComponent<Mask>();
        mask.showMaskGraphic = false;

        var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        content.transform.SetParent(viewport.transform, false);
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

        // Layout sizing
        var srt = scrollObj.GetComponent<RectTransform>();
        srt.sizeDelta = new Vector2(0, 480);

        // Build pages into stacked containers; only one enabled at a time
        foreach (var page in ModMenu.Pages)
        {
            var pageRoot = new GameObject(page.Id, typeof(RectTransform));
            pageRoot.transform.SetParent(content.transform, false);
            var prt = pageRoot.GetComponent<RectTransform>();
            prt.anchorMin = new Vector2(0, 1);
            prt.anchorMax = new Vector2(1, 1);
            prt.pivot = new Vector2(0.5f, 1);

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

        // Activate first page
        if (ModMenu.Pages.Count > 0)
        {
            SwitchPage(ModMenu.Pages[0], _tabButtons[0]);
        }
    }

    private void SwitchPage(IMenuPage page, Button sender)
    {
        if (page == _activePage)
            return;

        foreach (var kv in _pageRoots)
        {
            kv.Value.gameObject.SetActive(kv.Key == page.Id);
        }

        foreach (var btn in _tabButtons)
        {
            var colors = btn.colors;
            colors.normalColor = btn == sender ? Style.Accent : Style.Panel;
            colors.highlightedColor = Style.Accent;
            btn.colors = colors;
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

        if (_card != null)
        {
            _card.localScale = Vector3.Lerp(new Vector3(0.9f, 0.9f, 1f), Vector3.one, _animCurrent);
        }
    }
}
