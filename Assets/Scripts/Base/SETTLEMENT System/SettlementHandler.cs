using System.Collections.Generic;
using UnityEngine;
using NEXUS.Utilities;
using Unity.VisualScripting;
using UnityEngine.UI;


public class SettlementHandler : MonoBehaviour
{
    public static SettlementHandler Instance { get; private set; }
    public ItemSpriteDatabase spriteDatabase;

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
        settlements = wrapper != null && wrapper.settlements != null
            ? wrapper.settlements
            : new List<Settlement>();

        settlements.RemoveAll(x => x == null);

        Settlement home = HomeSettlementHandler.Instance != null
            ? HomeSettlementHandler.Instance.homeSettlement
            : null;

        if (home != null && !settlements.Contains(home))
        {
            // The home settlement lives in its own file, so it always goes back in front.
            settlements.RemoveAll(x => x.Name == home.Name);
            settlements.Insert(0, home);
        }
    }
    public Settlement GetCurrentSettlement()
    {
        return settlement;
    }
    public void LoadSettlementsFromSourceData()
    {
        JSONhandler = new JSONDataHandler("SourceData");
        SettlementListWrapper wrapper = JSONhandler.LoadData<SettlementListWrapper>("settlements.json");
        settlements = wrapper != null && wrapper.settlements != null
            ? wrapper.settlements
            : new List<Settlement>();

        settlements.RemoveAll(x => x == null);

        if (settlements.Count == 0)
            Debug.LogWarning("SettlementHandler: No settlements found in SourceData/settlements.json.");
    }
    public void SaveSettlements()
    {
        if (settlements == null)
            settlements = new List<Settlement>();

        if (HomeSettlementHandler.Instance != null && HomeSettlementHandler.Instance.homeSettlement != null)
            settlements.Remove(HomeSettlementHandler.Instance.homeSettlement);

        // Quest settlements are temporary, they are rebuilt from the player's quests on load.
        settlements.RemoveAll(x => x == null || x.Type == SettlementType.Quest);

        JSONhandler = new JSONDataHandler(PlayerPrefs.GetInt("Slot"));
        JSONhandler.SaveData(new SettlementListWrapper { settlements = settlements }, "settlements.json");
    }

    // `settlement` is null whenever the player is not inside one — OnSettlementExited
    // clears it. Enabling or disabling this component in that state used to throw.
    void OnEnable()
    {
        if (settlement == null)
            return;

        settlement.OnSettlementEntered += OnSettlementEntered;
        settlement.OnSettlementExited += OnSettlementExited;

        settlement.OnSettlementUnlocked += OnSettlmenUnlocked;

        settlement.OnPopulationChanged += HandlePopulationChanged;
        settlement.OnWealthChanged += HandleWealthChanged;
        settlement.OnQualityChanged += HandleQualityChanged;
        // settlement.OnSettlementUpgraded += HandleSettlementUpgraded; // Event not defined
        settlement.OnTavernEntered += HandleTavernEntered;
        settlement.OnTownHallEntered += HandleTownHallEntered;
        settlement.OnWallEntered += HandleWallEntered;
        settlement.OnShopEntered += HandleShopEntered;
    }

    void OnDisable()
    {
        if (settlement == null)
            return;

        settlement.OnSettlementEntered -= OnSettlementEntered;
        settlement.OnSettlementExited -= OnSettlementExited;

        settlement.OnSettlementUnlocked -= OnSettlmenUnlocked;

        settlement.OnPopulationChanged -= HandlePopulationChanged;
        settlement.OnWealthChanged -= HandleWealthChanged;
        settlement.OnQualityChanged -= HandleQualityChanged;
        // settlement.OnSettlementUpgraded -= HandleSettlementUpgraded; // Event not defined
        settlement.OnTavernEntered -= HandleTavernEntered;
        settlement.OnTownHallEntered -= HandleTownHallEntered;
        settlement.OnWallEntered -= HandleWallEntered;
        settlement.OnShopEntered -= HandleShopEntered;
    }
    Quest_SO_Constructor PickRandomQuestFromJSON()
    {
        JSONhandler = new JSONDataHandler("SourceData");
        QuestListWrapper wrapper = JSONhandler.LoadData<QuestListWrapper>("quests.json");
        List<Quest_SO_Constructor> quests = wrapper != null && wrapper.quests != null
            ? wrapper.quests
            : new List<Quest_SO_Constructor>();

        if (quests.Count == 0)
        {
            Debug.LogWarning("SettlementHandler: No quests found in SourceData/quests.json.");
            return null;
        }

        return Dice.Pick(quests);
    }

    Job_SO_Constructor PickRandomJobFromJSON()
    {
        JSONhandler = new JSONDataHandler("SourceData");
        JobListWrapper wrapper = JSONhandler.LoadData<JobListWrapper>("jobs.json");
        List<Job_SO_Constructor> jobs = wrapper != null && wrapper.jobs != null
            ? wrapper.jobs
            : new List<Job_SO_Constructor>();

        if (jobs.Count == 0)
        {
            Debug.LogWarning("SettlementHandler: No jobs found in SourceData/jobs.json.");
            return null;
        }

        return Dice.Pick(jobs);
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
        if (_settlement == null)
        {
            Debug.LogWarning("SettlementHandler: Tried to enter a null settlement.");
            return;
        }

        settlement = settlements != null ? settlements.Find(x => x != null && x.Name == _settlement.Name) : null;

        // Not in the list yet (new game, or a quest location): use the one we were given.
        if (settlement == null)
            settlement = _settlement;

        if (HomeSettlementHandler.Instance != null)
            HomeSettlementHandler.Instance.GenerateRandomHappenings();

        if (settlement.Type == SettlementType.Quest)
        {
            UIHandler.Instance.UpdateSettlementInfo(settlement);
            HandleQuestSettlementEntered();

            return;
        }

        Settlement lastVisited = PlayerStatHandler.Instance != null
            ? PlayerStatHandler.Instance.LastVisitedSettlement()
            : null;

        if (lastVisited == null || lastVisited.Name != settlement.Name)
        {
            if (settlement.Tavern != null)
            {
                for (int i = 0; i < GenerateRandomSettlementTavernQuestCount(); i++)
                {
                    Quest_SO_Constructor quest = PickRandomQuestFromJSON();

                    if (quest != null)
                        settlement.Tavern.Quests.Add(quest);
                }
            }

            if (settlement.TownHall != null)
            {
                for (int i = 0; i < GenerateRandomSettlementJobCount(); i++)
                {
                    Job_SO_Constructor job = PickRandomJobFromJSON();

                    if (job != null)
                        settlement.TownHall.Jobs.Add(job);
                }
            }
        }

        if (PlayerStatHandler.Instance != null && PlayerStatHandler.Instance.pd != null)
            PlayerStatHandler.Instance.pd.LastSettlementName = settlement.Name;

        if (HomeSettlementHandler.Instance != null && settlement == HomeSettlementHandler.Instance.homeSettlement)
        {
            HomeSettlementHandler.Instance.OnSettlmentEntered();
        }

        // Add shop inventory refresh logic here
        if (settlement.Shops != null && settlement.Shops.Count > 0)
        {
            foreach (var shop in settlement.Shops)
            {
                shop.Items.Clear(); // Clear existing inventory
                shop.Items.AddRange(ItemGenerator.GenerateItems(shop.ShopType, shop.level, spriteDatabase)); // Generate new inventory
                Debug.Log($"Shop {shop.Name} refreshed with new items.");
            }
        }

        if (MapHandler.Instance != null)
        {
            MapHandler.Instance.lastVisitedSettlement = null;
            MapHandler.Instance.destinationSettlement = null;
        }

        if (UIHandler.Instance != null)
            UIHandler.Instance.UpdateSettlementInfo(settlement);
    }

    public void HandleQuestSettlementEntered()
    {
        if (settlement == null || settlement.Tavern == null ||
            settlement.Tavern.Quests == null || settlement.Tavern.Quests.Count == 0)
        {
            Debug.LogWarning("SettlementHandler: Quest settlement has no quest attached to it.");
            return;
        }

        foreach (Button button in UIHandler.Instance.GoBackButtons)
        {
            button.onClick.RemoveAllListeners();
            SettlementButtonPointer settlementButtonPointer = TravelSystem.Instance.GetSettlementButtonPointerByID(settlement.Tavern.Quests[0].ID);
            button.AddComponent<SettlementButtonPointer>().settlement = settlementButtonPointer.settlement;

            MapHandler.Instance.RemoveQuestSettlement(settlementButtonPointer);
            MapHandler.Instance.map.transform.parent.gameObject.SetActive(true);
        }

        UIHandler.Instance.QuestInfo.text = $"You have entered {settlement.Name}. It seems that there is no one here except you. But in the distance, you can see mentioned area. Do you want to go there?";

        UIHandler.Instance.FightButton.onClick.RemoveAllListeners();
        UIHandler.Instance.FightButton.onClick.AddListener(() =>
        {
            UIHandler.Instance.QuestPanelBG.SetActive(false);
            UIHandler.Instance.ResultsPanel.SetActive(true);
            UIHandler.Instance.ResultsPanel.GetComponentInChildren<TMPro.TMP_Text>().text = $"You fight the your way to the mentioned area. Now there is really no one here. You can go back to the tavern to report your journey.";
        });
    }


    public void OnSettlementExited()
    {
        if (settlement == null)
        {
            return;
        }

        if (settlement.Tavern != null && settlement.Tavern.Quests.Count > 0)
        {
            settlement.Tavern.Quests.Clear();
        }
        if (settlement.TownHall != null && settlement.TownHall.Jobs.Count > 0)
        {
            settlement.TownHall.Jobs.Clear();
        }

        UIHandler.Instance.HomePanelBG.SetActive(false);
        UIHandler.Instance.QuestPanelBG.SetActive(false);
        UIHandler.Instance.SettlementPanelBG.SetActive(false);
        settlement = null;
        MapHandler.Instance.PopulateMap();
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