using System;
using System.Collections.Generic;
using System.Reflection;
using MonoMod.RuntimeDetour;

namespace LensIslandModMenu.MonoMod
{
    internal static class Detour_WateringCan_Equip
    {
        private static Hook _hook;
        private delegate void EquipOrig(WateringCan self, int slot, int amount);

        // Remember original charges per instance (explicit type for C# 7.3)
        private static readonly Dictionary<WateringCan, int> _origCharges =
            new Dictionary<WateringCan, int>();

        // Use your existing global toggle
        internal static bool InfiniteWaterEnabled = true;

        public static void Apply()
        {
            Type tWC = typeof(WateringCan);
            MethodInfo target = tWC.GetMethod(
                "Equip",
                BindingFlags.Instance | BindingFlags.Public,
                null,
                new Type[] { typeof(int), typeof(int) },
                null);

            if (target == null)
                throw new MissingMethodException(tWC.FullName + ".Equip(int,int) not found");

            _hook = new Hook(target, new Action<EquipOrig, WateringCan, int, int>(Detour));
            ModMenuPlugin.Log?.LogInfo("[Detour] Hooked " + target.DeclaringType.FullName + "." + target.Name + "(int,int)");
        }

        public static void Remove()
        {
            if (_hook != null) _hook.Dispose();
            _hook = null;
        }

        private static void Detour(EquipOrig orig, WateringCan self, int slot, int amount)
        {
            // Cache original once per instance
            if (!_origCharges.ContainsKey(self))
                _origCharges[self] = self.chargesOnUse;

            if (!InfiniteWaterEnabled)
            {
                self.chargesOnUse = _origCharges[self];
                orig(self, slot, amount);
                return;
            }

            self.chargesOnUse = 0;       // zero cost
            orig(self, slot, amount);    // run vanilla equip
            self.FillCan();              // play the refill SFX + set water + save
        }
    }
}
