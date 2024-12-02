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

        Wrappers();
    }

    public List<Event_SO_Constructor> events = new List<Event_SO_Constructor>();

    public Event_SO_Constructor currentEvent;

    JSONDataHandler JSONDataHandler;

    public void Wrappers()
    {
        JSONDataHandler = new JSONDataHandler("SourceData");
        EventWrapper wrapper = JSONDataHandler.LoadData<EventWrapper>("events.json");
        events = wrapper != null ? wrapper.events : new List<Event_SO_Constructor>();
    }

    public void OnDestroy()
    {
        JSONDataHandler.SaveData(new EventWrapper { events = events }, "events.json");
    }

    public void HandleEvent(int choice)
    {
        currentEvent = events[GenerateEvent()];

        switch (choice)
        {
            case 0:
                currentEvent.EventSuccessful(PlayerStatHandler.Instance);
                break;
            case 1:
                currentEvent.EventFailed(PlayerStatHandler.Instance);
                break;
            case 2:
                currentEvent.EventNeutral(PlayerStatHandler.Instance);
                break;
            case 3:
                currentEvent.EventCritical(PlayerStatHandler.Instance);
                break;
            case 4:
                currentEvent.EventDeclined(PlayerStatHandler.Instance);
                break;
        }

        currentEvent = null;
        TravelSystem.Instance.isEventActive = false;
    }

    public int GenerateEvent()
    {
        int eventID = Random.Range(0, events.Count);

        return eventID;
    }
}

[System.Serializable]
public class EventWrapper
{
    public List<Event_SO_Constructor> events;
}
