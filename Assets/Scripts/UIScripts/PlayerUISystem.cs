using TMPro;
using UnityEngine;

public class PlayerUISystem : MonoBehaviour
{
    public static PlayerUISystem Instance { get; private set; }

    private TimeSystem timeSystem;

    public TMP_Text HealthText;
    public TMP_Text currencyText;
    public TMP_Text ClockText;
    public TMP_Text DayText;
    public TMP_Text ExhaustText;
    public TMP_Text RationPackText;
    public TMP_Text ActionLogText;

    // İstersen sonra inspector'dan bağlarız
    public TMP_Text WeightText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        timeSystem = TimeSystem.Instance;

        if (TimeSystem.Instance != null)
        {
            TimeSystem.Instance.clockText = ClockText;
            TimeSystem.Instance.dayText = DayText;
        }

        UpdateUIObjects();
        UpdateClockText();
    }

    public void UpdateHealthText()
    {
        if (HealthText == null) return;
        if (PlayerStatHandler.Instance == null || PlayerStatHandler.Instance.pd == null) return;

        HealthText.text = $"{PlayerStatHandler.Instance.pd.Health}";
    }

    public void UpdateCurrencyUI()
    {
        if (currencyText == null) return;
        if (PlayerStatHandler.Instance == null || PlayerStatHandler.Instance.pd == null) return;

        Currency money = PlayerStatHandler.Instance.pd.GetMoney();
        currencyText.text = $"Gold: {money.Gold}, Silver: {money.Silver}";
    }

    public void UpdateRationText()
    {
        if (RationPackText == null) return;
        if (PlayerStatHandler.Instance == null) return;

        RationPackText.text = $"Ration: {PlayerStatHandler.Instance.GetRations()}";
    }

    public void UpdateExhaustionText()
    {
        if (ExhaustText == null) return;
        if (PlayerStatHandler.Instance == null) return;

        ExhaustText.text = $"Exhaustion: {PlayerStatHandler.Instance.GetExhaustionLevel()}";
    }

    public void UpdateWeightText()
    {
        if (WeightText == null) return;
        if (PlayerStatHandler.Instance == null || PlayerStatHandler.Instance.pd == null) return;

        float currentWeight = PlayerStatHandler.Instance.GetCurrentWeight();
        float capacity = PlayerStatHandler.Instance.GetCarryCapacity();

        WeightText.text = $"Load: {currentWeight:0.0} / {capacity:0.0}";
    }

    public void UpdateClockText()
    {
        if (timeSystem == null)
        {
            timeSystem = TimeSystem.Instance;
        }

        if (timeSystem == null)
        {
            Debug.LogWarning("PlayerUISystem.UpdateClockText: timeSystem is null.");
            return;
        }

        if (ClockText != null)
        {
            ClockText.text = $"{timeSystem.Hour:00}:{timeSystem.Minute:00}";
        }

        if (DayText != null)
        {
            DayText.text = $"Day: {timeSystem.Day}";
        }

        UpdateUIObjects();
    }

    public void UpdateActionLog(string actionLog)
    {
        if (ActionLogText == null) return;
        ActionLogText.text = actionLog;
    }

    public void UpdateUIObjects()
    {
        UpdateHealthText();
        UpdateCurrencyUI();
        UpdateExhaustionText();
        UpdateRationText();
        UpdateWeightText();
    }
}