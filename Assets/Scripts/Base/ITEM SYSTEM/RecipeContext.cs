using System.Collections.Generic;

[System.Serializable]
public class RecipeContext
{
    public PlayerData Player;
    public Settlement Settlement;
    public NpcData Npc;

    public List<string> ExtraPlayerTags = new List<string>();
    public List<string> ExtraSettlementTags = new List<string>();
    public List<string> ExtraNpcTags = new List<string>();

    public List<string> GetPlayerTags()
    {
        List<string> tags = new List<string>();

        if (Player != null)
        {
            if (Player.HistoryTags != null) tags.AddRange(Player.HistoryTags);
            if (Player.ActiveTraitTags != null) tags.AddRange(Player.ActiveTraitTags);
            if (Player.LearnedStations != null) tags.AddRange(Player.LearnedStations);
            if (Player.LearnedTools != null) tags.AddRange(Player.LearnedTools);
        }

        if (ExtraPlayerTags != null)
            tags.AddRange(ExtraPlayerTags);

        return tags;
    }

    public List<string> GetSettlementTags()
    {
        List<string> tags = new List<string>();

        if (Settlement != null && Settlement.SettlementTags != null)
            tags.AddRange(Settlement.SettlementTags);

        if (ExtraSettlementTags != null)
            tags.AddRange(ExtraSettlementTags);

        return tags;
    }

    public List<string> GetNpcTags()
    {
        List<string> tags = new List<string>();

        if (Npc != null)
            tags.AddRange(Npc.GetAllTags());

        if (ExtraNpcTags != null)
            tags.AddRange(ExtraNpcTags);

        return tags;
    }
}