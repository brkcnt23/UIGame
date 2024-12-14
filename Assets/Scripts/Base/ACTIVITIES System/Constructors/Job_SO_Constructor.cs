using UnityEngine;
[System.Serializable]
public class Job_SO_Constructor : SO_Base
{
    public Job_SO_Constructor()
    {
        Type = SOTypes.JOB;

        ID = 0;
        Name = "New Job";
        Description = "This is a new job.";

        DC = 10;

        CompletionDay = 0;
        CompletionHour = 2;
        CompletionMinute = 0;

        Silver = 100;
        
        TargetStat = StatType.Constitution;
        StatRewardMin = 1;
        StatRewardMax = 3;
    }
}
