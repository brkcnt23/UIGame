using System.Collections.Generic;
using UnityEngine;
using NEXUS.Utilities;

/// <summary>
/// The world keeps turning while the player is elsewhere.
///
/// Runs once per day tick — never in Update(). Fifteen settlements times one
/// pass per day is unmeasurable, so realism costs nothing here.
///
/// Every change it makes emits a SettlementReportEvent line. The player reads
/// those lines by completing a job at that settlement's Town Hall, which is
/// what turns background simulation into something worth travelling for.
/// </summary>
public sealed class WorldSimSystem : GameSystemBase
{
    public override int Priority => SystemPriority.WorldSim;

    public static WorldSimSystem Instance { get; private set; }

    [Header("Named events")]
    [Range(0f, 1f)]
    [Tooltip("Chance per settlement per day that a named event fires.")]
    [SerializeField] private float dailyEventChance = 0.04f;

    [Header("Shop / crafter growth")]
    [Tooltip("Days between growth rolls.")]
    [SerializeField] private int growthCheckIntervalDays = 10;

    [Range(0f, 0.05f)]
    [Tooltip("Chance per shop per roll. 0.005 = 0.5% ≈ one upgrade a month across the world.")]
    [SerializeField] private float shopGrowthChance = 0.005f;

    [Header("Debug")]
    [SerializeField] private bool verbose;

    /// <summary>Report lines per settlement id, oldest first.</summary>
    private readonly Dictionary<int, List<string>> _reports = new();

    [Tooltip("Lines kept per settlement before the oldest are dropped.")]
    [SerializeField] private int maxReportLinesPerSettlement = 200;

    protected override void OnInitialize()
    {
        Instance = this;
        Log($"World simulation active. Event chance {dailyEventChance:P1}/day, " +
            $"growth roll every {growthCheckIntervalDays} days at {shopGrowthChance:P2}.");
    }

    protected override void OnShutdown()
    {
        if (Instance == this)
            Instance = null;
    }

    // -----------------------------------------------------------------
    // Daily pass
    // -----------------------------------------------------------------

    protected override void OnDayTick(int day)
    {
        var settlements = GetSettlements();
        if (settlements == null || settlements.Count == 0)
            return;

        bool growthDay = growthCheckIntervalDays > 0 && day % growthCheckIntervalDays == 0;

        foreach (var settlement in settlements)
        {
            if (settlement == null || settlement.Type == SettlementType.Quest)
                continue;

            DriftEconomy(settlement, day);

            if (Dice.RollD100() <= Mathf.RoundToInt(dailyEventChance * 100f))
                RollNamedEvent(settlement, day);

            if (growthDay)
                RollGrowth(settlement, day);
        }

        if (verbose)
            Log($"Day {day} simulated for {settlements.Count} settlements.");
    }

    /// <summary>
    /// Quiet background movement. Small numbers on purpose — the player should
    /// notice drift over weeks, not overnight.
    /// </summary>
    private void DriftEconomy(Settlement s, int day)
    {
        int popDelta = Dice.Roll(-2, 3);          // -2..+2
        int wealthDelta = Dice.Roll(-30, 61);     // -30..+60, trends up

        // A settlement with a healthy quality score grows a little more easily.
        if (s.Quality > 5)
            popDelta += 1;

        if (popDelta != 0)
            s.Population = Mathf.Max(0, s.Population + popDelta);

        if (wealthDelta != 0)
            s.Wealth.Add(0, wealthDelta);

        // Fields are set directly rather than through Settlement.AddPopulation().
        // Those helpers fire OnPopulationChanged, which SettlementHandler prints
        // verbatim — the source of the repeated "POPULATION: -4 / WEALTH: +250"
        // wall in the old report. Reporting is this system's job now.

        // Drift is deliberately not reported line by line. Only named events
        // and growth are worth the player's reading time; otherwise the report
        // becomes the wall of "POPULATION -4 / WEALTH +250" it used to be.
    }

    private void RollNamedEvent(Settlement s, int day)
    {
        var evt = WorldEventTable.Pick();

        s.Population = Mathf.Max(0, s.Population + evt.PopulationDelta);
        s.Quality = Mathf.Max(0, s.Quality + evt.QualityDelta);
        s.Wealth.Add(0, evt.WealthDelta);

        if (evt.UpgradesACrafter)
            TryUpgradeRandomShop(s, day, silent: true);

        AddReport(s, day, $"{evt.DisplayName} — {evt.Summary}");

        if (verbose)
            Log($"Day {day}: {evt.DisplayName} in {s.Name}.");
    }

    private void RollGrowth(Settlement s, int day)
    {
        if (s.Shops == null)
            return;

        int threshold = Mathf.RoundToInt(shopGrowthChance * 10000f);   // per 10,000

        foreach (var shop in s.Shops)
        {
            if (shop == null || shop.level >= shop.maxLevel)
                continue;

            if (Dice.Roll(10000) <= threshold)
            {
                shop.level++;
                AddReport(s, day, $"A master arrives — {shop.Name} reaches level {shop.level}.");
            }
        }
    }

    private void TryUpgradeRandomShop(Settlement s, int day, bool silent)
    {
        if (s.Shops == null || s.Shops.Count == 0)
            return;

        var candidates = s.Shops.FindAll(sh => sh != null && sh.level < sh.maxLevel);
        if (candidates.Count == 0)
            return;

        var shop = candidates[Dice.Roll(0, candidates.Count)];
        shop.level++;

        if (!silent)
            AddReport(s, day, $"{shop.Name} reaches level {shop.level}.");
    }

    // -----------------------------------------------------------------
    // Reports
    // -----------------------------------------------------------------

    private void AddReport(Settlement s, int day, string line)
    {
        if (!_reports.TryGetValue(s.ID, out var list))
        {
            list = new List<string>();
            _reports[s.ID] = list;
        }

        list.Add($"Day {day} · {line}");

        if (list.Count > maxReportLinesPerSettlement)
            list.RemoveAt(0);

        Events?.Dispatch(new SettlementReportEvent(s.ID, day, line));
    }

    /// <summary>
    /// Everything that happened in a settlement, oldest first.
    /// The Town Hall panel calls this after the player completes a job there.
    /// </summary>
    public IReadOnlyList<string> GetReport(int settlementId)
    {
        return _reports.TryGetValue(settlementId, out var list)
            ? list
            : (IReadOnlyList<string>)System.Array.Empty<string>();
    }

    public void ClearReport(int settlementId)
    {
        _reports.Remove(settlementId);
    }

    private List<Settlement> GetSettlements()
    {
        return SettlementHandler.Instance != null ? SettlementHandler.Instance.settlements : null;
    }
}
