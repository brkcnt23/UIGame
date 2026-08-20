#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Builds one ItemSO asset per entry in ItemCatalog and wires up the sprites
/// that already sit in Assets/UI Elements.
///
/// Sprite matching, in order of preference:
///   1. "&lt;SpriteBase&gt;1..5"  -> quality tiers Crude..Legendary
///   2. "&lt;SpriteBase&gt;"      -> one sprite used for every tier
///   3. case-insensitive match ignoring spaces, so "Cloth Wraps1" also
///      answers to "ClothWraps"
///
/// Re-running is safe: existing assets are updated in place, so hand-tuned
/// values on an asset survive unless the catalog explicitly overwrites them.
///
/// Tools > UIGame > Items
/// </summary>
public static class ItemSOImporter
{
    private const string OutputFolder = "Assets/Resources/Items";
    private const string DatabasePath = "Assets/Resources/ItemDatabase.asset";

    private static readonly string[] SpriteSearchFolders =
    {
        "Assets/UI Elements",
    };

    [MenuItem("Tools/UIGame/Items/Generate ItemSO assets from catalog")]
    public static void Generate()
    {
        EnsureFolder(OutputFolder);

        var spriteIndex = BuildSpriteIndex();
        Debug.Log($"[ItemImporter] Indexed {spriteIndex.Count} sprites.");

        var catalog = ItemCatalog.All;
        int created = 0, updated = 0, missingArt = 0;
        var missingList = new List<string>();

        var assets = new List<ItemSO>();

        foreach (var def in catalog)
        {
            string assetPath = $"{OutputFolder}/{Sanitize(def.Name)}.asset";
            var so = AssetDatabase.LoadAssetAtPath<ItemSO>(assetPath);

            bool isNew = so == null;
            if (isNew)
            {
                so = ScriptableObject.CreateInstance<ItemSO>();
                AssetDatabase.CreateAsset(so, assetPath);
                created++;
            }
            else
            {
                updated++;
            }

            ApplyDef(so, def);

            var sprites = ResolveSprites(spriteIndex, def.SpriteBase);
            if (sprites.Count == 0)
            {
                missingArt++;
                missingList.Add($"{def.Name}  (looked for '{def.SpriteBase}')");
            }
            else
            {
                so.spritesByQuality = sprites;
                so.icon = sprites[Mathf.Min(1, sprites.Count - 1)];
            }

            EditorUtility.SetDirty(so);
            assets.Add(so);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        RebuildDatabase(assets);

        Debug.Log($"[ItemImporter] Done. Created {created}, updated {updated}, " +
                  $"{missingArt} without art.");

        if (missingList.Count > 0)
        {
            Debug.LogWarning("[ItemImporter] No sprite found for:\n  " +
                             string.Join("\n  ", missingList));
        }
    }

    [MenuItem("Tools/UIGame/Items/Report unmatched sprites")]
    public static void ReportUnmatchedSprites()
    {
        var spriteIndex = BuildSpriteIndex();
        var used = new HashSet<string>();

        foreach (var def in ItemCatalog.All)
        {
            foreach (var s in ResolveSprites(spriteIndex, def.SpriteBase))
                used.Add(s.name);
        }

        var unused = spriteIndex.Values
            .SelectMany(l => l)
            .Select(s => s.name)
            .Distinct()
            .Where(n => !used.Contains(n))
            .OrderBy(n => n)
            .ToList();

        Debug.Log($"[ItemImporter] {unused.Count} sprites are not referenced by any catalog entry:\n  " +
                  string.Join("\n  ", unused.Take(200)));
    }

    // -----------------------------------------------------------------

    private static void ApplyDef(ItemSO so, ItemDef d)
    {
        so.ID = d.Id;
        so.itemName = d.Name;
        so.category = d.Category;
        so.goldValue = d.Gold;
        so.silverValue = d.Silver;
        so.weight = d.Weight;
        so.stackable = d.Stackable;
        so.maxStack = d.Stackable ? Mathf.Max(1, d.MaxStack) : 1;

        so.weaponClass = d.Weapon;
        so.scaling = d.Scaling;
        so.damageDie = d.DamageDie;
        so.damageDiceCount = Mathf.Max(1, d.DamageDiceCount);
        so.twoHanded = d.TwoHanded;

        so.armorValue = d.ArmorValue;
        so.armorWeight = d.ArmorClass;

        so.healthRecovery = d.Health;
        so.exhaustionReduction = d.ExhaustionReduction;
        so.rationValue = d.RationValue;

        so.craftable = d.Craftable;
        so.isUnique = d.Unique;
        so.isMagical = d.Magical;

        if (!string.IsNullOrEmpty(d.Flavor))
            so.flavorText = d.Flavor;

        // Quality is a per-instance roll; the asset itself is the Common
        // reference version.
        so.quality = ItemQuality.Common;
    }

    /// <summary>
    /// Key is the normalised sprite base (lowercase, no spaces or punctuation).
    /// Value is the tier list, index 0 = Crude.
    /// </summary>
    private static Dictionary<string, List<Sprite>> BuildSpriteIndex()
    {
        var byBase = new Dictionary<string, SortedDictionary<int, Sprite>>();
        var singles = new Dictionary<string, Sprite>();

        var guids = AssetDatabase.FindAssets("t:Sprite", SpriteSearchFolders);

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null) continue;

            string name = Path.GetFileNameWithoutExtension(path);

            // Trailing digits mean a quality tier.
            int split = name.Length;
            while (split > 0 && char.IsDigit(name[split - 1]))
                split--;

            string baseName = name.Substring(0, split);
            string digits = name.Substring(split);

            string key = Normalize(baseName);
            if (string.IsNullOrEmpty(key)) continue;

            if (digits.Length > 0 && int.TryParse(digits, out int tier))
            {
                if (!byBase.TryGetValue(key, out var tiers))
                    byBase[key] = tiers = new SortedDictionary<int, Sprite>();

                tiers[tier] = sprite;
            }
            else if (!singles.ContainsKey(key))
            {
                singles[key] = sprite;
            }
        }

        var result = new Dictionary<string, List<Sprite>>();

        foreach (var kv in byBase)
            result[kv.Key] = kv.Value.Values.ToList();

        foreach (var kv in singles)
            if (!result.ContainsKey(kv.Key))
                result[kv.Key] = new List<Sprite> { kv.Value };

        return result;
    }

    private static List<Sprite> ResolveSprites(Dictionary<string, List<Sprite>> index, string spriteBase)
    {
        if (string.IsNullOrEmpty(spriteBase))
            return new List<Sprite>();

        string key = Normalize(spriteBase);

        if (index.TryGetValue(key, out var exact))
            return PadToFiveTiers(exact);

        // Fall back to a prefix match — "Falchion" finds "FalchionBlade".
        var partial = index.FirstOrDefault(kv => kv.Key.StartsWith(key) || key.StartsWith(kv.Key));
        if (partial.Value != null && partial.Value.Count > 0)
            return PadToFiveTiers(partial.Value);

        return new List<Sprite>();
    }

    /// <summary>
    /// Five entries so GetSpriteForQuality never indexes past the end.
    /// Fewer authored tiers repeat the last one — three sprites means
    /// Masterwork and Legendary reuse the third.
    /// </summary>
    private static List<Sprite> PadToFiveTiers(List<Sprite> source)
    {
        var padded = new List<Sprite>(5);

        for (int i = 0; i < 5; i++)
            padded.Add(source[Mathf.Min(i, source.Count - 1)]);

        return padded;
    }

    private static void RebuildDatabase(List<ItemSO> assets)
    {
        var db = AssetDatabase.LoadAssetAtPath<ItemDatabase>(DatabasePath);

        if (db == null)
        {
            db = ScriptableObject.CreateInstance<ItemDatabase>();
            AssetDatabase.CreateAsset(db, DatabasePath);
            Debug.Log($"[ItemImporter] Created {DatabasePath}");
        }

        db.items = assets.OrderBy(a => a.ID).ToList();
        db.RebuildIndex();

        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();

        Debug.Log($"[ItemImporter] Database rebuilt with {db.items.Count} items.");
    }

    private static string Normalize(string s)
    {
        var chars = s.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant);
        return new string(chars.ToArray());
    }

    private static string Sanitize(string s)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
            s = s.Replace(c, '_');
        return s;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
            return;

        string parent = Path.GetDirectoryName(path).Replace('\\', '/');
        string leaf = Path.GetFileName(path);

        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, leaf);
    }
}
#endif
