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

    public void MovePlayerToLastVisitedSettlement(Settlement _settlement)
    {
        PopulateMap();

        if (_settlement == null)
        {
            Debug.LogWarning("MapHandler: There is no settlement to move the player to.");
            return;
        }

        for (int i = 0; i < children.Count; i++)
        {
            if (children[i] == null)
            {
                continue;
            }

            SettlementButtonPointer settlementButtonPointer = children[i].GetComponent<SettlementButtonPointer>();

            if (settlementButtonPointer == null)
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
                selectedSettlement = children[i];
            }
        }

        if (MapAvatarHandler.Instance != null)
            MapAvatarHandler.Instance.CreatePlayerIcon();

        PopulateMap();
    }

    public void PopulateMap()
    {
        if (map == null)
        {
            Debug.LogError("MapHandler: map is not assigned.");
            return;
        }

        settlements = SettlementHandler.Instance != null
            ? SettlementHandler.Instance.settlements
            : new List<Settlement>();

        if (settlements == null)
            settlements = new List<Settlement>();

        children.Clear();

        foreach (Transform child in map.transform)
        {
            children.Add(child.gameObject);
        }

        if (settlements.Count > children.Count)
        {
            Debug.LogWarning($"MapHandler: There are {settlements.Count} settlements but only {children.Count} map slots. " +
                             "The extra settlements will not be shown on the map.");
        }

        for (int index = 0; index < settlements.Count && index < children.Count; index++)
        {
            Settlement settlement = settlements[index];

            if (settlement == null || children[index] == null)
            {
                continue;
            }

            SettlementButtonPointer settlementButtonPointer = children[index].GetComponent<SettlementButtonPointer>();

            if (settlementButtonPointer == null)
            {
                continue;
            }

            if (settlementButtonPointer.settlement != null &&
                settlementButtonPointer.settlement.Type == SettlementType.Quest)
            {
                continue;
            }
            settlementButtonPointer.SetSettlement(settlement);

            Image settlementImage = settlementButtonPointer.GetComponent<Image>();

            if (settlementImage != null)
            {
                settlementImage.color = settlement.isUnlocked ? Color.white : Color.gray;
            }
        }

        CheckPlayerLevelAndUnlockSettlements();
    }

    public void CheckPlayerLevelAndUnlockSettlements()
    {
        if (settlements == null || PlayerStatHandler.Instance == null || PlayerStatHandler.Instance.pd == null)
        {
            return;
        }

        foreach (Settlement settlement in settlements)
        {
            if (settlement == null)
                continue;

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
            if (child == null)
            {
                continue;
            }

            SettlementButtonPointer settlementButtonPointer = child.GetComponent<SettlementButtonPointer>();

            if (settlementButtonPointer == null)
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
            if (child == null)
            {
                continue;
            }

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
        if (PlayerStatHandler.Instance == null || PlayerStatHandler.Instance.pd == null ||
            PlayerStatHandler.Instance.pd.Quests == null)
        {
            return;
        }

        foreach (Quest_SO_Constructor quest in PlayerStatHandler.Instance.pd.Quests)
        {
            if (quest == null)
                continue;

            if (quest.questType == QuestType.Location)
            {
                Settlement questsSettlement = new Settlement(quest);
                AddQuestSettlement(questsSettlement, ref quest.questLocationCoordinates);
            }
        }
    }

    public void AddQuestSettlement(Settlement _settlement, ref float[] _coordinates)
    {
        if (QuestSettlementPrefab == null || map == null || _settlement == null)
        {
            Debug.LogWarning("MapHandler: Cannot add quest settlement, prefab or map is missing.");
            return;
        }

        SettlementButtonPointer settlementButtonPointer = Instantiate(QuestSettlementPrefab, map.transform).GetComponent<SettlementButtonPointer>();

        if (settlementButtonPointer == null)
        {
            Debug.LogError("MapHandler: QuestSettlementPrefab has no SettlementButtonPointer component.");
            return;
        }

        Vector2 position;
        if (_coordinates != null && _coordinates.Length >= 2 && _coordinates[0] != 0 && _coordinates[1] != 0)
        {
            position = new Vector2(_coordinates[0], _coordinates[1]);
        }
        else
        {
            List<Vector2> positions = new List<Vector2>();
            foreach (Transform child in map.transform)
            {
                positions.Add(child.localPosition);
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

        settlementButtonPointer.transform.localPosition = position;

        settlementButtonPointer.settlement = _settlement;

        print("Quest Settlement added: " + _settlement.Name);
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
