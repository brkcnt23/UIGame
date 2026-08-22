#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Finds systems that exist in code but in no scene.
///
/// A GameSystemBase does nothing until something in a scene holds it —
/// GameBootstrapper discovers systems by scanning the loaded scene, so a system
/// nobody attached is never initialised and never ticks. Nothing about that is
/// visible in the code: it compiles, it has no callers by design, and only the
/// boot log's system count gives it away.
///
/// Tools > UIGame > Systems
/// </summary>
public static class GameSystemsTool
{
    [MenuItem("Tools/UIGame/Systems/Report which systems are missing", false, 0)]
    public static void Report()
    {
        var present = new List<string>();
        var missing = new List<string>();

        foreach (var type in SystemTypes())
        {
            bool inScene = Object.FindObjectsByType(type, FindObjectsInactive.Include,
                                                    FindObjectsSortMode.None).Length > 0;

            (inScene ? present : missing).Add(type.Name);
        }

        var text = $"[Systems] {present.Count} in the scene, {missing.Count} missing.\n" +
                   "  in scene: " + string.Join(", ", present);

        if (missing.Count > 0)
            text += "\n  MISSING:  " + string.Join(", ", missing) +
                    "\n  These never initialise. Add them with the menu item below.";

        Debug.Log(text);
    }

    [MenuItem("Tools/UIGame/Systems/Add every missing system to the scene", false, 1)]
    public static void AddMissing()
    {
        var holder = GameObject.Find("Manager Holder") ?? GameObject.Find("Managers");

        if (holder == null)
        {
            holder = new GameObject("Manager Holder");
            Undo.RegisterCreatedObjectUndo(holder, "Create manager holder");
        }

        var added = new List<string>();

        foreach (var type in SystemTypes())
        {
            if (Object.FindObjectsByType(type, FindObjectsInactive.Include,
                                          FindObjectsSortMode.None).Length > 0)
                continue;

            Undo.AddComponent(holder, type);
            added.Add(type.Name);
        }

        if (added.Count == 0)
        {
            Debug.Log("[Systems] Every system was already in the scene.");
            return;
        }

        EditorUtility.SetDirty(holder);

        Debug.Log($"[Systems] Added to '{holder.name}': {string.Join(", ", added)}." +
                  "\n  Save the scene, then check the boot log counts them.");
    }

    private static IEnumerable<System.Type> SystemTypes()
        => typeof(GameSystemBase).Assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(GameSystemBase)) && !t.IsAbstract)
            .OrderBy(t => t.Name);
}
#endif
