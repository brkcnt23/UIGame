using System.Collections.Generic;
using UnityEngine;
using NEXUS.Utilities;
using Unity.VisualScripting;
using UnityEngine.UI;


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

    public void LoadSettlements()
    {
        JSONhandler = new JSONDataHandler(PlayerPrefs.GetInt("Slot"));
        SettlementListWrapper wrapper = JSONhandler.LoadData<SettlementListWrapper>("settlements.json");
        settlements = wrapper != null ? wrapper.settlements : new List<Settlement>();

        settlements.Insert(0, HomeSettlementHandler.Instance.homeSettlement);

        settlement = settlements.Find(x => x.Name == PlayerStatHandler.Instance.LastVisitedSettlement().Name);
    }
    public Settlement GetCurrentSettlement()
    {
        return settlement;
    }
    public void LoadSettlementsFromSourceData()
    {
        JSONhandler = new JSONDataHandler("SourceData");
        SettlementListWrapper wrapper = JSONhandler.LoadData<SettlementListWrapper>("settlements.json");
        settlements = wrapper != null ? wrapper.settlements : new List<Settlement>();
    }
    public void SaveSettlements()
    {
        settlements.Remove(HomeSettlementHandler.Instance.homeSettlement);
        settlements.Remove(settlements.Find(x => x.Type == SettlementType.Quest));
        JSONhandler = new JSONDataHandler(PlayerPrefs.GetInt("Slot"));
        JSONhandler.SaveData(new SettlementListWrapper { settlements = settlements }, "settlements.json");
    }

    void OnEnable()
    {
        settlement.OnSettlementEntered += OnSettlementEntered;
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
        settlement.OnSettlementEntered -= OnSettlementEntered;
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
        return UnityEngine.Random.Range(0, 2);
    }

    public int GenerateRandomSettlementTavernQuestCount()
    {
        return UnityEngine.Random.Range(1, 4);
    }
    public void OnSettlementEntered(Settlement _settlement)
    {
        settlement = settlements.Find(x => x.Name == _settlement.Name);
        HomeSettlementHandler.Instance.GenerateRandomHappenings();
        if (settlement.Type == SettlementType.Quest)
        {
            UIHandler.Instance.UpdateSettlementInfo(settlement);
            HandleQuestSettlementEntered();

            return;
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

        if (settlement == HomeSettlementHandler.Instance.homeSettlement)
        {
            HomeSettlementHandler.Instance.OnSettlmentEntered();

        }


        MapHandler.Instance.lastVisitedSettlement = null; // Remove this line
        MapHandler.Instance.destinationSettlement = null; // Remove this line

        UIHandler.Instance.UpdateSettlementInfo(settlement);
    }

    public void HandleQuestSettlementEntered()
    {
        foreach (Button button in UIHandler.Instance.GoBackButtons)
        {
            button.onClick.RemoveAllListeners();
            SettlementButtonPointer settlementButtonPointer = TravelSystem.Instance.GetSettlementButtonPointerByID(settlement.Tavern.Quests[0].ID);
            button.AddComponent<SettlementButtonPointer>().settlement = settlementButtonPointer.settlement;
        }

        UIHandler.Instance.QuestInfo.text = $"You have entered {settlement.Name}. It seems that there is no one here except you. But in the distance, you can see mentioned area. Do you want to go there?";

        UIHandler.Instance.FightButton.onClick.RemoveAllListeners();
        UIHandler.Instance.FightButton.onClick.AddListener(() =>
        {
            UIHandler.Instance.QuestPanelBG.SetActive(false);
            UIHandler.Instance.ResultsPanel.SetActive(true);
            UIHandler.Instance.ResultsPanel.GetComponentInChildren<TMPro.TMP_Text>().text = $"You fight the your way to the mentioned area. Now there is really no one here. You can go back to the {TravelSystem.Instance.GetSettlementButtonPointerByID(settlement.Tavern.Quests[0].ID).settlement.Name} to report your journey.";
        });
    }


    public void OnSettlementExited()
    {
        if (settlement.Tavern != null && settlement.Tavern.Quests.Count > 0)
        {
            settlement.Tavern.Quests.Clear();
        }
        if (settlement.TownHall != null && settlement.TownHall.Jobs.Count > 0)
        {
            settlement.TownHall.Jobs.Clear();
        }

        UIHandler.Instance.HomePanelBG.SetActive(false);
    }

    public void OnSettlmenUnlocked(Settlement settlement)
    {
        Print($"Unlocked {settlement.Name}");
    }
    void HandlePopulationChanged(int population)
    {
        Print($"Population changed to {population}");
    }

    void HandleWealthChanged(Currency wealth)
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

    void HandleTownHallEntered()
    {
        Print($"Entered {settlement.TownHall.Name}");
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