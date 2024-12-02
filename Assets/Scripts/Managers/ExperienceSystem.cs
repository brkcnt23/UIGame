using System;
using UnityEngine;

public static class ExperienceSystem
{
    // Karakter seviyeleri için temel XP
    private const int BaseCharacterXP = 1000;

    // Zanaatkarlık seviyeleri için temel XP
    private const int BaseCraftXP = 500;

    public static Action OnLevelUp;

    // Karakter için toplam gerekli XP'yi hesaplar
    public static int GetTotalCharacterXPForLevel(int level)
    {
        return Mathf.RoundToInt(BaseCharacterXP * (Mathf.Pow(2f, (level - 1) / 5f) - 1));
    }

    // Zanaatkarlık için toplam gerekli XP'yi hesaplar
    public static int GetTotalCraftXPForLevel(int level)
    {
        return Mathf.RoundToInt(BaseCraftXP * (Mathf.Pow(2f, (level - 1) / 5f) - 1));
    }

    public static void AddExperience(PlayerData playerData, int experience)
    {
        playerData.Experience += experience;
        UpdateCharacterLevel(playerData);
    }

    // Karakterin seviyesini günceller
    public static void UpdateCharacterLevel(PlayerData playerData)
    {
        int currentLevel = playerData.Level;
        int totalXP = playerData.Experience;
        int maxExperience = GetTotalCharacterXPForLevel(currentLevel);
        int newLevel = currentLevel;

        while (totalXP >= maxExperience && newLevel < 100)
        {
            newLevel++;
            maxExperience = GetTotalCharacterXPForLevel(newLevel);
            totalXP -= maxExperience;
            Debug.Log($"Seviyeniz {newLevel} oldu!");
        }

        playerData.Level = newLevel;

        playerData.Experience = totalXP;

        playerData.MaxExperience = maxExperience;

        Debug.Log($"Toplam XP: {totalXP} / {maxExperience}");

        if (newLevel > currentLevel)
        {
            OnLevelUp?.Invoke();
        }

    }

    // Zanaatkarlık seviyesini günceller
    public static void UpdateCraftLevel(PlayerData playerData, CraftType craftType, int gainedXP)
    {
        int currentLevel = GetCraftLevel(playerData, craftType);
        int totalXP = GetTotalCraftXP(playerData, craftType) + gainedXP;

        int newLevel = currentLevel;

        while (totalXP >= GetTotalCraftXPForLevel(newLevel + 1) && newLevel < 20)
        {
            newLevel++;
            Debug.Log($"{craftType} beceri seviyeniz {newLevel} oldu!");
        }

        SetCraftLevel(playerData, craftType, newLevel);
        SetTotalCraftXP(playerData, craftType, totalXP);
    }

    // Oyuncunun zanaatkarlık seviyesini alır
    private static int GetCraftLevel(PlayerData playerData, CraftType craftType)
    {
        switch (craftType)
        {
            case CraftType.Smither:
                return playerData.SmitherSkillLevel;
            case CraftType.Tanner:
                return playerData.TannerSkillLevel;
            case CraftType.Carpenter:
                return playerData.CarpenterSkillLevel;
            case CraftType.Mason:
                return playerData.MasonSkillLevel;
            case CraftType.Alchemist:
                return playerData.AlchemistSkillLevel;
            default:
                return 1;
        }
    }

    // Oyuncunun zanaatkarlık seviyesini ayarlar
    private static void SetCraftLevel(PlayerData playerData, CraftType craftType, int level)
    {
        switch (craftType)
        {
            case CraftType.Smither:
                playerData.SmitherSkillLevel = level;
                break;
            case CraftType.Tanner:
                playerData.TannerSkillLevel = level;
                break;
            case CraftType.Carpenter:
                playerData.CarpenterSkillLevel = level;
                break;
            case CraftType.Mason:
                playerData.MasonSkillLevel = level;
                break;
            case CraftType.Alchemist:
                playerData.AlchemistSkillLevel = level;
                break;
        }
    }

    // Oyuncunun toplam zanaatkarlık XP'sini alır
    private static int GetTotalCraftXP(PlayerData playerData, CraftType craftType)
    {
        switch (craftType)
        {
            case CraftType.Smither:
                return playerData.SmitherSkillXP;
            case CraftType.Tanner:
                return playerData.TannerSkillXP;
            case CraftType.Carpenter:
                return playerData.CarpenterSkillXP;
            case CraftType.Mason:
                return playerData.MasonSkillXP;
            case CraftType.Alchemist:
                return playerData.AlchemistSkillXP;
            default:
                return 0;
        }
    }

    // Oyuncunun toplam zanaatkarlık XP'sini ayarlar
    private static void SetTotalCraftXP(PlayerData playerData, CraftType craftType, int totalXP)
    {
        switch (craftType)
        {
            case CraftType.Smither:
                playerData.SmitherSkillXP = totalXP;
                break;
            case CraftType.Tanner:
                playerData.TannerSkillXP = totalXP;
                break;
            case CraftType.Carpenter:
                playerData.CarpenterSkillXP = totalXP;
                break;
            case CraftType.Mason:
                playerData.MasonSkillXP = totalXP;
                break;
            case CraftType.Alchemist:
                playerData.AlchemistSkillXP = totalXP;
                break;
        }
    }
}
