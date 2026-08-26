using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Who you can hire, what they cost, and what they eat.
///
/// Every soldier is a mercenary on a daily wage, so an army is never something
/// you simply own — it is a bill that arrives every morning. That is the whole
/// tension: a horseman is worth ten footmen in a fight and costs ten footmen
/// every single day whether or not there is a fight.
///
/// The upgrade tree is deliberately shallow. It exists to make a veteran worth
/// keeping, not to be a subject of study, and it goes exactly as far as the art
/// does - one line per portrait, no rung without a picture behind it.
///
///   PoorSoldier  -> Soldier -> Knight
///   PoorShielder -> Soldier
///   PoorHorseman -> Cavalry -> Knight
///   PoorArcher   -> HeavyArcher
///   PoorPikeman  -> HeavyPikeman
/// </summary>
public static class UnitCatalog
{
    private static List<UnitDef> _all;

    public static List<UnitDef> All => _all ??= Build();

    public static UnitDef Get(UnitType type) => All.FirstOrDefault(u => u.Type == type);

    /// <summary>Everything a settlement of this size and character will hire out.</summary>
    public static List<UnitDef> AvailableAt(Settlement settlement)
    {
        if (settlement == null) return new List<UnitDef>();

        return All.Where(u => u.IsOfferedAt(settlement)).ToList();
    }

    /// <summary>Silver a day for the whole army, which is what actually bankrupts a player.</summary>
    public static int DailyUpkeep(IEnumerable<Unit> units)
    {
        if (units == null) return 0;

        int total = 0;

        foreach (var unit in units)
        {
            if (unit == null) continue;

            var def = Get(unit.Type);
            if (def != null) total += def.UpkeepSilverPerDay * unit.Count;
        }

        return total;
    }

    // -----------------------------------------------------------------

    private static UnitDef U(UnitType type, string name, string sprite,
                             int hireGold, int upkeepSilver,
                             int attack, int defense, int hp, UnitRole role,
                             UnitRarity rarity, SettlementTier minTier,
                             UnitType upgradesTo = UnitType.None,
                             int upgradeGold = 0, string requiresTag = null)
        => new UnitDef
        {
            Type = type,
            DisplayName = name,
            SpriteName = sprite,
            HireGold = hireGold,
            UpkeepSilverPerDay = upkeepSilver,
            Attack = attack,
            Defense = defense,
            Health = hp,
            Role = role,
            Rarity = rarity,
            MinimumTier = minTier,
            UpgradesTo = upgradesTo,
            UpgradeGold = upgradeGold,
            RequiresTag = requiresTag ?? ""
        };

    private static List<UnitDef> Build() => new List<UnitDef>
    {
        // --- the levy: found in any hamlet, dies in any battle ------------
        U(UnitType.PoorSoldier,  "Levy",           "PoorSoldier",   10,  100,  4,  3, 12, UnitRole.Foot,
          UnitRarity.Common,   SettlementTier.Hamlet,  UnitType.Soldier,      400),

        U(UnitType.Shielder,     "Shield Levy",    "PoorShielder",  15,  100,  3,  6, 14, UnitRole.Foot,
          UnitRarity.Common,   SettlementTier.Hamlet,  UnitType.Soldier,      400),

        U(UnitType.Archer,       "Bowman",         "PoorArcher",    20,  100,  5,  2, 10, UnitRole.Archer,
          UnitRarity.Common,   SettlementTier.Hamlet,  UnitType.HeavyArcher,  800),

        U(UnitType.Pikeman,      "Pikeman",        "PoorPikeman",   20,  100,  6,  3, 11, UnitRole.Pike,
          UnitRarity.Common,   SettlementTier.Hamlet,  UnitType.HeavyPikeman, 750),

        // --- trained foot: a village can raise these ----------------------
        U(UnitType.Soldier,      "Man-at-Arms",    "Soldier",       60,  300,  8,  7, 20, UnitRole.Foot,
          UnitRarity.Common,   SettlementTier.Village, UnitType.Knight,       900),

        // --- horse: a stable is not a village thing -----------------------
        U(UnitType.PoorHorseman, "Rider",          "PoorHorseman", 120,  400, 10,  5, 22, UnitRole.Horse,
          UnitRarity.Uncommon, SettlementTier.Village, UnitType.Cavalry,      300, "pastoral"),

        // --- heavy foot: needs a town's craftsmen to equip -----------------
        U(UnitType.HeavyPikeman, "Halberdier",     "HeavyPikeman", 110,  400, 12,  8, 26, UnitRole.Pike,
          UnitRarity.Uncommon, SettlementTier.Town),

        U(UnitType.HeavyArcher,  "Longbowman",     "HeavyArcher",  120,  400, 13,  4, 22, UnitRole.Archer,
          UnitRarity.Uncommon, SettlementTier.Town),

        // --- the expensive ones: a town at most, and not every town --------
        U(UnitType.Cavalry,      "Cavalry",        "Cavalry",      500, 1000, 18, 10, 34, UnitRole.Horse,
          UnitRarity.Rare,     SettlementTier.Town,    UnitType.Knight,       600),

        U(UnitType.Knight,       "Knight",         "Knight",      1200, 2500, 26, 18, 48, UnitRole.Horse,
          UnitRarity.VeryRare, SettlementTier.City),
    };
}

/// <summary>
/// What a soldier does on a field, which is what decides who beats whom.
///
/// Four roles rather than ten unit types, because the counter cycle is about
/// the shape of the fight: pikes stop horses whether the rider is a farm boy or
/// a knight. Adding a new portrait should not mean editing a matrix.
/// </summary>
public enum UnitRole
{
    Foot,
    Pike,
    Archer,
    Horse
}

public enum UnitRarity
{
    Common,
    Uncommon,
    Rare,
    VeryRare
}

/// <summary>
/// One kind of soldier. Data only - nothing here knows how a battle is fought.
/// </summary>
public class UnitDef
{
    public UnitType Type;
    public string DisplayName;

    /// <summary>File name under UI Elements/Soldier Icons, without extension.</summary>
    public string SpriteName;

    public int HireGold;

    /// <summary>
    /// Silver, not gold. A levy at one gold a day is 100 silver, and keeping
    /// wages in the same unit as everything else avoids a rounding class of bug
    /// that only shows up after thirty in-game days.
    /// </summary>
    public int UpkeepSilverPerDay;

    public int Attack;
    public int Defense;
    public int Health;

    public UnitRole Role;

    /// <summary>
    /// What one of them is worth in a line of battle.
    ///
    /// Deliberately not linear in the stats. A knight with roughly six times a
    /// levy's numbers is worth far more than six levies, because he survives
    /// the exchange that kills them - so the product of staying power and
    /// hitting power is the honest measure.
    ///
    /// The scale is chosen so this lands near the daily wage: a knight fights
    /// like twenty-five levies and costs twenty-five levies a day. Every troop
    /// then earns its keep at about the same rate, and the choice between them
    /// is about how many bodies you want to feed rather than which one is
    /// simply better.
    /// </summary>
    public float CombatValue => (Attack + Defense) * Health / 20f;

    public UnitRarity Rarity;

    /// <summary>Smallest settlement that will hire this out at all.</summary>
    public SettlementTier MinimumTier;

    /// <summary>
    /// Optional settlement tag on top of the tier. Horses come from places that
    /// keep animals, which is what stops every village fielding cavalry.
    /// </summary>
    public string RequiresTag = "";

    public UnitType UpgradesTo = UnitType.None;
    public int UpgradeGold;

    public bool CanUpgrade => UpgradesTo != UnitType.None;

    /// <summary>
    /// Whether this settlement hires them out. Tier is the floor, the tag is the
    /// character of the place: a rich mining town still has no horses.
    /// </summary>
    public bool IsOfferedAt(Settlement settlement)
    {
        if (settlement == null) return false;

        if (TierOf(settlement) < MinimumTier)
            return false;

        if (string.IsNullOrEmpty(RequiresTag))
            return true;

        return settlement.SettlementTags != null
            && settlement.SettlementTags.Contains(RequiresTag);
    }

    /// <summary>
    /// Settlement size, read from population because that is the number the
    /// game already keeps and grows. Type would be the obvious source, but a
    /// settlement can be a Village by type long after it has a town's people.
    /// </summary>
    private static SettlementTier TierOf(Settlement settlement)
    {
        int people = settlement.Population;

        if (people >= 2000) return SettlementTier.City;
        if (people >= 500)  return SettlementTier.Town;
        if (people >= 100)  return SettlementTier.Village;

        return SettlementTier.Hamlet;
    }
}
