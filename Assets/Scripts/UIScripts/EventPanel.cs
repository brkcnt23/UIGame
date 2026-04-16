using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EventPanel : MonoBehaviour
{
    public GameObject InfoHolder;
    public GameObject ButtonHolder;
    public GameObject ButtonPrefab;
    public GameObject OutcomeHolder;

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

        InfoHolder.SetActive(true);
        ButtonHolder.SetActive(true);
        OutcomeHolder.SetActive(false);

        TMP_Text titleText = InfoHolder.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        TMP_Text descText = InfoHolder.transform.GetChild(1).GetComponent<TextMeshProUGUI>();

        if (titleText != null) titleText.text = _event.Name;
        if (descText != null) descText.text = _event.Description;

        foreach (Transform child in ButtonHolder.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (Choice c in _event.choices)
        {
            Button b = Instantiate(ButtonPrefab, ButtonHolder.transform).GetComponent<Button>();
            TextMeshProUGUI buttonText = b.GetComponentInChildren<TextMeshProUGUI>();

            if (buttonText != null)
                buttonText.text = c.choiceText;

            bool meetsRequirements = c.CheckRequirements(PlayerStatHandler.Instance);
            b.interactable = meetsRequirements;

            if (!meetsRequirements && buttonText != null)
            {
                buttonText.color = Color.gray;
            }

            b.onClick.RemoveAllListeners();
            b.onClick.AddListener(() =>
            {
                if (!meetsRequirements) return;

                if (TimeSystem.Instance != null)
                {
                    TimeSystem.Instance.AdvanceTime(_event.CompletionDay, _event.CompletionHour, _event.CompletionMinute);
                }

                if (EventHandler.Instance != null)
                {
                    EventHandler.Instance.HandleEvent(c);
                }

                InfoHolder.SetActive(false);
                ButtonHolder.SetActive(false);
                OutcomeHolder.SetActive(true);

                TMP_Text outcomeText = OutcomeHolder.GetComponentInChildren<TextMeshProUGUI>();
                if (outcomeText != null)
                    outcomeText.text = c.outcome;

                Button outcomeButton = OutcomeHolder.GetComponentInChildren<Button>();
                if (outcomeButton == null)
                {
                    Debug.LogError("EventPanel: OutcomeHolder is missing a Button child!");
                    return;
                }

                outcomeButton.onClick.RemoveAllListeners();
                outcomeButton.onClick.AddListener(() =>
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
                    {
                        PlayerUISystem.Instance.UpdateUIObjects();
                    }

                    if (InventoryUI.Instance != null)
                    {
                        InventoryUI.Instance.UpdateInventoryUI();
                    }
                });
            });
        }

        RandomizeButtonOrder();
    }

    private void RandomizeButtonOrder()
    {
        Button[] buttons = ButtonHolder.GetComponentsInChildren<Button>();

        for (int i = 0; i < buttons.Length; i++)
        {
            int rnd = Random.Range(0, buttons.Length);

            Transform currentParent = buttons[i].transform.parent;
            int currentIndex = buttons[i].transform.GetSiblingIndex();
            int rndIndex = buttons[rnd].transform.GetSiblingIndex();

            buttons[i].transform.SetSiblingIndex(rndIndex);
            buttons[rnd].transform.SetSiblingIndex(currentIndex);
        }
    }
}