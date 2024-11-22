using UnityEngine;

[CreateAssetMenu(fileName = "NewEvent", menuName = "SO System/EVENT")]
public class Event_SO_Constructor : SO_Base
{
    public Event_SO_Constructor()
    {
        Type = SOTypes.EVENT;

        ID = 0;
        Name = "New Event";
        Description = "This is a new event.";

        DC = 10;

        CompletionTime = 1;

        Reward = 100;
    }
}