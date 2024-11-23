using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "NewQuest", menuName = "SO System/QUEST")]
public class Quest_SO_Constructor : SO_Base
{
    public Quest_SO_Constructor()
    {
        Type = SOTypes.QUEST;

        ID = 0;
        Name = "New Quest";
        Description = "This is a new quest.";

        DC = 10;

        CompletionHour = 1;
        CompletionMinute = 0;

        Reward = 100;
    }
}