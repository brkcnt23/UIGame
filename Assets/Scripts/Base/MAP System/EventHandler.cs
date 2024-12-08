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
        currentEvent.encounterCooldown = 7;

        events.Find(x => x.ID == currentEvent.ID).encounterCooldown = currentEvent.encounterCooldown;

        currentEvent.HandleEvent(PlayerStatHandler.Instance, choice);

        currentEvent = null;
    }

    public Event_SO_Constructor GenerateEvent()
    {
        Event_SO_Constructor _event = events[Random.Range(0, events.Count)];

        if (_event.encounterCooldown > 0)
        {
            return GenerateEvent(); //recursive call but it stack overflows
        }
        else
        {
            currentEvent = _event;
            return _event;
        }
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
