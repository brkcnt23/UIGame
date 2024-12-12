using UnityEngine;
using NEXUS.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class TravelSystem : MonoBehaviour
{
    public static TravelSystem Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public SettlementButtonPointer currentSettlement;
    public SettlementButtonPointer destination;

    public bool inTravel = false;

    [Header("Travel Multipliers")]
    public int distanceMultiplier = 1;

    [Header("Travel Variables")]
    int remainingTime;
    int remainingTimeMinutes;
    int minEvents = 0;

    List<int> eventTimes = new List<int>();

    [Header("TravelDecider Panel")]

    public GameObject TravelingDeciderPanel;
    public bool isHuntingForRations = false;
    public bool isSleeping = false;

    public TMP_Text travelInfoText;
    public TMP_Text travelTimeText;


    public GameObject eventPanel;

    public bool isEventActive = false;
    public GameObject TravelingPanel;
    public bool PlayerWantsToHandleEventorEnterSettlement = false;

    public Event_SO_Constructor currentEvent;

    public TravelWrapper travelData = new TravelWrapper();

    public void SaveTravelData()
    {
        if (!inTravel)
        {
            return;
        }
        travelData.inTravel = inTravel;
        currentSettlement = MapHandler.Instance.GetLastVisitedSettlement();
        destination = MapHandler.Instance.GetDestinationSettlement();
        currentEvent = EventHandler.Instance.currentEvent;
        travelData.currentSettlementID = currentSettlement != null ? currentSettlement.settlement.ID : 0;
        travelData.destinationID = destination != null ? destination.settlement.ID : 0;
        if (isEventActive)
        {
            travelData.isEventActive = isEventActive;
            if (currentEvent != null || currentEvent.ID != 0)
            {
                travelData.currentEventID = currentEvent.ID;
            }
        }
        travelData.remainingTime = remainingTime;
        travelData.remainingTimeMinutes = remainingTimeMinutes;
        travelData.minEvents = minEvents;
        travelData.eventTimes = eventTimes;
        travelData.PlayerWantsToHandleEventorEnterSettlement = PlayerWantsToHandleEventorEnterSettlement;
        travelData.elapsedTravelTime = elapsedTravelTime;
        travelData.eventIndex = eventIndex;

        JSONDataHandler jSONDataHandler = new JSONDataHandler(PlayerPrefs.GetInt("Slot"));
        jSONDataHandler.SaveData(travelData, "travel.json");
    }

    public void LoadTravelData()
    {
        JSONDataHandler jSONDataHandler = new JSONDataHandler(PlayerPrefs.GetInt("Slot"));
        TravelWrapper wrapper = jSONDataHandler.LoadData<TravelWrapper>("travel.json");

        travelData = wrapper != null ? wrapper : new TravelWrapper();

        if (travelData.inTravel)
        {
            if (MapHandler.Instance == null || GameManager.Instance == null || EventHandler.Instance == null)
            {
                Debug.LogError("Required instances are not initialized. Cannot load travel data.");
                return;
            }
            GameManager.Instance.DisableAllPanels();
            GameManager.Instance.navPanel.SetActive(true);
            GameManager.Instance.infoPanel.SetActive(true);


            MapHandler.Instance.map.SetActive(true);
            MapHandler.Instance.map.transform.parent.gameObject.SetActive(true);
            MapHandler.Instance.PopulateMap();

            currentSettlement = GetSettlementButtonPointerByID(travelData.currentSettlementID);
            destination = GetSettlementButtonPointerByID(travelData.destinationID);
            currentEvent = GetEventByID(travelData.currentEventID);

            remainingTime = travelData.remainingTime;
            remainingTimeMinutes = travelData.remainingTimeMinutes;
            minEvents = travelData.minEvents;
            eventTimes = travelData.eventTimes;
            isEventActive = travelData.isEventActive;
            PlayerWantsToHandleEventorEnterSettlement = travelData.PlayerWantsToHandleEventorEnterSettlement;
            elapsedTravelTime = travelData.elapsedTravelTime;
            eventIndex = travelData.eventIndex;
            inTravel = travelData.inTravel;

            if (EventHandler.Instance != null)
            {
                EventHandler.Instance.currentEvent = currentEvent;
            }
            else
            {
                Debug.LogError("EventHandler.Instance is null, cannot set currentEvent.");
            }

            ContinueTravel();
        }
    }

    public void LoadTravelDataFromSourceData()
    {
        JSONDataHandler jSONDataHandler = new JSONDataHandler("SourceData");
        TravelWrapper wrapper = jSONDataHandler.LoadData<TravelWrapper>("trav   el.json");

        travelData = wrapper != null ? wrapper : new TravelWrapper();
    }

    public SettlementButtonPointer GetSettlementButtonPointerByID(int id)
    {
        foreach (GameObject child in MapHandler.Instance.children)
        {
            SettlementButtonPointer settlementButtonPointer = child.GetComponent<SettlementButtonPointer>();

            if (settlementButtonPointer.settlement.ID == id)
            {
                return settlementButtonPointer;
            }
        }

        return null;
    }

    public Event_SO_Constructor GetEventByID(int id)
    {
        foreach (Event_SO_Constructor e in EventHandler.Instance.events)
        {
            if (e.ID == id)
            {
                return e;
            }
        }

        return null;
    }
    public void TravelToSettlement()
    {
        inTravel = true;
        remainingTime = 0;
        int minutes = CalculateDistance();

        int hours = minutes / 60;
        minutes = minutes % 60;
        remainingTime = hours;
        remainingTimeMinutes = minutes;

        int days = hours / 24;
        hours = hours % 24;

        SettlementHandler.Instance.OnSettlementExited();
        ContinueTravel();


    }

    public int CalculateDistance()
    {
        Vector2 currentPos = currentSettlement.transform.position;
        Vector2 destinationPos = destination.transform.position;

        float distance = Vector2.Distance(currentPos, destinationPos) * distanceMultiplier;
        int minutes = (int)distance;
        int hours = minutes / 60;
        int days = hours / 24;

        if (isHuntingForRations)
        {
            distance += 60 * days; //add 60 minutes for each day of hunting
        }

        if (isSleeping)
        {
            distance += 360 * days + PlayerStatHandler.Instance.pd.CurrentExhaustionLevel * 120; //add 360 minutes for each day of sleeping and 120 minutes for each exhaustion level
        }

        return (int)distance;
    }

    public void ContinueTravel()
    {
        StopAllCoroutines();

        if (!TravelingPanel.activeSelf)
        {
            TravelingPanel.SetActive(true);
        }

        if (currentEvent.ID != 0)
        {
            HandleEvent();
        }
        else
        {
            StartCoroutine(TravelUntilEvent());
        }
    }


    private int elapsedTravelTime = 0;
    private int eventIndex = 0;

    public IEnumerator TravelUntilEvent()
    {
        int totalTravelTime = remainingTime + elapsedTravelTime;
        // If eventTimes is null or empty, generate them
        if (eventTimes == null || eventTimes.Count == 0)
        {
            // Determine the number of events
            int minEvents = this.minEvents;
            int maxEvents = Mathf.Max(1, totalTravelTime / 15);
            int numberOfEvents = Random.Range(minEvents, maxEvents + 1);

            // Generate event times
            eventTimes = new List<int>();
            for (int i = 0; i < numberOfEvents; i++)
            {
                int eventTime = Random.Range(1, totalTravelTime - 1);
                eventTimes.Add(eventTime);
            }
            eventTimes.Sort();

            travelData.eventTimes = eventTimes;
        }

        while (remainingTime > 0)
        {
            int timeUntilNextEvent = remainingTime;

            if (eventIndex < eventTimes.Count)
            {
                timeUntilNextEvent = eventTimes[eventIndex] - elapsedTravelTime;
                timeUntilNextEvent = Mathf.Max(1, timeUntilNextEvent);
            }

            int travelSegment = Mathf.Min(timeUntilNextEvent, remainingTime);

            // Advance time smoothly
            yield return StartCoroutine(TimeSystem.Instance.AdvanceTimeCoroutine(0, travelSegment, 0));

            remainingTime -= travelSegment;
            elapsedTravelTime += travelSegment;

            if (eventIndex < eventTimes.Count && elapsedTravelTime >= eventTimes[eventIndex])
            {
                HandleEvent();
                eventIndex++;
                isEventActive = true;

                print("Event at " + elapsedTravelTime + " hours");

                // Wait until the event is resolved
                yield return new WaitUntil(() => !isEventActive);
                isEventActive = false;
                currentEvent.ID = 0;
            }
        }

        if (remainingTime <= 0)
        {
            print("Arrived at destination");

            TravelDone();
        }
    }

    public void TravelDone()
    {
        EnterSettlement();

        inTravel = false;
        ResetTravelData();
    }

    public void EnterSettlement()
    {
        MapHandler.Instance.map.SetActive(false);
        MapHandler.Instance.map.transform.parent.gameObject.SetActive(false);
        GameManager.Instance.ShowSettlementPanel();


        currentSettlement = destination;
        MapHandler.Instance.MovePlayerToLastVisitedSettlement(currentSettlement.settlement);

        TimeSystem.Instance.AdvanceTimeCoroutine(0,0,remainingTimeMinutes);
    }

    public void HandleEvent()
    {
        TravelingPanel.transform.GetChild(0).gameObject.SetActive(true);

        ShowEventPanel();
    }

 public void ShowEventPanel()
{
    if (eventPanel == null)
    {
        Debug.LogError("eventPanel is not assigned to TravelSystem!");
        return;
    }

    EventPanel ep = eventPanel.GetComponent<EventPanel>();
    if (ep == null)
    {
        Debug.LogError("eventPanel does not have an EventPanel component attached!");
        return;
    }

    if (currentEvent == null)
    {
        Debug.LogWarning("currentEvent is null, generating a new event.");
        currentEvent = EventHandler.Instance.GenerateEvent();
        if (currentEvent == null)
        {
            Debug.LogError("No events could be generated. Cannot show event panel.");
            return;
        }
    }

    if (currentEvent.ID == 0)
    {
        // If ID == 0, attempt to generate a new event
        Event_SO_Constructor randomEvent = EventHandler.Instance.GenerateEvent();
        currentEvent = randomEvent;
        if (currentEvent == null)
        {
            Debug.LogError("No events available to generate. Cannot show event panel.");
            return;
        }
    }
    eventPanel.SetActive(true);
    ep.ShowEvent(currentEvent, remainingTime);
}


    public void ResetTravelData()
    {
        destination = null;
        currentEvent.ID = 0;
        remainingTime = 0;
        remainingTimeMinutes = 0;
        minEvents = 0;
        eventTimes = new List<int>();
        isEventActive = false;
        PlayerWantsToHandleEventorEnterSettlement = false;
        elapsedTravelTime = 0;
        eventIndex = 0;
    }

    public void UpdateTravelTimeText()
    {
        int minutes = CalculateDistance();
        int hours = minutes / 60;
        minutes = minutes % 60;
        int days = hours / 24;
        hours = hours % 24;
        travelTimeText.text = $"Travel time: {days} days, {hours} hours, {minutes} minutes";
    }

    public void PlayerWantsToHunt()
    {
        isHuntingForRations = !isHuntingForRations;
        UpdateTravelTimeText();
    }

    public void PlayerWantsToSleep()
    {
        isSleeping = !isSleeping;
        UpdateTravelTimeText();
    }

    public void PlayerAcceptedToTravel()
    {
        travelTimeText.text = "";
        travelInfoText.text = "";
        TravelingDeciderPanel.SetActive(false);
        destination = MapHandler.Instance.selectedSettlement.GetComponent<SettlementButtonPointer>();
        TravelToSettlement();
    }

    public void PlayerDeclinedToTravel()
    {
        TravelingDeciderPanel.SetActive(false);
        MapHandler.Instance.map.SetActive(false);
        MapHandler.Instance.map.transform.parent.gameObject.SetActive(false);
        GameManager.Instance.ShowSettlementPanel();
        destination = null;
    }

    public void PlayerMiniGameClosed()
    {
        PlayerWantsToHandleEventorEnterSettlement = true;
        TravelingPanel.SetActive(false);
    }
}

[System.Serializable]
public class TravelWrapper
{
    public bool inTravel = false;
    public int currentSettlementID = 0;
    public int destinationID = 0;
    public int remainingTime = 0;
    public int remainingTimeMinutes = 0;
    public int minEvents = 0;
    public List<int> eventTimes = new List<int>();
    public bool isEventActive = false;
    public bool PlayerWantsToHandleEventorEnterSettlement = false;
    public int currentEventID = 0;
    public int elapsedTravelTime = 0;
    public int eventIndex = 0;
}