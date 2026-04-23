using UnityEngine;
using NEXUS.Utilities;
using System.Collections;
using System.Collections.Generic;
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
            return;
        }
    }

    public SettlementButtonPointer currentSettlement;
    public SettlementButtonPointer destination;

    public bool inTravel = false;

    [Header("Travel Multipliers")]
    public int distanceMultiplier = 1;

    [Header("Weight System")]
    [Tooltip("Her kapasite aşım oranı için travel süresine eklenecek yüzde.")]
    public float overweightTravelPenaltyPerRatio = 0.25f;

    [Tooltip("Kapasite aşımı ciddi boyuttaysa ek exhaustion uygulanır.")]
    public float heavyOverweightThreshold = 1.5f;

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

    // -----------------------------
    // SAVE / LOAD
    // -----------------------------

    public void SaveTravelData()
    {
        travelData.inTravel = inTravel;

        if (inTravel)
        {
            if (MapAvatarHandler.Instance != null && MapAvatarHandler.Instance.playerIcon != null)
            {
                Vector2 playerPosition = MapAvatarHandler.Instance.GetPlayerPosition();
                travelData.playerPosition = new int[2] { (int)playerPosition.x, (int)playerPosition.y };
            }

            currentSettlement = MapHandler.Instance != null ? MapHandler.Instance.GetLastVisitedSettlement() : null;
            destination = MapHandler.Instance != null ? MapHandler.Instance.GetDestinationSettlement() : null;
            currentEvent = EventHandler.Instance != null ? EventHandler.Instance.currentEvent : null;

            travelData.currentSettlementID = currentSettlement != null ? currentSettlement.settlement.ID : 0;
            travelData.destinationID = destination != null ? destination.settlement.ID : 0;

            if (isEventActive)
            {
                travelData.isEventActive = isEventActive;

                if (currentEvent != null && currentEvent.ID != 0)
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
        }

        JSONDataHandler jsonDataHandler = new JSONDataHandler(PlayerPrefs.GetInt("Slot"));
        jsonDataHandler.SaveData(travelData, "travel.json");
    }

    public void LoadTravelData()
    {
        JSONDataHandler jsonDataHandler = new JSONDataHandler(PlayerPrefs.GetInt("Slot"));
        TravelWrapper wrapper = jsonDataHandler.LoadData<TravelWrapper>("travel.json");

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

            if (travelData.playerPosition != null && travelData.playerPosition.Length == 2)
            {
                Vector2 playerPosition = new Vector2(travelData.playerPosition[0], travelData.playerPosition[1]);
                if (MapAvatarHandler.Instance != null)
                {
                    MapAvatarHandler.Instance.CreatePlayerIcon();
                    MapAvatarHandler.Instance.UpdatePlayerPosition(playerPosition);
                    MapAvatarHandler.Instance.currentPosition = MapAvatarHandler.Instance.playerIcon.transform;
                    if (MapHandler.Instance != null)
                    {
                        MapAvatarHandler.Instance.startPosition = MapHandler.Instance.GetLastVisitedSettlement().transform;
                    }
                }
            }

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
        JSONDataHandler jsonDataHandler = new JSONDataHandler("SourceData");
        TravelWrapper wrapper = jsonDataHandler.LoadData<TravelWrapper>("travel.json");
        travelData = wrapper != null ? wrapper : new TravelWrapper();
    }

    // -----------------------------
    // HELPERS
    // -----------------------------

    public SettlementButtonPointer GetSettlementButtonPointerByID(int id)
    {
        if (MapHandler.Instance == null) return null;

        foreach (GameObject child in MapHandler.Instance.children)
        {
            SettlementButtonPointer settlementButtonPointer = child.GetComponent<SettlementButtonPointer>();

            if (settlementButtonPointer != null && settlementButtonPointer.settlement.ID == id)
            {
                return settlementButtonPointer;
            }
        }

        return null;
    }

    public Event_SO_Constructor GetEventByID(int id)
    {
        if (EventHandler.Instance == null) return null;

        foreach (Event_SO_Constructor e in EventHandler.Instance.events)
        {
            if (e.ID == id)
            {
                return e;
            }
        }

        return null;
    }

    private float GetWeightPenaltyMultiplier()
    {
        if (PlayerStatHandler.Instance == null)
            return 1f;

        float ratio = PlayerStatHandler.Instance.GetWeightRatio();

        if (ratio <= 1f)
            return 1f;

        float excessRatio = ratio - 1f;
        return 1f + (excessRatio * overweightTravelPenaltyPerRatio);
    }

    private bool IsHeavilyOverweight()
    {
        if (PlayerStatHandler.Instance == null)
            return false;

        return PlayerStatHandler.Instance.GetWeightRatio() >= heavyOverweightThreshold;
    }

    // -----------------------------
    // TRAVEL CORE
    // -----------------------------

    public void TravelToSettlement(Vector2 _currentPos = new Vector2())
    {
        remainingTime = 0;

        int minutes = CalculateDistance(_currentPos);

        int hours = minutes / 60;
        minutes = minutes % 60;

        remainingTime = hours;
        remainingTimeMinutes = minutes;

        int days = hours / 24;
        hours = hours % 24;

        Debug.Log("Started Travel");

        if (!inTravel && SettlementHandler.Instance != null)
            SettlementHandler.Instance.OnSettlementExited();

        ContinueTravel();
    }

    public int CalculateDistance(Vector2 _currentPos)
    {
        if (_currentPos == Vector2.zero)
        {
            _currentPos = currentSettlement.transform.localPosition;
        }
        else
        {
            _currentPos = MapAvatarHandler.Instance.GetPlayerPosition();
            MapAvatarHandler.Instance.currentPosition = MapAvatarHandler.Instance.playerIcon.transform;
        }

        Vector2 destinationPos = destination.transform.localPosition;

        float distance = Vector2.Distance(_currentPos, destinationPos) * distanceMultiplier;
        int minutes = (int)distance;
        int hours = minutes / 60;
        int days = hours / 24;

        if (isHuntingForRations)
        {
            distance += 60 * days;
        }

        if (isSleeping)
        {
            distance += 360 * days;

            if (PlayerStatHandler.Instance != null && PlayerStatHandler.Instance.GetExhaustionLevel() > 0)
            {
                distance += 120;
            }
        }

        // Weight penalty
        distance *= GetWeightPenaltyMultiplier();

        return Mathf.Max(1, Mathf.RoundToInt(distance));
    }

    public void ContinueTravel()
    {
        StopAllCoroutines();

        MapAvatarHandler.Instance.CreatePlayerIcon();
        MapAvatarHandler.Instance.endPosition = destination.transform;

        if (!inTravel)
        {
            MapAvatarHandler.Instance.startPosition = currentSettlement.transform;
            MapAvatarHandler.Instance.currentPosition = currentSettlement.transform;
            MapAvatarHandler.Instance.MovePlayerIconToLastVisitedSettlement();
        }
        else
        {
            MapAvatarHandler.Instance.startPosition = MapAvatarHandler.Instance.currentPosition;
        }

        inTravel = true;

        if (currentEvent != null)
        {
            if (currentEvent.ID != 0)
            {
                HandleEvent();
            }
            else
            {
                StartCoroutine(TravelUntilEvent());
            }
        }
        else
        {
            StartCoroutine(TravelUntilEvent());
        }
    }

    private int elapsedTravelTime = 0;
    public int eventIndex = 0;

    public IEnumerator TravelUntilEvent()
    {
        int totalTravelTime = remainingTime + elapsedTravelTime;

        if (eventTimes == null || eventTimes.Count == 0)
        {
            int minEvents = this.minEvents;
            int maxEvents = Mathf.Max(1, totalTravelTime / 20);
            int numberOfEvents = Random.Range(minEvents, maxEvents + 1);

            eventTimes = new List<int>();

            for (int i = 0; i < numberOfEvents; i++)
            {
                int eventTime = Random.Range(3, totalTravelTime - 3);
                eventTimes.Add(eventTime);
            }

            eventTimes.Sort();
            travelData.eventTimes = eventTimes;
        }

        Debug.Log($"Started Travel in {totalTravelTime}");

        List<int> eventT = new List<int>(eventTimes);
        for (int i = 0; i < eventT.Count; i++)
        {
            if (eventT[i] <= elapsedTravelTime)
            {
                eventT.RemoveAt(i);
                i--;
            }
        }

        MapAvatarHandler.Instance.SetSegments(eventT, eventIndex);

        while (remainingTime > 0)
        {
            int timeUntilNextEvent = remainingTime;

            if (eventIndex < eventTimes.Count)
            {
                timeUntilNextEvent = eventTimes[eventIndex] - elapsedTravelTime;
                timeUntilNextEvent = Mathf.Max(1, timeUntilNextEvent);
            }

            int travelSegment = Mathf.Min(timeUntilNextEvent, remainingTime);

            yield return StartCoroutine(TimeSystem.Instance.AdvanceTimeCoroutine(0, travelSegment, 0));

            remainingTime -= travelSegment;
            elapsedTravelTime += travelSegment;

            if (eventIndex < eventTimes.Count && elapsedTravelTime >= eventTimes[eventIndex])
            {
                HandleEvent();
                eventIndex++;
                isEventActive = true;

                Debug.Log("Event at " + elapsedTravelTime + " hours");

                yield return new WaitUntil(() => !isEventActive);
                isEventActive = false;

                if (currentEvent != null)
                {
                    currentEvent.ID = 0;
                }

                if (MapAvatarHandler.Instance.segments.Count > 0)
                {
                    MapAvatarHandler.Instance.segments.RemoveAt(0);
                }
            }
        }

        if (remainingTime <= 0)
        {
            TravelDone();
        }
    }

    public void TravelDone()
    {
        EnterSettlement();
        Debug.Log("Travel Done");
        inTravel = false;
        travelData.inTravel = false;
        ResetTravelData();
    }

    public void EnterSettlement()
    {
        currentSettlement = destination;

        if (MapHandler.Instance != null)
            MapHandler.Instance.map.transform.parent.gameObject.SetActive(false);

        if (MapHandler.Instance != null)
            MapHandler.Instance.MovePlayerToLastVisitedSettlement(currentSettlement.settlement);

        TimeSystem.Instance.AdvanceTimeCoroutine(0, 0, remainingTimeMinutes);

        if (GameManager.Instance != null)
            GameManager.Instance.ShowSettlementPanel();

        if (UIHandler.Instance != null)
            UIHandler.Instance.UpdateSettlementInfo(currentSettlement.settlement);
    }

    public void HandleEvent()
    {
        if (TravelingPanel != null)
            TravelingPanel.transform.GetChild(0).gameObject.SetActive(true);

        Debug.Log("Handle Event");
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

    public void CancelTravelAndReturn(int settlementID)
    {
        destination = GetSettlementButtonPointerByID(settlementID);

        StopAllCoroutines();
        remainingTime = elapsedTravelTime;
        elapsedTravelTime = 0;
        eventTimes.Clear();
        eventIndex = 0;
        TravelToSettlement();
    }

    public void ResetTravelData()
    {
        destination = null;

        if (currentEvent != null)
        {
            currentEvent.ID = 0;
        }

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
        int minutes = CalculateDistance(MapAvatarHandler.Instance.GetPlayerPosition());
        int hours = minutes / 60;
        minutes = minutes % 60;
        int days = hours / 24;
        hours = hours % 24;

        string weightInfo = "";
        if (PlayerStatHandler.Instance != null && PlayerStatHandler.Instance.IsOverweight())
        {
            weightInfo = "\nHeavy load is slowing you down.";
        }

        travelTimeText.text = $"Travel time: {days} days, {hours} hours, {minutes} minutes{weightInfo}";
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
        ResetTravelData();
        destination = MapHandler.Instance.selectedSettlement.GetComponent<SettlementButtonPointer>();

        Debug.Log("Accepted travel");

        if (PlayerStatHandler.Instance != null && PlayerStatHandler.Instance.IsOverweight())
        {
            Debug.Log("Player is overweight. Travel will be slower.");
        }

        if (!inTravel)
        {
            TravelToSettlement();
        }
        else
        {
            StopAllCoroutines();
            TravelToSettlement(MapAvatarHandler.Instance.GetPlayerPosition());
        }
    }

    public void PlayerDeclinedToTravel()
    {
        if (!inTravel)
        {
            TravelingDeciderPanel.SetActive(false);

            if (MapHandler.Instance != null)
                MapHandler.Instance.map.transform.parent.gameObject.SetActive(false);

            if (GameManager.Instance != null)
                GameManager.Instance.ShowSettlementPanel();

            if (UIHandler.Instance != null && currentSettlement != null)
                UIHandler.Instance.UpdateSettlementInfo(currentSettlement.settlement);

            destination = null;
        }
        else
        {
            StopAllCoroutines();
            TravelingDeciderPanel.SetActive(false);
            ResetTravelData();
            destination = GetSettlementButtonPointerByID(currentSettlement.settlement.ID);
            TravelToSettlement(MapAvatarHandler.Instance.GetPlayerPosition());
        }
    }

    public void PlayerMiniGameClosed()
    {
        PlayerWantsToHandleEventorEnterSettlement = true;

        if (TravelingPanel != null)
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
    public int[] playerPosition = new int[2];
}