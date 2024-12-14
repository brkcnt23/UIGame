using UnityEngine;
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

    public void StartJob(Job_SO_Constructor job)
    {
        Debug.Log($"Starting job: {job.Name}");
        // Advance time for job completion
        int jobDurationMinutes = (job.CompletionHour * 60) + job.CompletionMinute;
        timeSystem.AdvanceTimeCoroutine(0, jobDurationMinutes / 60, jobDurationMinutes % 60);

        // Grant stat reward
        int rewardPoints = Random.Range(job.StatRewardMin, job.StatRewardMax + 1);
        GrantStatReward(job.TargetStat, rewardPoints);

        Debug.Log($"Completed job: {job.Name}. Gained {rewardPoints} {job.TargetStat}.");
    }

    private void GrantStatReward(StatType stat, int rewardPoints)
    {
        switch (stat)
        {
            case StatType.Constitution:
                playerData.Constitution += rewardPoints;
                break;
            case StatType.Charisma:
                playerData.Charisma += rewardPoints;
                break;
            case StatType.Dexterity:
                playerData.Dexterity += rewardPoints;
                break;
            default:
                Debug.LogWarning($"Unknown stat: {stat}");
                break;
        }
        PlayerUISystem.Instance.UpdateExhaustionText(); // Update UI if necessary
    }

    public List<Job_SO_Constructor> GetAvailableJobs()
    {
        return availableJobs;
    }
}
