using BepInEx.Logging;
using System.Reflection;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LensIslandModMenu.Cheats
{
    internal class ItemCheats
    {
        public static void SpawnItem(ManualLogSource Log, int amount = 1, ResourceTypes type = ResourceTypes.GoldCoins)
        {
            Log.LogInfo("Attempting to spawn item...");
            Player_ResourceManager rm = Singleton<Player>.Instance.playerRecources;
            Scriptable_Resource resource = rm.Find(type);
            if (resource != null)
            {
                Log.LogError($"ResourceType {type} found. Spawning...");
                rm.AddResource(resource, amount, true, true);
            }
            
        }
    }
}
