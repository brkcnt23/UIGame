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

    public GameObject QuestSettlementPrefab;

    public List<GameObject> children = new List<GameObject>();

    public Settlement lastVisitedSettlement;
    public Settlement destinationSettlement;
    public GameObject playerIcon;

    public void MovePlayerToLastVisitedSettlement(Settlement _settlement)
    {
        PopulateMap();

        foreach (GameObject child in children)
        {
            SettlementButtonPointer settlementButtonPointer = child.GetComponent<SettlementButtonPointer>();

            if(settlementButtonPointer == null)
            {
                continue;
            }

            if (settlementButtonPointer.settlement == _settlement)
            {
                TravelSystem.Instance.currentSettlement = settlementButtonPointer;

                SettlementHandler.Instance.settlement = _settlement;

                if (!TravelSystem.Instance.travelData.inTravel)
                    SettlementHandler.Instance.OnSettlementEntered(_settlement);
                else
                    OnOpenField();
                selectedSettlement = child;

                UpdatePlayerPosition(child.transform.position);
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

            if (settlementButtonPointer.settlement.Type == SettlementType.Quest)
            {
                continue;
            }
            settlementButtonPointer.SetSettlement(settlement);

            if (settlement.isUnlocked)
            {
                settlementButtonPointer.GetComponent<Image>().color = Color.green;
            }
            else
            {
                settlementButtonPointer.GetComponent<Image>().color = Color.gray;
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
    public void UpdatePlayerPosition(Vector2 position)
    {
        if (playerIcon != null)
        {
            playerIcon.transform.position = position;
        }
    }

    public SettlementButtonPointer GetLastVisitedSettlement()
    {
        foreach (GameObject child in children)
        {
            SettlementButtonPointer settlementButtonPointer = child.GetComponent<SettlementButtonPointer>();

            if(settlementButtonPointer == null)
            {
                continue;
            }

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

    public void LoadQuestSettlements()
    {
        foreach (Quest_SO_Constructor quest in PlayerStatHandler.Instance.pd.Quests)
        {
            if (quest.questType == QuestType.Location)
            {
                Settlement questsSettlement = new Settlement(quest);
                questsSettlement.ID = quest.settlementID;
                AddQuestSettlement(questsSettlement, ref quest.questLocationCoordinates);
            }
        }
    }

    public void AddQuestSettlement(Settlement _settlement, ref float[] _coordinates)
    {
        SettlementButtonPointer settlementButtonPointer = Instantiate(QuestSettlementPrefab, map.transform).GetComponent<SettlementButtonPointer>();

        Vector2 position;
        if (_coordinates != null && _coordinates[0] != 0 && _coordinates[1] != 0)
        {
            position = new Vector2(_coordinates[0], _coordinates[1]);
        }
        else
        {
            List<Vector2> positions = new List<Vector2>();
            foreach (Transform child in map.transform)
            {
                positions.Add(child.position);
            }

            float mapMinX = -1400f;
            float mapMaxX = 1400f;
            float mapMinY = -1400f;
            float mapMaxY = 1400f;
            float edgeMargin = 100f;
            float minDistance = 100f;

            float minX = mapMinX + edgeMargin;
            float maxX = mapMaxX - edgeMargin;
            float minY = mapMinY + edgeMargin;
            float maxY = mapMaxY - edgeMargin;

            do
            {
                position = new Vector2(Random.Range(minX, maxX), Random.Range(minY, maxY));
            } while (positions.Exists(p => Vector2.Distance(position, p) < minDistance));
            _coordinates = new float[2];
            _coordinates[0] = (int)position.x;
            _coordinates[1] = (int)position.y;
        }

        settlementButtonPointer.transform.position = position;

        settlementButtonPointer.settlement = _settlement;

        SettlementHandler.Instance.settlements.Add(_settlement);


        settlementButtonPointer.SetQuestSettlement(_settlement);

        PopulateMap();
    }

    public void RemoveQuestSettlement(SettlementButtonPointer _settlement)
    {
        SettlementHandler.Instance.settlements.Remove(_settlement.settlement);
        Destroy(_settlement.gameObject);
        PopulateMap();
    }
}
