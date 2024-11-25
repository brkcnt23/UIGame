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

        Wrappers();
    }

    public List<Settlement> settlements = new List<Settlement>();

    public Settlement settlement = new Settlement();


    JSONDataHandler JSONhandler = new JSONDataHandler();


    void OnEnable()
    {
        settlement.OnPopulationChanged += HandlePopulationChanged;
        settlement.OnWealthChanged += HandleWealthChanged;
        settlement.OnQualityChanged += HandleQualityChanged;
        settlement.OnSettlementUpgraded += HandleSettlementUpgraded;
        settlement.OnTavernEntered += HandleTavernEntered;
        settlement.OnTownHallEntered += HandleTownHallEntered;
        settlement.OnWallEntered += HandleWallEntered;
        settlement.OnShopEntered += HandleShopEntered;
    }

    public void Wrappers()
    {
        SettlementListWrapper wrapper = JSONhandler.LoadData<SettlementListWrapper>("settlements.json");
        settlements = wrapper != null ? wrapper.settlements : new List<Settlement>();

        //our tavern has a quest
        QuestListWrapper questWrapper = JSONhandler.LoadData<QuestListWrapper>("quests.json");
        settlement.Tavern.Quests = questWrapper != null ? questWrapper.quests : new List<Quest_SO_Constructor>();

        //our town hall has jobs
        JobListWrapper jobWrapper = JSONhandler.LoadData<JobListWrapper>("jobs.json");
        settlement.TownHall.Jobs = jobWrapper != null ? jobWrapper.jobs : new List<Job_SO_Constructor>();
    }

    void OnDisable()
    {
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
        SettlementListWrapper wrapper = new SettlementListWrapper { settlements = settlements };
        JSONhandler.SaveData(wrapper, "settlements.json");

        QuestListWrapper questWrapper = new QuestListWrapper { quests = settlement.Tavern.Quests };
        JSONhandler.SaveData(questWrapper, "quests.json");

        JobListWrapper jobWrapper = new JobListWrapper { jobs = settlement.TownHall.Jobs };
        JSONhandler.SaveData(jobWrapper, "jobs.json");
    }

    //listener for the events
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
public class EventListWrapper
{
    public List<Event_SO_Constructor> events;
}

[System.Serializable]
public class JobListWrapper
{
    public List<Job_SO_Constructor> jobs;
}