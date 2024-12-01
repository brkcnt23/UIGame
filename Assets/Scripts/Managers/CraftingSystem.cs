using System;
using UnityEngine;
using NEXUS.Utilities;

public enum CraftType
{
    Smither,
    Tanner,
    Carpenter,
    Mason,
    Alchemist
}

public class CraftingSystem
{
    private PlayerData playerData;
    private EconomySystem economySystem;
    private TimeSystem timeSystem;

    public CraftingSystem(PlayerData pd, TimeSystem ts)
    {
        playerData = pd;
        economySystem = new EconomySystem(playerData);
        timeSystem = ts;
    }

    public void WorkAsApprentice(CraftType craftType, int jobLevel)
    {
        // Oyuncunun beceri seviyesini ve seviye farkını alalım
        int playerSkillLevel = GetCraftLevel(craftType);
        int levelDifference = playerSkillLevel - jobLevel;

        if (levelDifference < -5)
        {
            Debug.Log("Beceri seviyeniz bu işi yapmak için çok düşük.");
            return;
        }

        // Başarı şansını hesaplayalım
        int successChance = Mathf.Clamp(50 + (levelDifference * 5), 0, 100);
        int randomValueForSuccess = Dice.RollD100();

        if (randomValueForSuccess > successChance)
        {
            Debug.Log("Üretim başarısız oldu.");
            int workDuration = CalculateWorkDuration(jobLevel);
            timeSystem.AdvanceTime(workDuration);
            return;
        }

        // Üretim başarılı, ödülleri hesapla ve uygula
        CalculateAndApplyRewards(craftType, jobLevel);

        // İş süresini ilerlet
        int workDurationInMinutes = CalculateWorkDuration(jobLevel);
        timeSystem.AdvanceTime(workDurationInMinutes);

        // Sonuçları yazdır
        Debug.Log($"Çırak olarak {craftType} alanında çalıştınız.");
        Debug.Log($"Toplam Altın: {playerData.Gold}, Toplam Gümüş: {playerData.Silver}");
        Debug.Log($"Zaman ilerledi: {workDurationInMinutes / 60} saat. {timeSystem.GetTimeString()}");
    }

    private void CalculateAndApplyRewards(CraftType craftType, int jobLevel)
    {
        // Mevcut ödül hesaplamaları
        float maxBonus = 10 + (playerData.Charisma / 2f);
        float randomValue = UnityEngine.Random.Range(0f, maxBonus);

        float successMultiplier = (playerData.Strength + playerData.Constitution + randomValue) / 2f;
        float statMultiplier = (playerData.Strength + playerData.Constitution) / 10f;
        float baseMultiplier = 1 + (playerData.Level / 10f);
        int difficultyIndex = GetDifficultyIndex(jobLevel);

        float goldModifier = 0.5f;
        float expModifier = 0.5f;

        float reward = ((difficultyIndex * successMultiplier * goldModifier) * randomValue + statMultiplier) * baseMultiplier;

        int silverReward = Mathf.RoundToInt(reward);
        float expReward = reward * expModifier;

        // Gümüş ödülü
        economySystem.AddSilver(silverReward);

        // Crafting EXP kazancı
        int craftExp = Mathf.RoundToInt(expReward);
        ExperienceSystem.UpdateCraftLevel(playerData, craftType, craftExp);

        // Karakter EXP kazancı (Crafting EXP'in yarısı kadar)
        int characterExpGain = Mathf.RoundToInt(expReward / 2);
        PlayerStatHandler.Instance.AddCharacterExperience(characterExpGain);

        // %50 ihtimalle stat kazancı
        if (craftType != CraftType.Alchemist)
        {
            if (UnityEngine.Random.Range(0, 100) < 50) // %50 ihtimal
            {
                PlayerStatHandler.Instance.AddStats("Strength", 1);
                Debug.Log("Strength statı kazandınız!");
            }
            if (UnityEngine.Random.Range(0, 100) < 50) // %50 ihtimal
            {
                PlayerStatHandler.Instance.AddStats("Constitution", 1);
                Debug.Log("Constitution statı kazandınız!");
            }
        }

        // Sonuçları yazdır
        Debug.Log($"Üretim başarılı! {silverReward} gümüş ve {craftExp} crafting EXP kazandınız.");
        Debug.Log($"Karakter {characterExpGain} EXP kazandı.");
    }

    private int GetDifficultyIndex(int jobLevel)
    {
        if (jobLevel >= 1 && jobLevel <= 5)
        {
            return 1;
        }
        else if (jobLevel > 5 && jobLevel <= 8)
        {
            return 2;
        }
        else if (jobLevel > 8 && jobLevel <= 10)
        {
            return 3;
        }
        else
        {
            return 1; // Varsayılan olarak 1
        }
    }

    private int GetCraftLevel(CraftType craftType)
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

    private int CalculateWorkDuration(int jobLevel)
    {
        // İş süresini işin seviyesine göre belirleyelim
        int baseWorkDuration = 8 * 60; // Temel iş süresi: 8 saat
        int workDuration = baseWorkDuration + ((jobLevel - 1) * 30); // Her seviye için 30 dakika eklenir
        return workDuration;
    }
    private void AddCraftedItem(CraftType craftType)
    {
        // Create an item based on the crafting type
        Item craftedItem = craftType switch
        {
            CraftType.Smither => ItemDatabase.GetItemByID(1), // Iron Sword
            CraftType.Tanner => ItemDatabase.GetItemByID(3), // Leather Armor
            CraftType.Carpenter => ItemDatabase.GetItemByID(4), // Wooden Plank
            CraftType.Mason => ItemDatabase.GetItemByID(5), // Stone Brick
            CraftType.Alchemist => ItemDatabase.GetItemByID(2), // Health Potion
            _ => null
        };

        if (craftedItem != null)
        {
            InventorySystem.Instance.AddItem(craftedItem);
            Debug.Log($"Crafted {craftedItem.Name} and added to inventory.");
        }
    }

}
