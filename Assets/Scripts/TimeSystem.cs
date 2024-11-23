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


    private void Start(){
        Hour = 6;
        Minute = 0;
        Day = 1;
        playerData = PlayerStatHandler.Instance.pd;
        playerData.LastSleepTime = GetTotalMinutes();
        playerData.LastMealTime = GetTotalMinutes();
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
            // Ölüm kontrolü kodunu şu an eklemiyoruz
        }

        // Zamanı ilerlet
        AdvanceTime(totalSleepDuration);

        // Son uyku zamanını güncelle
        playerData.LastSleepTime = GetTotalMinutes();
    }

    // Yorgunluk seviyesini kontrol eder
    private void CheckExhaustion()
    {
        int timeSinceLastSleep = GetTotalMinutes() - playerData.LastSleepTime;
        if (timeSinceLastSleep >= 1440) // 24 saatten fazla uyumamışsa
        {
            playerData.CurrentExhaustionLevel += 1;
            Debug.Log("24 saatten fazla uyumadınız! Yorgunluk seviyeniz arttı.");
            CheckExhaustionDeath();
            // Son uyku zamanını güncelle ki tekrar artmasın
            playerData.LastSleepTime = GetTotalMinutes();
        }
    }

    // Yemek yeme zamanını kontrol eder
    private void CheckMealTime()
    {
        int timeSinceLastMeal = GetTotalMinutes() - playerData.LastMealTime;
        if (timeSinceLastMeal >= 840) // 14 saat (14 * 60 dakika)
        {
            if (playerData.Rations > 0)
            {
                playerData.Rations -= 1;
                Debug.Log("Yemek yediniz.");
                playerData.LastMealTime = GetTotalMinutes();
            }
            else
            {
                playerData.CurrentExhaustionLevel += 1;
                Debug.Log("Yemek yok! Yorgunluk seviyeniz arttı.");
                CheckExhaustionDeath();
                // Son yemek zamanını güncelle ki tekrar artmasın
                playerData.LastMealTime = GetTotalMinutes();
            }
        }
    }

    // Toplam geçen dakika sayısını hesaplar
    private int GetTotalMinutes()
    {
        return (Day * 24 * 60) + (Hour * 60) + Minute;
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
