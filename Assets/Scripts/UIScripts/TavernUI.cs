using UnityEngine;
using UnityEngine.UI;

public class TavernUI : MonoBehaviour
{
    public Button SleepButton;
    public Button EatButton;

    [Header("Tavern Costs")]
    public int sleepGoldCost = 2;
    public int sleepSilverCost = 0;

    public int eatGoldCost = 1;
    public int eatSilverCost = 0;

    private void Start()
    {
        if (SleepButton != null)
        {
            SleepButton.onClick.RemoveAllListeners();
            SleepButton.onClick.AddListener(SleepInTavern);
        }

        if (EatButton != null)
        {
            EatButton.onClick.RemoveAllListeners();
            EatButton.onClick.AddListener(EatInTavern);
        }
    }

    public void SleepInTavern()
    {
        if (PlayerStatHandler.Instance == null || TimeSystem.Instance == null)
        {
            Debug.LogError("TavernUI: Required systems are null.");
            return;
        }

        bool paid = PlayerStatHandler.Instance.ConsumeMoney(sleepGoldCost, sleepSilverCost);
        if (!paid)
        {
            Debug.Log("Not enough money to sleep in tavern.");
            return;
        }

        Debug.Log($"You eat and sleep for {sleepGoldCost} gold {sleepSilverCost} silver.");
        TimeSystem.Instance.SleepTavern();

        RefreshUI();
    }

    public void EatInTavern()
    {
        if (PlayerStatHandler.Instance == null || TimeSystem.Instance == null)
        {
            Debug.LogError("TavernUI: Required systems are null.");
            return;
        }

        bool paid = PlayerStatHandler.Instance.ConsumeMoney(eatGoldCost, eatSilverCost);
        if (!paid)
        {
            Debug.Log("Not enough money to eat in tavern.");
            return;
        }

        Debug.Log($"You eat for {eatGoldCost} gold {eatSilverCost} silver.");
        TimeSystem.Instance.UpdateLastMealTime();

        RefreshUI();
    }

    private void RefreshUI()
    {
        if (PlayerUISystem.Instance != null)
        {
            PlayerUISystem.Instance.UpdateUIObjects();
        }

        if (InventoryUI.Instance != null)
        {
            InventoryUI.Instance.UpdateInventoryUI();
        }
    }
}