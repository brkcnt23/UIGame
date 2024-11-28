using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapHandler : MonoBehaviour
{
    //we will handle the map here
    //we will reach settlements handler and get the settlements list and then change the map accordingly and keep track of the whic settlement is selected and player is in, and then reach settlemt handler and and change the settlement accordingly
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

    public SettlementHandler settlementHandler;

    List<GameObject> children = new List<GameObject>();

    public bool isHunting;


    void Start()
    {
        settlementHandler = SettlementHandler.Instance;
        settlements = settlementHandler.settlements;

        settlementHandler.settlement = settlements[0];

        TravelSystem.Instance.currentSettlement = map.GetComponentInChildren<SettlementButtonPointer>();

        PopulateMap();
    }

    public void PopulateMap()
    {

        //get all children of the map

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

            if (settlement == settlementHandler.settlement)
            {
                settlementButtonPointer.GetComponent<Image>().color = Color.blue;
            }
        }
    }

    public void SendHandlerToSelectedSettlement(Settlement settlement)
    {
        settlementHandler.settlement = settlement;
        PopulateMap();

        Debug.Log($"Selected settlement: {settlement.Name}");
    }
}
