using TMPro;
using Unity.UI;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

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

    public TMP_Text ClockText;
    public TMP_Text ExhaustText;
    public TMP_Text RationPackText;
    public TMP_Text ActionLogText;
    private void Start()
    {
        timeSystem = TimeSystem.Instance;
        UpdateClockText();
        UpdateExhaustionText();
        UpdateRationText();
    }
    public void UpdateRationText()
    {
        RationPackText.text = $"Ration Packs: {PlayerStatHandler.Instance.GetRations()}";
    }
    public void UpdateExhaustionText()
    {
        ExhaustText.text = $"Exhaustion Level: {PlayerStatHandler.Instance.GetExhaustionLevel()}";
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

        ClockText.text = timeSystem.GetTimeString();
    }
    public void UpdateActionLog(string ActionLog)
    {
        ActionLogText.text = $"{ActionLog}";
    }
}