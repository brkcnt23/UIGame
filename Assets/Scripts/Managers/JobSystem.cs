using UnityEngine;
using NEXUS.Utilities;
using System.Collections.Generic;

public class JobSystem : MonoBehaviour
{
    public static JobSystem Instance { get; set; }

    [SerializeField] private List<Job_SO_Constructor> availableJobs;
    private TimeSystem timeSystem;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        timeSystem = TimeSystem.Instance;

        if (timeSystem == null)
        {
            Debug.LogWarning("JobSystem: TimeSystem.Instance is null!");
        }
    }

    // -----------------------------
    // STABLE JOBS
    // -----------------------------

    public void StartHelpMerchants()
    {
        Debug.Log("Starting job: Help the Merchants");
        StartJobWithDuration();
        GrantMerchantsReward();
    }

    public void StartHelpScouts()
    {
        Debug.Log("Starting job: Help the Scouts");
        StartJobWithDuration();
        GrantScoutsReward();
    }

    public void StartGatherHerbs()
    {
        Debug.Log("Starting job: Gathering Herbs");
        StartJobWithDuration();
        GrantGatherHerbsReward();
    }

    public void StartCuttingWoods()
    {
        Debug.Log("Starting job: Cutting Woods");
        StartJobWithDuration();
        GrantCuttingWoodsReward();
    }

    public void StartLaboringMines()
    {
        Debug.Log("Starting job: Laboring Mines");
        StartJobWithDuration();
        GrantLaboringMinesReward();
    }

    private void StartJobWithDuration()
    {
        if (TimeSystem.Instance != null)
        {
            TimeSystem.Instance.AnimateTimeChange(0, 12, 0, 0.5f);
        }
        else
        {
            Debug.LogError("JobSystem: TimeSystem.Instance is null! Cannot advance time.");
        }
    }

    public void StartJob(Job_SO_Constructor job)
    {
        if (job == null)
        {
            Debug.LogWarning("JobSystem: job is null.");
            return;
        }

        Debug.Log($"Starting job: {job.Name}");
        StartJobWithDuration();
        GrantJobRewards(job);
    }

    private int GetRand(int min, int max)
    {
        return Random.Range(min, max + 1);
    }

    // -----------------------------
    // STABLE JOB REWARDS
    // -----------------------------

    private void GrantMerchantsReward()
    {
        if (PlayerStatHandler.Instance == null)
        {
            Debug.LogError("JobSystem: PlayerStatHandler.Instance is null! Cannot grant merchants reward.");
            return;
        }

        int randxp = GetRand(20, 40);
        int randMoney = GetRand(80, 120);

        PlayerStatHandler.Instance.pd.AddMoney(0, randMoney);
        PlayerStatHandler.Instance.AddStatXP(StatType.Charisma, randxp);

        Debug.Log($"Reward: {randMoney} Silver & {randxp} Charisma XP");
        RefreshUI();
    }

    private void GrantScoutsReward()
    {
        if (PlayerStatHandler.Instance == null)
        {
            Debug.LogError("JobSystem: PlayerStatHandler.Instance is null! Cannot grant scouts reward.");
            return;
        }

        int randxp = GetRand(20, 40);
        int randMoney = GetRand(120, 150);

        PlayerStatHandler.Instance.pd.AddMoney(0, randMoney);
        PlayerStatHandler.Instance.AddStatXP(StatType.Dexterity, randxp);

        Debug.Log($"Reward: {randMoney} Silver & {randxp} Dexterity XP");
        RefreshUI();
    }

    private void GrantGatherHerbsReward()
    {
        if (InventorySystem.Instance == null || PlayerStatHandler.Instance == null)
        {
            Debug.LogError("JobSystem: required systems are null! Cannot grant gather herbs reward.");
            return;
        }

        int randHerbs = GetRand(5, 10);
        int randXP = GetRand(15, 30);

        InventorySystem.Instance.AddItem(new Item(7, "Herb", 0, 10, ItemCategory.CraftingMaterial, randHerbs, 0.2f, true, 99));
        PlayerStatHandler.Instance.AddStatXP(StatType.Dexterity, randXP);

        Debug.Log($"Reward: {randHerbs} Herbs & {randXP} Dexterity XP");
        RefreshUI();
    }

    private void GrantCuttingWoodsReward()
    {
        if (InventorySystem.Instance == null || PlayerStatHandler.Instance == null)
        {
            Debug.LogError("JobSystem: required systems are null! Cannot grant cutting woods reward.");
            return;
        }

        int randWood = GetRand(3, 6);
        int randxp = GetRand(20, 40);

        InventorySystem.Instance.AddItem(new Item(9, "Wood", 0, 10, ItemCategory.Resource, randWood, 1.5f, true, 99));
        PlayerStatHandler.Instance.AddStatXP(StatType.Strength, randxp);

        Debug.Log($"Reward: Wood x{randWood} & Strength XP");
        RefreshUI();
    }

    private void GrantLaboringMinesReward()
    {
        if (InventorySystem.Instance == null || PlayerStatHandler.Instance == null)
        {
            Debug.LogError("JobSystem: required systems are null! Cannot grant laboring mines reward.");
            return;
        }

        int randxp = GetRand(20, 40);
        int randStone = GetRand(3, 6);

        InventorySystem.Instance.AddItem(new Item(8, "Stone", 0, 10, ItemCategory.Resource, randStone, 2.0f, true, 99));

        if (Dice.Roll(100) <= 20)
        {
            InventorySystem.Instance.AddItem(new Item(5, "Iron Ingot", 1, 0, ItemCategory.CraftingMaterial, 1, 1.0f, true, 99));
            Debug.Log("Bonus Reward: Iron Ingot");
        }

        if (Dice.Roll(100) <= 5)
        {
            InventorySystem.Instance.AddItem(new Item(10, "Gold Nugget", 5, 0, ItemCategory.Misc, 1, 0.3f, true, 99));
            Debug.Log("Bonus Reward: Gold Nugget");
        }

        PlayerStatHandler.Instance.AddStatXP(StatType.Constitution, randxp);

        Debug.Log($"Reward: Stone x{randStone} & {randxp} Constitution XP");
        RefreshUI();
    }

    // -----------------------------
    // CUSTOM JOB REWARDS
    // -----------------------------

    private void GrantJobRewards(Job_SO_Constructor job)
    {
        if (PlayerStatHandler.Instance == null)
        {
            Debug.LogError("JobSystem: PlayerStatHandler.Instance is null! Cannot grant job rewards.");
            return;
        }

        int statXp = Random.Range(job.StatRewardMin, job.StatRewardMax + 1);

        PlayerStatHandler.Instance.pd.AddMoney(0, job.Silver);
        PlayerStatHandler.Instance.AddStatXP(job.TargetStat, statXp);

        Debug.Log($"Gained {job.Silver} Silver and {statXp} {job.TargetStat} XP.");
        RefreshUI();
    }

    public List<Job_SO_Constructor> GetAvailableJobs()
    {
        return availableJobs;
    }

    private void RefreshUI()
    {
        if (PlayerUISystem.Instance != null)
        {
            PlayerUISystem.Instance.UpdateUIObjects();
        }

        if (InventoryUI.Instance != null)
        {
            InventoryUI.Instance.UpdateInventoryUI();
        }
    }
}