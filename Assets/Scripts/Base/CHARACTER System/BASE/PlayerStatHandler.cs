using System;
using UnityEngine;
using NEXUS.Utilities;
using System.Collections.Generic;

public class PlayerStatHandler : MonoBehaviour
{
    public static PlayerStatHandler Instance { get; private set; }

    public JSONDataHandler JSONhandler;
    private EconomySystem economySystem;
    public Item EquippedSword { get; private set; }
    public Item EquippedArmor { get; private set; }
    public Item EquippedPotion { get; private set; }
    public Item EquippedMisc { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    public PlayerData pd = new PlayerData();

    private void Start()
    {
        economySystem = new EconomySystem(pd);
        //UpdateArmyCapacity();
    }

    public void Wrappers(int slot)
    {
        JSONhandler = new JSONDataHandler(slot);
        PlayerDataWrapper wrapper = JSONhandler.LoadData<PlayerDataWrapper>("playerData.json");
        pd = wrapper != null ? wrapper.pd : new PlayerData();
        CompanionListWrapper companionWrapper = JSONhandler.LoadData<CompanionListWrapper>("companions.json");
        pd.Companions = companionWrapper != null ? companionWrapper.Companions : new List<Companion>();
    }

    void OnEnable()
    {
        ExperienceSystem.OnLevelUp += LevelUp;
        ExperienceSystem.OnlevelDown += LevelDown;
        ExperienceSystem.OnExperienceNegative += () => Debug.Log("Forget everything you know.");
    }

    void OnDisable()
    {
        ExperienceSystem.OnLevelUp -= LevelUp;
        ExperienceSystem.OnlevelDown -= LevelDown;
    }
    public void AddCharacterExperience(int xp)
    {
        ExperienceSystem.AddExperience(pd, xp);
    }
    public void AddSilverToPlayer(int silver)
    {
        economySystem.AddSilver(silver);
    }

    public void LevelUp()
    {
        Debug.Log("Level Up!");
    }

    public void LevelDown()
    {
        Debug.Log("Level Down!");
    }

    public void AddStats(string statType, int amount)
    {
        switch (statType.ToLower())
        {
            case "strength":
                pd.Strength += amount;
                Debug.Log($"STR increased by {amount}. Total: {pd.Strength}");
                break;
            case "constitution":
                pd.Constitution += amount;
                Debug.Log($"CONST increased by {amount}. Total: {pd.Constitution}");
                break;
            case "charisma":
                pd.Charisma += amount;
                Debug.Log($"CHA increased by {amount}. Total: {pd.Charisma}");
                break;
            case "dexterity":
                pd.Dexterity += amount;
                Debug.Log($"DEX increased by {amount}. Total: {pd.Dexterity}");
                break;
            default:
                Debug.LogWarning($"Unknown stat type: {statType}");
                break;
        }
    }
    private void OnApplicationQuit()
    {
        EndWrappers();
    }

    public void EndWrappers()
    {
        JSONhandler = new JSONDataHandler(PlayerPrefs.GetInt("Slot"));
        JSONhandler.SaveData(new PlayerDataWrapper { pd = pd }, "playerData.json");
    }
    public void EquipItem(Item item)
    {
        UnequipItem(item.Category); // Unequip any existing item in this slot
        ApplyModifiers(item);       // Apply the item's stat modifiers

        // Assign the item to the appropriate slot
        switch (item.Category)
        {
            case ItemCategory.Weapon:
                EquippedSword = item;
                break;
            case ItemCategory.Armor:
                EquippedArmor = item;
                break;
            case ItemCategory.Potion:
                EquippedPotion = item;
                break;
            case ItemCategory.CraftingMaterial:
                Debug.Log("Cannot equip crafting materials.");
                return;
            case ItemCategory.Resource:
                Debug.Log("Cannot equip resources.");
                return;
            case ItemCategory.Misc:
                EquippedMisc = item;
                break;
        }
    }
    public void UnequipItem(ItemCategory category)
    {
        Item itemToUnequip = null;

        switch (category)
        {
            case ItemCategory.Weapon:
                itemToUnequip = EquippedSword;
                EquippedSword = null;
                break;
            case ItemCategory.Armor:
                itemToUnequip = EquippedArmor;
                EquippedArmor = null;
                break;
            case ItemCategory.Potion:
                itemToUnequip = EquippedPotion;
                EquippedPotion = null;
                break;
            case ItemCategory.Misc:
                itemToUnequip = EquippedMisc;
                EquippedMisc = null;
                break;
        }

        if (itemToUnequip != null)
        {
            RemoveModifiers(itemToUnequip);
        }
    }
    private void ApplyModifiers(Item item)
    {
        pd.Strength += item.StrengthModifier;
        pd.Constitution += item.ConstitutionModifier;
        pd.Dexterity += item.DexterityModifier;
        pd.Charisma += item.CharismaModifier;
    }
    private void RemoveModifiers(Item item)
    {
        pd.Strength -= item.StrengthModifier;
        pd.Constitution -= item.ConstitutionModifier;
        pd.Dexterity -= item.DexterityModifier;
        pd.Charisma -= item.CharismaModifier;
    }
    public void EquipItem(Item item)
    {
        UnequipItem(item.Category); // Unequip any existing item in this slot
        ApplyModifiers(item);       // Apply the item's stat modifiers

        // Assign the item to the appropriate slot
        switch (item.Category)
        {
            case ItemCategory.Weapon:
                EquippedSword = item;
                break;
            case ItemCategory.Armor:
                EquippedArmor = item;
                break;
            case ItemCategory.Potion:
                EquippedPotion = item;
                break;
            case ItemCategory.CraftingMaterial:
                Debug.Log("Cannot equip crafting materials.");
                return;
            case ItemCategory.Resource:
                Debug.Log("Cannot equip resources.");
                return;
            case ItemCategory.Misc:
                EquippedMisc = item;
                break;
        }
    }
    public void UnequipItem(ItemCategory category)
    {
        Item itemToUnequip = null;

        switch (category)
        {
            case ItemCategory.Weapon:
                itemToUnequip = EquippedSword;
                EquippedSword = null;
                break;
            case ItemCategory.Armor:
                itemToUnequip = EquippedArmor;
                EquippedArmor = null;
                break;
            case ItemCategory.Potion:
                itemToUnequip = EquippedPotion;
                EquippedPotion = null;
                break;
            case ItemCategory.Misc:
                itemToUnequip = EquippedMisc;
                EquippedMisc = null;
                break;
        }

        if (itemToUnequip != null)
        {
            RemoveModifiers(itemToUnequip);
        }
    }
    private void ApplyModifiers(Item item)
    {
        pd.Strength += item.StrengthModifier;
        pd.Constitution += item.ConstitutionModifier;
        pd.Dexterity += item.DexterityModifier;
        pd.Charisma += item.CharismaModifier;
    }
    private void RemoveModifiers(Item item)
    {
        pd.Strength -= item.StrengthModifier;
        pd.Constitution -= item.ConstitutionModifier;
        pd.Dexterity -= item.DexterityModifier;
        pd.Charisma -= item.CharismaModifier;
    }
    /// <summary>
    /// Günlük rasyon tüketimini gerçekleştirir.
    /// </summary>
    public void ConsumeDailyRations()
    {
        int totalConsumption = pd.PlayerArmy.GetTotalUnits() + pd.Companions.Count + 1; // Army + Companions + Player

        if (pd.Rations >= totalConsumption)
        {
            DecreaseRations(totalConsumption);
            Debug.Log($"Consumed {totalConsumption} rations (Player + Army + Companions).");
        }
        else
        {
            int missingRations = totalConsumption - pd.Rations;
            DecreaseRations(pd.Rations);
            IncreaseExhaustion();
            Debug.Log($"Not enough rations! Missing: {missingRations}. Increased exhaustion level.");
        }
    }
    /// <summary>
    /// Ordunun kapasitesini günceller.
    /// </summary>
    public void UpdateArmyCapacity()
    {
        int maxUnits = pd.Charisma * 10; // Charisma stat * 10
        int currentUnits = pd.PlayerArmy.GetTotalUnits();

        if (currentUnits > maxUnits)
        {
            Debug.LogWarning($"Army exceeds max capacity ({maxUnits}). Excess units will not count.");
            foreach (var unit in pd.PlayerArmy.Units)
            {
                if (maxUnits <= 0)
                    break;

                int allowedUnits = Mathf.Min(unit.Count, maxUnits);
                maxUnits -= allowedUnits;
                unit.Count = allowedUnits;
            }
        }
        Debug.Log($"Army capacity updated: {pd.PlayerArmy.GetTotalUnits()}/{pd.Charisma * 10}");
    }

    /// <summary>
    /// Rasyonları azaltır.
    /// </summary>
    /// <param name="value">Azaltılacak rasyon sayısı.</param>
    public void DecreaseRations(int value)
    {
        pd.Rations = Mathf.Max(0, pd.Rations - value);
        Debug.Log($"Rations decreased by {value}. Remaining: {pd.Rations}");
    }

    /// <summary>
    /// Rasyonları artırır.
    /// </summary>
    /// <param name="value">Artırılacak rasyon sayısı.</param>
    public void IncreaseRations(int value)
    {
        pd.Rations += value;
        Debug.Log($"Rations increased by {value}. Total: {pd.Rations}");
    }

    /// <summary>
    /// Yorgunluk seviyesini artırır.
    /// </summary>
    public void IncreaseExhaustion()
    {
        pd.CurrentExhaustionLevel += 1;
        Debug.Log($"Increased exhaustion. Current level: {pd.CurrentExhaustionLevel}");

        // Yorgunluk seviyesinin maksimum seviyeyi aştığını kontrol et
        CheckExhaustionMaxed();
    }

    /// <summary>
    /// Yorgunluk seviyesinin maksimum seviyeyi aşıp aşmadığını kontrol eder.
    /// </summary>
    public void CheckExhaustionMaxed()
    {
        if (pd.CurrentExhaustionLevel >= pd.MaxExhaustionLevel)
        {
            GameManager.Instance.Death(); // Oyuncunun ölmesini tetikler
        }
    }

    /// <summary>
    /// Ordunun kapasitesini günceller.
    /// </summary>
    public void AddUnitToArmy(UnitType type, int count)
    {
        var existingUnit = pd.PlayerArmy.Units.Find(unit => unit.Type == type);
        if (existingUnit != null)
        {
            existingUnit.Count += count;
        }
        else
        {
            pd.PlayerArmy.Units.Add(new Unit(type, count));
        }
        UpdateArmyCapacity();
    }

    /// <summary>
    /// Ordudan birimleri çıkarır.
    /// </summary>
    public void RemoveUnitFromArmy(UnitType type, int count)
    {
        var unit = pd.PlayerArmy.Units.Find(u => u.Type == type);
        if (unit != null)
        {
            unit.Count = Mathf.Max(0, unit.Count - count);
            if (unit.Count == 0)
            {
                pd.PlayerArmy.Units.Remove(unit);
            }
        }
        UpdateArmyCapacity();
    }

    public void UpdateCharisma(int newCharismaValue)
    {
        pd.Charisma = newCharismaValue;
        UpdateArmyCapacity();
        Debug.Log($"Charisma updated to {newCharismaValue}. Army capacity recalculated.");
    }

    public int GetExhaustionLevel()
    {
        return pd.CurrentExhaustionLevel;
    }

    public int GetRations()
    {
        return pd.Rations;
    }

    public void SetExhaustionLevel(int nextValue)
    {
        pd.CurrentExhaustionLevel = nextValue;
        Print($"Exhaustion level set to: {GetExhaustionLevel()}");
    }

    public void ConsumeArmyRations()
    {
        int totalUnits = pd.PlayerArmy.GetTotalUnits();

        if (pd.Rations >= totalUnits)
        {
            DecreaseRations(totalUnits);
            Debug.Log($"Army consumed {totalUnits} rations.");
        }
        else
        {
            int missingRations = totalUnits - pd.Rations;
            DecreaseRations(pd.Rations);
            IncreaseExhaustion();
            Debug.Log($"Not enough rations! Missing: {missingRations}. Increased exhaustion level.");
        }
    }


    void Print(string message)
    {
        Debug.Log($"{message}\nSender:\"{this.GetType().Name}\" class in \"{this.gameObject.name}\" object");
    }
}

[System.Serializable]
public class PlayerDataWrapper
{
    public PlayerData pd;
}

[System.Serializable]
public class CompanionListWrapper
{
    public List<Companion> Companions;
}