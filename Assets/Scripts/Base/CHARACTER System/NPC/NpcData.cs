using System.Collections.Generic;

[System.Serializable]
public class NpcData
{
    public string NpcId;
    public int SettlementId;

    public string Name;
    public string Surname;

    public List<string> LineageTags = new();     // noble_blooded / non_noble_blooded
    public List<string> RoleTags = new();        // ruler, steward, shopkeeper, tavern_owner, patron
    public List<string> ProfessionTags = new();  // merchant, blacksmith, alchemist, tanner
    public List<string> StatusTags = new();      // wealthy, respected, court_connected
    public List<string> PersonalityTags = new(); // proud, blunt, honest, greedy
    public List<string> BiasTags = new();        // distrusts_nobles, respects_strength

    public string ShopBinding;
    public string TavernBinding;
    public string TownHallBinding;

    public bool CanGiveJobs;
    public bool CanGiveQuests;
    public bool CanBeCompanion;
    public bool CanAppearInEvents;

    public bool HasTag(string tag)
    {
        return LineageTags.Contains(tag) ||
               RoleTags.Contains(tag) ||
               ProfessionTags.Contains(tag) ||
               StatusTags.Contains(tag) ||
               PersonalityTags.Contains(tag) ||
               BiasTags.Contains(tag);
    }

    public List<string> GetAllTags()
    {
        List<string> tags = new();
        tags.AddRange(LineageTags);
        tags.AddRange(RoleTags);
        tags.AddRange(ProfessionTags);
        tags.AddRange(StatusTags);
        tags.AddRange(PersonalityTags);
        tags.AddRange(BiasTags);
        return tags;
    }
}

[System.Serializable]
public class NpcDataWrapper
{
    public List<NpcData> npcs = new();
}