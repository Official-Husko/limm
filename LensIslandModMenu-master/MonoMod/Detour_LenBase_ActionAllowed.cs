using System;
using System.Reflection;
using MonoMod.RuntimeDetour;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LensIslandModMenu.MonoMod
{
    internal static class Detour_LenStateBase_Jump
    {
        private static Hook _hook;

        // Menu toggles
        internal static bool UnlimitedJumpsEnabled = true;
        internal static bool DrainHungerOnJump = false;   // set true if you still want the starvation cost

        // Original: void Jump(InputAction.CallbackContext i)
        private delegate void JumpOrig(LenState_Base self, InputAction.CallbackContext i);

        public static void Apply()
        {
            Type t = typeof(LenState_Base);
            MethodInfo target = t.GetMethod(
                "Jump",
                BindingFlags.Instance | BindingFlags.Public);

            if (target == null)
                throw new MissingMethodException(t.FullName + ".Jump(InputAction.CallbackContext) not found");

            _hook = new Hook(target, new Action<JumpOrig, LenState_Base, InputAction.CallbackContext>(Detour));
            ModMenuPlugin.Log?.LogInfo("[Detour] Hooked " + target.DeclaringType.FullName + "." + target.Name + "(InputAction.CallbackContext)");
        }

        public static void Remove()
        {
            if (_hook != null) _hook.Dispose();
            _hook = null;
        }

        private static void Detour(JumpOrig orig, LenState_Base self, InputAction.CallbackContext i)
        {
            // If cheat is off, run vanilla.
            if (!UnlimitedJumpsEnabled)
            {
                orig(self, i);
                return;
            }

            // Mirror the first vanilla line (the game reads this elsewhere)
            LenState_Base.jumpInut = i;

            // Only react on an actual button press; otherwise fall back to vanilla behavior.
            // Note: vanilla checks (i.ReadValueAsButton() && Time.time > PlayerInfo.nextJump)
            // We ignore cooldown, but still require a button press to avoid constant jump spam.
            if (!i.ReadValueAsButton())
            {
                orig(self, i);
                return;
            }

            // Optional: hunger drain for balance/telemetry
            if (DrainHungerOnJump)
                self.DrainActionStarvationCost(StateActions.Jump);

            // Kill the cooldown and force grace:
            PlayerInfo.nextJump = Time.time;             // cooldown satisfied
            PlayerInfo.timeSinceJumpRequested = 0f;      // request is "fresh"
            PlayerInfo.timeSinceLastAbleToJump = 0f;     // grace window open (flag3 becomes true)
            PlayerInfo.AllowJumpingWhenSliding = true;   // loosens ground requirement path

            // Trigger a jump the same way vanilla does (let ProcessJumpRequest handle physics)
            PlayerInfo.jumpRequested = true;

            // Don’t call orig() — that would re-check A
