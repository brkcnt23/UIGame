using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MapAvatarHandler : MonoBehaviour
{
    public static MapAvatarHandler Instance { get; private set; }

    [Header("Player Icon")]
    public GameObject playerIcon;
    public GameObject playerIconPrefab;
    public GameObject playerIconParent;

    [Header("Player Icon Positions")]
    public Transform currentPosition;
    public Transform startPosition;
    public Transform endPosition;
    public List<Vector2> segments = new List<Vector2>();


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

    public void CreatePlayerIcon()
    {
        if (playerIcon == null)
        {
            if (playerIconPrefab == null || playerIconParent == null)
            {
                Debug.LogError("[MapAvatarHandler] playerIconPrefab or playerIconParent is null!");
                return;
            }

            playerIcon = Instantiate(playerIconPrefab);
            playerIcon.transform.SetParent(playerIconParent.transform);
            MovePlayerIconToLastVisitedSettlement();
        }
    }

    public void MovePlayerIconToLastVisitedSettlement()
    {
        if (playerIcon == null)
        {
            Debug.LogError("[MapAvatarHandler] playerIcon is null!");
            return;
        }

        if (PlayerStatHandler.Instance == null)
        {
            Debug.LogError("[MapAvatarHandler] PlayerStatHandler.Instance is null!");
            return;
        }

        if (MapHandler.Instance == null)
        {
            Debug.LogError("[MapAvatarHandler] MapHandler.Instance is null!");
            return;
        }

        // Get settlement data (validates that a settlement exists, falls back to home)
        var settlement = PlayerStatHandler.Instance.LastVisitedSettlement();
        if (settlement == null)
        {
            Debug.LogError("[MapAvatarHandler] LastVisitedSettlement() returned null!");
            return;
        }

        // Find the SettlementButtonPointer UI element on map for this settlement
        var settlementButtonPointer = MapHandler.Instance.GetLastVisitedSettlement();
        if (settlementButtonPointer == null)
        {
            Debug.LogError("[MapAvatarHandler] SettlementButtonPointer not found on map for settlement: " + settlement.Name);
            return;
        }

        playerIcon.transform.localPosition = settlementButtonPointer.transform.localPosition;
    }

    public void DestroyPlayerIcon()
    {
        Destroy(playerIcon);
    }

    public Vector2 GetPlayerPosition()
    {
        return playerIcon.transform.localPosition;
    }

    public void UpdatePlayerPosition(Vector2 _position)
    {
        playerIcon.transform.localPosition = _position;
    }

    public void SetSegments(List<int> _segments, int index)
    {
        segments.Clear();

        Vector2 startPos = startPosition.localPosition;
        Vector2 endPos = endPosition.localPosition;
        Vector2 direction = endPos - startPos;
        if (index == 0)
        {
            segments.Add(startPos);
        }

        // Evenly distribute based on count of _segments
        for (int i = 0; i < _segments.Count; i++)
        {
            float fraction = (float)(i + 1) / (_segments.Count + 1);
            Vector2 segmentPos = startPos + direction * fraction;
            segments.Add(segmentPos);
        }

        segments.Add(endPos);
    }
    public IEnumerator MovePlayerIconToNextSegment(float _progress)
    {
        while (true)
        {
            if (segments.Count > 1)
            {
                playerIcon.transform.localPosition = Vector2.Lerp(segments[0], segments[1], _progress);
                yield return null;
            }
            else if (segments.Count == 1)
            {
                playerIcon.transform.localPosition = Vector2.Lerp(segments[0], endPosition.localPosition, _progress);
                yield return null;
            }
            else
            {
                yield break;
            }
        }
    }
}