#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Turns RecipeCatalog into RecipeSO assets, resolving ingredient names to
/// item IDs through ItemCatalog.
///
/// Validates as it goes: a recipe naming an item that does not exist is
/// reported rather than silently written with ID 0, which would look fine in
/// the Inspector and fail at runtime.
///
/// Run the item importer first — recipes reference item IDs.
///
/// Tools > UIGame > Recipes
/// </summary>
public static class RecipeSOImporter
{
    private const string OutputFolder = "Assets/Resources/Recipes";
    private const string DatabasePath = "Assets/Resources/RecipeDatabase.asset";

    [MenuItem("Tools/UIGame/Recipes/Generate recipe assets")]
    public static void Generate()
    {
        EnsureFolder(OutputFolder);

        var itemIds = BuildItemIndex();
        var assets = new List<RecipeSO>();

        int created = 0, updated = 0;
        var problems = new List<string>();

        foreach (var def in RecipeCatalog.All)
        {
            if (!itemIds.TryGetValue(Norm(def.Output), out int outputId))
            {
                problems.Add($"'{def.Name}': output item '{def.Output}' is not in ItemCatalog.");
                continue;
            }

            string path = $"{OutputFolder}/{Sanitize(def.Name)}.asset";
            var so = AssetDatabase.LoadAssetAtPath<RecipeSO>(path);

            if (so == null)
            {
                so = ScriptableObject.CreateInstance<RecipeSO>();
                AssetDatabase.CreateAsset(so, path);
                created++;
            }
            else
            {
                updated++;
            }

            so.RecipeId = def.Id;
            so.recipeName = def.Name;
            so.description = def.Description;
            so.OutputItemId = outputId;
            so.OutputQuantity = def.OutputQty;
            so.discipline = def.Discipline;
            so.requiredDisciplineLevel = def.Level;
            so.baseCraftTimeMinutes = def.Minutes;
            so.baseSuccessChance = def.SuccessChance;
            so.learnedByDefault = def.KnownByDefault;
            so.requiredStations = new List<string>(def.Stations);
            so.requiredTools = new List<string>(def.Tools);

            so.ingredients = new List<RecipeIngredient>();

            foreach (var (itemName, qty) in def.Ingredients)
            {
                if (!itemIds.TryGetValue(Norm(itemName), out int ingId))
                {
                    problems.Add($"'{def.Name}': ingredient '{itemName}' is not in ItemCatalog.");
                    continue;
                }

                so.ingredients.Add(new RecipeIngredient { ItemId = ingId, Quantity = qty });
            }

            EditorUtility.SetDirty(so);
            assets.Add(so);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        RebuildDatabase(assets);

        Debug.Log($"[RecipeImporter] Done. Created {created}, updated {updated}, " +
                  $"{problems.Count} problem(s).");

        if (problems.Count > 0)
            Debug.LogWarning("[RecipeImporter] Unresolved references:\n  " + string.Join("\n  ", problems));
    }

    /// <summary>
    /// Walks the whole production chain and reports anything unreachable —
    /// an item nobody can make and nobody sells is a dead end the player will
    /// find before you do.
    /// </summary>
    [MenuItem("Tools/UIGame/Recipes/Validate production chain")]
    public static void Validate()
    {
        var recipesByOutput = new Dictionary<string, RecipeDef>();
        foreach (var r in RecipeCatalog.All)
            recipesByOutput[Norm(r.Output)] = r;

        var rawResources = new HashSet<string>();
        foreach (var item in ItemCatalog.All)
        {
            if (item.Category == ItemCategory.Resource ||
                item.Category == ItemCategory.TradeGood ||
                item.Category == ItemCategory.QuestItem ||
                item.Category == ItemCategory.Trinket ||
                !item.Craftable)
            {
                rawResources.Add(Norm(item.Name));
            }
        }

        var unreachable = new List<string>();
        var deepest = new List<(string name, int depth)>();

        foreach (var recipe in RecipeCatalog.All)
        {
            int depth = Depth(Norm(recipe.Output), recipesByOutput, rawResources, 0, new HashSet<string>());

            if (depth < 0)
                unreachable.Add(recipe.Name);
            else
                deepest.Add((recipe.Name, depth));
        }

        deepest.Sort((a, b) => b.depth.CompareTo(a.depth));

        Debug.Log($"[RecipeImporter] {RecipeCatalog.All.Count} recipes. " +
                  $"Deepest chains:\n  " +
                  string.Join("\n  ", deepest.Take(10).Select(d => $"{d.depth} steps  {d.name}")));

        if (unreachable.Count > 0)
            Debug.LogWarning("[RecipeImporter] Cannot be produced from raw resources:\n  " +
                             string.Join("\n  ", unreachable));

        // Items with no recipe and no shop presence are only obtainable as loot.
        var craftable = new HashSet<string>(RecipeCatalog.All.Select(r => Norm(r.Output)));
        var orphans = ItemCatalog.All
            .Where(i => i.Craftable
                        && i.Category != ItemCategory.Resource
                        && i.Category != ItemCategory.TradeGood
                        && i.Category != ItemCategory.QuestItem
                        && !craftable.Contains(Norm(i.Name)))
            .Select(i => i.Name)
            .ToList();

        if (orphans.Count > 0)
            Debug.Log($"[RecipeImporter] {orphans.Count} craftable items have no recipe yet:\n  " +
                      string.Join("\n  ", orphans));
    }

    /// <summary>
    /// How many production steps separate this item from raw resources.
    /// Returns -1 when the chain cannot bottom out — a missing ingredient or
    /// a cycle.
    /// </summary>
    private static int Depth(string item, Dictionary<string, RecipeDef> recipes,
                             HashSet<string> raw, int guard, HashSet<string> visiting)
    {
        if (raw.Contains(item)) return 0;
        if (guard > 20) return -1;
        if (!visiting.Add(item)) return -1;      // cycle

        if (!recipes.TryGetValue(item, out var recipe))
        {
            visiting.Remove(item);
            return -1;
        }

        int max = 0;
        foreach (var (ing, _) in recipe.Ingredients)
        {
            int d = Depth(Norm(ing), recipes, raw, guard + 1, visiting);
            if (d < 0) { visiting.Remove(item); return -1; }
            if (d > max) max = d;
        }

        visiting.Remove(item);
        return max + 1;
    }

    private static Dictionary<string, int> BuildItemIndex()
    {
        var map = new Dictionary<string, int>();

        foreach (var item in ItemCatalog.All)
            map[Norm(item.Name)] = item.Id;

        return map;
    }

    private static void RebuildDatabase(List<RecipeSO> assets)
    {
        var db = AssetDatabase.LoadAssetAtPath<RecipeDatabaseSO>(DatabasePath);

        if (db == null)
        {
            db = ScriptableObject.CreateInstance<RecipeDatabaseSO>();
            AssetDatabase.CreateAsset(db, DatabasePath);
            Debug.Log($"[RecipeImporter] Created {DatabasePath}");
        }

        db.recipes = assets.OrderBy(a => a.RecipeId).ToList();
        db.RebuildIndex();

        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();

        Debug.Log($"[RecipeImporter] Database rebuilt with {db.recipes.Count} recipes.");
    }

    private static string Norm(string s)
        => new string(s.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());

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
