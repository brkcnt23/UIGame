using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class SettlementButtonPointer : MonoBehaviour
{
    public Settlement settlement;

    private TMP_Text SettlementName;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        SettlementName = GetComponentInChildren<TMP_Text>();
        //send the settlement to the map handler and repopulate the map

        if (settlement != null)
        {
            button.onClick.AddListener(() =>
                {
                    if(SettlementHandler.Instance.settlement == settlement)
                    {
                        return;
                    }

                    MapHandler.Instance.selectedSettlement = gameObject;
                    MapHandler.Instance.PopulateMap();

                    MapHandler.Instance.settlementHandler.settlement = settlement;

                    SettlementHandler.Instance.OnSettlmentEntered(settlement);

                    TravelSystem.Instance.SetSettlements(this);
                    TravelSystem.Instance.TravelToSettlement(DecidedToHunt());

                    TravelSystem.Instance.currentSettlement = this;

                    MapHandler.Instance.isHunting = false;
                });
        }
    }

    public void SetSettlement(Settlement settlement)
    {
        this.settlement = settlement;

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

    public bool DecidedToHunt()
    {
        //pup up a window to ask if the player wants to hunt or not (Debug.Log for now)
        Debug.Log("Do you want to hunt?");
        return MapHandler.Instance.isHunting;
    }
}