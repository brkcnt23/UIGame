using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class SettlementButtonPointer : MonoBehaviour
{
    public Settlement settlement;

    private TMP_Text SettlementName;

    public Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        SettlementName = GetComponentInChildren<TMP_Text>();
        //send the settlement to the map handler and repopulate the map

        if (settlement != null)
        {
            button.onClick.AddListener(() =>
                {
                    MapHandler.Instance.selectedSettlement = gameObject;
                    MapHandler.Instance.PopulateMap();

                    if (SettlementHandler.Instance.settlement != settlement)
                    {
                        TravelSystem.Instance.TravelingDeciderPanel.SetActive(true);
                        
                        TravelSystem.Instance.travelInfoText.text = "Do you want to travel to " + settlement.Name + "?";

                        TravelSystem.Instance.destination = this;

                        TravelSystem.Instance.UpdateTravelTimeText();

                    }
                    else
                    {
                        Debug.Log("You are already in this settlement");
                        TravelSystem.Instance.PlayerDeclinedToTravel();
                    }
                });
        }
    }

    public void SetSettlement(Settlement settlement)
    {
        this.settlement = settlement;
        button = GetComponent<Button>();
        SettlementName = GetComponentInChildren<TMP_Text>();
        SettlementName.text = settlement.Name;

        if (settlement.isUnlocked)
        {
            button.interactable = true;
        }
        else
        {
            button.interactable = false;
        }
    }

    public void OpenTheTravelDecider(SettlementButtonPointer settlementButtonPointer)
    {
    }
}