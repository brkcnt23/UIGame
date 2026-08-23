using System.Collections.Generic;
using System.Linq;

/// <summary>
/// The twenty-six rung title ladder, as designed in GAME_DESIGN_DECISIONS 5.3.
///
/// Kept in code for the same reason ItemCatalog and TraitCatalog are: the data
/// is the design, and an importer turns it into assets. TitleDatabaseSO knew the
/// shape of a title but never held a single one, so nothing could name a rank.
///
/// Two tracks run in parallel — Administrative advances on Standing, Martial on
/// Renown — and neither reaches the next milestone alone. A milestone needs six
/// titles behind it with at least two from each track, which is what stops a
/// player walking one column to the top.
/// </summary>
public static class TitleLadder
{
    private static List<TitleRung> _all;

    public static List<TitleRung> All => _all ??= Build();

    /// <summary>The four shared thresholds, in order. These are the ranks people say aloud.</summary>
    public static List<TitleRung> Milestones =>
        All.Where(t => t.Track == TitleTrack.Milestone)
           .OrderBy(t => t.Segment)
           .ToList();

    public static TitleRung ById(string id) =>
        All.FirstOrDefault(t => t.Id == id);

    private static TitleRung A(string id, string name, TitleTrack track, int rank, int segment)
        => new TitleRung
        {
            Id = id,
            DisplayName = name,
            Track = track,
            RankInTrack = rank,
            Segment = segment,
            Tier = SettlementTier.None,
            CompanionSlots = 0
        };

    private static TitleRung M(string id, string name, int segment, SettlementTier tier, int slots)
        => new TitleRung
        {
            Id = id,
            DisplayName = name,
            Track = TitleTrack.Milestone,
            RankInTrack = 0,
            Segment = segment,
            Tier = tier,
            CompanionSlots = slots
        };

    private static List<TitleRung> Build() => new List<TitleRung>
    {
        // --- to Reeve ---------------------------------------------------
        A("freeman",          "Freeman",          TitleTrack.Administrative, 1, 0),
        A("tithingman",       "Tithingman",       TitleTrack.Administrative, 2, 0),
        A("footman",          "Footman",          TitleTrack.Martial,        1, 0),
        A("man_at_arms",      "Man-at-Arms",      TitleTrack.Martial,        2, 0),
        M("reeve",            "Reeve",            0, SettlementTier.Hamlet,  1),

        // --- to Bailiff -------------------------------------------------
        A("hayward",          "Hayward",          TitleTrack.Administrative, 3, 1),
        A("beadle",           "Beadle",           TitleTrack.Administrative, 4, 1),
        A("constable",        "Constable",        TitleTrack.Administrative, 5, 1),
        A("veteran",          "Veteran",          TitleTrack.Martial,        3, 1),
        A("sergeant",         "Sergeant",         TitleTrack.Martial,        4, 1),
        A("squire",           "Squire",           TitleTrack.Martial,        5, 1),
        M("bailiff",          "Bailiff",          1, SettlementTier.Village, 2),

        // --- to Baron ---------------------------------------------------
        A("warden",           "Warden",           TitleTrack.Administrative, 6, 2),
        A("provost",          "Provost",          TitleTrack.Administrative, 7, 2),
        A("chamberlain",      "Chamberlain",      TitleTrack.Administrative, 8, 2),
        A("bannerman",        "Bannerman",        TitleTrack.Martial,        6, 2),
        A("household_knight", "Household Knight", TitleTrack.Martial,        7, 2),
        A("knight",           "Knight",           TitleTrack.Martial,        8, 2),
        M("baron",            "Baron",            2, SettlementTier.Town,    3),

        // --- to Duke ----------------------------------------------------
        A("steward",          "Steward",          TitleTrack.Administrative,  9, 3),
        A("seneschal",        "Seneschal",        TitleTrack.Administrative, 10, 3),
        A("justiciar",        "Justiciar",        TitleTrack.Administrative, 11, 3),
        A("knight_banneret",  "Knight Banneret",  TitleTrack.Martial,         9, 3),
        A("castellan",        "Castellan",        TitleTrack.Martial,        10, 3),
        A("marshal",          "Marshal",          TitleTrack.Martial,        11, 3),
        M("duke",             "Duke",             3, SettlementTier.City,    4),
    };
}

/// <summary>
/// One rung. Deliberately lighter than TitleDefinition, which carries sprites
/// and quest ids that only make sense once a rung has an asset behind it.
/// </summary>
public class TitleRung
{
    public string Id;
    public string DisplayName;
    public TitleTrack Track;

    /// <summary>Position inside its own track. Milestones sit outside both.</summary>
    public int RankInTrack;

    /// <summary>Which of the four stretches this rung belongs to.</summary>
    public int Segment;

    /// <summary>How large the seat's settlement is. Only milestones hold one.</summary>
    public SettlementTier Tier;

    public int CompanionSlots;

    /// <summary>
    /// "Rufus, Baron of Dunmoor". Written out rather than assembled at each call
    /// site so every screen says a name the same way.
    /// </summary>
    public string Styled(string playerName, string seatName)
    {
        string who = string.IsNullOrWhiteSpace(playerName) ? "You" : playerName.Trim();

        if (string.IsNullOrWhiteSpace(seatName))
            return who + ", " + DisplayName;

        return who + ", " + DisplayName + " of " + seatName.Trim();
    }
}
