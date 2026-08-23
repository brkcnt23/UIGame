using UnityEngine;
using NEXUS.Utilities;
using System.Collections.Generic;

public class CraftWorkSystem : MonoBehaviour
{
    private PlayerData playerData;
    private TimeSystem timeSystem;

    public ItemSpriteDatabase spriteDatabase;
    public ItemDatabase itemDatabase;

    public static CraftWorkSystem Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        RefreshReferences();

        if (itemDatabase == null)
        {
            itemDatabase = Resources.Load<ItemDatabase>("ItemDatabase");
            if (itemDatabase == null)
                Debug.LogWarning("CraftWorkSystem: ItemDatabase not found in Resources.");
        }
    }

    private void RefreshReferences()
    {
        if (PlayerStatHandler.Instance != null)
            playerData = PlayerStatHandler.Instance.pd;

        if (TimeSystem.Instance != null)
            timeSystem = TimeSystem.Instance;
    }

    public void WorkAsBlacksmith(int jobLevel, string itemType)
    {
        if (!ValidateSystems("blacksmith")) return;

        int levelDifference = GetCraftLevel(CraftDiscipline.Smither) - jobLevel;
        if (levelDifference < -5)
        {
            Debug.Log("Beceri seviyeniz bu işi yapmak için çok düşük.");
            return;
        }

        int requiredMaterialId = 6; // Iron Ingot
        int requiredQuantity = itemType.ToLower() == "armor" ? 5 : 2;

        if (itemType.ToLower() != "weapon" && itemType.ToLower() != "armor")
        {
            Debug.Log("Geçersiz eşya türü. Lütfen 'weapon' veya 'armor' seçin.");
            return;
        }

        var stateManager = GameBootstrapper.State;
        if (stateManager == null) return;

        bool hasItem = stateManager.GetValue(state =>
        {
            var item = state.Inventory.Items.Find(i => i.ItemId == requiredMaterialId);
            return item != null && item.Quantity >= requiredQuantity;
        });

        if (!hasItem)
        {
            Debug.Log($"Yeterli malzeme yok. Gerekli miktar: {requiredQuantity}");
            return;
        }

        float maxBonus = 10 + (playerData.Charisma / 2f);
        float randomValue = UnityEngine.Random.Range(0f, maxBonus);

        float successMultiplier = (playerData.Strength + playerData.Constitution + randomValue) / 2f;
        float statMultiplier = (playerData.Strength + playerData.Constitution) / 10f;
        int successChance = Mathf.Clamp(50 + (levelDifference * 5) + Mathf.RoundToInt(statMultiplier * 5), 0, 100);

        if (Dice.RollD100() >= successChance)
        {
            Debug.Log("Üretim başarısız oldu.");
            AdvanceCraftTime(jobLevel);
            return;
        }

        GameBootstrapper.Events?.Dispatch(new RemoveItemEvent(requiredMaterialId, requiredQuantity));

        ProduceBlacksmithItem(itemType, successChance);
        CalculateAndApplyRewards(CraftDiscipline.Smither, jobLevel, successMultiplier, statMultiplier, randomValue);

        AdvanceCraftTime(jobLevel);
        RefreshUI();
    }

    public void WorkAsTanner(int jobLevel)
    {
        if (!ValidateSystems("tanner")) return;

        int levelDifference = GetCraftLevel(CraftDiscipline.Tanner) - jobLevel;
        if (levelDifference < -5)
        {
            Debug.Log("Beceri seviyeniz bu işi yapmak için çok düşük.");
            return;
        }

        int requiredMaterialId = 7; // Leather
        int requiredQuantity = 1;

        var stateManager = GameBootstrapper.State;
        if (stateManager == null) return;

        bool hasItem = stateManager.GetValue(state =>
        {
            var item = state.Inventory.Items.Find(i => i.ItemId == requiredMaterialId);
            return item != null && item.Quantity >= requiredQuantity;
        });

        if (!hasItem)
        {
            Debug.Log($"Yeterli malzeme yok. Gerekli miktar: {requiredQuantity}");
            return;
        }

        float maxBonus = 10 + (playerData.Charisma / 2f);
        float randomValue = UnityEngine.Random.Range(0f, maxBonus);

        float successMultiplier = (playerData.Dexterity + playerData.Constitution + randomValue) / 2f;
        float statMultiplier = (playerData.Dexterity + playerData.Constitution) / 10f;
        int successChance = Mathf.Clamp(50 + (levelDifference * 5) + Mathf.RoundToInt(statMultiplier * 5), 0, 100);

        if (Dice.RollD100() >= successChance)
        {
            Debug.Log("Üretim başarısız oldu.");
            AdvanceCraftTime(jobLevel);
            return;
        }

        GameBootstrapper.Events?.Dispatch(new RemoveItemEvent(requiredMaterialId, requiredQuantity));

        ProduceTanningItem(successChance);
        CalculateAndApplyRewards(CraftDiscipline.Tanner, jobLevel, successMultiplier, statMultiplier, randomValue);

        AdvanceCraftTime(jobLevel);
        RefreshUI();
    }

    public void WorkAsAlchemist(int jobLevel)
    {
        if (!ValidateSystems("alchemist")) return;

        int levelDifference = GetCraftLevel(CraftDiscipline.Alchemist) - jobLevel;
        if (levelDifference < -5)
        {
            Debug.Log("Beceri seviyeniz bu işi yapmak için çok düşük.");
            return;
        }

        int requiredMaterialId = 8; // Herbs
        int requiredQuantity = 5;

        var stateManager = GameBootstrapper.State;
        if (stateManager == null) return;

        bool hasItem = stateManager.GetValue(state =>
        {
            var item = state.Inventory.Items.Find(i => i.ItemId == requiredMaterialId);
            return item != null && item.Quantity >= requiredQuantity;
        });

        if (!hasItem)
        {
            Debug.Log($"Yeterli malzeme yok. Gerekli miktar: {requiredQuantity}");
            return;
        }

        float maxBonus = 10 + (playerData.Charisma / 2f);
        float randomValue = UnityEngine.Random.Range(0f, maxBonus);

        float successMultiplier = (playerData.Dexterity + playerData.Charisma + randomValue) / 2f;
        float statMultiplier = (playerData.Dexterity + playerData.Charisma) / 10f;
        int successChance = Mathf.Clamp(50 + (levelDifference * 5) + Mathf.RoundToInt(statMultiplier * 5), 0, 100);

        if (Dice.RollD100() >= successChance)
        {
            Debug.Log("Üretim başarısız oldu.");
            AdvanceCraftTime(jobLevel);
            return;
        }

        GameBootstrapper.Events?.Dispatch(new RemoveItemEvent(requiredMaterialId, requiredQuantity));

        ProduceAlchemyItem(successChance);
        CalculateAndApplyRewards(CraftDiscipline.Alchemist, jobLevel, successMultiplier, statMultiplier, randomValue);

        AdvanceCraftTime(jobLevel);
        RefreshUI();
    }

    private void CalculateAndApplyRewards(CraftDiscipline craftType, int jobLevel, float successMultiplier, float statMultiplier, float randomValue)
    {
        if (playerData == null)
        {
            Debug.LogError("CraftWorkSystem: playerData is null.");
            return;
        }

        int difficultyIndex = GetDifficultyIndex(jobLevel);
        float baseMultiplier = 1 + (playerData.Level / 10f);
        float goldModifier = 0.5f;
        float expModifier = 0.5f;

        float rewardGold = ((difficultyIndex * successMultiplier * goldModifier) * randomValue + statMultiplier) * baseMultiplier;
        int silverReward = Mathf.RoundToInt(rewardGold);

        playerData.AddMoney(0, silverReward);

        float rewardExp = ((difficultyIndex * successMultiplier * expModifier) * randomValue + statMultiplier) * baseMultiplier;
        int craftExp = Mathf.RoundToInt(rewardExp);

        GrantWorkSkillXP(craftType, craftExp);

        if (PlayerStatHandler.Instance != null)
            PlayerStatHandler.Instance.AddCharacterExperience(craftExp);

        switch (craftType)
        {
            case CraftDiscipline.Smither:
                if (UnityEngine.Random.Range(0, 100) < 50)
                    PlayerStatHandler.Instance.AddStatXP(StatType.Strength, 50);

                if (UnityEngine.Random.Range(0, 100) < 50)
                    PlayerStatHandler.Instance.AddStatXP(StatType.Constitution, 50);
                break;

            case CraftDiscipline.Tanner:
                if (UnityEngine.Random.Range(0, 100) < 50)
                    PlayerStatHandler.Instance.AddStatXP(StatType.Dexterity, 50);
                break;

            case CraftDiscipline.Alchemist:
                if (UnityEngine.Random.Range(0, 100) < 50)
                    PlayerStatHandler.Instance.AddStatXP(StatType.Dexterity, 50);

                if (UnityEngine.Random.Range(0, 100) < 50)
                    PlayerStatHandler.Instance.AddStatXP(StatType.Charisma, 50);
                break;
        }

        Debug.Log($"Üretim başarılı! {silverReward} gümüş ve {craftExp} crafting EXP kazandınız.");
    }

    private Item ProduceBlacksmithItem(string itemType, int successChance)
    {
        if (spriteDatabase == null)
        {
            Debug.LogError("CraftWorkSystem: spriteDatabase is null.");
            return null;
        }

        int itemId = UnityEngine.Random.Range(1000, 9999);

        string itemName;
        int baseValue;
        ItemCategory category;
        int quality = Mathf.Clamp(successChance / 20, 1, 3);

        if (itemType.ToLower() == "weapon")
        {
            string[] weapons = { "Iron Sword", "Steel Dagger", "Battle Axe", "War Hammer", "Morning Star" };
            itemName = weapons[UnityEngine.Random.Range(0, weapons.Length)];
            baseValue = 150;
            category = ItemCategory.Weapon;
        }
        else
        {
            string[] armors = { "Iron Armor", "Steel Chestplate", "Chainmail", "Plate Armor" };
            itemName = armors[UnityEngine.Random.Range(0, armors.Length)];
            baseValue = 200;
            category = ItemCategory.Armor;
        }

        Sprite itemSprite = spriteDatabase.GetSprite(category, quality);

        int roundedSuccess = Mathf.RoundToInt(successChance / 5f) * 5;
        int modifier = Mathf.RoundToInt((roundedSuccess / 100f) * 20) - 10;

        int strengthModifier = Mathf.RoundToInt(UnityEngine.Random.Range(modifier - 1, modifier + 2));
        int constitutionModifier = Mathf.RoundToInt(UnityEngine.Random.Range(modifier - 1, modifier + 2));
        int dexterityModifier = Mathf.RoundToInt(UnityEngine.Random.Range(modifier - 1, modifier + 2));

        float qualityMultiplier = 1 + ((successChance - 50) / 100f);
        float randomFactor = UnityEngine.Random.Range(-0.05f, 0.05f);
        float newValue = baseValue * qualityMultiplier * (1 + randomFactor);

        int finalValue = Mathf.Max(Mathf.RoundToInt(newValue), Mathf.RoundToInt(baseValue * 0.5f));
        int gold = finalValue / 100;
        int silver = finalValue % 100;

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
        return newItem;
    }

    private Item ProduceTanningItem(int successChance)
    {
        if (spriteDatabase == null)
        {
            Debug.LogError("CraftWorkSystem: spriteDatabase is null.");
            return null;
        }

        int itemId = UnityEngine.Random.Range(1000, 9999);
        string itemName = "Leather Boots";
        int baseValue = 100;
        ItemCategory category = ItemCategory.Boots;
        int quality = Mathf.Clamp(successChance / 20, 1, 3);

        int roundedSuccess = Mathf.RoundToInt(successChance / 5f) * 5;
        int modifier = Mathf.RoundToInt((roundedSuccess / 100f) * 20) - 10;

        List<StatModifier> modifiers = new List<StatModifier>();

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

        float qualityMultiplier = 1 + ((successChance - 50) / 100f);
        float randomFactor = UnityEngine.Random.Range(-0.05f, 0.05f);
        float newValue = baseValue * qualityMultiplier * (1 + randomFactor);

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
        return newItem;
    }

    private Item ProduceAlchemyItem(int successChance)
    {
        if (spriteDatabase == null)
        {
            Debug.LogError("CraftWorkSystem: spriteDatabase is null.");
            return null;
        }

        int itemId = UnityEngine.Random.Range(1000, 9999);
        string itemName = "Health Potion";
        int baseValue = 50;
        int quality = Mathf.Clamp(successChance / 20, 1, 3);

        int roundedSuccess = Mathf.RoundToInt(successChance / 5f) * 5;
        int healthRecovery = Mathf.RoundToInt((roundedSuccess / 100f) * 50) + 10;
        int exhaustionReduction = Mathf.RoundToInt((roundedSuccess / 100f) * 4) + 1;

        float qualityMultiplier = 1 + ((successChance - 50) / 100f);
        float randomFactor = UnityEngine.Random.Range(-0.05f, 0.05f);
        float newValue = baseValue * qualityMultiplier * (1 + randomFactor);

        int finalValue = Mathf.Max(Mathf.RoundToInt(newValue), Mathf.RoundToInt(baseValue * 0.5f));
        int gold = finalValue / 100;
        int silver = finalValue % 100;

        Sprite itemSprite = spriteDatabase.GetSprite(ItemCategory.Potion, quality);

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
        return newItem;
    }

    private bool ValidateSystems(string professionName)
    {
        RefreshReferences();

        if (playerData == null)
        {
            Debug.LogError($"CraftWorkSystem: playerData is null! Cannot work as {professionName}.");
            return false;
        }

        if (timeSystem == null)
        {
            Debug.LogError($"CraftWorkSystem: timeSystem is null! Cannot work as {professionName}.");
            return false;
        }

        if (InventorySystem.Instance == null)
        {
            Debug.LogError($"CraftWorkSystem: InventorySystem.Instance is null! Cannot work as {professionName}.");
            return false;
        }

        return true;
    }

    private void AdvanceCraftTime(int jobLevel)
    {
        int workDurationInMinutes = CalculateWorkDuration(jobLevel);

        if (timeSystem != null)
        {
            StartCoroutine(timeSystem.AdvanceTimeCoroutine(0, workDurationInMinutes / 60, workDurationInMinutes % 60));
        }
    }

    private int GetDifficultyIndex(int jobLevel)
    {
        if (jobLevel >= 1 && jobLevel <= 5) return 1;
        if (jobLevel > 5 && jobLevel <= 8) return 2;
        if (jobLevel > 8 && jobLevel <= 10) return 3;
        return 1;
    }

    private int GetCraftLevel(CraftDiscipline craftType)
    {
        if (playerData == null)
            return 1;

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

    private int CalculateWorkDuration(int jobLevel)
    {
        int baseWorkDuration = 8 * 60;
        return baseWorkDuration + ((jobLevel - 1) * 30);
    }

    private void AddCraftedItem(Item itemToAdd)
    {
        if (itemToAdd == null)
            return;

        if (InventorySystem.Instance != null)
        {
            GameBootstrapper.Events?.Dispatch(new AddItemEvent(itemToAdd.ID, 1));
            Debug.Log($"Crafted {itemToAdd.Name} and added to inventory.");
        }
    }

    private void GrantWorkSkillXP(CraftDiscipline craftType, int amount)
    {
        // Origins and traits shape how fast a craft is learned. Applied once,
        // here, so every path that grants discipline XP gets it - the effect
        // was defined and named in the profile panel but never reached the XP.
        amount = TraitSystem.ApplyOrPass(EffectType.SkillXpGain, amount, CraftDisciplineNames.Qualifier(craftType));

        switch (craftType)
        {
            case CraftDiscipline.Smither:
                AddSkillXP(ref playerData.SmitherSkillXP, ref playerData.SmitherSkillLevel, amount);
                break;
            case CraftDiscipline.Tanner:
                AddSkillXP(ref playerData.TannerSkillXP, ref playerData.TannerSkillLevel, amount);
                break;
            case CraftDiscipline.Carpenter:
                AddSkillXP(ref playerData.CarpenterSkillXP, ref playerData.CarpenterSkillLevel, amount);
                break;
            case CraftDiscipline.Mason:
                AddSkillXP(ref playerData.MasonSkillXP, ref playerData.MasonSkillLevel, amount);
                break;
            case CraftDiscipline.Alchemist:
                AddSkillXP(ref playerData.AlchemistSkillXP, ref playerData.AlchemistSkillLevel, amount);
                break;
        }
    }

    private void AddSkillXP(ref int xpField, ref int levelField, int amount)
    {
        xpField += amount;

        while (xpField >= 100)
        {
            xpField -= 100;
            levelField += 1;
        }
    }

    private void RefreshUI()
    {
        if (PlayerUISystem.Instance != null)
            PlayerUISystem.Instance.UpdateUIObjects();

        // UI updates handled by StateManager listeners
    }
}