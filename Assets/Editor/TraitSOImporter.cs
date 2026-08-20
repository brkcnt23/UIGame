#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Turns TraitCatalog into TraitSO assets and wires each one to its icon in
/// UI Elements/ProfilePanel/traits/250x250.
///
/// Re-running updates existing assets in place.
///
/// Tools > UIGame > Traits
/// </summary>
public static class TraitSOImporter
{
    private const string OutputFolder = "Assets/Resources/Traits";
    private const string DatabasePath = "Assets/Resources/TraitDatabase.asset";
    private const string IconFolder = "Assets/UI Elements/ProfilePanel/traits/250x250";

    [MenuItem("Tools/UIGame/Traits/Generate trait assets")]
    public static void Generate()
    {
        EnsureFolder(OutputFolder);

        var icons = BuildIconIndex();
        Debug.Log($"[TraitImporter] Indexed {icons.Count} trait icons.");

        var assets = new List<TraitSO>();
        int created = 0, updated = 0;
        var missing = new List<string>();

        foreach (var def in TraitCatalog.All)
        {
            string path = $"{OutputFolder}/{Sanitize(def.Id)}.asset";
            var so = AssetDatabase.LoadAssetAtPath<TraitSO>(path);

            if (so == null)
            {
                so = ScriptableObject.CreateInstance<TraitSO>();
                AssetDatabase.CreateAsset(so, path);
                created++;
            }
            else
            {
                updated++;
            }

            so.traitId = def.Id;
            so.displayName = def.Name;
            so.kind = def.Kind;
            so.tone = def.Tone;
            so.description = def.Description;
            so.durationHours = def.DurationHours;
            so.effects = new List<GameplayEffect>(def.Effects);
            so.removesTraitIds = new List<string>(def.Removes);
            so.grantsTags = new List<string>(def.Tags);

            var icon = Resolve(icons, def.IconName);
            if (icon != null)
                so.icon = icon;
            else
                missing.Add($"{def.Name}  (looked for '{def.IconName}')");

            EditorUtility.SetDirty(so);
            assets.Add(so);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        RebuildDatabase(assets);

        Debug.Log($"[TraitImporter] Done. Created {created}, updated {updated}, " +
                  $"{missing.Count} without an icon.");

        if (missing.Count > 0)
            Debug.LogWarning("[TraitImporter] No icon for:\n  " + string.Join("\n  ", missing));
    }

    [MenuItem("Tools/UIGame/Traits/Report unused icons")]
    public static void ReportUnusedIcons()
    {
        var icons = BuildIconIndex();
        var used = new HashSet<string>();

        foreach (var def in TraitCatalog.All)
        {
            var s = Resolve(icons, def.IconName);
            if (s != null) used.Add(s.name);
        }

        var unused = icons.Values
            .Select(s => s.name)
            .Distinct()
            .Where(n => !used.Contains(n))
            .OrderBy(n => n)
            .ToList();

        Debug.Log($"[TraitImporter] {unused.Count} icons have no catalog entry yet:\n  " +
                  string.Join("\n  ", unused));
    }

    // -----------------------------------------------------------------

    private static Dictionary<string, Sprite> BuildIconIndex()
    {
        var index = new Dictionary<string, Sprite>();

        if (!AssetDatabase.IsValidFolder(IconFolder))
        {
            Debug.LogError($"[TraitImporter] Icon folder not found: {IconFolder}");
            return index;
        }

        foreach (var guid in AssetDatabase.FindAssets("t:Sprite", new[] { IconFolder }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null) continue;

            string key = Normalize(Path.GetFileNameWithoutExtension(path));
            if (!index.ContainsKey(key))
                index[key] = sprite;
        }

        return index;
    }

    private static Sprite Resolve(Dictionary<string, Sprite> index, string name)
    {
        if (string.IsNullOrEmpty(name)) return null;

        string key = Normalize(name);
        if (index.TryGetValue(key, out var exact))
            return exact;

        // Numbered variants: "Bleeding2" answers for "Bleeding".
        var partial = index.FirstOrDefault(kv => kv.Key.StartsWith(key));
        return partial.Value;
    }

    private static void RebuildDatabase(List<TraitSO> assets)
    {
        var db = AssetDatabase.LoadAssetAtPath<TraitDatabaseSO>(DatabasePath);

        if (db == null)
        {
            db = ScriptableObject.CreateInstance<TraitDatabaseSO>();
            AssetDatabase.CreateAsset(db, DatabasePath);
            Debug.Log($"[TraitImporter] Created {DatabasePath}");
        }

        db.traits = assets
            .OrderBy(a => a.kind)
            .ThenBy(a => a.displayName)
            .ToList();

        db.RebuildIndex();

        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();

        Debug.Log($"[TraitImporter] Database rebuilt with {db.traits.Count} traits.");
    }

    /// <summary>
    /// Lowercase alphanumerics only, so "Duelist's Focus", "duelists_focus"
    /// and "DuelistsFocus" all collapse to the same key. The apostrophe in the
    /// icon files is a typographic one (U+2019), which is exactly the kind of
    /// character that breaks naive string matching.
    /// </summary>
    private static string Normalize(string s)
    {
        return new string(s.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());
    }

    private static string Sanitize(string s)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
            s = s.Replace(c, '_');
        return s;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;

        string parent = Path.GetDirectoryName(path).Replace('\\', '/');
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
    }
}
#endif
