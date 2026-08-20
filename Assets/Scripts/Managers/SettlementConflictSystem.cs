using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Wars, blockades and the way one town's misfortune reaches its neighbours.
///
/// Three rules keep this from wrecking the game:
///
/// 1. Wars are rare and finite. A conflict lasts weeks, not forever, and only
///    one can run at a time. The world should feel alive, not chaotic.
///
/// 2. Damage is capped as a fraction, never a flat number. A war can cost a
///    settlement a quarter of its treasury; it can never bankrupt it. A dead
///    economy is a dead shop, and a dead shop is a player standing in an empty
///    room wondering what happened.
///
/// 3. Everything recovers. Each day a settlement drifts back toward the wealth
///    its population and tier support. A bad year is survivable; a permanent
///    scar is not, because the player may only arrive afterwards.
///
/// The spillover is the interesting part: when a trade hub is hit, the towns
/// that trade with it lose a little too. That is how the player learns the map
/// has a shape — prices move together, and a war two towns away is felt at
/// home.
/// </summary>
public sealed class SettlementConflictSystem : GameSystemBase
{
    public override int Priority => SystemPriority.WorldSim + 5;

    public static SettlementConflictSystem Instance { get; private set; }

    [Header("Frequency")]
    [Tooltip("Daily chance a new conflict starts, when none is running.")]
    [Range(0f, 0.05f)]
    [SerializeField] private float dailyWarChance = 0.004f;   // ~1 war every 8 months

    [SerializeField] private int minWarDays = 12;
    [SerializeField] private int maxWarDays = 40;

    [Header("Severity")]
    [Tooltip("Fraction of treasury a belligerent loses per day.")]
    [Range(0f, 0.05f)]
    [SerializeField] private float dailyWealthDrain = 0.012f;

    [Tooltip("Fraction of population lost per day.")]
    [Range(0f, 0.02f)]
    [SerializeField] private float dailyPopulationDrain = 0.003f;

    [Tooltip("Neighbours feel this share of the belligerents' loss.")]
    [Range(0f, 1f)]
    [SerializeField] private float spilloverShare = 0.35f;

    [Header("Recovery")]
    [Tooltip("Fraction of the gap to its natural level a settlement closes per day.")]
    [Range(0f, 0.2f)]
    [SerializeField] private float dailyRecovery = 0.02f;

    [Tooltip("A settlement can never fall below this share of its natural wealth.")]
    [Range(0f, 1f)]
    [SerializeField] private float wealthFloor = 0.25f;

    [SerializeField] private bool verbose;

    // -----------------------------------------------------------------

    [System.Serializable]
    public class Conflict
    {
        public int SettlementA;
        public int SettlementB;
        public int StartedDay;
        public int EndsDay;
        public string Cause;
    }

    private Conflict _active;

    /// <summary>Natural wealth per settlement, learned on the first tick.</summary>
    private readonly Dictionary<int, int> _baseline = new();

    public Conflict Active => _active;
    public bool IsAtWar(int settlementId)
        => _active != null && (_active.SettlementA == settlementId || _active.SettlementB == settlementId);

    protected override void OnInitialize()
    {
        Instance = this;
        Log("Conflict simulation ready.");
    }

    protected override void OnShutdown()
    {
        if (Instance == this) Instance = null;
    }

    // -----------------------------------------------------------------

    protected override void OnDayTick(int day)
    {
        var settlements = GetSettlements();
        if (settlements == null || settlements.Count < 2) return;

        CaptureBaselines(settlements);

        if (_active != null)
        {
            if (day >= _active.EndsDay) EndWar(day, settlements);
            else ApplyWarDamage(day, settlements);
        }
        else if (Random.value < dailyWarChance)
        {
            StartWar(day, settlements);
        }

        Recover(settlements);
    }

    /// <summary>
    /// The wealth a settlement returns to. Taken from its starting value the
    /// first time it is seen, so designer numbers stay the reference rather
    /// than whatever the simulation has drifted to.
    /// </summary>
    private void CaptureBaselines(List<Settlement> settlements)
    {
        foreach (var s in settlements)
        {
            if (s == null || _baseline.ContainsKey(s.ID)) continue;
            _baseline[s.ID] = Mathf.Max(1, s.Wealth.Gold);
        }
    }

    // -----------------------------------------------------------------

    private void StartWar(int day, List<Settlement> settlements)
    {
        var candidates = settlements.FindAll(s =>
            s != null && s.Type != SettlementType.Quest && s.Population > 500);

        if (candidates.Count < 2) return;

        var a = candidates[Random.Range(0, candidates.Count)];
        var b = candidates[Random.Range(0, candidates.Count)];

        // Two different places, and not the player's home — losing your own
        // village to a war you had no part in is a bad surprise, not a story.
        int guard = 0;
        while ((b == a || IsPlayerHome(a) || IsPlayerHome(b)) && guard++ < 20)
        {
            a = candidates[Random.Range(0, candidates.Count)];
            b = candidates[Random.Range(0, candidates.Count)];
        }

        if (b == a || IsPlayerHome(a) || IsPlayerHome(b)) return;

        _active = new Conflict
        {
            SettlementA = a.ID,
            SettlementB = b.ID,
            StartedDay = day,
            EndsDay = day + Random.Range(minWarDays, maxWarDays + 1),
            Cause = PickCause()
        };

        Report(a, day, $"War with {b.Name} — {_active.Cause}.");
        Report(b, day, $"War with {a.Name} — {_active.Cause}.");

        Log($"Day {day}: {a.Name} and {b.Name} are at war. {_active.Cause}");
    }

    private void ApplyWarDamage(int day, List<Settlement> settlements)
    {
        var a = settlements.Find(s => s != null && s.ID == _active.SettlementA);
        var b = settlements.Find(s => s != null && s.ID == _active.SettlementB);

        int drainedA = Drain(a);
        int drainedB = Drain(b);

        // Neighbours trading with a belligerent feel part of the loss. This is
        // what makes the map read as connected rather than as thirteen
        // independent economies.
        int total = drainedA + drainedB;
        if (total > 0) Spill(settlements, total, a, b);
    }

    private int Drain(Settlement s)
    {
        if (s == null) return 0;

        int floor = FloorFor(s);
        int loss = Mathf.RoundToInt(s.Wealth.Gold * dailyWealthDrain);
        loss = Mathf.Min(loss, Mathf.Max(0, s.Wealth.Gold - floor));

        if (loss > 0) s.Wealth.Subtract(loss, 0);

        int popLoss = Mathf.RoundToInt(s.Population * dailyPopulationDrain);
        s.Population = Mathf.Max(50, s.Population - popLoss);

        return loss;
    }

    private void Spill(List<Settlement> settlements, int totalLoss, Settlement a, Settlement b)
    {
        var neighbours = settlements.FindAll(s =>
            s != null && s != a && s != b && s.Type != SettlementType.Quest);

        if (neighbours.Count == 0) return;

        // Trade hubs are exposed to everyone's trouble; remote places barely
        // notice a war they have no commerce with.
        foreach (var n in neighbours)
        {
            float exposure = 1f;
            if (n.SettlementTags != null)
            {
                if (n.SettlementTags.Contains("trade_hub")) exposure = 1.6f;
                else if (n.SettlementTags.Contains("remote")) exposure = 0.4f;
            }

            int share = Mathf.RoundToInt(totalLoss * spilloverShare * exposure / neighbours.Count);
            if (share <= 0) continue;

            int floor = FloorFor(n);
            share = Mathf.Min(share, Mathf.Max(0, n.Wealth.Gold - floor));

            if (share > 0) n.Wealth.Subtract(share, 0);
        }
    }

    private void EndWar(int day, List<Settlement> settlements)
    {
        var a = settlements.Find(s => s != null && s.ID == _active.SettlementA);
        var b = settlements.Find(s => s != null && s.ID == _active.SettlementB);

        string ending = PickEnding();

        if (a != null) Report(a, day, $"The war ends — {ending}.");
        if (b != null) Report(b, day, $"The war ends — {ending}.");

        Log($"Day {day}: the war between {a?.Name} and {b?.Name} is over. {ending}");

        _active = null;
    }

    // -----------------------------------------------------------------

    /// <summary>
    /// Every settlement drifts back toward its natural wealth. Recovery is
    /// proportional to the gap, so a badly hurt town recovers quickly at
    /// first and then levels off — and one that got rich on a windfall settles
    /// back down the same way.
    /// </summary>
    private void Recover(List<Settlement> settlements)
    {
        foreach (var s in settlements)
        {
            if (s == null || s.Type == SettlementType.Quest) continue;
            if (IsAtWar(s.ID)) continue;
            if (!_baseline.TryGetValue(s.ID, out int natural)) continue;

            int gap = natural - s.Wealth.Gold;
            if (Mathf.Abs(gap) < 2) continue;

            int step = Mathf.RoundToInt(gap * dailyRecovery);
            if (step == 0) step = gap > 0 ? 1 : -1;

            if (step > 0) s.Wealth.Add(step, 0);
            else s.Wealth.Subtract(-step, 0);
        }
    }

    private int FloorFor(Settlement s)
    {
        int natural = _baseline.TryGetValue(s.ID, out int b) ? b : Mathf.Max(1, s.Wealth.Gold);
        return Mathf.RoundToInt(natural * wealthFloor);
    }

    private static bool IsPlayerHome(Settlement s)
    {
        var home = HomeSettlementHandler.Instance != null
            ? HomeSettlementHandler.Instance.homeSettlement
            : null;

        return home != null && s != null && home.ID == s.ID;
    }

    private static string PickCause()
    {
        string[] causes =
        {
            "a dispute over the road tolls",
            "a border stone moved in the night",
            "an insult at a wedding nobody has forgotten",
            "a caravan seized and never returned",
            "two lords who inherited the same claim",
            "a mill burned and blamed on the wrong men",
        };
        return causes[Random.Range(0, causes.Length)];
    }

    private static string PickEnding()
    {
        string[] endings =
        {
            "both sides too poor to continue",
            "a truce brokered by a third party",
            "a marriage arranged in haste",
            "winter arrived and settled it",
            "one side simply stopped coming",
        };
        return endings[Random.Range(0, endings.Length)];
    }

    private void Report(Settlement s, int day, string line)
    {
        if (s == null) return;
        Events?.Dispatch(new SettlementReportEvent(s.ID, day, line));
    }

    private static List<Settlement> GetSettlements()
        => SettlementHandler.Instance != null ? SettlementHandler.Instance.settlements : null;
}
