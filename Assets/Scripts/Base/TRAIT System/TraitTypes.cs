using System;
using UnityEngine;

/// <summary>
/// What kind of thing a trait is. The kind decides how it is gained, whether
/// it expires, and where it appears in the UI.
/// </summary>
public enum TraitKind
{
    /// <summary>Where you grew up. Chosen at creation, never lost.</summary>
    Origin,

    /// <summary>Who you are. Earned and lost through choices over the whole game.</summary>
    Personality,

    /// <summary>What your hands know. Earned by doing the work.</summary>
    Familiarity,

    /// <summary>How you are right now. Expires.</summary>
    Condition
}

/// <summary>Positive, negative or neither — drives the colour in the UI.</summary>
public enum TraitTone
{
    Neutral,
    Positive,
    Negative
}

/// <summary>
/// Everything a trait, companion or item can modify.
///
/// Deliberately broader than StatType: traits mostly do not change raw
/// attributes, they change how the world responds to you.
/// </summary>
public enum EffectType
{
    // Attributes
    Strength,
    Dexterity,
    Constitution,
    Charisma,

    // Combat
    Attack,
    Defense,
    Accuracy,
    CriticalChance,
    DamageTaken,

    // Survival
    MaxHealth,
    HealthRegen,
    ExhaustionGain,
    RationConsumption,
    CarryCapacity,

    // Work
    CraftQuality,
    CraftSpeed,
    CraftResourceCost,
    SkillXpGain,
    JobReward,
    BuildTime,

    // World
    TravelTime,
    ShopBuyPrice,
    ShopSellPrice,
    EventSuccess,
    Persuasion,
    IllnessResistance,
    AmbushAvoidance
}

/// <summary>
/// One effect line. Percent effects are additive within their type and capped
/// by TraitRules.Cap so stacking cannot run away.
/// </summary>
[Serializable]
public class GameplayEffect
{
    public EffectType Type;

    [Tooltip("Flat points, or percent when isPercent is on.")]
    public int Value;

    public bool isPercent;

    [Tooltip("Shown under the description as the mechanical line, e.g. 'Attack +1'. " +
             "Leave empty to auto-generate.")]
    public string displayOverride;

    public GameplayEffect() { }

    public GameplayEffect(EffectType type, int value, bool percent = false)
    {
        Type = type;
        Value = value;
        isPercent = percent;
    }

    /// <summary>The mechanical line. Always signed, so +1 and -1 read clearly.</summary>
    public string ToDisplayString()
    {
        if (!string.IsNullOrEmpty(displayOverride))
            return displayOverride;

        string label = TraitRules.EffectLabel(Type);
        string sign = Value >= 0 ? "+" : "";
        string unit = isPercent ? "%" : "";

        return $"{label} {sign}{Value}{unit}";
    }
}

public static class TraitRules
{
    /// <summary>
    /// Percent cap per effect type. Three sources at -15% each would otherwise
    /// take crafting costs to -45% and break the economy.
    /// </summary>
    public const int PercentCap = 40;

    public static int Cap(int accumulatedPercent)
    {
        return Mathf.Clamp(accumulatedPercent, -PercentCap, PercentCap);
    }

    public static string EffectLabel(EffectType t)
    {
        switch (t)
        {
            case EffectType.Strength:          return "Strength";
            case EffectType.Dexterity:         return "Dexterity";
            case EffectType.Constitution:      return "Constitution";
            case EffectType.Charisma:          return "Charisma";
            case EffectType.Attack:            return "Attack";
            case EffectType.Defense:           return "Defense";
            case EffectType.Accuracy:          return "Accuracy";
            case EffectType.CriticalChance:    return "Critical chance";
            case EffectType.DamageTaken:       return "Damage taken";
            case EffectType.MaxHealth:         return "Max health";
            case EffectType.HealthRegen:       return "Recovery";
            case EffectType.ExhaustionGain:    return "Exhaustion gain";
            case EffectType.RationConsumption: return "Ration use";
            case EffectType.CarryCapacity:     return "Carry capacity";
            case EffectType.CraftQuality:      return "Craft quality";
            case EffectType.CraftSpeed:        return "Craft speed";
            case EffectType.CraftResourceCost: return "Material use";
            case EffectType.SkillXpGain:       return "Skill experience";
            case EffectType.JobReward:         return "Job reward";
            case EffectType.BuildTime:         return "Build time";
            case EffectType.TravelTime:        return "Travel time";
            case EffectType.ShopBuyPrice:      return "Buy price";
            case EffectType.ShopSellPrice:     return "Sell price";
            case EffectType.EventSuccess:      return "Event success";
            case EffectType.Persuasion:        return "Persuasion";
            case EffectType.IllnessResistance: return "Illness resistance";
            case EffectType.AmbushAvoidance:   return "Ambush avoidance";
            default:                           return t.ToString();
        }
    }

    public static Color ToneColor(TraitTone tone)
    {
        switch (tone)
        {
            case TraitTone.Positive: return new Color(0.56f, 0.77f, 0.42f);
            case TraitTone.Negative: return new Color(0.83f, 0.48f, 0.42f);
            default:                 return new Color(0.88f, 0.86f, 0.80f);
        }
    }

    public static bool IsPermanent(TraitKind kind) => kind != TraitKind.Condition;
}
