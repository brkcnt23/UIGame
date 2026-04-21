using System;
using UnityEngine;

public static class ExperienceSystem
{
    public static Action OnLevelUp;
    public static Action OnlevelDown;
    public static Action OnExperienceNegative;

    public static int CalculateCharacterLevelByExperience(int experience)
    {
        if (experience < 0) return 1;
        return Mathf.Max(1, Mathf.FloorToInt(experience / 100f) + 1);
    }

    public static int CalculateTotalCharacterExperienceForLevel(int level)
    {
        level = Mathf.Max(1, level);
        return (level - 1) * 100;
    }

    public static int CalculateTotalCraftExperienceForLevel(int level)
    {
        level = Mathf.Max(1, level);
        return (level - 1) * 100;
    }

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

        if (newLevel > oldLevel)
        {
            playerData.Level = newLevel;
            OnLevelUp?.Invoke();
            Debug.Log($"Seviyeniz {newLevel} oldu.");
        }
        else if (newLevel < oldLevel)
        {
            playerData.Level = newLevel;
            OnlevelDown?.Invoke();
            Debug.Log($"Seviyeniz {newLevel} seviyesine düştü.");
        }
        else
        {
            playerData.Level = newLevel;
        }
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