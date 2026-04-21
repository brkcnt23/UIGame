// cspell:disable
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
    public Item EquippedLeggings { get; private set; }
    public Item EquippedBoots { get; private set; }
    public Item EquippedPotion { get; private set; }
    public Item EquippedMisc { get; private set; }

    public PlayerData pd = new PlayerData();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Application.targetFrameRate = 60;
    }

    private void Start()
    {
        NormalizePlayerData();
        economySystem = new EconomySystem(pd);
    }

    private void NormalizePlayerData()
    {
        if (pd == null)
            pd = new PlayerData();

        if (pd.Companions == null)
            pd.Companions = new List<Companion>();

        if (pd.Items == null)
            pd.Items = new List<Item>();

        if (pd.ItemStacks == null)
            pd.ItemStacks = new List<ItemStackData>();

        if (pd.Quests == null)
            pd.Quests = new List<Quest_SO_Constructor>();

        if (pd.Units == null)
            pd.Units = new List<Unit>();

        if (pd.PlayerArmy == null)
            pd.PlayerArmy = new Army();

        pd.InitializeMoneyFromLegacyIfNeeded();
    }

    // -----------------------------
    // SAVE / LOAD
    // -----------------------------

    public void SavePlayerData()
    {
        if (HomeSettlementHandler.Instance != null)
            HomeSettlementHandler.Instance.SaveHomeSettlement();

        if (SettlementHandler.Instance != null)
            SettlementHandler.Instance.SaveSettlements();

        if (TravelSystem.Instance != null)
            TravelSystem.Instance.SaveTravelData();

        if (EventHandler.Instance != null)
            EventHandler.Instance.SaveEvents();

        if (SettlementHandler.Instance != null && SettlementHandler.Instance.settlement != null)
        {
            pd.LastSettlementName = SettlementHandler.Instance.settlement.Name;
        }

        pd.SyncLegacyMoneyFromMoney();

        JSONhandler = new JSONDataHandler(PlayerPrefs.GetInt("Slot"));

        try
        {
            JSONhandler.SaveData(new PlayerDataWrapper { pd = pd }, "playerData.json");
            Debug.Log("Player data saved successfully.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error saving player data: {e.Message}");
        }
    }

    public void LoadPlayerData()
    {
        JSONhandler = new JSONDataHandler(PlayerPrefs.GetInt("Slot"));
        PlayerDataWrapper wrapper = JSONhandler.LoadData<PlayerDataWrapper>("playerData.json");
        pd = wrapper != null ? wrapper.pd : new PlayerData();

        NormalizePlayerData();

        pd.PlayerArmy.SetUnits(pd.Units);

        if (EventHandler.Instance != null)
            EventHandler.Instance.LoadEvents();

        if (HomeSettlementHandler.Instance != null)
            HomeSettlementHandler.Instance.LoadHomeSettlement();

        if (SettlementHandler.Instance != null)
            SettlementHandler.Instance.LoadSettlements();

        if (MapHandler.Instance != null)
            MapHandler.Instance.LoadQuestSettlements();

        if (TravelSystem.Instance != null)
            TravelSystem.Instance.LoadTravelData();

        if (MapHandler.Instance != null)
            MapHandler.Instance.MovePlayerToLastVisitedSettlement(LastVisitedSettlement());

        if (TimeSystem.Instance != null)
            TimeSystem.Instance.InitializeLastActionTimes();

        if (PlayerUISystem.Instance != null)
            PlayerUISystem.Instance.UpdateClockText();

        economySystem = new EconomySystem(pd);
    }

    private void OnApplicationQuit()
    {
        if (GameManager.Instance != null && GameManager.Instance.isEnteredSettlement)
        {
            if (pd.HasDied)
            {
                pd = new PlayerData();
                NormalizePlayerData();
            }

            SavePlayerData();
        }
    }

    public void GetPlayerCompanions()
    {
        CompanionListWrapper wrapper = JSONhandler.LoadData<CompanionListWrapper>("playerCompanions.json");
        pd.Companions = wrapper != null ? wrapper.Companions : new List<Companion>();
    }

    // -----------------------------
    // EVENTS
    // -----------------------------

    private void OnEnable()
    {
        ExperienceSystem.OnLevelUp += LevelUp;
        ExperienceSystem.OnlevelDown += LevelDown;
        ExperienceSystem.OnExperienceNegative += OnExperienceNegative;
    }

    private void OnDisable()
    {
        ExperienceSystem.OnLevelUp -= LevelUp;
        ExperienceSystem.OnlevelDown -= LevelDown;
        ExperienceSystem.OnExperienceNegative -= OnExperienceNegative;
    }

    private void OnExperienceNegative()
    {
        Debug.Log("Forget everything you know.");
    }

    // -----------------------------
    // EXPERIENCE / MONEY
    // -----------------------------

    public void AddCharacterExperience(int xp)
    {
        ExperienceSystem.AddExperience(pd, xp);
        ExperienceSystem.UpdateCharacterLevel(pd);
    }

    public void AddSilverToPlayer(int silver)
    {
        pd.AddMoney(0, silver);
        Debug.Log($"Added {silver} Silver. New Balance: {pd.GetMoney()}");
        RefreshPlayerUI();
    }

    public bool SpendMoney(int gold, int silver)
    {
        bool success = pd.TrySpendMoney(gold, silver);

        if (!success)
        {
            Debug.Log("Not enough money!");
            return false;
        }

        Debug.Log($"Spent {gold} Gold and {silver} Silver. Remaining Balance: {pd.GetMoney()}");
        RefreshPlayerUI();
        return true;
    }

    public void LevelUp()
    {
        Debug.Log("Level Up!");
    }

    public void LevelDown()
    {
        Debug.Log("Level Down!");
    }

    // -----------------------------
    // STATS
    // -----------------------------

    public void AddStats(StatType statType, int amount)
    {
        switch (statType)
        {
            case StatType.Strength:
                pd.Strength += amount;
                Debug.Log($"STR changed by {amount}. Total: {pd.Strength}");
                break;

            case StatType.Constitution:
                pd.Constitution += amount;
                Debug.Log($"CONST changed by {amount}. Total: {pd.Constitution}");
                break;

            case StatType.Charisma:
                pd.Charisma += amount;
                Debug.Log($"CHA changed by {amount}. Total: {pd.Charisma}");
                UpdateArmyCapacity();
                break;

            case StatType.Dexterity:
                pd.Dexterity += amount;
                Debug.Log($"DEX changed by {amount}. Total: {pd.Dexterity}");
                break;

            default:
                Debug.LogWarning("Unknown stat type.");
                break;
        }
    }

    public void AddStatXP(StatType statType, int xpAmount)
    {
        switch (statType)
        {
            case StatType.Strength:
                pd.StrengthXP += xpAmount;
                while (pd.StrengthXP >= 100)
                {
                    AddStats(StatType.Strength, 1);
                    pd.StrengthXP -= 100;
                    Debug.Log($"Strength leveled up! New Strength: {pd.Strength}");
                }
                break;

            case StatType.Dexterity:
                pd.DexterityXP += xpAmount;
                while (pd.DexterityXP >= 100)
                {
                    AddStats(StatType.Dexterity, 1);
                    pd.DexterityXP -= 100;
                    Debug.Log($"Dexterity leveled up! New Dexterity: {pd.Dexterity}");
                }
                break;

            case StatType.Constitution:
                pd.ConstitutionXP += xpAmount;
                while (pd.ConstitutionXP >= 100)
                {
                    AddStats(StatType.Constitution, 1);
                    pd.ConstitutionXP -= 100;
                    Debug.Log($"Constitution leveled up! New Constitution: {pd.Constitution}");
                }
                break;

            case StatType.Charisma:
                pd.CharismaXP += xpAmount;
                while (pd.CharismaXP >= 100)
                {
                    AddStats(StatType.Charisma, 1);
                    pd.CharismaXP -= 100;
                    Debug.Log($"Charisma leveled up! New Charisma: {pd.Charisma}");
                }
                break;

            default:
                Debug.LogWarning("Unknown stat type.");
                break;
        }

        RefreshPlayerUI();
    }


// -----------------------------
// SETTLEMENT / LAST LOCATION
// -----------------------------

public Settlement LastVisitedSettlement()
{
    if (SettlementHandler.Instance == null || SettlementHandler.Instance.settlements == null)
        return HomeSettlementHandler.Instance != null ? HomeSettlementHandler.Instance.homeSettlement : null;

    Settlement lastVisited =
        !string.IsNullOrEmpty(pd.LastSettlementName)
            ? SettlementHandler.Instance.settlements.Find(s => s.Name == pd.LastSettlementName)
            : (HomeSettlementHandler.Instance != null ? HomeSettlementHandler.Instance.homeSettlement : null);

    return lastVisited;
}

// -----------------------------
// EQUIPMENT
// -----------------------------

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

        case ItemCategory.Leggings:
            itemToUnequip = EquippedLeggings;
            EquippedLeggings = null;
            break;

        case ItemCategory.Boots:
            itemToUnequip = EquippedBoots;
            EquippedBoots = null;
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

public void ApplyModifiers(Item item)
{
    if (item == null || item.Modifiers == null) return;

    foreach (var modifier in item.Modifiers)
    {
        AddStats(modifier.Type, modifier.Value);
    }
}

public void RemoveModifiers(Item item)
{
    if (item == null || item.Modifiers == null) return;

    foreach (var modifier in item.Modifiers)
    {
        AddStats(modifier.Type, -modifier.Value);
    }
}

public void EquipItem(Item item)
{
    if (item == null) return;

    UnequipItem(item.Category);
    ApplyModifiers(item);

    switch (item.Category)
    {
        case ItemCategory.Weapon:
            EquippedSword = item;
            break;

        case ItemCategory.Armor:
            EquippedArmor = item;
            break;

        case ItemCategory.Leggings:
            EquippedLeggings = item;
            break;

        case ItemCategory.Boots:
            EquippedBoots = item;
            break;

        case ItemCategory.Potion:
            EquippedPotion = item;
            break;

        case ItemCategory.Misc:
            EquippedMisc = item;
            break;
    }

    RefreshPlayerUI();
}

// -----------------------------
// RATIONS / EXHAUSTION / CARRY
// -----------------------------

public void ConsumeDailyRations()
{
    if (pd == null)
    {
        Debug.LogError("Player data is null!");
        return;
    }

    if (pd.Companions == null)
        pd.Companions = new List<Companion>();

    int totalConsumption;

    if (pd.PlayerArmy == null && (pd.Companions == null || pd.Companions.Count == 0))
    {
        totalConsumption = 1;
    }
    else if (pd.PlayerArmy == null)
    {
        totalConsumption = pd.Companions.Count + 1;
    }
    else
    {
        int armyUnits = pd.PlayerArmy.GetTotalUnits();
        totalConsumption = armyUnits + pd.Companions.Count + 1;
    }

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

        int lostHungryUnits = 0;
        int lostUnits = 0;

        for (int i = 0; i < missingRations; i++)
        {
            if (pd.PlayerArmy == null || pd.PlayerArmy.GetTotalUnits() <= 0)
                break;

            if (Dice.Roll(0, 2) == 0)
            {
                pd.PlayerArmy.RemoveUnit((UnitType)Dice.Roll(0, 5), 1);
                lostHungryUnits++;

                if (Dice.Roll(0, 10) == 0)
                {
                    pd.PlayerArmy.RemoveUnit((UnitType)Dice.Roll(0, 5), 1);
                    lostUnits++;

                    if (pd.PlayerArmy.GetTotalUnits() <= 0)
                    {
                        Debug.LogWarning("All units have been removed from the army.");
                        break;
                    }
                }
            }
        }

        Debug.Log($"Ordudan {lostHungryUnits} asker rasyon yetersizliğinden dolayı ayrıldı. {lostUnits} asker de ordunu doyuramadığın için yanında gitti.");
    }

    RefreshPlayerUI();
}

public void UpdateArmyCapacity()
{
    if (pd.PlayerArmy == null)
        return;

    int maxUnits = pd.Charisma * 10;
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

public void DecreaseRations(int value)
{
    pd.Rations = Mathf.Max(0, pd.Rations - value);
    Debug.Log($"Rations decreased by {value}. Remaining: {pd.Rations}");
    RefreshPlayerUI();
}

public void IncreaseRations(int value)
{
    pd.Rations += value;
    Debug.Log($"Rations increased by {value}. Total: {pd.Rations}");
    RefreshPlayerUI();
}

public void IncreaseExhaustion()
{
    pd.CurrentExhaustionLevel += 1;
    Debug.Log($"Increased exhaustion. Current level: {pd.CurrentExhaustionLevel}");
    CheckExhaustionMaxed();
    RefreshPlayerUI();
}

public void CheckExhaustionMaxed()
{
    if (pd.CurrentExhaustionLevel == pd.MaxExhaustionLevel)
    {
        Debug.LogError("You have succumbed to exhaustion!");
    }
    else if (pd.CurrentExhaustionLevel > pd.MaxExhaustionLevel)
    {
        if (GameManager.Instance != null)
            GameManager.Instance.Death();
    }
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
    RefreshPlayerUI();
}

public void ConsumeArmyRations()
{
    if (pd.PlayerArmy == null)
        return;

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

public bool ConsumeMoney(int gold, int silver)
{
    if (!pd.HasEnoughMoney(gold, silver))
    {
        Debug.Log("Not enough money!");
        return false;
    }

    pd.TrySpendMoney(gold, silver);
    Debug.Log($"Transaction successful! Remaining Money: {pd.GetMoney()}");
    RefreshPlayerUI();
    return true;
}

// Weight / Carry helpers
public float GetCurrentWeight()
{
    return pd.GetCurrentInventoryWeight();
}

public float GetCarryCapacity()
{
    return pd.GetCarryCapacity();
}

public bool IsOverweight()
{
    return pd.IsOverweight();
}

public float GetWeightRatio()
{
    return pd.GetWeightRatio();
}

// -----------------------------
// UI HELPERS
// -----------------------------

private void RefreshPlayerUI()
{
    if (PlayerUISystem.Instance != null)
    {
        PlayerUISystem.Instance.UpdateUIObjects();
    }

    if (InventoryUI.Instance != null)
    {
        InventoryUI.Instance.UpdateInventoryUI();
    }
}

private void Print(string message)
{
    Debug.Log($"{message}\nSender:\"{GetType().Name}\" class in \"{gameObject.name}\" object");
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