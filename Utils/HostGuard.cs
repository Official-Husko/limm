using System.Linq;
using UnityEngine;


namespace limm.Utils;

/// <summary>
/// Ensures we keep a single persistent host object across reloads.
/// </summary>
public static class HostGuard
{
    /// <summary>
    /// Removes any existing DontDestroyOnLoad object with the given name, then returns a fresh one.
    /// </summary>
    public static GameObject CreateFresh(string name)
    {
        var existing = FindExisting(name);
        if (existing != null)
        {
            Object.Destroy(existing);
            var logger = BepInEx.Logging.Logger.CreateLogSource("HostGuard");
            logger.LogInfo($"Removed existing host object '{name}' from previous load.");
        }

        var host = new GameObject(name);
        Object.DontDestroyOnLoad(host);
        return host;
    }

    private static GameObject FindExisting(string name)
    {
        // Resources.FindObjectsOfTypeAll lets us see objects in the hidden DontDestroyOnLoad scene.
        return Resources.FindObjectsOfTypeAll<GameObject>()
            .FirstOrDefault(go => go != null &&
                                  go.name == name &&
                                  go.scene.name == "DontDestroyOnLoad");
    }
}
