using UnityEngine;
using NEXUS.Utilities;
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
    }

    public SettlementButtonPointer currentSettlement;
    public SettlementButtonPointer destination;

    public int distanceMultiplier = 1;
    public int huntingMultiplier = 1;
    public int remainingTime;

    public int eventCooldown = 2;
    int evenCooldowReset;
    public int eventChance = 10;
    public int eventIncrease = 1;

    public GameObject eventPanel;

    public bool isEventActive = false;

    Button[] eventButtons;

    void Start()
    {
        evenCooldowReset = eventCooldown;
    }

    public void SetSettlements(SettlementButtonPointer destination)
    {
        this.destination = destination;
    }

    public void TravelToSettlement(bool isHunting)
    {

        SettlementHandler.Instance.OnSettlementExited();

        remainingTime = 0;
        int minutes = CalculateDistance(isHunting);

        print("Travel time is " + minutes + " minutes");

        int hours = minutes / 60;
        minutes = minutes % 60;
        remainingTime = hours;

        print("Travel time is " + hours + " hours and " + minutes + " minutes");

        int days = hours / 24;
        hours = hours % 24;

        print("Travel time is " + days + " days and " + hours + " hours and " + minutes + " minutes");

        ContinueTravel(remainingTime);
    }

    public int CalculateDistance(bool isHunting)
    {
        Vector2 currentPos = currentSettlement.transform.position;
        Vector2 destinationPos = destination.transform.position;

        float distance = Vector2.Distance(currentPos, destinationPos) * (distanceMultiplier + (isHunting ? huntingMultiplier : 0));

        return (int)distance;
    }

    public void ContinueTravel(int remainingTime)
    {
        TravelUntilEvent(remainingTime);
    }

    public void TravelUntilEvent(int travelTime)
    {
        for (int i = 0; i < travelTime; i++)
        {
            if (i > eventCooldown)
            {
                if (RandomEventHappened())
                {
                    print("Encountered an event after " + i + " hours of travel");
                    eventCooldown += eventIncrease;
                    HandleEvent();
                    break;
                }
            }

            remainingTime--;

            if (remainingTime <= 0)
            {
                remainingTime = 0;
                eventCooldown = evenCooldowReset;
                print("Arrived at destination");
                TravelDone();
            }
        }
    }

    public void TravelDone()
    {
        SettlementHandler.Instance.OnSettlmentEntered(destination.settlement);
    }

    public void HandleEvent()
    {
        isEventActive = true;
        ShowEventPanel();
    }

    public bool RandomEventHappened()
    {
        int dice = Dice.RollD100();

        if (dice < eventChance)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void ShowEventPanel()
    {
        eventPanel.SetActive(true);

        eventButtons = eventPanel.GetComponentsInChildren<Button>();

        Event_SO_Constructor currentEvent = EventHandler.Instance.events[EventHandler.Instance.GenerateEvent()];


        for (int i = 0; i < eventButtons.Length; i++)
        {
            eventButtons[i].onClick.RemoveAllListeners();
            eventButtons[i].GetComponentInChildren<TMP_Text>().text = currentEvent.choices[i];
            int choice = i;
            eventButtons[i].onClick.AddListener(() => {
                EventHandler.Instance.HandleEvent(PlayerStatHandler.Instance, choice);
                HideEventPanel();
                });
        }
    }

    public void HideEventPanel()
    {
        eventPanel.SetActive(false);
    }
}
