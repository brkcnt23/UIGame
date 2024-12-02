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

    List<GameObject> children = new List<GameObject>();

    public bool isHunting;

    public void MovePlayerToLastVisitedSettlement()
    {
        settlements = SettlementHandler.Instance.settlements;
        
        Settlement lastVisitedSettlement = PlayerStatHandler.Instance.LastVisitedSettlement();

        PopulateMap();

        foreach (GameObject child in children)
        {
            SettlementButtonPointer settlementButtonPointer = child.GetComponent<SettlementButtonPointer>();

            if (settlementButtonPointer.settlement == lastVisitedSettlement)
            {
                TravelSystem.Instance.currentSettlement = settlementButtonPointer;
                TravelSystem.Instance.SetSettlements(settlementButtonPointer);

                SettlementHandler.Instance.settlement = lastVisitedSettlement;

                selectedSettlement = child;
            }
        }
    }

    public void PopulateMap()
    {
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
    }
}
