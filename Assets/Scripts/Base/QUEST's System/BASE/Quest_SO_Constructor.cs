using UnityEngine;

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

        CompletionTime = 1;

        Reward = 100;
    }
}