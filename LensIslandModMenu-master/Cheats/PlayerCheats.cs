using BepInEx.Logging;
using System;

namespace LensIslandModMenu.Cheats
{
    internal class PlayerCheats
    {
        public static void IncreaseBackpackLevel(ManualLogSource Log) //Doesn't do anything?
        {
            try
            {
                Log.LogInfo("Increasing backpack level...");
                int backPackLvl = Singleton<Player>.Instance.backPackLvl;
                Singleton<Player>.Instance.SetBackPack(backPackLvl + 1);
                Log.LogInfo($"Backpack level increased to {backPackLvl + 1}.");
            }
            catch (Exception ex)
            {
                Log.LogError($"Failed to increase backpack level: {ex.Message}");
            }
        }

        public static void KillPlayer(ManualLogSource Log) //Kills the player instantly.
        {
            Log.LogInfo("Killing player...");
            try
            {
                Singleton<Player>.Instance.Die();
            }
            catch (Exception ex)
            {
                Log.LogError($"Failed to kill player: {ex.Message}");
            }
        }

        public static void GiveXP(ManualLogSource Log, int amount) //Gives XP.
        {
            Log.LogInfo($"Granting players {amount} XP.");
            try
            {
                global::EventHandler.OnGiveAllPlayersXP(amount);
                SkillSystem.Instance.GiveXP(amount);
                Log.LogInfo($"Granted {amount} XP to player.");
            }
            catch (Exception ex)
            {
                Log.LogError($"Failed to give XP ({amount}). Message: {ex.Message}");
            }
        }
        public static bool ToggleGodMode(ManualLogSource Log)
        {
            try
            {
                DebugConsole.GodMode = !DebugConsole.GodMode;
                bool result = DebugConsole.GodMode;
                Log.LogInfo("Godmode toggled.");
                return result;
            }
            catch (Exception ex)
            {
                Log.LogInfo($"Godmode toggle FAILED: {ex.Message}");
                return DebugConsole.GodMode;
            }
            
        }
    }
}
