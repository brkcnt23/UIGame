using System;
using UnityEngine;
using DICE;

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
        // Oyuncunun statlarını alalım
        int charisma = playerData.Charisma;
        int strength = playerData.Strength;
        int constitution = playerData.Constitution;
        int level = playerData.Level;

        // Max bonus ve randomValue hesaplaması
        float maxBonus = 10 + (charisma / 2f);
        float randomValue = UnityEngine.Random.Range(0f, maxBonus);

        // Başarı çarpanı
        float successMultiplier = (strength + constitution + randomValue) / 2f;

        // Stat çarpanı
        float statMultiplier = (strength + constitution) / 10f;

        // Temel çarpan
        float baseMultiplier = 1 + (level / 10f);

        // Zorluk indeksi belirleme
        int difficultyIndex = GetDifficultyIndex(jobLevel);

        // Gold ve deneyim çarpanları
        float goldModifier = 0.5f;
        float expModifier = 0.5f;

        // Nihai ödül hesaplaması
        float reward = ((difficultyIndex * successMultiplier * goldModifier) * randomValue + statMultiplier) * baseMultiplier;

        int silverReward = Mathf.RoundToInt(reward);
        float expReward = reward * expModifier;

        // Oyuncunun gümüş ve deneyim değerlerini güncelleyelim
        playerData.Silver += silverReward;
        playerData.Experience += Mathf.RoundToInt(expReward);

        // Zanaatkârlık deneyim puanını güncelle
        int craftExp = Mathf.RoundToInt(expReward);
        ExperienceSystem.UpdateCraftLevel(playerData, craftType, craftExp);

        // Karakter deneyim puanını güncelle
        ExperienceSystem.UpdateCharacterLevel(playerData);

        // Para birimi dönüşümünü gerçekleştir
        economySystem.ConvertSilverToGold();

        AddCraftedItem(craftType); // Add the crafted item to inventory
        Debug.Log("Crafting successful!");


        // Seçilen zanaatkârlık alanında seviyeyi artırma işlemini kaldırdık
        // Artık seviye artışı ExperienceSystem üzerinden yapılıyor

        // Sonuçları yazdır
        Debug.Log($"Üretim başarılı! {silverReward} gümüş ve {Mathf.RoundToInt(expReward)} deneyim kazandınız.");
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
