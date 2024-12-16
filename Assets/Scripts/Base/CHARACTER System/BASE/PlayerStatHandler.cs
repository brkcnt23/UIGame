using System;
using UnityEngine;
using NEXUS.Utilities;
using System.Collections.Generic;

public enum StatType { Strength, Constitution, Charisma, Dexterity }
//when we add stats we will use this enum for controling.
//this enum is NOT the PLAYER DATA STATS

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

        Application.targetFrameRate = 60;
    }

    public PlayerData pd = new PlayerData();


    private void Start()
    {
        economySystem = new EconomySystem(pd);
        //UpdateArmyCapacity();
    }

    public void SavePlayerData()
    {
        HomeSettlementHandler.Instance.SaveHomeSettlement();
        SettlementHandler.Instance.SaveSettlements();

        TravelSystem.Instance.SaveTravelData();

        EventHandler.Instance.SaveEvents();

        pd.LastSettlementName = SettlementHandler.Instance.settlement.Name;

        //JSONhandler.SaveData(new CompanionListWrapper { Companions = pd.Companions }, "playerCompanions.json");

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

        // Ensure Companions and Army are not null after loading
        if (pd.Companions == null)
        {
            pd.Companions = new List<Companion>();
        }

        if (pd.PlayerArmy == null)
        {
            pd.PlayerArmy = new Army(); // Or handle if the army is optional
        }

        EventHandler.Instance.LoadEvents();
        HomeSettlementHandler.Instance.LoadHomeSettlement();
        SettlementHandler.Instance.LoadSettlements();

        TravelSystem.Instance.LoadTravelData();
        MapHandler.Instance.MovePlayerToLastVisitedSettlement(LastVisitedSettlement());

        MapHandler.Instance.LoadQuestSettlements();

        TimeSystem.Instance.InitializeLastActionTimes();
        PlayerUISystem.Instance.UpdateClockText();
    }


    private void OnApplicationQuit()
    {
        if (GameManager.Instance.isEnteredSettlement)
        {
            if (pd.HasDied)
            {
                pd = new PlayerData();
            }
            SavePlayerData();
        }
    }

    public void GetPlayerCompanions()
    {
        CompanionListWrapper wrapper = JSONhandler.LoadData<CompanionListWrapper>("playerCompanions.json");
        pd.Companions = wrapper != null ? wrapper.Companions : new List<Companion>();
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


    public void AddStats(StatType statType, int amount)
    {
        switch (statType)
        {
            case StatType.Strength:
                pd.Strength += amount;
                Debug.Log($"STR increased by {amount}. Total: {pd.Strength}");
                break;
            case StatType.Constitution:
                pd.Constitution += amount;
                Debug.Log($"CONST increased by {amount}. Total: {pd.Constitution}");
                break;
            case StatType.Charisma:
                pd.Charisma += amount;
                Debug.Log($"CHA increased by {amount}. Total: {pd.Charisma}");
                break;
            case StatType.Dexterity:
                pd.Dexterity += amount;
                Debug.Log($"DEX increased by {amount}. Total: {pd.Dexterity}");
                break;
            default:
                Debug.LogWarning("Unknown stat type.");
                break;
        }
    }



    public Settlement LastVisitedSettlement()
    {
        Settlement lastVisited = pd.LastSettlementName != "" ? SettlementHandler.Instance.settlements.Find(s => s.Name == pd.LastSettlementName) : HomeSettlementHandler.Instance.homeSettlement;

        return lastVisited;
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
    public void ApplyModifiers(Item item)
    {
        foreach (var modifier in item.Modifiers)
        {
            AddStats(modifier.Stat, modifier.Value);
        }
    }

    public void RemoveModifiers(Item item)
    {
        foreach (var modifier in item.Modifiers)
        {
            AddStats(modifier.Stat, -modifier.Value);
        }
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
            case ItemCategory.Misc:
                EquippedMisc = item;
                break;
        }
    }
    /// <summary>
    /// Günlük rasyon tüketimini gerçekleştirir.
    /// </summary>
    public void ConsumeDailyRations()
    {
        if (pd == null)
        {
            Debug.LogError("Player data is null!");
            return;
        }

        if (pd.Companions == null)
        {
            pd.Companions = new List<Companion>();
        }

        int totalConsumption;

        if (pd.PlayerArmy == null && (pd.Companions == null || pd.Companions.Count == 0))
        {
            totalConsumption = 1; // just player
        }
        else if (pd.PlayerArmy == null)
        {
            totalConsumption = pd.Companions.Count + 1;     //player + companion
        }
        else
        {
            int armyUnits = pd.PlayerArmy != null ? pd.PlayerArmy.GetTotalUnits() : 0;
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

            int lostHungeryUnits = 0;
            int lostUnits = 0;
            for (int i = 0; i < missingRations; i++)
            {
                if (Dice.Roll(0, 2) == 0)
                {
                    pd.PlayerArmy.RemoveUnit((UnitType)Dice.Roll(0, 5), 1);
                    lostHungeryUnits++;

                    //ve giden her askerin yanında bir başka asker daha gitme şansı %10 olacak şekilde
                    if (Dice.Roll(0, 10) == 0)
                    {
                        pd.PlayerArmy.RemoveUnit((UnitType)Dice.Roll(0, 5), 1);
                        if (pd.PlayerArmy.GetTotalUnits() <= 0)
                        {
                            Debug.LogWarning("All units have been removed from the army.");
                            break;
                        }
                        lostUnits++;
                    }
                }
            }

            Debug.Log($"Ordudan {lostHungeryUnits} asker rasyon yetersizliğinden dolayı ayrıldı. {lostUnits} asker de ordunu doyuramadağın için yanlarında gitti.");
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
        if (pd.CurrentExhaustionLevel == pd.MaxExhaustionLevel)
        {
            Debug.LogError("You have succumbed to exhaustion!");
        }
        else if (pd.CurrentExhaustionLevel > pd.MaxExhaustionLevel)
        {
            GameManager.Instance.Death();
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
    public void ConsumeMoney(int gold, int silver)
    {
        if (silver >= 100)
        {
            gold += silver / 100;
            silver %= 100;
        }

        // Check if the player has enough gold and silver
        if (pd.Gold < gold || (pd.Gold == gold && pd.Silver < silver))
        {
            Debug.Log("Not enough money!");
            return;
        }

        // Deduct silver first
        if (pd.Silver >= silver)
        {
            pd.Silver -= silver;
        }
        else
        {
            // Borrow 1 gold to cover the silver deficit
            if (pd.Gold > 0)
            {
                pd.Gold -= 1;
                pd.Silver += 100;
                pd.Silver -= silver;
            }
            else
            {
                Debug.Log("Not enough silver!");
                return;
            }
        }

        // Deduct gold
        pd.Gold -= gold;

        Debug.Log($"Transaction successful! Remaining Gold: {pd.Gold}, Silver: {pd.Silver}");
    }
    public void AddStatXP(StatType statType, int xpAmount)
    {
        switch (statType)
        {
            case StatType.Strength:
                pd.StrengthXP += xpAmount;
                while (pd.StrengthXP >= 100)
                {
                    pd.Strength++;
                    pd.StrengthXP -= 100;
                    Debug.Log($"Strength leveled up! New Strength: {pd.Strength}");
                }
                break;

            case StatType.Dexterity:
                pd.DexterityXP += xpAmount;
                while (pd.DexterityXP >= 100)
                {
                    pd.Dexterity++;
                    pd.DexterityXP -= 100;
                    Debug.Log($"Dexterity leveled up! New Dexterity: {pd.Dexterity}");
                }
                break;

            case StatType.Constitution:
                pd.ConstitutionXP += xpAmount;
                while (pd.ConstitutionXP >= 100)
                {
                    pd.Constitution++;
                    pd.ConstitutionXP -= 100;
                    Debug.Log($"Constitution leveled up! New Constitution: {pd.Constitution}");
                }
                break;

            case StatType.Charisma:
                pd.CharismaXP += xpAmount;
                while (pd.CharismaXP >= 100)
                {
                    pd.Charisma++;
                    pd.CharismaXP -= 100;
                    Debug.Log($"Charisma leveled up! New Charisma: {pd.Charisma}");
                }
                break;

            default:
                Debug.LogWarning("Unknown stat type.");
                break;
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