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

        // Log to JobLogs panel
        if (JobLogger.Instance != null)
        {
            JobLogger.Instance.LogJobComplete("Help the Merchants", randMoney, randxp, "Charisma");
            if (PlayerStatHandler.Instance.pd.CurrentExhaustionLevel >= 7)
            {
                JobLogger.Instance.LogExhaustionWarning(PlayerStatHandler.Instance.pd.CurrentExhaustionLevel, PlayerStatHandler.Instance.pd.MaxExhaustionLevel);
            }
        }

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
        bool damageOccurred = false;
        int damageAmount = 0;

        PlayerStatHandler.Instance.pd.AddMoney(0, randMoney);
        PlayerStatHandler.Instance.AddStatXP(StatType.Dexterity, randxp);

        // 50% chance of taking damage from scout work (skirmish/pursuit)
        if (Random.value > 0.5f)
        {
            damageAmount = GetRand(5, 10);
            PlayerStatHandler.Instance.pd.Health = Mathf.Max(0, PlayerStatHandler.Instance.pd.Health - damageAmount);
            damageOccurred = true;
            Debug.Log($"Yaralandın! -{damageAmount} Health. Şu anki Can: {PlayerStatHandler.Instance.pd.Health}");
        }

        Debug.Log($"Reward: {randMoney} Silver & {randxp} Dexterity XP");

        // Log to JobLogs panel
        if (JobLogger.Instance != null)
        {
            JobLogger.Instance.LogJobComplete("Help the Scouts", randMoney, randxp, "Dexterity", damageOccurred, damageAmount);
        }

        RefreshUI();
    }

    private void GrantGatherHerbsReward()
    {
        if (PlayerStatHandler.Instance == null)
        {
            Debug.LogError("JobSystem: required systems are null! Cannot grant gather herbs reward.");
            return;
        }

        int randHerbs = GetRand(5, 10);
        int randXP = GetRand(15, 30);

        GameBootstrapper.Events?.Dispatch(new AddItemEvent(7, randHerbs));
        PlayerStatHandler.Instance.AddStatXP(StatType.Dexterity, randXP);

        Debug.Log($"Reward: {randHerbs} Herbs & {randXP} Dexterity XP");

        // Log to JobLogs panel
        if (JobLogger.Instance != null)
        {
            JobLogger.Instance.LogJobComplete("Gathering Herbs", randHerbs, randXP, "Dexterity");
            if (PlayerStatHandler.Instance.pd.CurrentExhaustionLevel >= 7)
            {
                JobLogger.Instance.LogExhaustionWarning(PlayerStatHandler.Instance.pd.CurrentExhaustionLevel, PlayerStatHandler.Instance.pd.MaxExhaustionLevel);
            }
        }

        RefreshUI();
    }

    private void GrantCuttingWoodsReward()
    {
        if (PlayerStatHandler.Instance == null)
        {
            Debug.LogError("JobSystem: required systems are null! Cannot grant cutting woods reward.");
            return;
        }

        int randWood = GetRand(3, 6);
        int randxp = GetRand(20, 40);

        GameBootstrapper.Events?.Dispatch(new AddItemEvent(9, randWood));
        PlayerStatHandler.Instance.AddStatXP(StatType.Strength, randxp);

        Debug.Log($"Reward: Wood x{randWood} & Strength XP");

        // Log to JobLogs panel
        if (JobLogger.Instance != null)
        {
            JobLogger.Instance.LogJobComplete("Cutting Woods", randWood, randxp, "Strength");
            if (PlayerStatHandler.Instance.pd.CurrentExhaustionLevel >= 7)
            {
                JobLogger.Instance.LogExhaustionWarning(PlayerStatHandler.Instance.pd.CurrentExhaustionLevel, PlayerStatHandler.Instance.pd.MaxExhaustionLevel);
            }
        }

        RefreshUI();
    }

    private void GrantLaboringMinesReward()
    {
        if (PlayerStatHandler.Instance == null)
        {
            Debug.LogError("JobSystem: required systems are null! Cannot grant laboring mines reward.");
            return;
        }

        int randxp = GetRand(20, 40);
        int randStone = GetRand(3, 6);

        GameBootstrapper.Events?.Dispatch(new AddItemEvent(8, randStone));

        if (Dice.Roll(100) <= 20)
        {
            GameBootstrapper.Events?.Dispatch(new AddItemEvent(5, 1));
            Debug.Log("Bonus Reward: Iron Ingot");
        }

        if (Dice.Roll(100) <= 5)
        {
            GameBootstrapper.Events?.Dispatch(new AddItemEvent(10, 1));
            Debug.Log("Bonus Reward: Gold Nugget");
        }

        PlayerStatHandler.Instance.AddStatXP(StatType.Constitution, randxp);

        Debug.Log($"Reward: Stone x{randStone} & {randxp} Constitution XP");

        // Log to JobLogs panel
        if (JobLogger.Instance != null)
        {
            JobLogger.Instance.LogJobComplete("Laboring Mines", randStone, randxp, "Constitution");
            if (PlayerStatHandler.Instance.pd.CurrentExhaustionLevel >= 7)
            {
                JobLogger.Instance.LogExhaustionWarning(PlayerStatHandler.Instance.pd.CurrentExhaustionLevel, PlayerStatHandler.Instance.pd.MaxExhaustionLevel);
            }
        }

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

        // Log to JobLogs panel
        if (JobLogger.Instance != null)
        {
            JobLogger.Instance.LogJobComplete(job.Name, job.Silver, statXp, job.TargetStat.ToString());
            if (PlayerStatHandler.Instance.pd.CurrentExhaustionLevel >= 7)
            {
                JobLogger.Instance.LogExhaustionWarning(PlayerStatHandler.Instance.pd.CurrentExhaustionLevel, PlayerStatHandler.Instance.pd.MaxExhaustionLevel);
            }
        }

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

        // UI updates handled by StateManager listeners
        {
        }
    }
}