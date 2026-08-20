using System.Collections.Generic;
using UnityEngine;
using NEXUS.Utilities;

/// <summary>
/// Every quest in the game, plus the papers and coins the notes are drawn
/// from.
///
/// The board asks this for a fresh set of offers; it decides what is
/// appropriate for the settlement and rolls a spread of tiers so a board is
/// never all errands or all charters.
/// </summary>
[CreateAssetMenu(fileName = "QuestDatabase", menuName = "UIGame/Quest Database")]
public class QuestDatabaseSO : ScriptableObject
{
    [Header("Quests")]
    public List<QuestSO> quests = new();

    [Header("Paper by tier")]
    [Tooltip("Coarse, torn sheets. Errands.")]
    public List<Sprite> errandPapers = new();

    [Tooltip("Plain intact sheets. Contracts.")]
    public List<Sprite> contractPapers = new();

    [Tooltip("Good parchment. Commissions.")]
    public List<Sprite> commissionPapers = new();

    [Tooltip("Formal vellum. Charters and royal work.")]
    public List<Sprite> charterPapers = new();

    [Header("Decoration")]
    public Sprite goldCoin;
    public Sprite silverCoin;

    [Tooltip("Plain wax seal, drawn on Commission and Charter.")]
    public Sprite waxSeal;

    [Tooltip("Gold seal with a ribbon. Royal work only — it should look like " +
             "nothing else on the board.")]
    public Sprite royalSeal;

    [Tooltip("Ornate frame drawn around a Royal note.")]
    public Sprite royalFrame;

    // ---------------------------------------------------------------

    /// <summary>
    /// How likely each tier is to appear. Errands are the bread of the board;
    /// a charter should feel like a good day.
    /// </summary>
    private static readonly (QuestTier tier, int weight)[] TierWeights =
    {
        (QuestTier.Errand,     40),
        (QuestTier.Contract,   32),
        (QuestTier.Commission, 20),
        (QuestTier.Charter,     8),
    };

    /// <summary>
    /// Rolls the notes for one board.
    ///
    /// Quests already offered elsewhere are not excluded — the same lost goat
    /// being posted in two villages is a real thing and costs nothing. What is
    /// excluded is the same quest twice on one board.
    /// </summary>
    public List<QuestSO> RollBoard(Settlement settlement, PlayerData player, int count = 6)
    {
        var result = new List<QuestSO>();
        var pool = Available(settlement, player);

        if (pool.Count == 0) return result;

        var used = new HashSet<int>();

        for (int i = 0; i < count; i++)
        {
            var tier = RollTier();
            var candidates = pool.FindAll(q => q.tier == tier && !used.Contains(q.questId));

            // Nothing of that rank left — fall back to anything unused rather
            // than leaving a nail empty.
            if (candidates.Count == 0)
                candidates = pool.FindAll(q => !used.Contains(q.questId));

            if (candidates.Count == 0) break;

            var pick = candidates[Dice.Index(candidates.Count)];
            used.Add(pick.questId);
            result.Add(pick);
        }

        return result;
    }

    /// <summary>
    /// Quests this settlement could plausibly post. Royal work is capital-only
    /// — that is what makes reaching a capital mean something.
    /// </summary>
    public List<QuestSO> Available(Settlement settlement, PlayerData player)
    {
        var result = new List<QuestSO>();
        var realm = RealmOf(settlement);
        bool isCapital = settlement != null && settlement.Type == SettlementType.defaultSettlement;

        foreach (var q in quests)
        {
            if (q == null) continue;

            if (q.realm != QuestRealm.Any && q.realm != realm) continue;
            if (q.tier == QuestTier.Royal && !IsCapital(settlement)) continue;

            // Work far above the player is hidden rather than shown greyed —
            // except Royal, which is deliberately visible as a target.
            if (q.tier != QuestTier.Royal && player != null
                && player.Level + 6 < q.minPlayerLevel) continue;

            result.Add(q);
        }

        return result;
    }

    private static QuestTier RollTier()
    {
        int total = 0;
        foreach (var (_, w) in TierWeights) total += w;

        int roll = Dice.Roll(1, total + 1);
        int cursor = 0;

        foreach (var (tier, weight) in TierWeights)
        {
            cursor += weight;
            if (roll <= cursor) return tier;
        }

        return QuestTier.Errand;
    }

    private static QuestRealm RealmOf(Settlement settlement)
    {
        if (settlement?.CultureTags == null) return QuestRealm.Any;

        foreach (var tag in settlement.CultureTags)
        {
            if (tag == "karnhold") return QuestRealm.Karnhold;
            if (tag == "averlyn")  return QuestRealm.Averlyn;
            if (tag == "sahenmar") return QuestRealm.Sahenmar;
        }

        return QuestRealm.Any;
    }

    private static bool IsCapital(Settlement settlement)
    {
        // Capitals are the three cities; the tier enum is the cheapest signal
        // available until Settlement carries the flag at runtime.
        return settlement != null && settlement.Population >= 9000;
    }

    // ---------------------------------------------------------------

    public Sprite GetPaper(QuestTier tier)
    {
        var pool = tier switch
        {
            QuestTier.Errand     => errandPapers,
            QuestTier.Contract   => contractPapers,
            QuestTier.Commission => commissionPapers,
            _                    => charterPapers
        };

        if (pool == null || pool.Count == 0) return null;
        return pool[Dice.Index(pool.Count)];
    }

    public Sprite GetCoin(QuestSO quest)
        => quest != null && quest.ShowsGoldCoin ? goldCoin : silverCoin;

    public bool ShowsSeal(QuestTier tier)
        => tier >= QuestTier.Commission;

    /// <summary>
    /// Royal work carries a gold ribboned seal; everything else below it gets
    /// the plain wax. The difference should be visible from across the board.
    /// </summary>
    public Sprite GetSeal(QuestTier tier)
    {
        if (tier == QuestTier.Royal && royalSeal != null) return royalSeal;
        return tier >= QuestTier.Commission ? waxSeal : null;
    }

    public Sprite GetFrame(QuestTier tier)
        => tier == QuestTier.Royal ? royalFrame : null;

    public QuestSO Get(int questId)
        => quests.Find(q => q != null && q.questId == questId);
}
