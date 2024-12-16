using System.Collections;
using UnityEngine;
using DG.Tweening;
using TMPro;
public class TimeSystem : MonoBehaviour
{
    public int Hour { get; private set; }    // Saat (0-23)
    public int Minute { get; private set; }  // Dakika (0-59)
    public int Day { get; private set; }     // Gün (1 ve üstü)

    public bool isTimeLapsing = false;
    private PlayerData playerData;

    // Singleton Instance
    public static TimeSystem Instance { get; private set; }

    [Header("UI")]
    [SerializeField] public TMP_Text clockText; // Reference to the clock text
    [SerializeField] public TMP_Text dayText; // Reference to the clock text

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
    public void InitializeLastActionTimes()
    {
        playerData = PlayerStatHandler.Instance.pd;
        Day = playerData.Day;
        Hour = playerData.Hour;
        Minute = playerData.Minute;

    }

    /// <summary>
    /// Zamanı ilerletir ve gerekli kontrolleri yapar.
    /// </summary>
    /// <param name="minutes">İlerletilecek dakika miktarı.</param>
    public void AdvanceTime(int minutes)
    {
        Minute += minutes;
        NormalizeTime();

        CheckExhaustion();

        PlayerUISystem.Instance.UpdateClockText();

    }

    public void AdvanceTime(int days, int hours, int minutes)
    {
        if (isTimeLapsing == true) return;
        int totalMinutes = days * 24 * 60 + hours * 60 + minutes;
        AdvanceTime(totalMinutes);
    }

    //we will advance time but with a IEnumerator so we can see the time passing
    public IEnumerator AdvanceTimeCoroutine(int days, int hours, int minutes)
    {
        isTimeLapsing = true;
        int totalMinutes = days * 24 * 60 + hours * 60 + minutes;
        int increment; // Adjust the increment (e.g., 10 minutes)

        // Determine the increment based on the total time to advance
        if (days > 0)
        {
            increment = 60; // 1 hour
        }
        else if (hours > 0)
        {
            increment = 10; // 10 minutes
        }
        else
        {
            increment = 1; // 1 minute
        }

        int minutesPassed = 0;
        while (minutesPassed < totalMinutes)
        {
            // Determine how much time to advance in this step
            int step = Mathf.Min(increment, totalMinutes - minutesPassed);

            // Advance time
            AdvanceTime(step);

            // Update the UI
            PlayerUISystem.Instance.UpdateClockText();

            minutesPassed += step;

            // Smooth the speed of time progression
            float progress = (float)minutesPassed / totalMinutes;
            float waitTime = Mathf.Lerp(0.1f, 0.01f, progress);

            yield return new WaitForSecondsRealtime(waitTime);
        }
        Debug.Log("AdvenceTimeCoroutine");
        isTimeLapsing = false;
        // Update player data
        playerData.Day = Day;
        playerData.Hour = Hour;
        playerData.Minute = Minute;
    }
    public void AnimateTimeChange(int day, int hour, int minute, float time)
    {
        if (isTimeLapsing == true) return;
        isTimeLapsing = true;
        int targetDay = Day + day;
        int targetHour = Hour + hour;
        int targetMinute = Minute + minute;

        // Handle minute overflow before the tween
        if (targetMinute >= 60)
        {
            targetHour += targetMinute / 60;
            targetMinute %= 60;
        }

        // Handle hour overflow before the tween
        if (targetHour >= 24)
        {
            targetDay += targetHour / 24;
            targetHour %= 24;
        }

        Sequence timeSequence = DOTween.Sequence();

        timeSequence.Append(DOTween.To(() => Minute, x => Minute = x, targetMinute, time)
        .OnUpdate(UpdateClockText));

        // Animate hour change
        timeSequence.Append(DOTween.To(() => Hour, x => Hour = x, targetHour, time)
            .OnUpdate(UpdateClockText));

        // Animate day change
        timeSequence.Append(DOTween.To(() => Day, x => Day = x, targetDay, 0.1f)
            .OnUpdate(UpdateClockText));

        timeSequence.Play().OnComplete(() =>
        {
            playerData.Day = targetDay;
            playerData.Hour = targetHour;
            playerData.Minute = targetMinute;
            isTimeLapsing = false;
            Debug.Log("Time advancement completed with tween.");
        });


    }
    private void UpdateClockText()
    {
        clockText.text = $"{Hour:D2}:{Minute:D2}";
        dayText.text = $"Day: {Day}";
    }
    private void NormalizeTime()
    {
        // Calculate total hours and minutes overflow at once
        Hour += Minute / 60;
        Minute %= 60;

        Day += Hour / 24;
        Hour %= 24;

        // Process quest hours reduction once based on the total overflow
        int hoursToReduce = Minute / 60 + Hour;
        if (hoursToReduce > 0)
        {
            foreach (var quest in playerData.Quests)
            {
                if (quest.hoursToComplete > 0)
                {
                    quest.hoursToComplete -= hoursToReduce;
                    if (quest.hoursToComplete < 0) quest.hoursToComplete = 0;
                    quest.QuestCheck(playerData);
                }
            }
        }

        // Process event cooldown reduction once based on the total day overflow
        if (Day > 0)
        {
            foreach (var e in EventHandler.Instance.events)
            {
                if (e.encounterCooldown > 0)
                {
                    e.encounterCooldown -= Day;
                    if (e.encounterCooldown < 0) e.encounterCooldown = 0;
                }
            }

            // Handle travel-related logic once for the day overflow
            if (TravelSystem.Instance.inTravel)
            {
                if (TravelSystem.Instance.isSleeping)
                {
                    SleepWhileTraveling();
                }
                else if (!TravelSystem.Instance.isHuntingForRations)
                {
                    FoodSystem.Instance.DailyRationConsumption();
                }
            }
        }
    }

    /// <summary>
    /// Uyuma işlemini gerçekleştirir ve rasyon tüketimini uygular.
    /// </summary>
    public void Sleep()
    {
        int baseSleepDuration = 6 * 60; // Temel uyku süresi: 6 saat
        int additionalSleepPerExhaustion = 2 * 60; // Her yorgunluk seviyesi için ek süre: 2 saat
        int totalSleepDuration = baseSleepDuration + (playerData.CurrentExhaustionLevel * additionalSleepPerExhaustion);

        FoodSystem.Instance.DailyRationConsumption(); // Yemek tüketimini burada çağırıyoruz

        if (playerData.Rations >= 0)
        {
            playerData.CurrentExhaustionLevel = 0;
            Debug.Log("Uyudunuz ve dinlendiniz. Yorgunluk seviyeniz sıfırlandı.");
        }
        else
        {
            PlayerStatHandler.Instance.IncreaseExhaustion();
            Debug.Log("Yemek yok! Uyudunuz ama yorgunluk seviyeniz arttı.");
        }

        //AdvanceTimeCoroutine(0, 0, totalSleepDuration);
        AnimateTimeChange(0, 0, totalSleepDuration, 1f);
        UpdateLastSleepTime();
        UpdateLastMealTime();
    }
    public void SleepTavern()
    {
        int baseSleepDuration = 6 * 60; // Temel uyku süresi: 6 saat
        int additionalSleepPerExhaustion = 2 * 60; // Her yorgunluk seviyesi için ek süre: 2 saat
        int totalSleepDuration = baseSleepDuration + (playerData.CurrentExhaustionLevel * additionalSleepPerExhaustion);

        AnimateTimeChange(0, 0, totalSleepDuration, 1f);
        UpdateLastSleepTime();
        UpdateLastMealTime();
    }
    
    public void SleepWhileTraveling()
    {

        FoodSystem.Instance.DailyRationConsumption();

        if (playerData.Rations >= 0)
        {
            playerData.CurrentExhaustionLevel = 0;
            Debug.Log("Uyudunuz ve dinlendiniz. Yorgunluk seviyeniz sıfırlandı.");
            UpdateLastMealTime();
        }
        else
        {
            PlayerStatHandler.Instance.IncreaseExhaustion();
            Debug.Log("Yemek yok! Uyudunuz ama yorgunluk seviyeniz arttı.");
        }

        UpdateLastSleepTime();
    }

    /// <summary>
    /// Yorgunluk seviyesini kontrol eder.
    /// </summary>
    private void CheckExhaustion()
    {
        if (playerData == null)
        {
            // Attempt to reinitialize if possible
            if (PlayerStatHandler.Instance != null && PlayerStatHandler.Instance.pd != null)
            {
                playerData = PlayerStatHandler.Instance.pd;
            }

            if (playerData == null)
            {
                Debug.LogWarning("TimeSystem: playerData is null, cannot check exhaustion.");
                return;
            }
        }
        int timeSinceLastMeal = GetTimeDifferenceInMinutes(
            playerData.LastMealDay,
            playerData.LastMealHour,
            playerData.LastMealMinute,
            Day,
            Hour,
            Minute);

        int timeSinceLastSleep = GetTimeDifferenceInMinutes(
            playerData.LastSleepDay,
            playerData.LastSleepHour,
            playerData.LastSleepMinute,
            Day,
            Hour,
            Minute);

        if (timeSinceLastSleep > 1440) // 24 saatten fazla uyumamışsa
        {
            PlayerStatHandler.Instance.IncreaseExhaustion();
            Debug.Log("24 saatten fazla uyumadınız! Yorgunluk seviyeniz arttı.");
            UpdateLastSleepTime();
        }
        if (timeSinceLastMeal > 1440)
        {
            PlayerStatHandler.Instance.IncreaseExhaustion();
            Debug.Log("24 saattir açsınız! Yorgunluk seviyeniz arttı.");
            UpdateLastMealTime();
        }
    }


    /// <summary>
    /// İki zaman noktası arasındaki farkı dakika cinsinden hesaplar.
    /// </summary>
    private int GetTimeDifferenceInMinutes(int startDay, int startHour, int startMinute, int endDay, int endHour, int endMinute)
    {
        int totalStartMinutes = (startDay * 24 * 60) + (startHour * 60) + startMinute;
        int totalEndMinutes = (endDay * 24 * 60) + (endHour * 60) + endMinute;
        return totalEndMinutes - totalStartMinutes;
    }

    /// <summary>
    /// Son uyuma zamanını günceller.
    /// </summary>
    public void UpdateLastSleepTime()
    {
        playerData.LastSleepDay = Day;
        playerData.LastSleepHour = Hour;
        playerData.LastSleepMinute = Minute;
    }

    public void UpdateLastMealTime()
    {
        playerData.LastMealDay = Day;
        playerData.LastMealHour = Hour;
        playerData.LastMealMinute = Minute;
    }

    /// <summary>
    /// Mevcut zamanı string formatında döndürür.
    /// </summary>
    /// <returns>Zaman string'i.</returns>
    public string GetTimeString()
    {
        return $"Gün {Day}, Saat {Hour:D2}:{Minute:D2}";
    }
}
