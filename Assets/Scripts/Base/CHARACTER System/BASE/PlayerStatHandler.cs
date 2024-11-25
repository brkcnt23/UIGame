using System;
using UnityEngine;
using NEXUS.Utilities;
using UnityEngine.Purchasing.MiniJSON;
using System.Collections.Generic;

public class PlayerStatHandler : MonoBehaviour
{
    public static PlayerStatHandler Instance { get; private set; }

    JSONDataHandler JSONhandler = new JSONDataHandler();

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
        pd.Companions = companionWrapper != null ? companionWrapper.Companions : new List<PlayerData>();
        
    }

    void OnApplicationQuit()
    {
        JSONhandler.SaveData(pd, "playerData.json");
        JSONhandler.SaveData(new CompanionListWrapper { Companions = pd.Companions }, "companions.json");
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
    public List<PlayerData> Companions;
}