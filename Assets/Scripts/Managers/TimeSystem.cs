using System;
using System.Collections;
using UnityEngine;

public class TimeSystem : MonoBehaviour
{
    public int Hour { get; private set; }    // Saat (0-23)
    public int Minute { get; private set; }  // Dakika (0-59)
    public int Day { get; private set; }     // Gün (1 ve üstü)

    private PlayerData playerData;

    // Singleton Instance
    public static TimeSystem Instance { get; private set; }

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
        int totalMinutes = days * 24 * 60 + hours * 60 + minutes;
        AdvanceTime(totalMinutes);
    }

    //we will advance time but with a IEnumerator so we can see the time passing
    public IEnumerator AdvanceTimeCoroutine(int days, int hours, int minutes)
    {
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

        // Update player data
        playerData.Day = Day;
        playerData.Hour = Hour;
        playerData.Minute = Minute;
    }


    private void NormalizeTime()
    {
        while (Minute >= 60)
        {
            Minute -= 60;
            Hour += 1;

            Quest_SO_Constructor[] quests = playerData.Quests.ToArray();
            foreach (Quest_SO_Constructor quest in quests)
            {
                if (quest.hoursToComplete > 0)
                {
                    quest.hoursToComplete--;
                    quest.QuestCheck(playerData);
                }
            }
        }

        while (Hour >= 24)
        {
            Hour -= 24;
            Day += 1;


            Event_SO_Constructor[] events = EventHandler.Instance.events.ToArray();
            // Eventlerin hepsinin süresini her gün için 1 azalt (min 0 olacak)
            foreach (Event_SO_Constructor e in events)
            {
                if (e.encounterCooldown > 0)
                {
                    e.encounterCooldown--;
                }
            }

            if (TravelSystem.Instance.inTravel)
            {
                if (TravelSystem.Instance.isSleeping)
                {
                    SleepWhileTraveling();
                }

                if (TravelSystem.Instance.isHuntingForRations)
                {

                }
                else
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

        AdvanceTimeCoroutine(0, 0, totalSleepDuration);
        UpdateLastSleepTime();
    }

    public void SleepWhileTraveling()
    {

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
    private void UpdateLastSleepTime()
    {
        playerData.LastSleepDay = Day;
        playerData.LastSleepHour = Hour;
        playerData.LastSleepMinute = Minute;
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
