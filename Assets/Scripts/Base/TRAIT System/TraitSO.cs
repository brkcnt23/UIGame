using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// One trait: an origin, a personality, a familiarity, or a condition.
///
/// The description is written in the world's voice, the effects carry the
/// numbers. Same rule as items — the player reads what it means, then reads
/// what it costs.
/// </summary>
[CreateAssetMenu(fileName = "Trait", menuName = "UIGame/Trait")]
public class TraitSO : ScriptableObject
{
    [Header("Identity")]
    public string traitId;
    public string displayName;
    public Sprite icon;

    public TraitKind kind = TraitKind.Condition;
    public TraitTone tone = TraitTone.Neutral;

    [Header("Text")]
    [Tooltip("In-world voice: what this feels like, not what it does numerically.")]
    [TextArea(2, 4)] public string description;

    [Header("Effects")]
    public List<GameplayEffect> effects = new();

    [Header("Duration (conditions only)")]
    [Tooltip("In-game hours. 0 means it lasts until something removes it.")]
    [Min(0)] public int durationHours = 0;

    [Tooltip("Re-applying refreshes the timer instead of being ignored.")]
    public bool refreshOnReapply = true;

    [Tooltip("Can be held more than once; effects add up.")]
    public bool stackable = false;

    [Min(1)] public int maxStacks = 1;

    [Header("Exclusivity")]
    [Tooltip("Gaining this removes these. Nourished and Starving cannot coexist.")]
    public List<string> removesTraitIds = new();

    [Tooltip("Cannot be gained while the player holds any of these.")]
    public List<string> blockedByTraitIds = new();

    [Header("Story hooks")]
    [Tooltip("Companions that become recruitable while this trait is held.")]
    public List<string> unlocksCompanionIds = new();

    [Tooltip("Companions that refuse the player while this trait is held.")]
    public List<string> blocksCompanionIds = new();

    [Tooltip("Tags this contributes for recipes, events and dialogue gating.")]
    public List<string> grantsTags = new();

    public bool IsPermanent => TraitRules.IsPermanent(kind);
    public bool Expires => kind == TraitKind.Condition && durationHours > 0;

    /// <summary>The mechanical lines, one per effect.</summary>
    public List<string> GetEffectLines()
    {
        var lines = new List<string>(effects.Count);
        foreach (var e in effects)
            lines.Add(e.ToDisplayString());
        return lines;
    }

    public int GetFlat(EffectType type, string qualifier = null)
    {
        int total = 0;
        foreach (var e in effects)
            if (e.Type == type && !e.isPercent && e.AppliesTo(qualifier))
                total += e.Value;
        return total;
    }

    public int GetPercent(EffectType type, string qualifier = null)
    {
        int total = 0;
        foreach (var e in effects)
            if (e.Type == type && e.isPercent && e.AppliesTo(qualifier))
                total += e.Value;
        return total;
    }
}

/// <summary>
/// A trait the player currently holds. Conditions carry an expiry; permanent
/// traits carry the day they were earned so the character sheet can show a
/// history.
/// </summary>
[System.Serializable]
public class ActiveTrait
{
    public string traitId;
    public int stacks = 1;

    [Tooltip("Absolute game hour when this expires. -1 = never.")]
    public int expiresAtHour = -1;

    public int gainedOnDay;

    public bool IsExpired(int currentAbsoluteHour)
    {
        return expiresAtHour >= 0 && currentAbsoluteHour >= expiresAtHour;
    }

    public int HoursRemaining(int currentAbsoluteHour)
    {
        return expiresAtHour < 0 ? -1 : Mathf.Max(0, expiresAtHour - currentAbsoluteHour);
    }

    /// <summary>"1h 25m" style label for the character sheet.</summary>
    public string RemainingLabel(int currentAbsoluteHour)
    {
        int h = HoursRemaining(currentAbsoluteHour);
        if (h < 0) return "";
        return h >= 24 ? $"{h / 24}d {h % 24}h" : $"{h}h";
    }
}
