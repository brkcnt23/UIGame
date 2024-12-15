using UnityEngine;
using NEXUS.Utilities;
using System.Collections.Generic;

public class JobManager : MonoBehaviour
{
    public static JobManager Instance { get; private set; }
    [SerializeField] private List<Job_SO_Constructor> availableJobs; // List of available jobs
    private PlayerData playerData;
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
        playerData = PlayerStatHandler.Instance.pd;
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
        int jobDurationMinutes = 12 * 60; // 12 hours
        timeSystem.AdvanceTimeCoroutine(0, jobDurationMinutes / 60, jobDurationMinutes % 60);
    }

    // Public method for starting custom jobs
    public void StartJob(Job_SO_Constructor job)
    {
        Debug.Log($"Starting job: {job.Name}");
        StartJobWithDuration();
        GrantJobRewards(job);
    }

    // Individual reward methods
    private void GrantMerchantsReward()
    {
        playerData.Silver += 100;
        PlayerStatHandler.Instance.AddStatXP(StatType.Charisma, Random.Range(20, 40));
        Debug.Log("Reward: 100 Silver & Charisma XP");
    }

    private void GrantScoutsReward()
    {
        playerData.Silver += 100;
        PlayerStatHandler.Instance.AddStatXP(StatType.Dexterity, Random.Range(20, 40));
        Debug.Log("Reward: 100 Silver & Dexterity XP");
    }

    private void GrantCuttingWoodsReward()
    {
        playerData.Items.Add(new Item(9, "Wood", 30, ItemCategory.Resource, quantity: Random.Range(3, 6)));
        PlayerStatHandler.Instance.AddStatXP(StatType.Strength, Random.Range(20, 40));
        Debug.Log("Reward: Wood (3-6) & Strength XP");
    }

    private void GrantLaboringMinesReward()
    {
        playerData.Items.Add(new Item(8, "Stone", 40, ItemCategory.Resource, quantity: Random.Range(3, 6)));
        if (Dice.Roll(100) <= 20)
        {
            playerData.Items.Add(new Item(5, "Iron Ingot", 100, ItemCategory.CraftingMaterial, quantity: 1));
            Debug.Log("Bonus Reward: Iron Ingot");
        }
        if (Dice.Roll(100) <= 5)
        {
            playerData.Items.Add(new Item(10, "Gold Nugget", 500, ItemCategory.CraftingMaterial, quantity: 1));
            Debug.Log("Bonus Reward: Gold Nugget");
        }
        PlayerStatHandler.Instance.AddStatXP(StatType.Constitution, Random.Range(20, 40));
        Debug.Log("Reward: Stone (3-6) & Constitution XP");
    }

    // General reward method for custom jobs
    private void GrantJobRewards(Job_SO_Constructor job)
    {
        switch (job.TargetStat)
        {
            case StatType.Charisma:
                playerData.Silver += job.Silver;
                playerData.CharismaXP += Random.Range(job.StatRewardMin, job.StatRewardMax + 1);
                break;

            case StatType.Dexterity:
                playerData.Silver += job.Silver;
                playerData.DexterityXP += Random.Range(job.StatRewardMin, job.StatRewardMax + 1);
                break;

            case StatType.Strength:
                playerData.StrengthXP += Random.Range(job.StatRewardMin, job.StatRewardMax + 1);
                break;

            case StatType.Constitution:
                playerData.ConstitutionXP += Random.Range(job.StatRewardMin, job.StatRewardMax + 1);
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
