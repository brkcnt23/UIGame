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


    public void WorkAsBlacksmithApprentice(int jobLevel, string itemType)
    {
        int playerSkillLevel = playerData.SmitherSkillLevel;
        int levelDifference = playerSkillLevel - jobLevel;

        // Skill level check
        if (levelDifference < -5)
        {
            Debug.Log("Beceri seviyeniz bu işi yapmak için çok düşük.");
            return;
        }

        // Determine required material and quantity based on item type
        Item requiredMaterial = new Item(5, "Iron Ingot", 100, ItemCategory.CraftingMaterial, 1);
        int requiredQuantity = itemType.ToLower() == "armor" ? 2 : 1; // Armor requires 2 ingots, weapon requires 1

        if (itemType.ToLower() != "weapon" && itemType.ToLower() != "armor")
        {
            Debug.Log("Geçersiz eşya türü. Lütfen 'weapon' veya 'armor' seçin.");
            return;
        }

        // Material check
        if (!InventorySystem.Instance.HasItem(requiredMaterial.ID, requiredQuantity))
        {
            Debug.Log($"Yeterli {requiredMaterial.Name} yok. Gerekli miktar: {requiredQuantity}");
            return;
        }

        // Calculate success chance
        float maxBonus = 10 + (playerData.Charisma / 2f);
        float randomValue = UnityEngine.Random.Range(0f, maxBonus);

        float successMultiplier = (playerData.Strength + playerData.Constitution + randomValue) / 2f;
        float statMultiplier = (playerData.Strength + playerData.Constitution) / 10f;
        int successChance = Mathf.Clamp(50 + (levelDifference * 5) + Mathf.RoundToInt(statMultiplier * 5), 0, 100);

        Debug.Log($"Başarı şansı: {successChance}%");

        int randomValueForSuccess = Dice.RollD100();

        if (randomValueForSuccess >= successChance)
        {
            Debug.Log("Üretim başarısız oldu.");
            int workDuration = CalculateWorkDuration(jobLevel);
            timeSystem.AdvanceTimeCoroutine(0, workDuration / 60, workDuration % 60);
            return;
        }

        // Reduce the required material
        InventorySystem.Instance.RemoveItem(requiredMaterial, requiredQuantity);

        // Produce the item
        Item producedItem = ProduceBlacksmithItem(itemType, successChance);

        // Add the produced item to the inventory
        InventorySystem.Instance.AddItem(producedItem);

        // Apply rewards
        CalculateAndApplyRewards(CraftType.Smither, jobLevel, successMultiplier, statMultiplier, randomValue);

        int workDurationInMinutes = CalculateWorkDuration(jobLevel);
        timeSystem.AdvanceTimeCoroutine(0, workDurationInMinutes / 60, workDurationInMinutes % 60);

        Debug.Log($"Başarıyla {producedItem.Name} ürettiniz.");
    }

    private Item ProduceBlacksmithItem(string itemType, int successChance)
    {
        // Determine stat modifiers based on success chance
        int modifier = Mathf.RoundToInt((successChance / 100f) * 20) - 10; // Range: -10 to +10
        modifier = Mathf.Clamp(modifier, -10, 10);

        // Generate a unique ID for the item
        int itemId = UnityEngine.Random.Range(1000, 9999);

        string itemName;
        int baseValue;

        if (itemType.ToLower() == "weapon")
        {
            // Random weapon names
            string[] weapons = { "Iron Sword", "Steel Dagger", "Battle Axe", "War Hammer" };
            itemName = weapons[UnityEngine.Random.Range(0, weapons.Length)];
            baseValue = 150;
        }
        else
        {
            // Random armor names
            string[] armors = { "Iron Armor", "Steel Chestplate", "Chainmail", "Plate Armor" };
            itemName = armors[UnityEngine.Random.Range(0, armors.Length)];
            baseValue = 200;
        }

        // Create the item with the calculated modifiers
        Item newItem = new Item(
            itemId,
            itemName,
            baseValue,
            itemType.ToLower() == "weapon" ? ItemCategory.Weapon : ItemCategory.Armor,
            strengthModifier: modifier,
            constitutionModifier: modifier,
            dexterityModifier: 0,
            charismaModifier: 0,
            quantity: 1
        );

        Debug.Log($"{itemName} üretildi. Stat Modifiers: STR {modifier}, CON {modifier}");
        return newItem;
    }

    private void CalculateAndApplyRewards(CraftType craftType, int jobLevel, float successMultiplier, float statMultiplier, float randomValue)
    {
        int difficultyIndex = GetDifficultyIndex(jobLevel);
        float baseMultiplier = 1 + (playerData.Level / 10f);
        float goldModifier = 0.5f;
        float expModifier = 0.5f;

        // Altın ödülü hesaplama
        float rewardGold = ((difficultyIndex * successMultiplier * goldModifier) * randomValue + statMultiplier) * baseMultiplier;
        int silverReward = Mathf.RoundToInt(rewardGold);
        economySystem.AddSilver(silverReward);

        // Deneyim ödülü hesaplama
        float rewardExp = ((difficultyIndex * successMultiplier * expModifier) * randomValue + statMultiplier) * baseMultiplier;
        int craftExp = Mathf.RoundToInt(rewardExp);
        ExperienceSystem.UpdateCraftLevel(playerData, craftType, craftExp);

        int characterExpGain = Mathf.RoundToInt(rewardExp);
        PlayerStatHandler.Instance.AddCharacterExperience(characterExpGain);

        // Craft türüne göre stat artışlarını belirle
        switch (craftType)
        {
            case CraftType.Smither:
                if (UnityEngine.Random.Range(0, 100) < 50)
                {
                    PlayerStatHandler.Instance.AddStats(StatType.Strength, 1);
                    Debug.Log("Strength statı kazandınız!");
                }
                if (UnityEngine.Random.Range(0, 100) < 50)
                {
                    PlayerStatHandler.Instance.AddStats(StatType.Constitution, 1);
                    Debug.Log("Constitution statı kazandınız!");
                }
                break;

            case CraftType.Tanner:
                if (UnityEngine.Random.Range(0, 100) < 50)
                {
                    PlayerStatHandler.Instance.AddStats(StatType.Dexterity, 1);
                    Debug.Log("Dexterity statı kazandınız!");
                }
                break;

            case CraftType.Alchemist:
                if (UnityEngine.Random.Range(0, 100) < 50)
                {
                    PlayerStatHandler.Instance.AddStats(StatType.Dexterity, 1);
                    Debug.Log("Dexterity statı kazandınız!");
                }
                if (UnityEngine.Random.Range(0, 100) < 50)
                {
                    PlayerStatHandler.Instance.AddStats(StatType.Charisma, 1);
                    Debug.Log("Charisma statı kazandınız!");
                }
                break;
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
