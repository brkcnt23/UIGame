using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class JobLogger : MonoBehaviour
{
    public static JobLogger Instance { get; private set; }

    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Transform contentParent;
    [SerializeField] private int maxEntries = 50;
    [SerializeField] private float entryHeight = 120f;

    // Assigned in the Inspector but never read — the layout group handles
    // spacing now. Kept as a serialized value so existing scene data is not
    // lost, marked so the compiler stops warning about it.
    [SerializeField, HideInInspector] private float spacing = 20f;
    public float Spacing => spacing;

    private Queue<LogEntry> logEntries = new Queue<LogEntry>();
    private LogEntry logEntryPrefab;

    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(true);

        // Find ScrollRect
        if (scrollRect == null)
            scrollRect = GetComponentInChildren<ScrollRect>(true);

        // Find Content parent
        if (contentParent == null && scrollRect != null)
            contentParent = scrollRect.content;

        // Create prefab programmatically
        CreateLogEntryPrefab();

        Debug.Log("[JobLogger] Initialized with " + maxEntries + " max entries");
    }

    private void CreateLogEntryPrefab()
    {
        // Create a temporary prefab (won't be saved, just for instantiation)
        GameObject entryGO = new GameObject("LogEntry");
        entryGO.SetActive(false);

        // Add RectTransform
        RectTransform rectTransform = entryGO.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(800, entryHeight);

        // Add LayoutElement for proper vertical layout group sizing
        LayoutElement layoutElement = entryGO.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = entryHeight;
        layoutElement.preferredWidth = 800;

        // Add TextMeshProUGUI
        TextMeshProUGUI textComponent = entryGO.AddComponent<TextMeshProUGUI>();
        textComponent.alignment = TextAlignmentOptions.TopLeft;
        textComponent.fontSize = 24;
        textComponent.color = Color.white;
        textComponent.margin = new Vector4(10, 10, 10, 10); // padding

        // Add LogEntry component
        logEntryPrefab = entryGO.AddComponent<LogEntry>();
    }

    public void LogJobComplete(string jobName, int money, int xp, string statType, bool damageOccurred = false, int damageAmount = 0)
    {
        string header = $"<color=#FFD700>★ {jobName.ToUpper()}</color>";
        string rewards = $"Ödül: {money} Gümüş | {xp} {statType} XP";

        string logText = header + "\n" + rewards;

        if (damageOccurred)
        {
            logText += $"\n<color=#FF6B6B>⚠ Yaralandın! -{damageAmount} Can</color>";
        }

        // Add exhaustion warning to same entry if needed
        if (PlayerStatHandler.Instance != null && PlayerStatHandler.Instance.pd.CurrentExhaustionLevel >= 7)
        {
            logText += $"\n<color=#FFA500>⚡ Yorgunluk: {PlayerStatHandler.Instance.pd.CurrentExhaustionLevel}/{PlayerStatHandler.Instance.pd.MaxExhaustionLevel}</color>";
        }

        AddLogEntry(logText);
    }

    public void LogItemReward(string itemName, int quantity)
    {
        AddLogEntry($"<color=#90EE90>+ {itemName} x{quantity}</color>");
    }

    public void LogExhaustionWarning(int currentLevel, int maxLevel)
    {
        AddLogEntry($"<color=#FFA500>⚡ Yorgunluk: {currentLevel}/{maxLevel}</color>");
    }

    private void AddLogEntry(string text)
    {
        if (contentParent == null)
        {
            Debug.LogError("[JobLogger] Content parent null!");
            return;
        }

        if (logEntryPrefab == null)
        {
            Debug.LogError("[JobLogger] LogEntry prefab is null! Cannot add entry.");
            return;
        }

        // Ensure panel is active
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        // If we have max entries, remove the oldest (recycle it)
        if (logEntries.Count >= maxEntries)
        {
            LogEntry oldest = logEntries.Dequeue();
            if (oldest != null && oldest.gameObject != null)
            {
                oldest.SetText(text);
                logEntries.Enqueue(oldest);
                oldest.transform.SetAsLastSibling();
            }
            else
            {
                Debug.LogWarning("[JobLogger] Recycled entry was null, creating new instead");
                CreateNewLogEntry(text);
            }
        }
        else
        {
            CreateNewLogEntry(text);
        }

        // Scroll to bottom
        StartCoroutine(ScrollToBottomCoroutine());
    }

    private void CreateNewLogEntry(string text)
    {
        GameObject entryGO = Instantiate(logEntryPrefab.gameObject, contentParent);
        if (entryGO == null)
        {
            Debug.LogError("[JobLogger] Failed to instantiate log entry!");
            return;
        }

        entryGO.SetActive(true);
        entryGO.transform.SetAsLastSibling();

        LogEntry entry = entryGO.GetComponent<LogEntry>();
        if (entry == null)
        {
            Debug.LogError("[JobLogger] Instantiated entry missing LogEntry component!");
            Destroy(entryGO);
            return;
        }

        entry.SetText(text);
        logEntries.Enqueue(entry);
    }

    private IEnumerator ScrollToBottomCoroutine()
    {
        yield return null;
        yield return null;

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.GetComponent<RectTransform>());
        scrollRect.verticalNormalizedPosition = 0f;
    }

    public void ClearLogs()
    {
        foreach (var entry in logEntries)
        {
            Destroy(entry.gameObject);
        }
        logEntries.Clear();
    }
}

/// <summary>
/// Represents a single log entry in the list
/// </summary>
public class LogEntry : MonoBehaviour
{
    private TextMeshProUGUI textComponent;

    private void OnEnable()
    {
        if (textComponent == null)
            textComponent = GetComponent<TextMeshProUGUI>();
    }

    public void SetText(string text)
    {
        if (textComponent == null)
            textComponent = GetComponent<TextMeshProUGUI>();

        textComponent.text = text;
    }

    public string GetText()
    {
        if (textComponent == null)
            textComponent = GetComponent<TextMeshProUGUI>();

        return textComponent.text;
    }
}
