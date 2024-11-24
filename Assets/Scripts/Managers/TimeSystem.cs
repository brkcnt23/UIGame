using UnityEngine;

public class TimeSystem : MonoBehaviour
{
    public int Hour { get; private set; }    // Saat (0-23)
    public int Minute { get; private set; }  // Dakika (0-59)
    public int Day { get; private set; }     // Gün (1 ve üstü)

    private PlayerData playerData;
    //get instance
    public static TimeSystem Instance { get; private set; }
    //check instance in awake func
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

        // Son uyku ve yemek zamanlarını başlangıç zamanı olarak ayarla
        playerData.LastSleepDay = Day;
        playerData.LastSleepHour = Hour;
        playerData.LastSleepMinute = Minute;

        playerData.LastMealDay = Day;
        playerData.LastMealHour = Hour;
        playerData.LastMealMinute = Minute;
    }
    // Zamanı belirli bir dakika kadar ilerletir
    public void AdvanceTime(int minutes)
    {
        Minute += minutes;
        NormalizeTime();

        // Zaman ilerlediğinde yorgunluk ve yemek kontrollerini yap
        CheckExhaustion();
        CheckMealTime();
        PlayerUISystem.Instance.UpdateClockText();
    }

    // Zamanı normalize eder (60 dakika olduğunda saati artırır, 24 saat olduğunda günü artırır)
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

    // Oyuncu uyur
    public void Sleep()
    {
        int baseSleepDuration = 6 * 60; // Temel uyku süresi: 6 saat
        int additionalSleepPerExhaustion = 2 * 60; // Her yorgunluk seviyesi için ek süre: 2 saat

        // Toplam uyku süresi hesaplanıyor
        int totalSleepDuration = baseSleepDuration + (playerData.CurrentExhaustionLevel * additionalSleepPerExhaustion);

        // Yemek kontrolü
        if (playerData.Rations > 0)
        {
            playerData.Rations -= 1;
            playerData.CurrentExhaustionLevel = 0;
            Debug.Log("Uyudunuz ve dinlendiniz. Yorgunluk seviyeniz sıfırlandı.");
        }
        else
        {
            // Yemek yoksa yorgunluk seviyesi artar
            playerData.CurrentExhaustionLevel += 1;
            Debug.Log("Yemek yok! Uyudunuz ama yorgunluk seviyeniz arttı.");
        }

        // Zamanı ilerlet
        AdvanceTime(totalSleepDuration);

        // Son uyku zamanını güncelle
        playerData.LastSleepDay = Day;
        playerData.LastSleepHour = Hour;
        playerData.LastSleepMinute = Minute;
    }


    // Yorgunluk seviyesini kontrol eder
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
            playerData.CurrentExhaustionLevel += 1;
            Debug.Log("24 saatten fazla uyumadınız! Yorgunluk seviyeniz arttı.");
            CheckExhaustionDeath();
            // Son uyku zamanını güncelle
            playerData.LastSleepDay = Day;
            playerData.LastSleepHour = Hour;
            playerData.LastSleepMinute = Minute;
        }
    }


    // Yemek yeme zamanını kontrol eder
    private void CheckMealTime()
    {
        int timeSinceLastMeal = GetTimeDifferenceInMinutes(
            playerData.LastMealDay,
            playerData.LastMealHour,
            playerData.LastMealMinute,
            Day,
            Hour,
            Minute);

        if (timeSinceLastMeal >= 840) // 14 saat (14 * 60 dakika)
        {
            if (playerData.Rations > 0)
            {
                playerData.Rations -= 1;
                Debug.Log("Yemek yediniz.");
                // Son yemek zamanını güncelle
                playerData.LastMealDay = Day;
                playerData.LastMealHour = Hour;
                playerData.LastMealMinute = Minute;
            }
            else
            {
                playerData.CurrentExhaustionLevel += 1;
                Debug.Log("Yemek yok! Yorgunluk seviyeniz arttı.");
                CheckExhaustionDeath();
                // Son yemek zamanını güncelle
                playerData.LastMealDay = Day;
                playerData.LastMealHour = Hour;
                playerData.LastMealMinute = Minute;
            }
        }
    }


    private int GetTimeDifferenceInMinutes(int startDay, int startHour, int startMinute, int endDay, int endHour, int endMinute)
    {
        int totalStartMinutes = (startDay * 24 * 60) + (startHour * 60) + startMinute;
        int totalEndMinutes = (endDay * 24 * 60) + (endHour * 60) + endMinute;
        return totalEndMinutes - totalStartMinutes;
    }

    // Yorgunluk seviyesinin maksimuma ulaşıp ulaşmadığını kontrol eder
    private void CheckExhaustionDeath()
    {
        if (playerData.CurrentExhaustionLevel >= playerData.MaxExhaustionLevel)
        {
            playerData.HasDied = true;
            Debug.Log("Yorgunluktan öldünüz!");
        }
    }

    // Zaman bilgisini string olarak döndürür
    public string GetTimeString()
    {
        return $"Gün {Day}, Saat {Hour:D2}:{Minute:D2}";
    }
}
