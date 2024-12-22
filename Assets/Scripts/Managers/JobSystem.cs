using UnityEngine;
using NEXUS.Utilities;
using System.Collections.Generic;

public class JobSystem : MonoBehaviour
{
    public static JobSystem Instance { get; set; }
    [SerializeField] private List<Job_SO_Constructor> availableJobs; // List of available jobs
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
        }
    }

    private void Start()
    {
        timeSystem = TimeSystem.Instance;
    }

    // Method for "Help the Merchants"
    public void StartHelpMerchants()
    {
        Debug.Log("Starting job: Help the Merchants");
        StartJobWithDuration();
        GrantMerchantsReward();
    }

    // Method for "Help the Scouts"
    public void StartHelpScouts()
    {
        Debug.Log("Starting job: Help the Scouts");
        StartJobWithDuration();
        GrantScoutsReward();
    }
    public void StartGatherHerbs()
    {
        Debug.Log("Starting job: Gathering Herbs");
        StartJobWithDuration(); // Use the time system to simulate job duration
        GrantGatherHerbsReward();
    }

    // Method for "Cutting Woods"
    public void StartCuttingWoods()
    {
        Debug.Log("Starting job: Cutting Woods");
        StartJobWithDuration();
        GrantCuttingWoodsReward();
    }

    // Method for "Laboring Mines"
    public void StartLaboringMines()
    {
        Debug.Log("Starting job: Laboring Mines");
        StartJobWithDuration();
        GrantLaboringMinesReward();
    }

    // General method to handle job duration
    private void StartJobWithDuration()
    {
        //int jobDurationMinutes = 12 * 60; // 12 hours
        //StartCoroutine(timeSystem.AdvanceTimeCoroutine(0,12,0));
        TimeSystem.Instance.AnimateTimeChange(0, 12, 0, 0.5f);
    }

    // Public method for starting custom jobs
    public void StartJob(Job_SO_Constructor job)
    {
        Debug.Log($"Starting job: {job.Name}");
        StartJobWithDuration();
        GrantJobRewards(job);
    }
    private int GetRand(int min, int max)
    {
        int rand = Random.Range(min, max + 1);
        return rand;
    }

    // Individual reward methods
    private void GrantMerchantsReward()
    {
        int randxp = GetRand(20, 40);
        int randmn = GetRand(80, 120);
        PlayerStatHandler.Instance.pd.Silver += randmn;
        PlayerStatHandler.Instance.AddStatXP(StatType.Charisma, randxp);
        Debug.Log($"Reward: {randmn} Silver & {randxp} Charisma XP");
    }

    private void GrantScoutsReward()
    {
        int randxp = GetRand(20, 40);
        int randmn = GetRand(120, 150);
        PlayerStatHandler.Instance.pd.Silver += randmn;
        PlayerStatHandler.Instance.AddStatXP(StatType.Dexterity, randxp);
        Debug.Log($"Reward: {randmn} Silver & {randxp} Dexterity XP");
    }

    private void GrantGatherHerbsReward()
    {
        int randH = GetRand(5, 10); // Randomly determine the number of herbs
        int randXP = GetRand(15, 30); // Random XP reward for gathering herbs
        InventorySystem.Instance.AddItem(new Item(7, "Herb", 0, 10, ItemCategory.CraftingMaterial, randH));
        Debug.Log($"Reward: {randH} Herbs & {randXP} Dexterity XP");

    }

    private void GrantCuttingWoodsReward()
    {
        int randw = GetRand(3, 6);
        int randxp = GetRand(20, 40);
        InventorySystem.Instance.AddItem(new Item(9, "Wood", 0, 10, ItemCategory.Resource, randw));
        PlayerStatHandler.Instance.AddStatXP(StatType.Strength, randxp);
        Debug.Log($"Reward: Wood {randw} & Strength XP");
    }

    private void GrantLaboringMinesReward()
    {
        int randxp = GetRand(20, 40);
        int rands = GetRand(3, 6);
        InventorySystem.Instance.AddItem(new Item(8, "Stone", 0, 10, ItemCategory.Resource, rands));
        if (Dice.Roll(100) <= 20)
        {
            InventorySystem.Instance.AddItem(new Item(5, "Iron Ingot", 1, 0, ItemCategory.CraftingMaterial, 1));
            Debug.Log("Bonus Reward: Iron Ingot");
        }
        if (Dice.Roll(100) <= 5)
        {
            InventorySystem.Instance.AddItem(new Item(10, "Gold Nugget", 5, 0, ItemCategory.Misc, 1));
            Debug.Log("Bonus Reward: Gold Nugget");
        }
        PlayerStatHandler.Instance.AddStatXP(StatType.Constitution, randxp);
        Debug.Log($"Reward: Stone {rands} & {randxp} Constitution XP");
    }

    // General reward method for custom jobs
    private void GrantJobRewards(Job_SO_Constructor job)
    {
        switch (job.TargetStat)
        {
            case StatType.Charisma:
                PlayerStatHandler.Instance.pd.Silver += job.Silver;
                PlayerStatHandler.Instance.pd.CharismaXP += Random.Range(job.StatRewardMin, job.StatRewardMax + 1);
                break;

            case StatType.Dexterity:
                PlayerStatHandler.Instance.pd.Silver += job.Silver;
                PlayerStatHandler.Instance.pd.DexterityXP += Random.Range(job.StatRewardMin, job.StatRewardMax + 1);
                break;

            case StatType.Strength:
                PlayerStatHandler.Instance.pd.Silver += job.Silver;
                PlayerStatHandler.Instance.pd.StrengthXP += Random.Range(job.StatRewardMin, job.StatRewardMax + 1);
                break;

            case StatType.Constitution:
                PlayerStatHandler.Instance.pd.Silver += job.Silver;
                PlayerStatHandler.Instance.pd.ConstitutionXP += Random.Range(job.StatRewardMin, job.StatRewardMax + 1);
                break;

            default:
                Debug.LogWarning("Unknown stat type for rewards.");
                break;
        }

        Debug.Log($"Gained {job.Silver} Silver and {job.TargetStat} XP.");
        PlayerUISystem.Instance.UpdateExhaustionText();
    }

    public List<Job_SO_Constructor> GetAvailableJobs()
    {
        return availableJobs;
    }
}
