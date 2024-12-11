using UnityEngine;
using NEXUS.Utilities;
using System.Collections.Generic;
using UnityEngine.UI;

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

    public Event_SO_Constructor currentEvent;

    JSONDataHandler JSONDataHandler;

    public void SaveEvents()
    {
        JSONDataHandler = new JSONDataHandler(PlayerPrefs.GetInt("Slot"));
        JSONDataHandler.SaveData(new EventWrapper { events = events }, "events.json");
    }

    public void LoadEvents()
    {
        JSONDataHandler = new JSONDataHandler(PlayerPrefs.GetInt("Slot"));
        EventWrapper wrapper = JSONDataHandler.LoadData<EventWrapper>("events.json");
        events = wrapper != null ? wrapper.events : new List<Event_SO_Constructor>();
    }

    public void LoadEventsFromSourceData()
    {
        JSONDataHandler = new JSONDataHandler("SourceData");
        EventWrapper wrapper = JSONDataHandler.LoadData<EventWrapper>("events.json");
        events = wrapper != null ? wrapper.events : new List<Event_SO_Constructor>();
    }

    public void HandleEvent(Choice choice)
    {
        // Update event cooldown
        events.Find(x => x.ID == currentEvent.ID).encounterCooldown = currentEvent.encounterCooldown;

        // Handle event outcome
        currentEvent.HandleEvent(PlayerStatHandler.Instance, choice);

        currentEvent = null;
    }

    public Event_SO_Constructor GenerateEvent()
    {
        //return a random event but not the event cooldown is not 0
        List<Event_SO_Constructor> availableEvents = events.FindAll(x => x.encounterCooldown == 0);

        if (availableEvents.Count == 0)
        {
            return null;
        }

        Event_SO_Constructor _event = availableEvents[Random.Range(0, availableEvents.Count)];

        currentEvent = _event;

        return _event;
    }

    public Event_SO_Constructor GetEventByID(int id)
    {
        return events.Find(x => x.ID == id);
    }
}

[System.Serializable]
public class EventWrapper
{
    public List<Event_SO_Constructor> events;
}
