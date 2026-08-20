using System.Collections;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class TimeSystem : MonoBehaviour
{
    public int Hour { get; private set; }
    public int Minute { get; private set; }
    public int Day { get; private set; }

    public bool isTimeLapsing = false;
    private PlayerData playerData;

    public static TimeSystem Instance { get; private set; }

    [Header("UI")]
    public TMP_Text clockText;
    public TMP_Text dayText;

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

    public void InitializeLastActionTimes()
    {
        if (PlayerStatHandler.Instance == null || PlayerStatHandler.Instance.pd == null)
        {
            Debug.LogWarning("TimeSystem: Player data not ready.");
            return;
        }

        playerData = PlayerStatHandler.Instance.pd;

        Day = playerData.Day;
        Hour = playerData.Hour;
        Minute = playerData.Minute;

        // Loading a save at day 185 must not replay 185 days of ticks.
        TimeTickDispatcher.Instance?.Reprime(Day, Hour);
    }

    // -----------------------------
    // ADVANCE TIME
    // -----------------------------

    public void AdvanceTime(int minutes)
    {
        EnsurePlayerData();
        if (playerData == null) return;

        Minute += minutes;
        NormalizeTime();
        CheckExhaustion();

        // Hand the new clock position to the tick dispatcher, which turns it
        // into HourTickEvent / DayTickEvent for every system that listens.
        TimeTickDispatcher.Instance?.SyncTo(Day, Hour);

        if (PlayerUISystem.Instance != null)
            PlayerUISystem.Instance.UpdateClockText();
    }

    public void AdvanceTime(int days, int hours, int minutes)
    {
        if (isTimeLapsing) return;

        int totalMinutes = days * 24 * 60 + hours * 60 + minutes;
        AdvanceTime(totalMinutes);
    }

    public IEnumerator AdvanceTimeCoroutine(int days, int hours, int minutes)
    {
        EnsurePlayerData();
        if (playerData == null) yield break;

        isTimeLapsing = true;

        int totalMinutes = days * 24 * 60 + hours * 60 + minutes;
        int increment;

        if (days > 0)
            increment = 60;
        else if (hours > 0)
            increment = 10;
        else
            increment = 1;

        int minutesPassed = 0;

        while (minutesPassed < totalMinutes)
        {
            int step = Mathf.Min(increment, totalMinutes - minutesPassed);

            AdvanceTime(step);

            if (PlayerUISystem.Instance != null)
                PlayerUISystem.Instance.UpdateClockText();

            minutesPassed += step;

            float progress = totalMinutes > 0 ? (float)minutesPassed / totalMinutes : 1f;
            float waitTime = Mathf.Lerp(0.1f, 0.05f, progress);

            if (MapAvatarHandler.Instance != null)
            {
                MapAvatarHandler.Instance.StopAllCoroutines();
                MapAvatarHandler.Instance.StartCoroutine(MapAvatarHandler.Instance.MovePlayerIconToNextSegment(progress));
            }

            yield return new WaitForSecondsRealtime(waitTime);
        }

        isTimeLapsing = false;

        playerData.Day = Day;
        playerData.Hour = Hour;
        playerData.Minute = Minute;

        if (PlayerUISystem.Instance != null)
            PlayerUISystem.Instance.UpdateUIObjects();
    }

    public void AnimateTimeChange(int day, int hour, int minute, float time, System.Action onComplete = null)
    {
        EnsurePlayerData();
        if (playerData == null) return;
        if (isTimeLapsing) return;

        isTimeLapsing = true;

        int targetDay = Day + day;
        int targetHour = Hour + hour;
        int targetMinute = Minute + minute;

        if (targetMinute >= 60)
        {
            targetHour += targetMinute / 60;
            targetMinute %= 60;
        }

        if (targetHour >= 24)
        {
            targetDay += targetHour / 24;
            targetHour %= 24;
        }

        Sequence timeSequence = DOTween.Sequence();

        timeSequence.Append(
            DOTween.To(() => Minute, x => Minute = x, targetMinute, time)
            .OnUpdate(UpdateClockTextInternal));

        timeSequence.Append(
            DOTween.To(() => Hour, x => Hour = x, targetHour, time)
            .OnUpdate(UpdateClockTextInternal));

        timeSequence.Append(
            DOTween.To(() => Day, x => Day = x, targetDay, 0.1f)
            .OnUpdate(UpdateClockTextInternal));

        timeSequence.Play().OnComplete(() =>
        {
            Day = targetDay;
            Hour = targetHour;
            Minute = targetMinute;

            playerData.Day = targetDay;
            playerData.Hour = targetHour;
            playerData.Minute = targetMinute;

            CheckExhaustion();
            isTimeLapsing = false;

            if (PlayerUISystem.Instance != null)
                PlayerUISystem.Instance.UpdateUIObjects();

            Debug.Log("Time advancement completed with tween.");

            // Call job completion callback if provided
            onComplete?.Invoke();
        });
    }

    // -----------------------------
    // INTERNAL TIME RULES
    // -----------------------------

    /// <summary>
    /// Carries minutes into hours and hours into days. Nothing else.
    ///
    /// Quest countdowns moved to QuestTimerSystem and event cooldowns to
    /// EventCooldownSystem — both listen to the tick events raised in
    /// AdvanceTime. TimeSystem no longer needs to know those systems exist.
    /// </summary>
    private void NormalizeTime()
    {
        Hour += Minute / 60;
        Minute %= 60;

        Day += Hour / 24;

        if (Hour >= 24 && TravelSystem.Instance != null && TravelSystem.Instance.inTravel)
        {
            // Still here because sleeping and eating on the road is bound to
            // travel state, not to the clock. Moves to TravelSystem next.
            SleepAndEatWhileTraveling();
        }

        Hour %= 24;
    }

    // -----------------------------
    // SLEEP / FOOD / EXHAUSTION
    // -----------------------------

    public void Sleep()
    {
        EnsurePlayerData();
        if (playerData == null) return;

        int baseSleepDuration = 6 * 60;
        int additionalSleepPerExhaustion = 2 * 60;
        int totalSleepDuration = baseSleepDuration + (playerData.CurrentExhaustionLevel * additionalSleepPerExhaustion);

        if (FoodSystem.Instance != null)
            FoodSystem.Instance.DailyRationConsumption();

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

        AnimateTimeChange(0, 0, totalSleepDuration, 1f);
        UpdateLastSleepTime();
        UpdateLastMealTime();
    }

    public void SleepTavern()
    {
        EnsurePlayerData();
        if (playerData == null) return;

        int baseSleepDuration = 6 * 60;
        int additionalSleepPerExhaustion = 2 * 60;
        int totalSleepDuration = baseSleepDuration + (playerData.CurrentExhaustionLevel * additionalSleepPerExhaustion);

        playerData.CurrentExhaustionLevel = 0;

        AnimateTimeChange(0, 0, totalSleepDuration, 1f);
        UpdateLastSleepTime();
        UpdateLastMealTime();
    }

    public void SleepAndEatWhileTraveling()
    {
        EnsurePlayerData();
        if (playerData == null) return;
        if (TravelSystem.Instance == null) return;

        bool isSleepingTravel = TravelSystem.Instance.isSleeping;
        bool isHunting = TravelSystem.Instance.isHuntingForRations;

        if (isSleepingTravel)
        {
            if (isHunting || playerData.Rations >= 0)
            {
                playerData.CurrentExhaustionLevel = 0;
            }
            else
            {
                PlayerStatHandler.Instance.IncreaseExhaustion();
            }
        }
        else
        {
            if (!isHunting && playerData.Rations < 0)
            {
                PlayerStatHandler.Instance.IncreaseExhaustion();
            }

            PlayerStatHandler.Instance.IncreaseExhaustion();
        }

        // Weight kaynaklı ekstra yorgunluk
        if (PlayerStatHandler.Instance != null && PlayerStatHandler.Instance.IsOverweight())
        {
            PlayerStatHandler.Instance.IncreaseExhaustion();

            if (PlayerStatHandler.Instance.GetWeightRatio() >= 1.5f)
            {
                PlayerStatHandler.Instance.IncreaseExhaustion();
            }
        }

        UpdateLastMealTime();
        UpdateLastSleepTime();
    }

    private void CheckExhaustion()
    {
        EnsurePlayerData();
        if (playerData == null)
        {
            Debug.LogWarning("TimeSystem: playerData is null, cannot check exhaustion.");
            return;
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

        if (timeSinceLastSleep > 1440)
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

    private int GetTimeDifferenceInMinutes(int startDay, int startHour, int startMinute, int endDay, int endHour, int endMinute)
    {
        int totalStartMinutes = (startDay * 24 * 60) + (startHour * 60) + startMinute;
        int totalEndMinutes = (endDay * 24 * 60) + (endHour * 60) + endMinute;
        return totalEndMinutes - totalStartMinutes;
    }

    public void UpdateLastSleepTime()
    {
        EnsurePlayerData();
        if (playerData == null) return;

        playerData.LastSleepDay = Day;
        playerData.LastSleepHour = Hour;
        playerData.LastSleepMinute = Minute;
    }

    public void UpdateLastMealTime()
    {
        EnsurePlayerData();
        if (playerData == null) return;

        playerData.LastMealDay = Day;
        playerData.LastMealHour = Hour;
        playerData.LastMealMinute = Minute;
    }

    // -----------------------------
    // UI
    // -----------------------------

    private void UpdateClockTextInternal()
    {
        if (clockText != null)
            clockText.text = $"{Hour:D2}:{Minute:D2}";

        if (dayText != null)
            dayText.text = $"Day: {Day}";
    }

    public string GetTimeString()
    {
        return $"Gün {Day}, Saat {Hour:D2}:{Minute:D2}";
    }

    // -----------------------------
    // SAFETY
    // -----------------------------

    private void EnsurePlayerData()
    {
        if (playerData == null && PlayerStatHandler.Instance != null)
        {
            playerData = PlayerStatHandler.Instance.pd;
        }
    }
}