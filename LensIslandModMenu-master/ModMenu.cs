using BepInEx;
using BepInEx.Logging;
using LensIslandModMenu.Cheats;
using LensIslandModMenu.MonoMod;
using System;
using UnityEngine;

namespace LensIslandModMenu
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class ModMenuPlugin : BaseUnityPlugin
    {
        public const string PluginGuid = "midnight.lensisland.modmenu";
        public const string PluginName = "Lens Island Mod Menu";
        public const string PluginVersion = "0.1.0";
        private static ModMenuPlugin _instance;

        internal static ManualLogSource Log;

        private Rect _menuRect = new Rect(20, 20, 350, 500);
        private bool _menuOpen;
        private Vector2 _resourceScroll;

        private bool _godModeEnabled = false;
        public static bool AlwaysWin { get; private set; } = false;
        public static bool InfiniteWaterEnabled { get; private set; } = false;
        public static bool UnlimitedJumpsEnabled { get; private set; } = false;

        private int _xpAmount = 500; // Default XP amount
        private int _rsAmount = 1; // Default resource amount
        private int _maxXpAmount = 10000; // Max XP Amount
        private int _rsMaxAmount = 250; // Max resource amount

        private int _selectedResourceIndex = -1;
        private ResourceTypes _resourceType = ResourceTypes.GoldCoins; // Default resource type
        private static readonly ResourceTypes[] _resourceValues = (ResourceTypes[])Enum.GetValues(typeof(ResourceTypes));
        private static readonly string[] _resourceNames = Array.ConvertAll(_resourceValues, v => v.ToString());

        private void Awake()
        {
            Log = Logger;
            Log.LogInfo($"{PluginName} v{PluginVersion} loaded. goActive={gameObject.activeInHierarchy}, compEnabled={enabled}");
            _instance = this;

            try
            {
                // Driver to keep Update/OnGUI alive
                var go = new GameObject("Midnight_ModMenu_Driver");
                go.hideFlags = HideFlags.HideAndDontSave;
                DontDestroyOnLoad(go);
                var driver = go.AddComponent<UpdateDriver>();
                driver.OnUpdate += PluginUpdate;
                driver.OnGUIEvent += PluginOnGUI;
                Log.LogInfo("Driver created & hooks attached.");
            }
            catch (Exception ex)
            {
                Log.LogError("Driver init FAILED:\n" + ex);
            }

            try
            {
                Detour_IsDlcUnlocked.Apply(Log);
                Detour_DealInitialCards.Apply(Log);
                Detour_WateringCan_Equip.Apply();
                Detour_LenStateBase_ActionAllowed.Apply();
            }
            catch (Exception ex)
            {
                Log.LogError("Apply Patches FAILED:\n" + ex);
            }
        }

        private void PluginUpdate()
        {

            if (Input.GetKeyDown(KeyCode.F1))
                _menuOpen = !_menuOpen;
        }

        private void PluginOnGUI()
        {
            if (!_menuOpen) return;

            _menuRect = GUILayout.Window(
                GUIUtility.GetControlID(FocusType.Passive),
                _menuRect,
                id =>
                {
                    GUILayout.BeginVertical();

                    // Suicide Button

                    if (GUILayout.Button("Kill Player"))
                    {
                        Log.LogInfo("Kill Player pressed...");
                        PlayerCheats.KillPlayer(Log);
                    }
                    GUILayout.Space(5);

                    // Give XP Amount

                    _xpAmount = (int)GUILayout.HorizontalSlider(_xpAmount, 0, _maxXpAmount);
                    if (GUILayout.Button($"Give {_xpAmount} XP"))
                    {
                        Log.LogInfo($"Give XP pressed... Amount: {_xpAmount}");
                        PlayerCheats.GiveXP(Log, _xpAmount);
                    }

                    // Toggle God Mode

                    if (GUILayout.Button($"God Mode: {(_godModeEnabled ? "ON" : "OFF")}"))
                    {
                        Log.LogInfo("TGM Pressed...");
                        _godModeEnabled = PlayerCheats.ToggleGodMode(Log);
                    }

                    // Dropdown toggle button
                    GUILayout.Label("Select Resource:");
                    _resourceScroll = GUILayout.BeginScrollView(_resourceScroll, GUILayout.Height(180));
                    int newIndex = GUILayout.SelectionGrid(
                        _selectedResourceIndex,
                        _resourceNames,
                        2,                           
                        GUILayout.ExpandWidth(true)
                    );
                    GUILayout.EndScrollView();

                    if (newIndex != _selectedResourceIndex && newIndex >= 0)
                    {
                        _selectedResourceIndex = newIndex;
                        _resourceType = _resourceValues[_selectedResourceIndex];
                    }

                    // Amount slider (0..10k like your XP slider)
                    GUILayout.Label($"Amount: {_rsAmount}");
                    _rsAmount = (int)GUILayout.HorizontalSlider(_rsAmount, 0, _rsMaxAmount);

                    // Spawn button (uses your existing pattern)
                    if (GUILayout.Button($"Spawn {_rsAmount}x {_resourceType}"))
                    {
                        Log.LogInfo("SpawnItem Pressed...");
                        ItemCheats.SpawnItem(Log, _rsAmount, _resourceType);
                    }

                    // Play Blackjack Button
                    if (GUILayout.Button($"Always Win Blackjack: {(AlwaysWin ? "ON" : "OFF")}"))
                    {
                        AlwaysWin = !AlwaysWin;
                        Log.LogInfo($"IsPlayingBlackjack set to {AlwaysWin}");
                    }

                    //Unlimited Water Button
                    if (GUILayout.Button($"Unlimited Water: " + (InfiniteWaterEnabled ? "ON" : "OFF")))
                    {
                        InfiniteWaterEnabled = !InfiniteWaterEnabled;
                        Log.LogInfo($"InfiniteWaterEnabled set to {InfiniteWaterEnabled}");
                    }

                    //Unlimited Jumps
                    if (GUILayout.Button($"Unlimited Jumps: " + (UnlimitedJumpsEnabled ? "ON" : "OFF")))
                    {
                        UnlimitedJumpsEnabled = !UnlimitedJumpsEnabled;
                        Log.LogInfo($"UnlimitedJumpsEnabled set to {UnlimitedJumpsEnabled}");
                    }

                    GUILayout.Space(8);
                    GUILayout.Label("Toggle: F1");
                    GUILayout.EndVertical();
                    GUI.DragWindow(new Rect(0, 0, 10000, 20));
                },
                "Midnight Mod Menu"
            );
        }
    }

    public sealed class UpdateDriver : MonoBehaviour
    {
        public event Action OnUpdate;
        public event Action OnGUIEvent;

        private void Awake()
        {
            hideFlags = HideFlags.HideAndDontSave;
            DontDestroyOnLoad(gameObject);
        }

        private void Update() => OnUpdate?.Invoke();
        private void OnGUI() => OnGUIEvent?.Invoke();
    }
}
