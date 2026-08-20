using System.Collections.Generic;
using UnityEngine;
using NEXUS.Utilities;

/// <summary>
/// Rolls the two hidden properties a crafted or upgraded item carries.
///
/// This is separate from the success roll on purpose. Whether the work
/// succeeded is one question; what came out of it is another. A smith who
/// never fails still cannot promise a perfect blade — and that uncertainty is
/// what makes a good roll worth keeping and a bad one worth selling.
///
/// Three things are random and one is not:
///   which properties      random, from a pool that suits the item
///   which direction       weighted by skill — a master is more often right
///   how much              scaled by quality tier
///   how many              always two
///
/// Skill does not remove the downside, it tilts it. At level 1 roughly half
/// the rolls go against you; at level 10 about three in four go your way. A
/// bad roll from a master is rarer, not impossible, which keeps the forge
/// interesting long after the recipes stop being.
/// </summary>
public static class CraftedStatRoller
{
    public const int RollCount = 2;

    // Sign weighting. A novice is a coin flip; mastery bends it without ever
    // guaranteeing the result.
    private const int BasePositiveChance = 45;
    private const int ChancePerSkillLevel = 3;
    private const int MaxPositiveChance = 85;

    /// <summary>Magnitude of a flat roll, by quality tier.</summary>
    private static readonly int[] FlatByQuality = { 1, 1, 2, 3, 4 };

    /// <summary>Magnitude of a percent roll, by quality tier.</summary>
    private static readonly int[] PercentByQuality = { 3, 5, 8, 12, 18 };

    // -----------------------------------------------------------------
    // Pools — what a given kind of item can roll
    // -----------------------------------------------------------------

    private static readonly EffectType[] WeaponPool =
    {
        EffectType.Attack, EffectType.Accuracy, EffectType.CriticalChance,
        EffectType.Strength, EffectType.Dexterity
    };

    private static readonly EffectType[] ArmorPool =
    {
        EffectType.Defense, EffectType.MaxHealth, EffectType.DamageTaken,
        EffectType.Constitution, EffectType.ExhaustionGain, EffectType.CarryCapacity
    };

    private static readonly EffectType[] ToolPool =
    {
        EffectType.CraftQuality, EffectType.CraftSpeed, EffectType.CraftResourceCost,
        EffectType.SkillXpGain
    };

    private static readonly EffectType[] TrinketPool =
    {
        EffectType.EventSuccess, EffectType.Persuasion, EffectType.IllnessResistance,
        EffectType.AmbushAvoidance, EffectType.ShopBuyPrice, EffectType.ShopSellPrice,
        EffectType.TravelTime
    };

    /// <summary>Effects where a negative number is the good outcome.</summary>
    private static readonly HashSet<EffectType> LowerIsBetter = new()
    {
        EffectType.DamageTaken, EffectType.ExhaustionGain, EffectType.RationConsumption,
        EffectType.CraftResourceCost, EffectType.TravelTime, EffectType.BuildTime,
        EffectType.ShopBuyPrice
    };

    /// <summary>Effects expressed as percentages rather than flat points.</summary>
    private static readonly HashSet<EffectType> IsPercent = new()
    {
        EffectType.CriticalChance, EffectType.DamageTaken, EffectType.ExhaustionGain,
        EffectType.RationConsumption, EffectType.CarryCapacity, EffectType.CraftQuality,
        EffectType.CraftSpeed, EffectType.CraftResourceCost, EffectType.SkillXpGain,
        EffectType.TravelTime, EffectType.ShopBuyPrice, EffectType.ShopSellPrice,
        EffectType.AmbushAvoidance
    };

    // -----------------------------------------------------------------

    /// <summary>
    /// Rolls the hidden properties for a freshly made item.
    /// The two rolls never land on the same property.
    /// </summary>
    public static List<GameplayEffect> Roll(ItemCategory category, ItemQuality quality, int craftLevel)
    {
        var pool = PoolFor(category);
        var result = new List<GameplayEffect>(RollCount);

        if (pool.Length == 0) return result;

        var taken = new HashSet<EffectType>();
        int positiveChance = PositiveChance(craftLevel);

        for (int i = 0; i < RollCount && taken.Count < pool.Length; i++)
        {
            EffectType type;
            int guard = 0;

            do
            {
                type = pool[Dice.Index(pool.Length)];
            }
            while (!taken.Add(type) && guard++ < 20);

            result.Add(MakeEffect(type, quality, positiveChance));
        }

        return result;
    }

    /// <summary>
    /// Rerolls an existing item's hidden properties — what an upgrade does.
    /// Keeps whichever roll was already better when <paramref name="keepBest"/>
    /// is set, so improving a good piece cannot make it worse.
    /// </summary>
    public static List<GameplayEffect> Reroll(Item item, ItemQuality newQuality,
                                              int craftLevel, bool keepBest = true)
    {
        var fresh = Roll(item.Category, newQuality, craftLevel);

        if (!keepBest || item.HiddenEffects == null || item.HiddenEffects.Count == 0)
            return fresh;

        // Compare like for like: an upgrade that hands back a worse blade
        // teaches the player never to upgrade again.
        for (int i = 0; i < fresh.Count && i < item.HiddenEffects.Count; i++)
        {
            var oldEffect = item.HiddenEffects[i];
            if (oldEffect == null) continue;

            if (Benefit(oldEffect) > Benefit(fresh[i]))
                fresh[i] = oldEffect;
        }

        return fresh;
    }

    /// <summary>
    /// How good an effect is, sign-corrected. Used to compare rolls without
    /// treating "-15% exhaustion" as worse than "+1 attack".
    /// </summary>
    public static int Benefit(GameplayEffect effect)
    {
        if (effect == null) return 0;

        int value = LowerIsBetter.Contains(effect.Type) ? -effect.Value : effect.Value;

        // Percent points are worth less each than a flat point.
        return effect.isPercent ? Mathf.RoundToInt(value / 3f) : value;
    }

    /// <summary>Whether a rolled effect helps the player.</summary>
    public static bool IsBeneficial(GameplayEffect effect) => Benefit(effect) > 0;

    public static int PositiveChance(int craftLevel)
        => Mathf.Min(MaxPositiveChance, BasePositiveChance + craftLevel * ChancePerSkillLevel);

    /// <summary>
    /// The craft level a quality tier expects. Higher tiers are gated so a
    /// novice cannot stumble into Masterwork — rarity has to be earned rather
    /// than rolled.
    /// </summary>
    public static int RequiredLevelFor(ItemQuality quality)
    {
        switch (quality)
        {
            case ItemQuality.Fine:       return 4;
            case ItemQuality.Masterwork: return 8;
            case ItemQuality.Legendary:  return 12;
            default:                     return 1;
        }
    }

    /// <summary>
    /// The quality a craft attempt produces. Skill sets the ceiling, the dice
    /// decide where under it the piece lands.
    /// </summary>
    public static ItemQuality RollQuality(int craftLevel, int qualityChanceBonus = 0)
    {
        int roll = Dice.RollD100() + qualityChanceBonus + craftLevel * 2;

        if (craftLevel >= RequiredLevelFor(ItemQuality.Legendary) && roll >= 100) return ItemQuality.Legendary;
        if (craftLevel >= RequiredLevelFor(ItemQuality.Masterwork) && roll >= 92) return ItemQuality.Masterwork;
        if (craftLevel >= RequiredLevelFor(ItemQuality.Fine) && roll >= 70) return ItemQuality.Fine;
        if (roll >= 25) return ItemQuality.Common;

        return ItemQuality.Crude;
    }

    // -----------------------------------------------------------------

    private static GameplayEffect MakeEffect(EffectType type, ItemQuality quality, int positiveChance)
    {
        int tier = Mathf.Clamp((int)quality, 0, 4);
        bool percent = IsPercent.Contains(type);

        int magnitude = percent ? PercentByQuality[tier] : FlatByQuality[tier];

        // Vary it a little so two Fine blades are not identical either.
        magnitude = Mathf.Max(1, magnitude + Dice.Roll(-1, 2));

        bool good = Dice.RollD100() <= positiveChance;

        // For a cost, "good" means a lower number.
        bool wantsNegative = LowerIsBetter.Contains(type) ? good : !good;
        int value = wantsNegative ? -magnitude : magnitude;

        return new GameplayEffect(type, value, percent);
    }

    private static EffectType[] PoolFor(ItemCategory category)
    {
        switch (category)
        {
            case ItemCategory.Weapon:
                return WeaponPool;

            case ItemCategory.Armor:
            case ItemCategory.Helmet:
            case ItemCategory.Shield:
            case ItemCategory.Boots:
            case ItemCategory.Leggings:
                return ArmorPool;

            // Gloves sit between armour and tools — they protect, and they are
            // what you work with.
            case ItemCategory.Gloves:
                return Dice.RollD100() <= 50 ? ArmorPool : ToolPool;

            case ItemCategory.Trinket:
                return TrinketPool;

            default:
                return new EffectType[0];
        }
    }

    /// <summary>The line shown in a hidden-stat box.</summary>
    public static string Describe(GameplayEffect effect)
    {
        if (effect == null) return "";

        string label = TraitRules.EffectLabel(effect.Type);
        string sign = effect.Value >= 0 ? "+" : "";
        string unit = effect.isPercent ? "%" : "";

        return $"{label} {sign}{effect.Value}{unit}";
    }

    public static Color ColorFor(GameplayEffect effect)
    {
        return IsBeneficial(effect)
            ? new Color(0.24f, 0.46f, 0.20f)
            : new Color(0.62f, 0.24f, 0.20f);
    }
}
