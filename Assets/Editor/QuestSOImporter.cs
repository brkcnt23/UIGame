#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Turns quests.json into QuestSO assets and wires each one to its sketch.
///
/// The catalogue lives in JSON because 49 quests are content, not code — but
/// the game reads ScriptableObjects, because that is what lets a designer drop
/// a different drawing on one quest without touching the data.
///
/// Sketches are matched by the quest's icon family: a quest tagged
/// "livestock" looks for a sprite whose name starts with that, so one goat
/// drawing serves every quest about a wandering animal.
///
/// Tools > UIGame > Quests > Generate quest assets
/// </summary>
public static class QuestSOImporter
{
    private const string JsonPath = "Assets/SourceData/quests.json";
    private const string OutputFolder = "Assets/Resources/Quests";
    private const string DatabasePath = "Assets/Resources/QuestDatabase.asset";
    private const string SketchFolder = "Assets/UI Elements/Quests/Icons";
    private const string PaperFolder = "Assets/UI Elements/Quests";

    /// <summary>
    /// Icon family to the file the artist actually drew.
    ///
    /// The catalogue names families by what they mean — "livestock" covers
    /// every quest about a wandering animal — while the drawing is named after
    /// what is in it. Mapping the two here means neither side has to be
    /// renamed to suit the other, and a family can be repointed at a better
    /// drawing later without touching the quest data.
    /// </summary>
    private static readonly Dictionary<string, string[]> IconAliases = new()
    {
        { "livestock", new[] { "goat", "cow", "cattle" } },
        { "vermin",    new[] { "rat", "crow" } },
        { "predator",  new[] { "wolf", "boar" } },
        { "herbs",     new[] { "herbs" } },
        { "parcel",    new[] { "crate", "parcel" } },
        { "shield",    new[] { "shield" } },
        { "letter",    new[] { "letter", "scroll" } },
        { "pick",      new[] { "pickaxelantern", "pickaxe", "pick" } },
        { "timber",    new[] { "axelog", "axe", "log" } },
        { "bandit",    new[] { "hoodedbandit", "bandit", "hood" } },

        // Not drawn yet — listed so the importer reports them by family name.
        { "missing",   new[] { "boot", "footprint", "shoe", "missing" } },
        { "ledger",    new[] { "ledger", "book" } },
        { "tools",     new[] { "tools", "hammer", "tongs" } },
        { "cart",      new[] { "cart", "wagon", "wheel" } },
        { "banner",    new[] { "banner", "heraldry" } },
        { "crown",     new[] { "crownquesticon", "crown" } },
        { "harvest",   new[] { "harvest", "wheat", "sickle" } },
    };

    [System.Serializable] private class Wrapper { public List<Entry> quests = new(); }

    [System.Serializable]
    private class Entry
    {
        public int ID;
        public string Name;
        public string Description;
        public string Tier;
        public string Realm;
        public string Icon;
        public int DC;
        public int CompletionDay;
        public int CompletionHour;
        public int Experience;
        public int Gold;
        public int Silver;
        public string TargetStat;
        public int StatRewardMin;
        public int StatRewardMax;
        public int MinPlayerLevel;
        public int hoursToComplete;
    }

    [MenuItem("Tools/UIGame/Quests/Generate quest assets")]
    public static void Generate()
    {
        if (!File.Exists(JsonPath))
        {
            Debug.LogError($"[QuestImporter] Not found: {JsonPath}");
            return;
        }

        EnsureFolder(OutputFolder);

        var data = JsonUtility.FromJson<Wrapper>(File.ReadAllText(JsonPath));
        if (data?.quests == null || data.quests.Count == 0)
        {
            Debug.LogError("[QuestImporter] No quests parsed.");
            return;
        }

        var sketches = LoadSprites(SketchFolder);
        var assets = new List<QuestSO>();

        int created = 0, updated = 0;
        var missingArt = new List<string>();

        foreach (var e in data.quests)
        {
            string path = $"{OutputFolder}/{Sanitize(e.Name)}.asset";
            var so = AssetDatabase.LoadAssetAtPath<QuestSO>(path);

            if (so == null)
            {
                so = ScriptableObject.CreateInstance<QuestSO>();
                AssetDatabase.CreateAsset(so, path);
                created++;
            }
            else updated++;

            so.questId = e.ID;
            so.questName = e.Name;
            so.description = e.Description;
            so.tier = ParseTier(e.Tier);
            so.realm = ParseRealm(e.Realm);
            so.minPlayerLevel = Mathf.Max(1, e.MinPlayerLevel);
            so.rewardGold = e.Gold;
            so.rewardSilver = e.Silver;
            so.rewardExperience = e.Experience;
            so.targetStat = ParseStat(e.TargetStat);
            so.statRewardMin = e.StatRewardMin;
            so.statRewardMax = e.StatRewardMax;
            so.hoursToComplete = e.CompletionDay * 24 + e.CompletionHour;
            so.hoursBeforeExpiry = Mathf.Max(24, e.hoursToComplete);
            so.difficultyClass = e.DC;

            // Royal work stays sealed until the player carries a title.
            so.requiredTitleId = so.tier == QuestTier.Royal ? "bailiff" : "";

            var sketch = Resolve(sketches, e.Icon);
            if (sketch != null) so.sketch = sketch;
            else missingArt.Add($"{e.Name}  (icon: {e.Icon})");

            EditorUtility.SetDirty(so);
            assets.Add(so);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        RebuildDatabase(assets);

        Debug.Log($"[QuestImporter] {created} created, {updated} updated, " +
                  $"{missingArt.Count} without a sketch.");

        if (missingArt.Count > 0)
        {
            var families = missingArt
                .Select(m => m.Substring(m.IndexOf("icon: ") + 6).TrimEnd(')'))
                .Distinct()
                .OrderBy(x => x);

            Debug.LogWarning($"[QuestImporter] Missing sketch families:\n  " +
                             string.Join("\n  ", families) +
                             $"\n\nPut them in {SketchFolder}, named after the family.");
        }
    }

    // -----------------------------------------------------------------

    private static void RebuildDatabase(List<QuestSO> assets)
    {
        var db = AssetDatabase.LoadAssetAtPath<QuestDatabaseSO>(DatabasePath);

        if (db == null)
        {
            db = ScriptableObject.CreateInstance<QuestDatabaseSO>();
            AssetDatabase.CreateAsset(db, DatabasePath);
        }

        db.quests = assets.OrderBy(a => a.questId).ToList();

        // Papers and coins are matched by name; anything the artist adds later
        // is picked up on the next run.
        var art = LoadSprites(PaperFolder);

        db.errandPapers     = Collect(art, "papirus", "paper");
        db.contractPapers   = Collect(art, "paper1", "papirus1");
        db.commissionPapers = Collect(art, "papirus2");
        db.charterPapers    = Collect(art, "papirus3");

        if (db.errandPapers.Count == 0) db.errandPapers = art.Values.Take(2).ToList();
        if (db.contractPapers.Count == 0) db.contractPapers = db.errandPapers;
        if (db.commissionPapers.Count == 0) db.commissionPapers = db.contractPapers;
        if (db.charterPapers.Count == 0) db.charterPapers = db.commissionPapers;

        db.goldCoin   = Resolve(art, "goldicon");
        db.silverCoin = Resolve(art, "silvericon");
        db.waxSeal    = Resolve(art, "muhur");
        db.royalSeal  = Resolve(art, "goldmuhurkurdele") ?? Resolve(art, "goldmuhur");
        db.royalFrame = Resolve(art, "royalquestframe");

        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();

        Debug.Log($"[QuestImporter] Database rebuilt: {db.quests.Count} quests, " +
                  $"papers {db.errandPapers.Count}/{db.contractPapers.Count}/" +
                  $"{db.commissionPapers.Count}/{db.charterPapers.Count}, " +
                  $"coins {(db.goldCoin != null ? "gold" : "—")}/" +
                  $"{(db.silverCoin != null ? "silver" : "—")}, " +
                  $"seal {(db.waxSeal != null ? "yes" : "no")}");
    }

    private static List<Sprite> Collect(Dictionary<string, Sprite> art, params string[] prefixes)
    {
        var result = new List<Sprite>();

        foreach (var prefix in prefixes)
        {
            string key = Normalize(prefix);

            foreach (var kv in art)
                if (kv.Key == key && !result.Contains(kv.Value))
                    result.Add(kv.Value);
        }

        return result;
    }

    private static Dictionary<string, Sprite> LoadSprites(string folder)
    {
        var map = new Dictionary<string, Sprite>();
        if (!AssetDatabase.IsValidFolder(folder)) return map;

        foreach (var guid in AssetDatabase.FindAssets("t:Sprite", new[] { folder }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null) continue;

            string key = Normalize(Path.GetFileNameWithoutExtension(path));
            if (!map.ContainsKey(key)) map[key] = sprite;
        }

        return map;
    }

    private static Sprite Resolve(Dictionary<string, Sprite> map, string name)
    {
        if (string.IsNullOrEmpty(name)) return null;

        string key = Normalize(name);

        // The family's own name, in case someone named the file after it.
        if (map.TryGetValue(key, out var exact)) return exact;

        // Then whatever the artist actually called the drawing.
        if (IconAliases.TryGetValue(key, out var aliases))
        {
            foreach (var alias in aliases)
            {
                string aliasKey = Normalize(alias);

                if (map.TryGetValue(aliasKey, out var hit)) return hit;

                var loose = map.FirstOrDefault(kv => kv.Key.StartsWith(aliasKey));
                if (loose.Value != null) return loose.Value;
            }
        }

        var partial = map.FirstOrDefault(kv => kv.Key.StartsWith(key));
        return partial.Value;
    }

    private static QuestTier ParseTier(string s) => s switch
    {
        "Contract"   => QuestTier.Contract,
        "Commission" => QuestTier.Commission,
        "Charter"    => QuestTier.Charter,
        "Royal"      => QuestTier.Royal,
        _            => QuestTier.Errand
    };

    private static QuestRealm ParseRealm(string s) => s switch
    {
        "Karnhold" => QuestRealm.Karnhold,
        "Averlyn"  => QuestRealm.Averlyn,
        "Sahenmar" => QuestRealm.Sahenmar,
        _          => QuestRealm.Any
    };

    private static StatType ParseStat(string s) => s switch
    {
        "Dexterity"    => StatType.Dexterity,
        "Constitution" => StatType.Constitution,
        "Charisma"     => StatType.Charisma,
        _              => StatType.Strength
    };

    private static string Normalize(string s)
        => new string(s.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());

    private static string Sanitize(string s)
    {
        foreach (char c in Path.GetInvalidFileNameChars()) s = s.Replace(c, '_');
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
