using UnityEngine;

[CreateAssetMenu(fileName = "NewJob", menuName = "SO System/JOB")]
public class Job_SO_Constructor : SO_Base
{
    public Job_SO_Constructor()
    {
        Type = SOTypes.JOB;

        ID = 0;
        Name = "New Job";
        Description = "This is a new job.";

        DC = 10;

        CompletionHour = 1;

        Reward = 100;
    }
}
