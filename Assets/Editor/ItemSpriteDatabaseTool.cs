#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Builds the fallback sprite table for procedurally generated items.
///
/// Shop stock is invented at runtime - "Blacksmith Item 3" has no entry in the
/// catalogue and therefore no art of its own - so ItemGenerator asks this table
/// for a picture by category and quality. Without the asset the call returned
/// null and generated stock appeared as blank squares.
///
/// The representatives are taken from the catalogue rather than picked by hand,
/// so a made-up quality 3 weapon borrows the face of a real quality 3 weapon and
/// the tiers look different from each other. Items with several tier variants
/// are preferred for exactly that reason: an item with one drawing would make
/// every quality of that category identical.
///
/// Naming follows ItemSOImporter, whose index and resolver this calls rather
/// than restating - one convention, in one place.
///
/// Tools > UIGame > Items
/// </summary>
public static class ItemSpriteDatabaseTool
{
    private const string AssetPath = "Assets/Resources/ItemSpriteDatabase.asset";

    /// <summary>Crude through Legendary. ItemGenerator clamps to 1-3, but a table with holes invites nulls.</summary>
    private const int TierCount = 5;

    [MenuItem("Tools/UIGame/Items/Build ItemSpriteDatabase", false, 10)]
    public static void Build()
    {
        var spriteIndex = ItemSOImporter.BuildSpriteIndex();

        if (spriteIndex.Count == 0)
        {
            Debug.LogError("[SpriteDb] No sprites indexed. Check the search folders in ItemSOImporter.");
            return;
        }

        var db = LoadOrCreate(out bool created);

        db.itemSprites.Clear();

        var covered = new List<string>();
        var bare = new List<string>();

        foreach (ItemCategory category in System.Enum.GetValues(typeof(ItemCategory)))
        {
            var candidates = CandidatesFor(category, spriteIndex);

            if (candidates.Count == 0)
            {
                bare.Add(category.ToString());
                continue;
            }

            int filled = 0;

            for (int tier = 0; tier < TierCount; tier++)
            {
                var sprite = PickForTier(candidates, tier);

                if (sprite == null)
                    continue;

                db.itemSprites.Add(new ItemSpriteDatabase.ItemSprite
                {
                    Category = category,
                    Quality = tier,
                    Image = sprite
                });

                filled++;
            }

            covered.Add($"{category} ({filled}/{TierCount})");
        }

        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = db;

        var text = $"[SpriteDb] {(created ? "Created" : "Rebuilt")} {AssetPath} " +
                   $"with {db.itemSprites.Count} entries.\n  " +
                   string.Join("\n  ", covered);

        if (bare.Count > 0)
            text += "\n  No catalogue art at all for: " + string.Join(", ", bare) +
                    "\n  Generated items in those categories will still draw blank.";

        Debug.Log(text);
    }

    [MenuItem("Tools/UIGame/Items/Report sprite coverage by category", false, 11)]
    public static void Report()
    {
        var spriteIndex = ItemSOImporter.BuildSpriteIndex();
        var lines = new List<string>();

        foreach (ItemCategory category in System.Enum.GetValues(typeof(ItemCategory)))
        {
            var items = ItemCatalog.All.Where(d => d.Category == category).ToList();
            var withArt = items.Count(d => ItemSOImporter.ResolveSprites(spriteIndex, d.SpriteBase).Count > 0);
            var tiered = items.Count(d => ItemSOImporter.ResolveSprites(spriteIndex, d.SpriteBase).Count > 1);

            lines.Add($"  {category,-18} {items.Count,3} items, {withArt,3} with art, {tiered,3} with tier variants");
        }

        Debug.Log("[SpriteDb] Catalogue art coverage:\n" + string.Join("\n", lines));
    }

    // =================================================================

    /// <summary>
    /// Sprite lists for every catalogue item of this category, best first.
    /// An item drawn once per tier beats one drawn only once, because the
    /// single drawing would make every quality look the same.
    /// </summary>
    private static List<List<Sprite>> CandidatesFor(ItemCategory category,
                                                    Dictionary<string, List<Sprite>> spriteIndex)
    {
        return ItemCatalog.All
            .Where(d => d.Category == category)
            .Select(d => ItemSOImporter.ResolveSprites(spriteIndex, d.SpriteBase))
            .Where(list => list.Count > 0)
            .OrderByDescending(list => list.Count)
            .ToList();
    }

    /// <summary>
    /// The first candidate that actually has a drawing for this tier. Falling
    /// back to a candidate's last drawing keeps the table dense: a category
    /// whose art stops at three tiers still answers for Legendary rather than
    /// handing back the null it used to.
    /// </summary>
    private static Sprite PickForTier(List<List<Sprite>> candidates, int tier)
    {
        foreach (var list in candidates)
            if (tier < list.Count)
                return list[tier];

        return candidates.Count > 0 ? candidates[0].Last() : null;
    }

    private static ItemSpriteDatabase LoadOrCreate(out bool created)
    {
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");

        var db = AssetDatabase.LoadAssetAtPath<ItemSpriteDatabase>(AssetPath);
        created = db == null;

        if (created)
        {
            db = ScriptableObject.CreateInstance<ItemSpriteDatabase>();
            AssetDatabase.CreateAsset(db, AssetPath);
        }

        return db;
    }
}
#endif
