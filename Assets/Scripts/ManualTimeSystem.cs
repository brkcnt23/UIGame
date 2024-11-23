using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ManualTimeSystem : MonoBehaviour
{
    public static ManualTimeSystem Instance { get; private set; }

    private int nextMealTime;
    private int hoursSinceLastSleep = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SetNextMealTime();
        UpdateClock();
    }

    public void AdvanceTime(int hours, int minutes)
    {
        PlayerStatHandler.Instance.pd.Minute += minutes;
        PlayerStatHandler.Instance.pd.Hour += hours + PlayerStatHandler.Instance.pd.Minute / 60;

        PlayerStatHandler.Instance.pd.Minute %= 60;
        PlayerStatHandler.Instance.pd.Hour %= 24;

        hoursSinceLastSleep += hours;

        if (hoursSinceLastSleep >= 24)
        {
            PlayerStatHandler.Instance.IncreaseExhaustion();
            hoursSinceLastSleep %= 24; // Reset to account for days
        }

        if (PlayerStatHandler.Instance.pd.Hour >= nextMealTime || nextMealTime == 0)
        {
            FoodSystem.Instance.ConsumeRationPack();
            SetNextMealTime();
        }

        UpdateClock();
    }

  public void Sleep()
{
    // Uyku sırasında rasyon kontrolü
    if (PlayerStatHandler.Instance.GetRations() > 0)
    {
        // Rasyon varsa, normal uyku süresi
        AdvanceTime(CalculateSleepTime(), 0);
        FoodSystem.Instance.ConsumeRationPack(); // Yemek tüketiliyor
        PlayerStatHandler.Instance.SetExhaustionLevel(0); // Yorgunluk sıfırlanıyor
        hoursSinceLastSleep = 0; // Uyandıktan sonra geçen süre sıfırlanıyor
        SetNextMealTime(); // Yeni yemek zamanı ayarlanıyor
    }
    else
    {
        // Rasyon yoksa, yorgunluk artışı ve ek saat
        AdvanceTime(CalculateSleepTime() + 2, 0); // 2 saat ekleniyor
        PlayerStatHandler.Instance.IncreaseExhaustion(); // Yorgunluk artışı
        Debug.LogWarning("No rations available! Gained exhaustion during sleep.");
    }

    UpdateClock(); // Saati güncelle
}

    private void SetNextMealTime()
    {
        nextMealTime = (PlayerStatHandler.Instance.pd.Hour + 14) % 24;
        Debug.Log("Next meal time set to: " + nextMealTime + ":00");
    }

    public void PerformAction(SO_Base action)
    {
        AdvanceTime(action.CompletionHour, action.CompletionMinute);
    }

    private void UpdateClock()
    {
        //PlayerUISystem.Instance.UpdateClockText();
    }

    public string GetCurrentTime()
    {
        return $"{PlayerStatHandler.Instance.pd.Hour:D2}:{PlayerStatHandler.Instance.pd.Minute:D2}";
    }

    public int[] GetCurrentTimes()
    {
        return new int[] { PlayerStatHandler.Instance.pd.Hour, PlayerStatHandler.Instance.pd.Minute };
    }

    public int GetExhaustionTime()
    {
        return PlayerStatHandler.Instance.GetExhaustionLevel() * 2;
    }

    public int CalculateSleepTime()
    {
        return 6 + GetExhaustionTime();
    }
}