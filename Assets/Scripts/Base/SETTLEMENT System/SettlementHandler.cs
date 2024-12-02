using System;
using System.Collections.Generic;
using UnityEngine;
using NEXUS.Utilities;
using Unity.VisualScripting;


public class SettlementHandler : MonoBehaviour
{
    public static SettlementHandler Instance { get; private set; }

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

    public List<Settlement> settlements = new List<Settlement>();

    public Settlement settlement = new Settlement();

    JSONDataHandler JSONhandler;

    public void Wrappers()
    {
        JSONhandler = new JSONDataHandler("SourceData");
        SettlementListWrapper wrapper = JSONhandler.LoadData<SettlementListWrapper>("settlements.json");
        settlements = wrapper != null ? wrapper.settlements : new List<Settlement>();

        settlement = PlayerStatHandler.Instance.LastVisitedSettlement();
    }


    void OnEnable()
    {
        settlement.OnSettlementEntered += OnSettlmentEntered;
        settlement.OnSettlementExited += OnSettlementExited;

        settlement.OnSettlementUnlocked += OnSettlmenUnlocked;

        settlement.OnPopulationChanged += HandlePopulationChanged;
        settlement.OnWealthChanged += HandleWealthChanged;
        settlement.OnQualityChanged += HandleQualityChanged;
        settlement.OnSettlementUpgraded += HandleSettlementUpgraded;
        settlement.OnTavernEntered += HandleTavernEntered;
        settlement.OnTownHallEntered += HandleTownHallEntered;
        settlement.OnWallEntered += HandleWallEntered;
        settlement.OnShopEntered += HandleShopEntered;
    }

    void OnDisable()
    {
        settlement.OnSettlementEntered -= OnSettlmentEntered;
        settlement.OnSettlementExited -= OnSettlementExited;

        settlement.OnSettlementUnlocked -= OnSettlmenUnlocked;

        settlement.OnPopulationChanged -= HandlePopulationChanged;
        settlement.OnWealthChanged -= HandleWealthChanged;
        settlement.OnQualityChanged -= HandleQualityChanged;
        settlement.OnSettlementUpgraded -= HandleSettlementUpgraded;
        settlement.OnTavernEntered -= HandleTavernEntered;
        settlement.OnTownHallEntered -= HandleTownHallEntered;
        settlement.OnWallEntered -= HandleWallEntered;
        settlement.OnShopEntered -= HandleShopEntered;
    }

    void OnApplicationQuit()
    {
    }

    public void EndWrappers()
    {
        JSONhandler = new JSONDataHandler("SourceData");
        SettlementListWrapper wrapper = new SettlementListWrapper();
        PlayerStatHandler.Instance.CheckHomeSettlementinSettlements();
        wrapper.settlements = settlements;
        JSONhandler.SaveData(wrapper, "settlements.json");
    }

    Quest_SO_Constructor PickRandomQuestFromJSON()
    {
        JSONhandler = new JSONDataHandler("SourceData");
        QuestListWrapper wrapper = JSONhandler.LoadData<QuestListWrapper>("quests.json");
        List<Quest_SO_Constructor> quests = wrapper != null ? wrapper.quests : new List<Quest_SO_Constructor>();

        return quests[UnityEngine.Random.Range(0, quests.Count)];
    }

    Job_SO_Constructor PickRandomJobFromJSON()
    {
        JSONhandler = new JSONDataHandler("SourceData");
        JobListWrapper wrapper = JSONhandler.LoadData<JobListWrapper>("jobs.json");
        List<Job_SO_Constructor> jobs = wrapper != null ? wrapper.jobs : new List<Job_SO_Constructor>();

        return jobs[UnityEngine.Random.Range(0, jobs.Count)];
    }

    public int GenerateRandomSettlementJobCount()
    {
        return UnityEngine.Random.Range(1, 4);
    }

    public int GenerateRandomSettlementTavernQuestCount()
    {
        return UnityEngine.Random.Range(1, 4);
    }
    public void LoadShopItems(Shops shop)
    {
        foreach (var itemData in shop.Items)
        {
            var newItem = new Item(
                itemData.ID,
                itemData.Name,
                itemData.Value,
                itemData.Category,
                itemData.StrengthModifier,
                itemData.ConstitutionModifier,
                itemData.DexterityModifier,
                itemData.CharismaModifier
            );
            shop.Items.Add(newItem);
        }
    }
    public void OnSettlmentEntered(Settlement settlement)
    {
        Print($"Entered {settlement.Name}");


        if (settlement == PlayerStatHandler.Instance.homeSettlement)
        {
            print("Entered home settlement");
        }


        if (PlayerStatHandler.Instance.LastVisitedSettlement().Name != settlement.Name)
        {
            if (settlement.Tavern != null)
            {
                for (int i = 0; i < GenerateRandomSettlementTavernQuestCount(); i++)
                {
                    settlement.Tavern.Quests.Add(PickRandomQuestFromJSON());
                }
            }

            if (settlement.TownHall != null)
            {
                for (int i = 0; i < GenerateRandomSettlementJobCount(); i++)
                {
                    settlement.TownHall.Jobs.Add(PickRandomJobFromJSON());
                }
            }
        }
        
        PlayerStatHandler.Instance.pd.LastSettlementName = settlement.Name;

        UIHandler.Instance.UpdateSettlementInfo(settlement);
    }


    public void OnSettlementExited()
    {
        settlement.Tavern.Quests.Clear();
        settlement.TownHall.Jobs.Clear();
    }

    public void OnSettlmenUnlocked(Settlement settlement)
    {
        Print($"Unlocked {settlement.Name}");
    }
    void HandlePopulationChanged(int population)
    {
        Print($"Population changed to {population}");
    }

    void HandleWealthChanged(int wealth)
    {
        Print($"Wealth changed to {wealth}");
    }

    void HandleQualityChanged(int quality)
    {
        Print($"Quality changed to {quality}");
    }

    void HandleSettlementUpgraded()
    {
        Print("Settlement upgraded");
    }

    void HandleTavernEntered(Taverns tavern)
    {
        Print($"Entered {tavern.Name}");
    }

    void HandleTownHallEntered(TownHalls townHall)
    {
        Print($"Entered {townHall.Name}");
    }

    void HandleWallEntered(Walls wall)
    {
        Print($"Entered {wall.Name}");
    }

    public void HandleShopEntered(Shops shop)
    {
        Print($"Entered {shop.Name}");
    }


    void Print(string message)
    {
        Debug.Log($"{message}\nSender:\"{this.GetType().Name}\" class in \"{this.gameObject.name}\"");
    }
}

[System.Serializable]
public class SettlementListWrapper
{
    public List<Settlement> settlements;
}

[System.Serializable]
public class HomeSettlementWrapper
{
    public Settlement homeSettlement;
}

[System.Serializable]
public class QuestListWrapper
{
    public List<Quest_SO_Constructor> quests;
}

[System.Serializable]
public class JobListWrapper
{
    public List<Job_SO_Constructor> jobs;
}