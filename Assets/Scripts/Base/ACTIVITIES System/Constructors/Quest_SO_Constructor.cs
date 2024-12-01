using UnityEngine;

[System.Serializable]
public class Quest_SO_Constructor : SO_Base
{
    public Quest_SO_Constructor()
    {
        Type = SOTypes.QUEST;

        ID = 0;
        Name = "New Quest";
        Description = "This is a new quest.";

        DC = 10;

        CompletionDay = 0;
        CompletionHour = 1;
        CompletionMinute = 0;

        Silver = 100;

        TargetStat = "Constitution";
        StatRewardMin = 1;
        StatRewardMax = 3;
    }
}