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
                    MapHandler.Instance.selectedSettlement = gameObject;
                    MapHandler.Instance.settlementHandler.settlement = settlement;
                    MapHandler.Instance.PopulateMap();
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
}