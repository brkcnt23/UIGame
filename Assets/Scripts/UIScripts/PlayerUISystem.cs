using TMPro;
using UnityEngine;

public class PlayerUISystem : MonoBehaviour
{
    public static PlayerUISystem Instance { get; private set; }
    private TimeSystem timeSystem;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public TMP_Text HealthText;
    public TMP_Text currencyText;
    public TMP_Text ClockText;
    public TMP_Text DayText;
    public TMP_Text ExhaustText;
    public TMP_Text RationPackText;
    public TMP_Text ActionLogText;
    private void Start()
    {
        timeSystem = TimeSystem.Instance;
        TimeSystem.Instance.clockText = ClockText;
        TimeSystem.Instance.dayText = DayText;
        UpdateClockText();
        UpdateExhaustionText();
        UpdateRationText();

    }

    public void UpdateHealthText()
    {
        HealthText.text = $"{PlayerStatHandler.Instance.pd.Health}";
    }
    public void UpdateCurrencyUI()
    {
        PlayerData pd = PlayerStatHandler.Instance.pd;
        currencyText.text = $"Gold: {pd.Currency.Gold}, Silver: {pd.Currency.Silver}";
    }
    public void UpdateRationText()
    {
        RationPackText.text = $"Ration: {PlayerStatHandler.Instance.GetRations()}";
    }
    public void UpdateExhaustionText()
    {
        ExhaustText.text = $"Exhaustion: {PlayerStatHandler.Instance.GetExhaustionLevel()}";
    }
    public void UpdateClockText()
    {
        if (timeSystem == null)
        {
            Debug.LogWarning("UpdateClockText: timeSystem is null.");
            return;
        }

        if (ClockText == null)
        {
            Debug.LogWarning("UpdateClockText: ClockText is null.");
            return;
        }

        if (DayText == null)
        {
            Debug.LogWarning("UpdateClockText: DayText is null.");
            return;
        }

        if (timeSystem == null)
        {
            Debug.LogWarning("UpdateClockText: timeSystem is null.");
            return;
        }

        DayText.text = $"Day: {timeSystem.Day}";

        ClockText.text = $"{timeSystem.Hour:00}:{timeSystem.Minute:00}";

        UpdateUIObjects();
    }
    public void UpdateActionLog(string ActionLog)
    {
        ActionLogText.text = $"{ActionLog}";
    }

    public void UpdateUIObjects()
    {
        UpdateHealthText();
        UpdateCurrencyUI();
        UpdateExhaustionText();
        UpdateRationText();
    }
}