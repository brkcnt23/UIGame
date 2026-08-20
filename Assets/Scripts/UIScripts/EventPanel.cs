using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Draws an event and its choices.
///
/// Guarantees the player can always leave. If every authored choice fails its
/// requirements, the panel adds a plain "Walk away" option, so a badly tuned
/// event can never trap the player in an unclickable screen.
/// </summary>
public class EventPanel : MonoBehaviour
{
    public GameObject InfoHolder;
    public GameObject ButtonHolder;
    public GameObject ButtonPrefab;
    public GameObject OutcomeHolder;

    [Header("Fallback")]
    [Tooltip("Shown only when no authored choice is available.")]
    [SerializeField] private string fallbackChoiceText = "Walk away.";

    [SerializeField] private string fallbackOutcomeText =
        "You weigh your chances, decide they are poor, and leave the matter alone.";

    private Event_SO_Constructor _currentEvent;

    private void OnEnable()
    {
        InfoHolder = transform.GetChild(1).gameObject;
        ButtonHolder = transform.GetChild(2).gameObject;
        OutcomeHolder = transform.GetChild(3).gameObject;
    }

    public void ShowEvent(Event_SO_Constructor _event, int remainingTime)
    {
        if (_event == null)
        {
            Debug.LogError("EventPanel: ShowEvent called with null event!");
            return;
        }

        if (InfoHolder == null || ButtonHolder == null || OutcomeHolder == null)
        {
            Debug.LogError("EventPanel: One or more holders are null!");
            return;
        }

        _currentEvent = _event;

        InfoHolder.SetActive(true);
        ButtonHolder.SetActive(true);
        OutcomeHolder.SetActive(false);

        TMP_Text titleText = InfoHolder.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        TMP_Text descText = InfoHolder.transform.GetChild(1).GetComponent<TextMeshProUGUI>();

        if (titleText != null) titleText.text = _event.Name;
        if (descText != null) descText.text = _event.Description;

        foreach (Transform child in ButtonHolder.transform)
            Destroy(child.gameObject);

        int availableCount = 0;

        foreach (Choice c in _event.choices)
        {
            Choice captured = c;
            bool meetsRequirements = captured.CheckRequirements(PlayerStatHandler.Instance);

            if (meetsRequirements)
                availableCount++;

            CreateChoiceButton(captured.choiceText, meetsRequirements,
                () => HandleChoiceSelected(captured, captured.outcome));
        }

        RandomizeButtonOrder();

        // Added after the shuffle so it always sits at the bottom.
        if (availableCount == 0)
        {
            Debug.LogWarning($"EventPanel: no choice in '{_event.Name}' passes its requirements. " +
                             "Adding a fallback so the player is not stuck. Check this event's tuning.");

            CreateChoiceButton(fallbackChoiceText, true,
                () => HandleChoiceSelected(null, fallbackOutcomeText));
        }
    }

    private void CreateChoiceButton(string label, bool interactable, UnityEngine.Events.UnityAction onClick)
    {
        Button b = Instantiate(ButtonPrefab, ButtonHolder.transform).GetComponent<Button>();
        TextMeshProUGUI buttonText = b.GetComponentInChildren<TextMeshProUGUI>();

        if (buttonText != null)
        {
            buttonText.text = label;
            if (!interactable)
                buttonText.color = Color.gray;
        }

        b.interactable = interactable;

        b.onClick.RemoveAllListeners();
        if (interactable)
            b.onClick.AddListener(onClick);
    }

    /// <summary>
    /// choice may be null — that is the fallback path. It costs no time and
    /// applies no rewards; it only closes the event.
    /// </summary>
    private void HandleChoiceSelected(Choice choice, string outcomeText)
    {
        if (choice != null)
        {
            if (TimeSystem.Instance != null)
            {
                TimeSystem.Instance.AdvanceTime(
                    _currentEvent.CompletionDay,
                    _currentEvent.CompletionHour,
                    _currentEvent.CompletionMinute);
            }

            if (EventHandler.Instance != null)
                EventHandler.Instance.HandleEvent(choice);
        }

        ShowOutcome(outcomeText);
    }

    private void ShowOutcome(string text)
    {
        InfoHolder.SetActive(false);
        ButtonHolder.SetActive(false);
        OutcomeHolder.SetActive(true);

        TMP_Text outcomeText = OutcomeHolder.GetComponentInChildren<TextMeshProUGUI>();
        if (outcomeText != null)
            outcomeText.text = text;

        Button outcomeButton = OutcomeHolder.GetComponentInChildren<Button>();
        if (outcomeButton == null)
        {
            Debug.LogError("EventPanel: OutcomeHolder is missing a Button child!");
            return;
        }

        outcomeButton.onClick.RemoveAllListeners();
        outcomeButton.onClick.AddListener(CloseEvent);
    }

    private void CloseEvent()
    {
        OutcomeHolder.SetActive(false);
        gameObject.SetActive(false);

        if (TravelSystem.Instance != null)
        {
            if (TravelSystem.Instance.travelData.isEventActive && TravelSystem.Instance.currentEvent != null)
            {
                TravelSystem.Instance.currentEvent.ID = 0;
                TravelSystem.Instance.ContinueTravel();
            }

            TravelSystem.Instance.isEventActive = false;
            TravelSystem.Instance.PlayerWantsToHandleEventorEnterSettlement = true;

            if (TravelSystem.Instance.TravelingPanel != null)
                TravelSystem.Instance.TravelingPanel.SetActive(true);
        }

        if (PlayerUISystem.Instance != null)
            PlayerUISystem.Instance.UpdateUIObjects();
    }

    /// <summary>
    /// Fisher-Yates. The previous version could swap an element with itself and
    /// produced a biased order; this one is uniform.
    /// </summary>
    private void RandomizeButtonOrder()
    {
        Button[] buttons = ButtonHolder.GetComponentsInChildren<Button>();

        for (int i = buttons.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            if (i == j) continue;

            int indexI = buttons[i].transform.GetSiblingIndex();
            int indexJ = buttons[j].transform.GetSiblingIndex();

            buttons[i].transform.SetSiblingIndex(indexJ);
            buttons[j].transform.SetSiblingIndex(indexI);
        }
    }
}
