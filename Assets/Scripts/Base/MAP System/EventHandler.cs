using UnityEngine;
using NEXUS.Utilities;
using System.Collections.Generic;

public class EventHandler : MonoBehaviour
{
    public static EventHandler Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    public List<Event_SO_Constructor> events = new List<Event_SO_Constructor>();

    JSONDataHandler JSONDataHandler = new JSONDataHandler();

    void Wrappers()
    {
        EventWrapper wrapper = JSONDataHandler.LoadData<EventWrapper>("events.json");
        events = wrapper != null ? wrapper.events : new List<Event_SO_Constructor>();
    }

    void Start()
    {
        Wrappers();
    }

    public void OnDestroy()
    {
        JSONDataHandler.SaveData(new EventWrapper { events = events }, "events.json");
    }

    public void HandleEvent()
    {
        Event_SO_Constructor currentEvent = events[Random.Range(0, events.Count)];

        Debug.Log("Event: " + currentEvent.Name);
        Debug.Log("Description: " + currentEvent.Description);
        Debug.Log("DC: " + currentEvent.DC);
        Debug.Log("Reward: " + currentEvent.Gold + " Gold");
        Debug.Log("Reward: " + currentEvent.StatRewardMin + " to " + currentEvent.StatRewardMax + " " + currentEvent.TargetStat);

        //check if the event is successful
    }
}

[System.Serializable]
public class EventWrapper
{
    public List<Event_SO_Constructor> events;
}
