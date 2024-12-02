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


    void OnEnable()
    {
        settlement.OnSettlementExited += OnSettlementExited;
        settlement.OnSettlementEntered += OnSettlmentEntered;
        
        settlement.OnSettlementUnlocked += OnSettlmenUnlocked;
    

        settlement.OnShopEntered += HandleShopEntered;
        settlement.OnPopulationChanged += HandlePopulationChanged;
        settlement.OnWealthChanged += HandleWealthChanged;
        settlement.OnQualityChanged += HandleQualityChanged;
        settlement.OnSettlementUpgraded += HandleSettlementUpgraded;
        settlement.OnTavernEntered += HandleTavernEntered;
        settlement.OnTownHallEntered += HandleTownHallEntered;
        settlement.OnWallEntered += HandleWallEntered;
    }

    public void Wrappers(int slot)
    {
        JSONhandler = new JSONDataHandler(slot);
        SettlementListWrapper wrapper = JSONhandler.LoadData<SettlementListWrapper>("settlements.json");
        settlements = wrapper != null ? wrapper.settlements : new List<Settlement>();

        //our tavern has a quest
        QuestListWrapper questWrapper = JSONhandler.LoadData<QuestListWrapper>("quests.json");
        settlement.Tavern.Quests = questWrapper != null ? questWrapper.quests : new List<Quest_SO_Constructor>();

        //our town hall has jobs
        JobListWrapper jobWrapper = JSONhandler.LoadData<JobListWrapper>("jobs.json");
        settlement.TownHall.Jobs = jobWrapper != null ? jobWrapper.jobs : new List<Job_SO_Constructor>();

        settlement = settlements[1];
        
    }

    void OnDisable()
    {
        settlement.OnSettlementEntered -= OnSettlmentEntered;
        settlement.OnSettlementUnlocked -= OnSettlmenUnlocked;
        settlement.OnSettlementExited -= OnSettlementExited;

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
        EndWrappers();
    }

    public void EndWrappers()
    {
        JSONhandler = new JSONDataHandler(PlayerPrefs.GetInt("Slot"));
        SettlementListWrapper wrapper = new SettlementListWrapper { settlements = settlements };
        JSONhandler.SaveData(wrapper, "settlements.json");
    }

    Quest_SO_Constructor PickRandomQuestFromJSON()
    {
        JSONhandler = new JSONDataHandler(3);
        QuestListWrapper wrapper = JSONhandler.LoadData<QuestListWrapper>("quests.json");
        List<Quest_SO_Constructor> quests = wrapper != null ? wrapper.quests : new List<Quest_SO_Constructor>();

        return quests[Random.Range(0, quests.Count)];
    }

    Job_SO_Constructor PickRandomJobFromJSON()
    {
        JSONhandler = new JSONDataHandler(3);
        JobListWrapper wrapper = JSONhandler.LoadData<JobListWrapper>("jobs.json");
        List<Job_SO_Constructor> jobs = wrapper != null ? wrapper.jobs : new List<Job_SO_Constructor>();

        return jobs[Random.Range(0, jobs.Count)];
    }

    public int GenerateRandomSettlementJobCount()
    {
        return Random.Range(1, 4);
    }

    public int GenerateRandomSettlementTavernQuestCount()
    {
        return Random.Range(1, 4);
    }

    public void OnSettlmentEntered(Settlement settlement)
    {
        Print($"Entered {settlement.Name}");

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

    void HandleShopEntered(Shops shop)
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
public class QuestListWrapper
{
    public List<Quest_SO_Constructor> quests;
}

[System.Serializable]
public class JobListWrapper
{
    public List<Job_SO_Constructor> jobs;
}