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

    public void IncreaseExhaustion()
    {
        pd.CurrentExhaustionLevel += 1;
        Print("Increased exhaustion. Current level: " + GetExhaustionLevel());
    }

    public void CheckExhaustionMaxed()
    {
        if (pd.CurrentExhaustionLevel >= pd.MaxExhaustionLevel)
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
    public int Gold;
    public int Silver;
    public int Strength;
    public int Dexterity;
    public int Constitution;
    public int Charisma;
    public int Rations;
    public int MaxExhaustionLevel;
    public int CurrentExhaustionLevel;
    public int SmitherSkillLevel;
    public int SmitherSkillXP;
    public int TannerSkillLevel;
    public int TannerSkillXP;
    public int CarpenterSkillLevel;
    public int CarpenterSkillXP;
    public int MasonSkillLevel;
    public int MasonSkillXP;
    public int AlchemistSkillLevel;
    public int AlchemistSkillXP;
    public int LastSleepTime;
    public int LastMealTime;
    public bool HasDied;

    public PlayerData(int saveSlot)
    {
        string filePath = Application.dataPath + "/Scripts/Base/PlayerData/SaveSlot_" + saveSlot + ".json";
        if (System.IO.File.Exists(filePath))
        {
            try
            {
                string jsonData = System.IO.File.ReadAllText(filePath);
                JsonUtility.FromJsonOverwrite(jsonData, this);
            }
            catch (Exception e)
            {
                Debug.LogError($"PlayerData yüklenirken hata oluştu: {e.Message}");
                InitializeDefaultValues(saveSlot);
            }
        }
        else
        {
            Debug.LogWarning("Kayıt dosyası bulunamadı. Varsayılan değerlerle başlatılıyor.");
            InitializeDefaultValues(saveSlot);
        }
    }

    private void InitializeDefaultValues(int saveSlot)
    {
        // Varsayılan değerleri burada ayarlayın
        ID = saveSlot;
        Name = "Yeni Oyuncu";
        Hour = 8;
        Minute = 0;
        Day = 1;
        Level = 1;
        MaxHealth = 100;
        Health = MaxHealth;
        Experience = 0;
        MaxExperience = 1000;
        Gold = 0;
        Silver = 0;
        Strength = 10;
        Dexterity = 10;
        Constitution = 10;
        Charisma = 10;
        Rations = 5;
        MaxExhaustionLevel = 3;
        CurrentExhaustionLevel = 0;
        SmitherSkillLevel = 1;
        SmitherSkillXP = 0;
        TannerSkillLevel = 0;
        TannerSkillXP = 0;
        CarpenterSkillLevel = 0;
        CarpenterSkillXP = 0;
        MasonSkillLevel = 0;
        MasonSkillXP = 0;
        AlchemistSkillLevel = 0;
        AlchemistSkillXP = 0;
        HasDied = false;

    }

    public void SaveData(int saveSlot)
    {
        string jsonData = JsonUtility.ToJson(this);
        System.IO.File.WriteAllText(Application.dataPath + "/Scripts/Base/PlayerData/SaveSlot_" + saveSlot + ".json", jsonData);
    }
}