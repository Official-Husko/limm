using System;
using System.Collections;
using System.Reflection;
using BepInEx.Logging;
using MonoMod.RuntimeDetour;
using UnityEngine;

namespace LensIslandModMenu.MonoMod
{
    internal static class Detour_DealInitialCards
    {
        private static Hook _hook;
        private static FieldInfo _fiFlop;
        private static FieldInfo _fiCardDrawTime;

        private delegate IEnumerator DealInitialCardsOrig(BlackjackController self);

        public static void Apply(ManualLogSource Log)
        {
            var t = typeof(BlackjackController);

            var target = t.GetMethod(
                "DealInitialCards",
                BindingFlags.Instance | BindingFlags.NonPublic);

            if (target == null)
                throw new MissingMethodException($"{t.FullName}.DealInitialCards not found");

            // cache private fields we must mirror from the original
            _fiFlop = t.GetField("flop", BindingFlags.Instance | BindingFlags.NonPublic);
            _fiCardDrawTime = t.GetField("cardDrawTime", BindingFlags.Instance | BindingFlags.NonPublic);

            _hook = new Hook(target, new Func<DealInitialCardsOrig, BlackjackController, IEnumerator>(Detour));

            Log.LogInfo(
                $"[Detour] Hooked {target.DeclaringType.Assembly.GetName().Name}:{target.DeclaringType.FullName}.{target.Name}()");
        }

        private static IEnumerator Detour(DealInitialCardsOrig orig, BlackjackController self)
        {
            ModMenuPlugin.Log?.LogInfo("[Detour] DealInitialCards called, applying custom logic.");
            bool alwaysWin = ModMenuPlugin.AlwaysWin;

            // read the original delay
            float delay = 0.5f;
            if (_fiCardDrawTime != null)
                delay = (float)_fiCardDrawTime.GetValue(self);

            if (alwaysWin)
            {
                self.DealCard(HandOwner.Player, new PlayingCard { number = 1, suit = CardSuit.Spade });   // Ace
            }
            else
            {
                self.DealPlayerCard();
            }
            yield return new WaitForSeconds(delay);

            self.playerText.gameObject.SetActive(true);

            if (alwaysWin)
            {
                self.DealCard(HandOwner.Player, new PlayingCard { number = 12, suit = CardSuit.Spade });  // Queen
            }
            else
            {
                self.DealPlayerCard();
            }
            yield return new WaitForSeconds(delay);

            // (optional small frame delay)
            yield return null;

            // Dealer gets one card like vanilla
            self.DealDealersCard();

            // vanilla sets flop=false and shows dealer text
            if (_fiFlop != null) _fiFlop.SetValue(self, false);
            self.dealerText.gameObject.SetActive(true);
        }
    }
}
