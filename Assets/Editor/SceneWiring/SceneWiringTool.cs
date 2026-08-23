#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Attaches components and fills their Inspector slots from a JSON recipe.
///
/// The work this replaces is dragging objects into fields, which is slow and
/// fails silently: an empty slot and an unwritten feature look identical at
/// runtime. Worse, regenerating a panel clears every reference into it, so the
/// dragging has to be redone each time the layout changes.
///
/// The field's declared type decides what gets fetched off the named object, so
/// a recipe names objects and never types. Private [SerializeField] fields work
/// the same as public ones - assignment goes through SerializedObject, which is
/// also what makes it undoable and marks the scene dirty.
///
/// Nothing here creates or moves objects. If a recipe names something the scene
/// does not have, it is reported and skipped.
///
/// Tools > UIGame > Wiring
/// </summary>
public static class SceneWiringTool
{
    private const string SpecPath = "Assets/Data/ui/wiring.json";

    private static readonly List<string> _report = new();
    private static int _assigned, _skipped;

    [MenuItem("Tools/UIGame/Wiring/Wire the scene from wiring.json", false, 0)]
    public static void Wire() => Run(dryRun: false);

    [MenuItem("Tools/UIGame/Wiring/Check without changing anything", false, 1)]
    public static void Check() => Run(dryRun: true);

    private static void Run(bool dryRun)
    {
        var spec = LoadSpec();
        if (spec == null) return;

        _report.Clear();
        _assigned = 0;
        _skipped = 0;

        var index = BuildSceneIndex();

        if (!dryRun)
            Undo.SetCurrentGroupName("Wire scene");

        int group = Undo.GetCurrentGroup();

        foreach (var recipe in spec.recipes)
            ApplyRecipe(recipe, index, dryRun);

        if (!dryRun)
        {
            Undo.CollapseUndoOperations(group);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        string head = dryRun
            ? $"[Wiring] Dry run: {_assigned} slots would be filled, {_skipped} could not be."
            : $"[Wiring] {_assigned} slots filled, {_skipped} could not be.";

        string body = _report.Count == 0 ? "" : "\n  " + string.Join("\n  ", _report);

        if (_skipped > 0) Debug.LogWarning(head + body);
        else Debug.Log(head + body);

        if (!dryRun && _assigned > 0)
            _report.Add("Save the scene to keep this.");
    }

    // =================================================================

    private static void ApplyRecipe(WiringRecipe recipe, Dictionary<string, List<GameObject>> index, bool dryRun)
    {
        var target = Resolve(recipe.target, index, "target");
        if (target == null) return;

        var type = FindType(recipe.component);

        if (type == null)
        {
            Fail($"Component type not found: {recipe.component}");
            return;
        }

        var component = target.GetComponent(type);

        if (component == null)
        {
            if (dryRun)
            {
                _report.Add($"Would add {recipe.component} to '{recipe.target}'.");
            }
            else
            {
                component = Undo.AddComponent(target, type);
                _report.Add($"Added {recipe.component} to '{recipe.target}'.");
            }
        }

        // A dry run on an object that has no component yet still has to say what
        // the slots would receive, so everything below reads the field list off
        // the type. The SerializedObject is only needed to write.
        var serialized = component != null ? new SerializedObject(component) : null;

        foreach (var f in recipe.fields)
            AssignSingle(type, serialized, f, index, recipe.keepExisting);

        foreach (var a in recipe.arrays)
            AssignArray(type, serialized, a, index, recipe.keepExisting);

        if (!dryRun && serialized != null)
            serialized.ApplyModifiedProperties();
    }

    private static void AssignSingle(System.Type componentType, SerializedObject serialized,
                                     WiringField f, Dictionary<string, List<GameObject>> index,
                                     bool keepExisting)
    {
        var fieldType = FieldType(componentType, f.field);

        if (fieldType == null)
        {
            Fail($"{componentType.Name} has no field '{f.field}'.");
            return;
        }

        var property = serialized?.FindProperty(f.field);

        if (serialized != null && property == null)
        {
            Fail($"{componentType.Name}.{f.field} is not serialized.");
            return;
        }

        if (keepExisting && property != null && property.objectReferenceValue != null)
            return;

        var value = FetchOff(f.obj, fieldType, index);

        if (value == null)
            return;

        if (property != null)
            property.objectReferenceValue = value;

        _assigned++;
    }

    private static void AssignArray(System.Type componentType, SerializedObject serialized,
                                    WiringArray a, Dictionary<string, List<GameObject>> index,
                                    bool keepExisting)
    {
        var property = serialized?.FindProperty(a.field);

        if (serialized != null && (property == null || !property.isArray))
        {
            Fail($"{componentType.Name} has no array field '{a.field}'.");
            return;
        }

        if (keepExisting && property != null && property.arraySize > 0)
            return;

        var elementType = FieldType(componentType, a.field)?.GetElementType();

        if (elementType == null)
        {
            Fail($"Could not read the element type of '{a.field}'.");
            return;
        }

        var found = a.objects
            .Select(name => FetchOff(name, elementType, index))
            .ToList();

        if (property != null)
            property.arraySize = found.Count;

        for (int i = 0; i < found.Count; i++)
        {
            if (property != null)
                property.GetArrayElementAtIndex(i).objectReferenceValue = found[i];

            if (found[i] != null)
                _assigned++;
        }
    }

    // =================================================================

    /// <summary>
    /// The named object, then whatever the field actually wants off it - the
    /// GameObject itself, or one of its components.
    /// </summary>
    private static Object FetchOff(string objectName, System.Type wanted,
                                   Dictionary<string, List<GameObject>> index)
    {
        var go = Resolve(objectName, index, "object");

        if (go == null)
            return null;

        if (wanted == typeof(GameObject))
            return go;

        var component = go.GetComponent(wanted);

        if (component == null)
            Fail($"'{objectName}' has no {wanted.Name}.");

        return component;
    }

    private static GameObject Resolve(string name, Dictionary<string, List<GameObject>> index, string role)
    {
        if (string.IsNullOrEmpty(name))
            return null;

        if (!index.TryGetValue(name, out var matches))
        {
            Fail($"No object named '{name}' in the scene ({role}).");
            return null;
        }

        if (matches.Count > 1)
        {
            // Ambiguous names are how the wrong object ends up in a slot, and it
            // would look like it worked. Better to stop and say so.
            Fail($"'{name}' matches {matches.Count} objects. Rename them so the recipe is unambiguous.");
            return null;
        }

        return matches[0];
    }

    private static System.Type FieldType(System.Type type, string fieldName)
    {
        while (type != null)
        {
            var field = type.GetField(fieldName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (field != null)
                return field.FieldType;

            type = type.BaseType;
        }

        return null;
    }

    private static System.Type FindType(string name)
        => System.Type.GetType(name + ", Assembly-CSharp")
           ?? System.Type.GetType(name)
           ?? System.AppDomain.CurrentDomain.GetAssemblies()
               .Select(a => a.GetType(name))
               .FirstOrDefault(t => t != null);

    /// <summary>Every object in the scene by name, inactive ones included.</summary>
    private static Dictionary<string, List<GameObject>> BuildSceneIndex()
    {
        var index = new Dictionary<string, List<GameObject>>();

        foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
            {
                if (!index.TryGetValue(t.name, out var list))
                    index[t.name] = list = new List<GameObject>();

                list.Add(t.gameObject);
            }
        }

        return index;
    }

    private static SceneWiringSpec LoadSpec()
    {
        if (!File.Exists(SpecPath))
        {
            EditorUtility.DisplayDialog("Scene wiring", "No recipe file at " + SpecPath + ".", "Right");
            return null;
        }

        try
        {
            var spec = JsonUtility.FromJson<SceneWiringSpec>(File.ReadAllText(SpecPath));

            if (spec == null || spec.recipes.Length == 0)
            {
                Debug.LogWarning("[Wiring] " + SpecPath + " has no recipes.");
                return null;
            }

            return spec;
        }
        catch (System.Exception e)
        {
            Debug.LogError("[Wiring] " + SpecPath + " could not be read: " + e.Message);
            return null;
        }
    }

    private static void Fail(string message)
    {
        _report.Add(message);
        _skipped++;
    }
}
#endif
