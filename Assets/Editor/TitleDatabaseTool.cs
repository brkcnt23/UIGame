#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Creates and fills the title database asset.
///
/// TitleDatabaseSO has carried the twenty-six rung ladder in
/// PopulateDefaultTitles since it was written, but no asset ever existed to put
/// it in, so ResourceProvider logged "TitleDatabase not found" on every boot and
/// nothing in the game could name a rank.
///
/// Refreshing keeps whatever has been tuned by hand. The reputation thresholds
/// ship as a draft (rank x 100) and the milestone gates as zero, so a rebuild
/// that overwrote them would throw away the balancing pass every time the tool
/// was run.
///
/// Tools > UIGame > Titles
/// </summary>
public static class TitleDatabaseTool
{
    private const string AssetPath = "Assets/Resources/TitleDatabase.asset";
    private const string BadgeFolder = "Assets/UI Elements/Titles";

    [MenuItem("Tools/UIGame/Titles/Create or refresh TitleDatabase", false, 0)]
    public static void CreateOrRefresh()
    {
        EnsureResourcesFolder();

        var db = AssetDatabase.LoadAssetAtPath<TitleDatabaseSO>(AssetPath);
        bool created = false;

        if (db == null)
        {
            db = ScriptableObject.CreateInstance<TitleDatabaseSO>();
            AssetDatabase.CreateAsset(db, AssetPath);
            created = true;
        }

        // What a designer has already set, so a refresh does not undo tuning.
        var tuned = db.titles.ToDictionary(
            t => t.titleId,
            t => new Tuning
            {
                RequiredReputation = t.requiredReputation,
                RequiredPopulation = t.requiredPopulation,
                RequiredWealth = t.requiredWealth,
                RequiredQuality = t.requiredQuality,
                MaxArmySize = t.maxArmySize,
                MapAvatarIcon = t.mapAvatarIcon,
                TitleBadge = t.titleBadge,
                FlavorText = t.flavorText
            });

        db.titles.Clear();
        db.PopulateDefaultTitles();

        int restored = 0;

        foreach (var title in db.titles)
        {
            if (!tuned.TryGetValue(title.titleId, out var old))
                continue;

            // Zero means the default was never touched, so the fresh value wins.
            if (old.RequiredReputation != 0) title.requiredReputation = old.RequiredReputation;
            if (old.RequiredPopulation != 0) title.requiredPopulation = old.RequiredPopulation;
            if (old.RequiredWealth != 0) title.requiredWealth = old.RequiredWealth;
            if (old.RequiredQuality != 0) title.requiredQuality = old.RequiredQuality;
            if (old.MaxArmySize != 0) title.maxArmySize = old.MaxArmySize;
            if (old.MapAvatarIcon != null) title.mapAvatarIcon = old.MapAvatarIcon;
            if (old.TitleBadge != null) title.titleBadge = old.TitleBadge;
            if (!string.IsNullOrEmpty(old.FlavorText)) title.flavorText = old.FlavorText;

            restored++;
        }

        int matched = AttachBadges(db);

        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = db;

        var milestones = db.titles.Count(t => t.track == TitleTrack.Milestone);

        Debug.Log(
            $"[TitleTool] {(created ? "Created" : "Refreshed")} {AssetPath}.\n" +
            $"  {db.titles.Count} titles, {milestones} milestones.\n" +
            $"  {restored} kept their tuned values.\n" +
            $"  {matched} badges matched from {BadgeFolder}.\n" +
            "  Reputation thresholds are still the rank x 100 draft; milestone " +
            "population, wealth and quality gates are zero until you set them.");
    }

    [MenuItem("Tools/UIGame/Titles/Report what still needs numbers", false, 20)]
    public static void ReportGaps()
    {
        var db = AssetDatabase.LoadAssetAtPath<TitleDatabaseSO>(AssetPath);

        if (db == null)
        {
            Debug.LogWarning("[TitleTool] No database yet. Run 'Create or refresh TitleDatabase' first.");
            return;
        }

        var lines = new List<string>();

        foreach (var t in db.titles.Where(t => t.track == TitleTrack.Milestone))
        {
            var missing = new List<string>();

            if (t.requiredPopulation == 0) missing.Add("population");
            if (t.requiredWealth == 0) missing.Add("wealth");
            if (t.requiredQuality == 0) missing.Add("quality");
            if (t.titleBadge == null) missing.Add("badge");

            if (missing.Count > 0)
                lines.Add($"  {t.displayName,-10} needs: {string.Join(", ", missing)}");
        }

        int noBadge = db.titles.Count(t => t.titleBadge == null);

        Debug.Log(lines.Count == 0
            ? $"[TitleTool] Every milestone has its gates set. {noBadge} titles still have no badge."
            : "[TitleTool] Milestone gates still unset:\n" + string.Join("\n", lines) +
              $"\n  ({noBadge} of {db.titles.Count} titles have no badge sprite.)");
    }

    /// <summary>
    /// Looks for a sprite named after each title. Nothing is reported as an
    /// error: badges are art that may not be drawn yet, and a title without one
    /// still works.
    /// </summary>
    private static int AttachBadges(TitleDatabaseSO db)
    {
        if (!Directory.Exists(BadgeFolder))
            return 0;

        var index = new Dictionary<string, Sprite>();

        foreach (string guid in AssetDatabase.FindAssets("t:Sprite", new[] { BadgeFolder }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

            if (sprite != null)
                index[Normalise(sprite.name)] = sprite;
        }

        int matched = 0;

        foreach (var title in db.titles)
        {
            if (title.titleBadge != null)
            {
                matched++;
                continue;
            }

            if (index.TryGetValue(Normalise(title.displayName), out var byName) ||
                index.TryGetValue(Normalise(title.titleId), out byName))
            {
                title.titleBadge = byName;
                matched++;
            }
        }

        return matched;
    }

    private static string Normalise(string value)
        => new string((value ?? "").ToLowerInvariant()
            .Where(char.IsLetterOrDigit).ToArray());

    private static void EnsureResourcesFolder()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");
    }

    private struct Tuning
    {
        public int RequiredReputation;
        public int RequiredPopulation;
        public int RequiredWealth;
        public int RequiredQuality;
        public int MaxArmySize;
        public Sprite MapAvatarIcon;
        public Sprite TitleBadge;
        public string FlavorText;
    }
}
#endif
