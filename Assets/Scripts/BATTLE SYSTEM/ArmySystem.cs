using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// The army as a running cost.
///
/// Hiring is the easy part. Every morning the whole company wants paying, and
/// that is what makes an army a decision rather than a purchase: a knight is
/// worth twenty levies in a fight and costs twenty levies on a quiet Tuesday.
/// A player who buys cavalry the day before a long journey should feel it.
///
/// When the purse cannot cover the wage bill, men leave. The expensive ones go
/// first - a knight has somewhere else to be, a levy does not - which means a
/// broke player loses exactly the troops they were most proud of. That is the
/// intended sting, and it is also the honest reading: the men with options use
/// them.
/// </summary>
public sealed class ArmySystem : GameSystemBase
{
    public override int Priority => SystemPriority.Battle;

    public static ArmySystem Instance { get; private set; }

    /// <summary>Raised when men are hired, promoted, or walk out.</summary>
    public System.Action OnArmyChanged;

    /// <summary>Raised with how many deserted, so the UI can say so loudly.</summary>
    public System.Action<int> OnDesertion;

    protected override void OnInitialize() => Instance = this;

    protected override void OnShutdown()
    {
        if (Instance == this) Instance = null;
    }

    // -----------------------------------------------------------------

    private static PlayerData Player => PlayerStatHandler.Instance != null
        ? PlayerStatHandler.Instance.pd
        : null;

    private static List<Unit> Units => Player?.PlayerArmy?.Units;

    /// <summary>Silver the company costs every day.</summary>
    public int DailyUpkeepSilver => UnitCatalog.DailyUpkeep(Units);

    public int TotalSoldiers => Units?.Sum(u => u?.Count ?? 0) ?? 0;

    /// <summary>Days the current purse can cover at the current wage bill.</summary>
    public int DaysAffordable
    {
        get
        {
            int wage = DailyUpkeepSilver;
            if (wage <= 0) return int.MaxValue;

            var pd = Player;
            if (pd == null) return 0;

            int purse = pd.Money.Gold * 100 + pd.Money.Silver;
            return purse / wage;
        }
    }

    // -----------------------------------------------------------------
    // Wages
    // -----------------------------------------------------------------

    protected override void OnDayTick(int day)
    {
        int wage = DailyUpkeepSilver;
        if (wage <= 0) return;

        var pd = Player;
        if (pd == null) return;

        int gold = wage / 100;
        int silver = wage % 100;

        if (PlayerStatHandler.Instance.ConsumeMoney(gold, silver))
        {
            Log($"Paid {gold}g {silver}s in wages to {TotalSoldiers} soldiers.");
            return;
        }

        int lost = Desert(wage);

        LogWarning($"Could not pay {gold}g {silver}s in wages. {lost} soldiers left.");
        OnDesertion?.Invoke(lost);
        OnArmyChanged?.Invoke();
    }

    /// <summary>
    /// Sheds men until the remaining wage bill is affordable, most expensive
    /// first. Losing one knight settles the books faster than losing a crowd of
    /// levies, which is both kinder to the player's numbers and truer to why
    /// people walk.
    /// </summary>
    private int Desert(int wage)
    {
        var units = Units;
        if (units == null) return 0;

        var pd = Player;
        int purse = pd.Money.Gold * 100 + pd.Money.Silver;
        int lost = 0;

        var byCost = units
            .Where(u => u != null && u.Count > 0)
            .OrderByDescending(u => UnitCatalog.Get(u.Type)?.UpkeepSilverPerDay ?? 0)
            .ToList();

        foreach (var unit in byCost)
        {
            var def = UnitCatalog.Get(unit.Type);
            if (def == null || def.UpkeepSilverPerDay <= 0) continue;

            while (unit.Count > 0 && wage > purse)
            {
                unit.Count--;
                wage -= def.UpkeepSilverPerDay;
                lost++;
            }

            if (wage <= purse) break;
        }

        units.RemoveAll(u => u == null || u.Count <= 0);

        return lost;
    }

    // -----------------------------------------------------------------
    // Hiring
    // -----------------------------------------------------------------

    /// <summary>What this settlement will hire out today.</summary>
    public List<UnitDef> AvailableAt(Settlement settlement) => UnitCatalog.AvailableAt(settlement);

    /// <summary>
    /// Hires men, if the town has them and the purse covers it. Refuses with a
    /// reason rather than silently doing nothing - a recruit button that does
    /// nothing is indistinguishable from a broken one.
    /// </summary>
    public bool Recruit(UnitType type, int count, Settlement where, out string reason)
    {
        reason = "";

        if (count <= 0) { reason = "Nothing to hire."; return false; }

        var def = UnitCatalog.Get(type);
        if (def == null) { reason = "No such soldier."; return false; }

        if (!def.IsOfferedAt(where))
        {
            reason = $"No {def.DisplayName} to be had here.";
            return false;
        }

        var pd = Player;
        if (pd == null) { reason = "No character."; return false; }

        int cost = def.HireGold * count;

        if (!pd.HasEnoughMoney(cost, 0))
        {
            reason = $"{cost} gold needed.";
            return false;
        }

        PlayerStatHandler.Instance.ConsumeMoney(cost, 0);
        Add(type, count);

        Log($"Hired {count} {def.DisplayName} for {cost}g.");
        OnArmyChanged?.Invoke();
        return true;
    }

    // -----------------------------------------------------------------
    // Promotion
    // -----------------------------------------------------------------

    /// <summary>
    /// Moves men up their line. Veterans - a stack that has won ten fights and
    /// come out of them - go up for nothing, which is the whole reason to keep
    /// a company alive rather than rehire after every bad battle. Everyone else
    /// pays, and paying is still cheaper than buying the better troop outright.
    /// </summary>
    public bool Promote(UnitType type, int count, out string reason)
    {
        reason = "";

        var def = UnitCatalog.Get(type);

        if (def == null || !def.CanUpgrade)
        {
            reason = "These cannot be promoted further.";
            return false;
        }

        var stack = Units?.FirstOrDefault(u => u != null && u.Type == type);

        if (stack == null || stack.Count < count || count <= 0)
        {
            reason = "Not that many to promote.";
            return false;
        }

        var pd = Player;
        if (pd == null) { reason = "No character."; return false; }

        bool free = stack.IsVeteran;
        int cost = free ? 0 : def.UpgradeGold * count;

        if (cost > 0 && !pd.HasEnoughMoney(cost, 0))
        {
            reason = $"{cost} gold needed.";
            return false;
        }

        if (cost > 0) PlayerStatHandler.Instance.ConsumeMoney(cost, 0);

        stack.Count -= count;
        if (stack.Count <= 0) Units.Remove(stack);

        Add(def.UpgradesTo, count);

        var into = UnitCatalog.Get(def.UpgradesTo);
        Log(free
            ? $"{count} veteran {def.DisplayName} became {into?.DisplayName}."
            : $"Promoted {count} {def.DisplayName} to {into?.DisplayName} for {cost}g.");

        OnArmyChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// Called when a battle is won. Only stacks that still have men in them
    /// count it: experience belongs to the survivors, and a stack wiped out has
    /// no one left to have learned anything.
    /// </summary>
    public void RecordVictory()
    {
        var units = Units;
        if (units == null) return;

        foreach (var unit in units)
            if (unit != null && unit.Count > 0)
                unit.BattlesWon++;

        OnArmyChanged?.Invoke();
    }

    // -----------------------------------------------------------------

    private void Add(UnitType type, int count)
    {
        var units = Units;
        if (units == null || count <= 0) return;

        var stack = units.FirstOrDefault(u => u != null && u.Type == type);

        if (stack != null)
        {
            stack.Count += count;
            return;
        }

        units.Add(new Unit { Type = type, Count = count, BattlesWon = 0 });
    }
}
