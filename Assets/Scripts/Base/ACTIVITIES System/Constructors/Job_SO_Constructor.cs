using UnityEngine;
[System.Serializable]
[CreateAssetMenu(fileName = "NewJob", menuName = "SO System/JOB")]
public class Job_SO_Constructor : SO_Base
{
    public int StatRewardMin; // Minimum stat points rewarded
    public int StatRewardMax; // Maximum stat points rewarded
    public string TargetStat; // The stat to be rewarded (Constitution, Charisma, Dexterity)

    public Job_SO_Constructor()
    {
        Type = SOTypes.JOB;

        ID = 0;
        Name = "New Job";
        Description = "This is a new job.";
        DC = 10;
        CompletionHour = 2;
        CompletionMinute = 0;
        Reward = 100;
        StatRewardMin = 1;
        StatRewardMax = 2;
        TargetStat = "Constitution";
    }
}
