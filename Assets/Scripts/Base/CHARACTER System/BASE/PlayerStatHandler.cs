using System;
using UnityEngine;
using NEXUS.Utilities;
using UnityEngine.Purchasing.MiniJSON;
using System.Collections.Generic;

public class PlayerStatHandler : MonoBehaviour
{
    public static PlayerStatHandler Instance { get; private set; }

    public JSONDataHandler JSONhandler = new JSONDataHandler();

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
        PlayerDataWrapper wrapper = JSONhandler.LoadData<PlayerDataWrapper>("playerData.json");
        CompanionListWrapper companionWrapper = JSONhandler.LoadData<CompanionListWrapper>("companions.json");

        pd = wrapper != null ? wrapper.pd : new PlayerData();
        pd.Companions = companionWrapper != null ? companionWrapper.Companions : new List<Companion>();

        //UpdateArmyCapacity();
    }
    public void AddCharacterExperience(int xp)
    {
        pd.Experience += xp;
        Debug.Log($"Character gained {xp} EXP. Total: {pd.Experience}");

        CheckLevelUp();
    }

    private void CheckLevelUp()
    {
        while (pd.Experience >= pd.MaxExperience)
        {
            pd.Experience -= pd.MaxExperience;
            pd.Level++;
            pd.MaxExperience = CalculateMaxExperience(pd.Level);
            Debug.Log($"Level up! New Level: {pd.Level}, Next Level EXP: {pd.MaxExperience}");
            AllocateLevelUpStats();
        }
    }

    private int CalculateMaxExperience(int level)
    {
        return Mathf.RoundToInt(1000 * Mathf.Pow(1.1f, level - 1)); // EXP gereksinimi her seviye için artar
    }

    private void AllocateLevelUpStats()
    {
        // Her seviye atlayışta Strength ve Constitution artışı
        pd.Strength += 1;
        pd.Constitution += 1;
        Debug.Log($"Stats increased! STR: {pd.Strength}, CONST: {pd.Constitution}");
    }

    public void AddStats(string statType, int amount)
    {
        switch (statType.ToLower())
        {
            case "str":
                pd.Strength += amount;
                Debug.Log($"STR increased by {amount}. Total: {pd.Strength}");
                break;
            case "const":
                pd.Constitution += amount;
                Debug.Log($"CONST increased by {amount}. Total: {pd.Constitution}");
                break;
            case "cha":
                pd.Charisma += amount;
                Debug.Log($"CHA increased by {amount}. Total: {pd.Charisma}");
                break;
            case "dex":
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
        JSONhandler.SaveData(new PlayerDataWrapper { pd = pd }, "playerData.json");
        JSONhandler.SaveData(new CompanionListWrapper { Companions = pd.Companions }, "companions.json");
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