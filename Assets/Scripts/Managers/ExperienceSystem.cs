using System;
using UnityEngine;

/// <summary>
/// Experience economy.
///
/// Curve — rising, not flat:
///   cost of level L -> L+1 = 100 + (L-1) * 50
///   L1->2: 100, L2->3: 150, L3->4: 200 ... L19->20: 1000
///   Total to reach L20 ≈ 10,450.
/// The old curve was a flat 100 per level, which is how one generous travel
/// event chain jumped a fresh character to level 27.
///
/// Reward scaling — work is worth less to a veteran:
///   factor = clamp(1 - 0.03 * (playerLevel - contentLevel), 0.40, 1.25)
///   "Help the Scouts" (content level 1, base 50 XP):
///     level 1 -> 50 · level 10 -> ~37 · level 20 -> ~21
/// Content above your level pays a small bonus (up to +25%).
///
/// Losing on purpose is not an exploit here: failure XP passes through the
/// same scaling, the dice decide outcomes, and exhaustion/hunger cap how many
/// attempts a day even allows.
///
/// PlayerData.Experience remains TOTAL lifetime XP. Level is derived from it.
/// MaxExperience is kept in sync for the UI bar.
/// </summary>
public static class ExperienceSystem
{
    public static Action OnLevelUp;
    public static Action OnlevelDown;
    public static Action OnExperienceNegative;

    // -----------------------------------------------------------------
    // Curve
    // -----------------------------------------------------------------

    private const int BaseCost = 100;
    private const int CostStep = 50;

    /// <summary>XP needed to go from `level` to `level + 1`.</summary>
    public static int CostForNextLevel(int level)
    {
        level = Mathf.Max(1, level);
        return BaseCost + (level - 1) * CostStep;
    }

    /// <summary>Total XP required to have reached `level`.</summary>
    public static int CalculateTotalCharacterExperienceForLevel(int level)
    {
        level = Mathf.Max(1, level);
        int n = level - 1;
        // Arithmetic series: n terms starting at BaseCost, step CostStep.
        return n * BaseCost + (n * (n - 1) / 2) * CostStep;
    }

    public static int CalculateCharacterLevelByExperience(int experience)
    {
        if (experience <= 0) return 1;

        int level = 1;
        int remaining = experience;

        while (remaining >= CostForNextLevel(level))
        {
            remaining -= CostForNextLevel(level);
            level++;

            if (level >= 99)   // hard cap, keeps the loop finite
                break;
        }

        return level;
    }

    /// <summary>XP progressed inside the current level (for the UI bar).</summary>
    public static int GetXpIntoCurrentLevel(PlayerData pd)
    {
        if (pd == null) return 0;
        return pd.Experience - CalculateTotalCharacterExperienceForLevel(pd.Level);
    }

    // -----------------------------------------------------------------
    // Reward scaling
    // -----------------------------------------------------------------

    private const float DecayPerLevel = 0.03f;
    private const float MinFactor = 0.40f;
    private const float MaxFactor = 1.25f;

    /// <summary>
    /// How much a reward authored for `contentLevel` is worth to a player of
    /// `playerLevel`.
    /// </summary>
    public static float RewardFactor(int playerLevel, int contentLevel)
    {
        float factor = 1f - DecayPerLevel * (playerLevel - Mathf.Max(1, contentLevel));
        return Mathf.Clamp(factor, MinFactor, MaxFactor);
    }

    public static int ScaleReward(int baseAmount, int playerLevel, int contentLevel = 1)
    {
        if (baseAmount == 0) return 0;

        float scaled = baseAmount * RewardFactor(playerLevel, contentLevel);

        // Penalties keep their sign and are never scaled below their base —
        // getting stronger should not soften your failures.
        if (baseAmount < 0)
            return baseAmount;

        return Mathf.Max(1, Mathf.RoundToInt(scaled));
    }

    // -----------------------------------------------------------------
    // Single gateway
    // -----------------------------------------------------------------

    /// <summary>
    /// The one place XP enters or leaves the character. Applies scaling,
    /// clamps, recomputes level, fires level events, syncs MaxExperience.
    ///
    /// contentLevel: the level this content was authored for (job tier,
    /// event tier). Defaults to 1 for early-game content.
    /// </summary>
    public static void GrantExperience(PlayerData playerData, int baseAmount, int contentLevel = 1)
    {
        if (playerData == null || baseAmount == 0) return;

        int amount = ScaleReward(baseAmount, playerData.Level, contentLevel);
        AddExperience(playerData, amount);
        UpdateCharacterLevel(playerData);
    }

    /// <summary>Raw add — no scaling. Prefer GrantExperience.</summary>
    public static void AddExperience(PlayerData playerData, int amount)
    {
        if (playerData == null) return;

        playerData.Experience += amount;

        if (playerData.Experience < 0)
        {
            playerData.Experience = 0;
            OnExperienceNegative?.Invoke();
        }
    }

    public static void UpdateCharacterLevel(PlayerData playerData)
    {
        if (playerData == null) return;

        int oldLevel = playerData.Level;
        int newLevel = CalculateCharacterLevelByExperience(playerData.Experience);

        playerData.Level = newLevel;
        playerData.MaxExperience = CostForNextLevel(newLevel);

        if (newLevel > oldLevel)
        {
            OnLevelUp?.Invoke();
            Debug.Log($"Seviyeniz {newLevel} oldu.");
        }
        else if (newLevel < oldLevel)
        {
            OnlevelDown?.Invoke();
            Debug.Log($"Seviyeniz {newLevel} seviyesine düştü.");
        }
    }

    // -----------------------------------------------------------------
    // Craft skills (unchanged flat curve for now — craft depth comes from
    // recipes and quality, not from steep levels)
    // -----------------------------------------------------------------

    public static int CalculateTotalCraftExperienceForLevel(int level)
    {
        level = Mathf.Max(1, level);
        return (level - 1) * 100;
    }

    public static void UpdateCraftLevel(PlayerData playerData, CraftDiscipline craftType, int gainedXp)
    {
        if (playerData == null) return;

        int currentXp = GetCraftXP(playerData, craftType);
        currentXp += gainedXp;

        if (currentXp < 0)
            currentXp = 0;

        int newLevel = Mathf.Max(1, (currentXp / 100) + 1);
        int remainderXp = currentXp % 100;

        SetCraftLevel(playerData, craftType, newLevel);
        SetCraftXP(playerData, craftType, remainderXp);

        Debug.Log($"{craftType} beceri seviyeniz {newLevel} oldu.");
    }

    public static int GetCraftLevel(PlayerData playerData, CraftDiscipline craftType)
    {
        if (playerData == null) return 1;

        switch (craftType)
        {
            case CraftDiscipline.Smither: return playerData.SmitherSkillLevel;
            case CraftDiscipline.Tanner: return playerData.TannerSkillLevel;
            case CraftDiscipline.Carpenter: return playerData.CarpenterSkillLevel;
            case CraftDiscipline.Mason: return playerData.MasonSkillLevel;
            case CraftDiscipline.Alchemist: return playerData.AlchemistSkillLevel;
            default: return 1;
        }
    }

    public static void SetCraftLevel(PlayerData playerData, CraftDiscipline craftType, int level)
    {
        if (playerData == null) return;

        level = Mathf.Max(1, level);

        switch (craftType)
        {
            case CraftDiscipline.Smither:
                playerData.SmitherSkillLevel = level;
                break;
            case CraftDiscipline.Tanner:
                playerData.TannerSkillLevel = level;
                break;
            case CraftDiscipline.Carpenter:
                playerData.CarpenterSkillLevel = level;
                break;
            case CraftDiscipline.Mason:
                playerData.MasonSkillLevel = level;
                break;
            case CraftDiscipline.Alchemist:
                playerData.AlchemistSkillLevel = level;
                break;
        }
    }

    public static int GetCraftXP(PlayerData playerData, CraftDiscipline craftType)
    {
        if (playerData == null) return 0;

        switch (craftType)
        {
            case CraftDiscipline.Smither: return playerData.SmitherSkillXP;
            case CraftDiscipline.Tanner: return playerData.TannerSkillXP;
            case CraftDiscipline.Carpenter: return playerData.CarpenterSkillXP;
            case CraftDiscipline.Mason: return playerData.MasonSkillXP;
            case CraftDiscipline.Alchemist: return playerData.AlchemistSkillXP;
            default: return 0;
        }
    }

    public static void SetCraftXP(PlayerData playerData, CraftDiscipline craftType, int xp)
    {
        if (playerData == null) return;

        xp = Mathf.Max(0, xp);

        switch (craftType)
        {
            case CraftDiscipline.Smither:
                playerData.SmitherSkillXP = xp;
                break;
            case CraftDiscipline.Tanner:
                playerData.TannerSkillXP = xp;
                break;
            case CraftDiscipline.Carpenter:
                playerData.CarpenterSkillXP = xp;
                break;
            case CraftDiscipline.Mason:
                playerData.MasonSkillXP = xp;
                break;
            case CraftDiscipline.Alchemist:
                playerData.AlchemistSkillXP = xp;
                break;
        }
    }

    public static int GetTotalCraftExperience(PlayerData playerData, CraftDiscipline craftType)
    {
        if (playerData == null) return 0;

        int level = GetCraftLevel(playerData, craftType);
        int xp = GetCraftXP(playerData, craftType);

        return CalculateTotalCraftExperienceForLevel(level) + xp;
    }

    public static void SetTotalCraftExperience(PlayerData playerData, CraftDiscipline craftType, int totalXp)
    {
        if (playerData == null) return;

        totalXp = Mathf.Max(0, totalXp);

        int level = Mathf.Max(1, (totalXp / 100) + 1);
        int xp = totalXp % 100;

        SetCraftLevel(playerData, craftType, level);
        SetCraftXP(playerData, craftType, xp);
    }
}
