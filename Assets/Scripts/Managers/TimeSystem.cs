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

    private void Start()
    {
        Hour = 6;
        Minute = 0;
        Day = 1;
        playerData = PlayerStatHandler.Instance.pd;

        InitializeLastActionTimes();
    }

    private void InitializeLastActionTimes()
    {
        playerData.LastSleepDay = Day;
        playerData.LastSleepHour = Hour;
        playerData.LastSleepMinute = Minute;
    }

    /// <summary>
    /// Zamanı ilerletir ve gerekli kontrolleri yapar.
    /// </summary>
    /// <param name="minutes">İlerletilecek dakika miktarı.</param>
    public void AdvanceTime(int minutes)
    {
        Minute += minutes;
        NormalizeTime();

        // Yeni bir gün başladığında (isteğe bağlı)
        if (Hour == 0 && Minute == 0)
        {
            // Günlük işlemler buraya eklenebilir
        }

        CheckExhaustion();

        PlayerUISystem.Instance.UpdateClockText();
    }

    private void NormalizeTime()
    {
        while (Minute >= 60)
        {
            Minute -= 60;
            Hour += 1;
        }

        while (Hour >= 24)
        {
            Hour -= 24;
            Day += 1;
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

        PlayerStatHandler.Instance.ConsumeDailyRations(); // Yemek tüketimini burada çağırıyoruz

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

        AdvanceTime(totalSleepDuration);
        UpdateLastSleepTime();
    }

    /// <summary>
    /// Yorgunluk seviyesini kontrol eder.
    /// </summary>
    private void CheckExhaustion()
    {
        int timeSinceLastSleep = GetTimeDifferenceInMinutes(
            playerData.LastSleepDay,
            playerData.LastSleepHour,
            playerData.LastSleepMinute,
            Day,
            Hour,
            Minute);

        if (timeSinceLastSleep >= 1440) // 24 saatten fazla uyumamışsa
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
