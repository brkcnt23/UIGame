using UnityEngine;
using NEXUS.Utilities;
using System.Collections.Generic;
public enum CraftType
{
    Smither,
    Tanner,
    Carpenter,
    Mason,
    Alchemist
}

public class CraftingSystem : MonoBehaviour
{
    private PlayerData playerData;
    private EconomySystem economySystem;
    private TimeSystem timeSystem;
    public ItemSpriteDatabase spriteDatabase;
    public static CraftingSystem Instance;

    //check instance in awake method
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public CraftingSystem(PlayerData pd, TimeSystem ts)
    {
        playerData = pd;
        economySystem = new EconomySystem(playerData);
        timeSystem = ts;
    }


    public void WorkAsBlacksmith(int jobLevel, string itemType)
    {
        int levelDifference = GetCraftLevel(CraftType.Smither) - jobLevel;

        // Skill level check
        if (levelDifference < -5)
        {
            Debug.Log("Beceri seviyeniz bu işi yapmak için çok düşük.");
            return;
        }

        // Determine required material and quantity based on item type
        Item requiredMaterial = new Item(5, "Iron Ingot", 1, 0, ItemCategory.CraftingMaterial, 1);
        int requiredQuantity = itemType.ToLower() == "armor" ? 5 : 2; // Armor requires 5 ingots, weapon requires 2

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

        ProduceBlacksmithItem(itemType, successChance);

        // Apply rewards
        CalculateAndApplyRewards(CraftType.Smither, jobLevel, successMultiplier, statMultiplier, randomValue);

        int workDurationInMinutes = CalculateWorkDuration(jobLevel);
        timeSystem.AdvanceTimeCoroutine(0, workDurationInMinutes / 60, workDurationInMinutes % 60);
    }

    public void WorkAsTanner(int jobLevel)
    {
        int levelDifference = GetCraftLevel(CraftType.Tanner) - jobLevel;

        // Skill level check
        if (levelDifference < -5)
        {
            Debug.Log("Beceri seviyeniz bu işi yapmak için çok düşük.");
            return;
        }

        // Determine required material and quantity
        Item requiredMaterial = new Item(6, "Leather", 0, 50, ItemCategory.CraftingMaterial, 1);
        int requiredQuantity = 1; // Boots üretimi için 1 Leather gerekli

        // Material check
        if (!InventorySystem.Instance.HasItem(requiredMaterial.ID, requiredQuantity))
        {
            Debug.Log($"Yeterli {requiredMaterial.Name} yok. Gerekli miktar: {requiredQuantity}");
            return;
        }

        // Calculate success chance
        float maxBonus = 10 + (playerData.Charisma / 2f);
        float randomValue = UnityEngine.Random.Range(0f, maxBonus);

        float successMultiplier = (playerData.Dexterity + playerData.Constitution + randomValue) / 2f;
        float statMultiplier = (playerData.Dexterity + playerData.Constitution) / 10f;
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
        InventorySystem.Instance.RemoveItem(requiredMaterial, requiredQuantity);

        // Produce the item
        ProduceTanningItem(successChance);

        // Apply rewards
        CalculateAndApplyRewards(CraftType.Tanner, jobLevel, successMultiplier, statMultiplier, randomValue);

        int workDurationInMinutes = CalculateWorkDuration(jobLevel);
        timeSystem.AdvanceTimeCoroutine(0, workDurationInMinutes / 60, workDurationInMinutes % 60);
    }

    public void WorkAsAlchemist(int jobLevel)
    {
        int levelDifference = GetCraftLevel(CraftType.Alchemist) - jobLevel;

        // Skill level check
        if (levelDifference < -5)
        {
            Debug.Log("Beceri seviyeniz bu işi yapmak için çok düşük.");
            return;
        }

        // Determine required material and quantity
        Item requiredMaterial = new Item(7, "Herb", 0, 30, ItemCategory.CraftingMaterial, 1);
        int requiredQuantity = 5; // Potion üretimi için 5 Herb gerekli

        // Material check
        if (!InventorySystem.Instance.HasItem(requiredMaterial.ID, requiredQuantity))
        {
            Debug.Log($"Yeterli {requiredMaterial.Name} yok. Gerekli miktar: {requiredQuantity}");
            return;
        }

        // Calculate success chance
        float maxBonus = 10 + (playerData.Charisma / 2f);
        float randomValue = UnityEngine.Random.Range(0f, maxBonus);

        float successMultiplier = (playerData.Dexterity + playerData.Charisma + randomValue) / 2f;
        float statMultiplier = (playerData.Dexterity + playerData.Charisma) / 10f;
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

        // Produce the potion
        ProduceAlchemyItem(successChance);

        // Apply rewards
        CalculateAndApplyRewards(CraftType.Alchemist, jobLevel, successMultiplier, statMultiplier, randomValue);

        int workDurationInMinutes = CalculateWorkDuration(jobLevel);
        timeSystem.AdvanceTimeCoroutine(0, workDurationInMinutes / 60, workDurationInMinutes % 60);

        Debug.Log("Başarıyla Health Potion ürettiniz.");
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
                    PlayerStatHandler.Instance.AddStatXP(StatType.Strength, 50);
                    Debug.Log("Strength statı kazandınız!");
                }
                if (UnityEngine.Random.Range(0, 100) < 50)
                {
                    PlayerStatHandler.Instance.AddStatXP(StatType.Constitution, 50);
                    Debug.Log("Constitution statı kazandınız!");
                }
                break;

            case CraftType.Tanner:
                if (UnityEngine.Random.Range(0, 100) < 50)
                {
                    PlayerStatHandler.Instance.AddStatXP(StatType.Dexterity, 50);
                    Debug.Log("Dexterity statı kazandınız!");
                }
                break;

            case CraftType.Alchemist:
                if (UnityEngine.Random.Range(0, 100) < 50)
                {
                    PlayerStatHandler.Instance.AddStatXP(StatType.Dexterity, 50);
                    Debug.Log("Dexterity statı kazandınız!");
                }
                if (UnityEngine.Random.Range(0, 100) < 50)
                {
                    PlayerStatHandler.Instance.AddStatXP(StatType.Charisma, 50);
                    Debug.Log("Charisma statı kazandınız!");
                }
                break;
        }

        // Sonuçları yazdır
        Debug.Log($"Üretim başarılı! {silverReward} gümüş ve {craftExp} crafting EXP kazandınız.");
        Debug.Log($"Karakter {characterExpGain} EXP kazandı.");
    }

    private Item ProduceBlacksmithItem(string itemType, int successChance)
    {
        int itemId = UnityEngine.Random.Range(1000, 9999);

        string itemName;
        int baseValue;
        ItemCategory category;
        int quality = Mathf.Clamp(successChance / 20, 1, 3);

        if (itemType.ToLower() == "weapon")
        {
            // Random weapon names
            string[] weapons = { "Iron Sword", "Steel Dagger", "Battle Axe", "War Hammer", "Light Bow", "Hand Crossbow", "Morning Star" };
            itemName = weapons[UnityEngine.Random.Range(0, weapons.Length)];
            baseValue = 150;
            category = ItemCategory.Weapon;
        }
        else
        {
            // Random armor names
            string[] armors = { "Iron Armor", "Steel Chestplate", "Chainmail", "Plate Armor", "Adamantine Plate" };
            itemName = armors[UnityEngine.Random.Range(0, armors.Length)];
            baseValue = 200;
            category = ItemCategory.Armor;
        }

        Sprite itemSprite = spriteDatabase.GetSprite(category, quality);


        // SuccessChance'i en yakın 5'in katına yuvarla
        int roundedSuccess = Mathf.RoundToInt(successChance / 5f) * 5;
        int modifier = Mathf.RoundToInt((roundedSuccess / 100f) * 20) - 10;

        // Determine stat modifiers based on success chance
        int strengthModifier = Mathf.RoundToInt(UnityEngine.Random.Range(modifier - 1, modifier + 2));
        int constitutionModifier = Mathf.RoundToInt(UnityEngine.Random.Range(modifier - 1, modifier + 2));
        int dexterityModifier = Mathf.RoundToInt(UnityEngine.Random.Range(modifier - 1, modifier + 2));



        float qualityMultiplier = 1 + ((successChance - 50) / 100f);

        // Rastgelelik için küçük bir faktör ekle (-5% ile +5% arasında)
        float randomFactor = UnityEngine.Random.Range(-0.05f, 0.05f);

        // Nihai değeri hesapla
        float newValue = baseValue * qualityMultiplier * (1 + randomFactor);

        // Değeri yuvarla ve en az baseValue kadar olmasını sağla
        int finalValue = Mathf.Max(Mathf.RoundToInt(newValue), Mathf.RoundToInt(baseValue * 0.5f));
        int gold = finalValue / 100;
        int silver = finalValue % 100;

        // Create the item with the calculated modifiers
        List<StatModifier> modifiers = new List<StatModifier>
        {
            new StatModifier(StatType.Strength, strengthModifier, "Crafting"),
            new StatModifier(StatType.Constitution, constitutionModifier, "Crafting"),
            new StatModifier(StatType.Dexterity, dexterityModifier, "Crafting")
        };


        Item newItem = new Item(
            itemId,
            itemName,
            gold,
            silver,
            category,
            modifiers,
            itemSprite,
            quality,
            1
        );
        AddCraftedItem(newItem);
        Debug.Log($"{itemName} üretildi. Stat Modifiers: STR {strengthModifier}, CON {constitutionModifier}, DEX {dexterityModifier}");
        return newItem;
    }

    private Item ProduceTanningItem(int successChance)
    {



//item category boots ve leggings
//LEGGINGS EKLE


        int itemId = UnityEngine.Random.Range(1000, 9999);
        string itemName = "Leather Boots";
        int baseValue = 100;
        // SuccessChance'i en yakın 5'in katına yuvarla
        int roundedSuccess = Mathf.RoundToInt(successChance / 5f) * 5;
        // Modifier hesapla: -10 ile +10 arasında
        int modifier = Mathf.RoundToInt((roundedSuccess / 100f) * 20) - 10;
        ItemCategory category = ItemCategory.Boots;
        int quality = Mathf.Clamp(successChance / 20, 1, 3);

        // Rastgele stat modifikasyonları belirle
        List<StatModifier> modifiers = new List<StatModifier>();

        // Ana stat: Dexterity ve Charisma, nadiren Strength ve Constitution
        if (Dice.RollD100() < 80)
        {
            modifiers.Add(new StatModifier(StatType.Dexterity, Mathf.RoundToInt(UnityEngine.Random.Range(modifier - 1, modifier + 2))));
            modifiers.Add(new StatModifier(StatType.Charisma, Mathf.RoundToInt(UnityEngine.Random.Range(modifier - 1, modifier + 2))));
        }
        else
        {
            modifiers.Add(new StatModifier(StatType.Strength, Mathf.RoundToInt(UnityEngine.Random.Range(modifier - 1, modifier + 2))));
            modifiers.Add(new StatModifier(StatType.Constitution, Mathf.RoundToInt(UnityEngine.Random.Range(modifier - 1, modifier + 2))));
        }

        Sprite itemSprite = spriteDatabase.GetSprite(category, quality);

        // Kaliteye göre değeri hesapla
        float qualityMultiplier = 1 + ((successChance - 50) / 100f);

        // Rastgelelik için küçük bir faktör ekle (-5% ile +5% arasında)
        float randomFactor = UnityEngine.Random.Range(-0.05f, 0.05f);

        // Nihai değeri hesapla
        float newValue = baseValue * qualityMultiplier * (1 + randomFactor);

        // Değeri yuvarla ve en az baseValue kadar olmasını sağla
        int finalValue = Mathf.Max(Mathf.RoundToInt(newValue), Mathf.RoundToInt(baseValue * 0.5f));
        int gold = finalValue / 100;
        int silver = finalValue % 100;
        Item newItem = new Item(
            itemId,
            itemName,
            gold,
            silver,
            category,
            modifiers,
            itemSprite,
            quality,
            1
        );

        AddCraftedItem(newItem);
        Debug.Log($"{itemName} üretildi. Value: {finalValue} silver. Stat Modifiers: {string.Join(", ", modifiers)}");
        return newItem;
    }

    private Item ProduceAlchemyItem(int successChance)
    {
        int itemId = UnityEngine.Random.Range(1000, 9999);
        string itemName = "Health Potion";
        int baseValue = 50;
        ItemCategory category = ItemCategory.Potion;
        int quality = Mathf.Clamp(successChance / 20, 1, 3);
        // SuccessChance'i en yakın 5'in katına yuvarla
        int roundedSuccess = Mathf.RoundToInt(successChance / 5f) * 5;

        // HealthRecovery ve ExhaustionReduction hesapla: başarı oranına göre değişir
        int healthRecovery = Mathf.RoundToInt((roundedSuccess / 100f) * 50) + 10; // 10 ile 60 arasında
        int exhaustionReduction = Mathf.RoundToInt((roundedSuccess / 100f) * 4) + 1; // 1 ile 5 arasında


        // Item değeri:
        float qualityMultiplier = 1 + ((successChance - 50) / 100f);

        // Rastgelelik için küçük bir faktör ekle (-5% ile +5% arasında)
        float randomFactor = UnityEngine.Random.Range(-0.05f, 0.05f);

        // Nihai değeri hesapla
        float newValue = baseValue * qualityMultiplier * (1 + randomFactor);

        // Değeri yuvarla ve en az baseValue kadar olmasını sağla
        int finalValue = Mathf.Max(Mathf.RoundToInt(newValue), Mathf.RoundToInt(baseValue * 0.5f));
        int gold = finalValue / 100;
        int silver = finalValue % 100;

        Sprite itemSprite = spriteDatabase.GetSprite(category, quality);


        Item newItem = new Item(
            itemId,
            itemName,
            gold,
            silver,
            healthRecovery,
            exhaustionReduction,
            itemSprite,
            quality,
            1
        );
        AddCraftedItem(newItem);
        Debug.Log($"{itemName} üretildi. Value: {finalValue} silver. Health Recovery: {healthRecovery}, Exhaustion Reduction: {exhaustionReduction}");
        return newItem;
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
            return 1;
        }
    }

    private int GetCraftLevel(CraftType craftType)
    {
        switch (craftType)
        {
            case CraftType.Smither:
                return PlayerStatHandler.Instance.pd.SmitherSkillLevel;
            case CraftType.Tanner:
                return PlayerStatHandler.Instance.pd.TannerSkillLevel;
            case CraftType.Alchemist:
                return PlayerStatHandler.Instance.pd.AlchemistSkillLevel;
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
    private void AddCraftedItem(Item itemToAdd)
    {
        if (itemToAdd != null)
        {
            InventorySystem.Instance.AddItem(itemToAdd);
            Debug.Log($"Crafted {itemToAdd.Name} and added to inventory.");
        }
    }
}
