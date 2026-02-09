using BepInEx.Logging;
using HarmonyLib;
using LensIslandModMenu;
using MonoMod.RuntimeDetour;
using System.Reflection;

internal static class Detour_IsDlcUnlocked
{
    private static Hook _dlcEnumHook;
    private static Hook _dlcIfaceHook;

    private delegate bool IsDlcUnlockedEnum_Orig(DLC_Manager.DLC dlc);
    private delegate bool IsDlcUnlockedIface_Orig(object dlcInterface /* DLC_Interface */);

    public static void Apply(ManualLogSource log)
    {
        // 1) bool DLC_Manager.IsDlcUnlocked(DLC_Manager.DLC)
        var t = typeof(DLC_Manager);
        var enumType = t.GetNestedType("DLC", BindingFlags.Public | BindingFlags.NonPublic);
        var mEnum = t.GetMethod("IsDlcUnlocked",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
            null, new[] { enumType }, null);

        if (mEnum != null)
        {
            _dlcEnumHook = new Hook(mEnum,
                typeof(Detour_IsDlcUnlocked).GetMethod(nameof(IsDlcUnlockedEnum_Repl), BindingFlags.NonPublic | BindingFlags.Static));
            log.LogInfo($"[Detour] Hooked {mEnum.DeclaringType.Assembly.GetName().Name}:{mEnum.DeclaringType.FullName}.{mEnum}");
        }
        else
        {
            log.LogWarning("[Detour] Couldn’t find IsDlcUnlocked(DLC)");
        }

        // 2) bool DLC_Manager.IsDlcUnlocked(DLC_Interface)
        var dlcIface = AccessTools.TypeByName("DLC_Interface");
        if (dlcIface != null)
        {
            var mIface = t.GetMethod("IsDlcUnlocked",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
                null, new[] { dlcIface }, null);

            if (mIface != null)
            {
                _dlcIfaceHook = new Hook(mIface,
                    typeof(Detour_IsDlcUnlocked).GetMethod(nameof(IsDlcUnlockedIface_Repl), BindingFlags.NonPublic | BindingFlags.Static));
                log.LogInfo($"[Detour] Hooked {mIface.DeclaringType.Assembly.GetName().Name}:{mIface.DeclaringType.FullName}.{mIface}");
            }
            else
            {
                log.LogWarning("[Detour] Couldn’t find IsDlcUnlocked(DLC_Interface)");
            }
        }
    }

    private static bool IsDlcUnlockedEnum_Repl(IsDlcUnlockedEnum_Orig orig, DLC_Manager.DLC dlc)
    {
        ModMenuPlugin.Log?.LogInfo($"[Detour] IsDlcUnlocked(DLC:{dlc}) -> TRUE");
        return true;
    }

    private static bool IsDlcUnlockedIface_Repl(IsDlcUnlockedIface_Orig orig, object dlcInterface)
    {
        ModMenuPlugin.Log?.LogInfo($"[Detour] IsDlcUnlocked(DLC_Interface:{dlcInterface}) -> TRUE");
        return true;
    }
}
