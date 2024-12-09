using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EventPanel : MonoBehaviour
{
    public GameObject InfoHolder;
    public GameObject ButtonHolder;

    public GameObject ButtonPrefab;

    public GameObject OutcomeHolder;

    void OnEnable()
    {
        InfoHolder = transform.GetChild(1).gameObject;
        ButtonHolder = transform.GetChild(2).gameObject;
        OutcomeHolder = transform.GetChild(3).gameObject;
    }

    public void ShowEvent(Event_SO_Constructor _event, int remainingTime)
    {
        InfoHolder.gameObject.SetActive(true);
        ButtonHolder.gameObject.SetActive(true);
        OutcomeHolder.gameObject.SetActive(false);
        InfoHolder.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = _event.Name;
        InfoHolder.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = _event.Description;

        foreach (Transform child in ButtonHolder.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (Choice c in _event.choices)
        {
            Button b = Instantiate(ButtonPrefab, ButtonHolder.transform).GetComponent<Button>();

            b.onClick.AddListener(() =>
            {
                TimeSystem.Instance.AdvanceTime(_event.CompletionDay, _event.CompletionHour, _event.CompletionMinute);
                EventHandler.Instance.HandleEvent(c);
                if (c.choiceType == "FloowUp")
                {
                    _event.choices.Remove(c);
                }

                InfoHolder.SetActive(false);
                ButtonHolder.SetActive(false);

                OutcomeHolder.SetActive(true);
                OutcomeHolder.GetComponentInChildren<TextMeshProUGUI>().text = c.outcome;

                OutcomeHolder.GetComponentInChildren<Button>().onClick.AddListener(() =>
                {
                    OutcomeHolder.SetActive(false);
                    gameObject.SetActive(false);

                    TravelSystem.Instance.isEventActive = false;
                    TravelSystem.Instance.PlayerWantsToHandleEventorEnterSettlement = true;

                    TravelSystem.Instance.TravelingPanel.SetActive(true);
                });
            });

            b.GetComponentInChildren<TextMeshProUGUI>().text = c.choiceText;
        }

        Button[] buttons = ButtonHolder.GetComponentsInChildren<Button>();
        //randomize button order
        for (int i = 0; i < buttons.Length; i++)
        {
            int rnd = Random.Range(0, buttons.Length);
            Button temp = buttons[rnd];
            buttons[rnd] = buttons[i];
            buttons[i] = temp;
        }
    }

}
