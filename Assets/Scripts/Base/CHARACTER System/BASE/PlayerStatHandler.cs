using System;
using UnityEngine;

public class PlayerStatHandler : MonoBehaviour
{
    public static PlayerStatHandler Instance { get; private set; }

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

    public PlayerData pd = new PlayerData(1);

    private void OnDestroy()
    {
        pd.SaveData(1);
    }

    public int GetExhaustionLevel()
    {
        return pd.CurrentExhaustion;
    }

    public int GetRations()
    {
        return pd.Rations;
    }

    public void SetExhaustionLevel(int nextValue)
    {
        pd.CurrentExhaustion = nextValue;
        Print($"Exhaustion level set to: {GetExhaustionLevel()}");
    }

    public void IncreaseExhaustion()
    {
        pd.CurrentExhaustion += 1;
        Print("Increased exhaustion. Current level: " + GetExhaustionLevel());
    }

    public void CheckExhaustionMaxed()
    {
        if (pd.CurrentExhaustion >= pd.MaxExhaustion)
        {
            GameManager.Instance.Death();
        }
    }

    public void DecreaseRations(int value)
    {
        pd.Rations -= value;
        Print($"Rations decreased by {value}. Remaining: {GetRations()}");
    }

    public void IncreaseRations(int value)
    {
        pd.Rations += value;
        Print($"Rations increased by {value}. Total: {GetRations()}");
    }

    public void Print(string message)
    {
        Debug.Log($"{message}{Environment.NewLine}Object: {this.name}");
    }
}

[System.Serializable]
public class PlayerData
{
    public int ID;
    public string Name;
    public int Hour;
    public int Minute;
    public int Day;
    public int Level;
    public int Health;
    public int MaxHealth;
    public int Experience;
    public int MaxExperience;
    public int Strength;
    public int Dexterity;
    public int Constitution;
    public int Charisma;
    public int Rations;
    public int MaxExhaustion;
    public int CurrentExhaustion;

    public PlayerData(int saveSlot)
    {
        string jsonData = System.IO.File.ReadAllText(Application.dataPath + "/Scripts/Base/PlayerData/SaveSlot_" + saveSlot + ".json");
        JsonUtility.FromJsonOverwrite(jsonData, this);
    }

    public void SaveData(int saveSlot)
    {
        string jsonData = JsonUtility.ToJson(this);
        System.IO.File.WriteAllText(Application.dataPath + "/Scripts/Base/PlayerData/SaveSlot_" + saveSlot + ".json", jsonData);
    }
}