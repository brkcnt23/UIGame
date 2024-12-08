using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapHandler : MonoBehaviour
{
    public static MapHandler Instance { get; private set; }

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

    public GameObject map;
    public List<Settlement> settlements = new List<Settlement>();

    public GameObject selectedSettlement;

    public List<GameObject> children = new List<GameObject>();

    public Settlement lastVisitedSettlement;
    public Settlement destinationSettlement;

    public void MovePlayerToLastVisitedSettlement(Settlement _settlement)
    {
        PopulateMap();

        foreach (GameObject child in children)
        {
            SettlementButtonPointer settlementButtonPointer = child.GetComponent<SettlementButtonPointer>();

            if (settlementButtonPointer.settlement == _settlement)
            {
                TravelSystem.Instance.currentSettlement = settlementButtonPointer;

                SettlementHandler.Instance.settlement = _settlement;

                if (!TravelSystem.Instance.travelData.inTravel)
                    SettlementHandler.Instance.OnSettlementEntered(_settlement);
                else
                    OnOpenField();
                selectedSettlement = child;
            }
        }

        PopulateMap();
    }

    public void PopulateMap()
    {
        settlements = SettlementHandler.Instance.settlements;

        children.Clear();

        foreach (Transform child in map.transform)
        {
            children.Add(child.gameObject);
        }

        foreach (Settlement settlement in settlements)
        {
            int index = settlements.IndexOf(settlement);
            SettlementButtonPointer settlementButtonPointer = children[index].GetComponent<SettlementButtonPointer>();

            settlementButtonPointer.SetSettlement(settlement);

            if (settlement.isUnlocked)
            {
                settlementButtonPointer.GetComponent<Image>().color = Color.green;
            }
            else
            {
                settlementButtonPointer.GetComponent<Image>().color = Color.red;
            }

            if (settlement == SettlementHandler.Instance.settlement)
            {
                settlementButtonPointer.GetComponent<Image>().color = Color.blue;
            }
        }

        CheckPlayerLevelAndUnlockSettlements();
    }

    public void CheckPlayerLevelAndUnlockSettlements()
    {
        foreach (Settlement settlement in settlements)
        {
            if (PlayerStatHandler.Instance.pd.Level >= settlement.levelToUnlock)
            {
                settlement.isUnlocked = true;
            }
        }
    }

    public SettlementButtonPointer GetLastVisitedSettlement()
    {
        foreach (GameObject child in children)
        {
            SettlementButtonPointer settlementButtonPointer = child.GetComponent<SettlementButtonPointer>();

            if (settlementButtonPointer.settlement == PlayerStatHandler.Instance.LastVisitedSettlement())
            {
                return settlementButtonPointer;
            }
        }

        return null;
    }

    public SettlementButtonPointer GetDestinationSettlement()
    {
        foreach (GameObject child in children)
        {
            SettlementButtonPointer settlementButtonPointer = child.GetComponent<SettlementButtonPointer>();

            if (settlementButtonPointer == TravelSystem.Instance.destination)
            {
                return settlementButtonPointer;
            }
        }

        return null;
    }

    public void OnOpenField()
    {
        lastVisitedSettlement = PlayerStatHandler.Instance.LastVisitedSettlement();
        destinationSettlement = TravelSystem.Instance.destination.settlement;
        UIHandler.Instance.UpdateSettlementInfo(lastVisitedSettlement);
    }
}
