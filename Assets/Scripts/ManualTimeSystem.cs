using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ManualTimeSystem : MonoBehaviour
{
    public static ManualTimeSystem Instance { get; private set; } // Singleton Instance

    public Text clockText; // Saat göstergesi
    public Text actionLogText; // Yapılan eylemleri göstermek için
    private int currentHour = 6; // Başlangıç saati (06:00)
    private int currentMinute = 0; // Başlangıç dakikası
    private int exhaustionLevel = 0; // Tükenmişlik seviyesi
    private int rationPacks = 10; // Başlangıç Ration Pack sayısı

    private int nextMealTime; // Uyanıştan 14 saat sonra yemek yeme zamanı

    private void Awake()
    {
        // Singleton kontrolü
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Zaman sistemini sahneler arasında koru
        }
    }

    void Start()
    {
        SetNextMealTime(); // İlk yemek zamanını ayarla
        UpdateClock(); // Oyun başladığında saati güncelle
    }

    // Zamanı manuel olarak ilerlet
    public void AdvanceTime(int hours, int minutes)
    {
        currentMinute += minutes;
        currentHour += hours + currentMinute / 60; // Dakika taşmasını saatlere ekle
        currentMinute %= 60;
        currentHour %= 24;

        // Yemek kontrolü
        if (currentHour >= nextMealTime && currentHour < 24)
        {
            ConsumeRationPack();
            SetNextMealTime(); // Bir sonraki yemek zamanını belirle
        }

        UpdateClock(); // Saati güncelle
    }

    // Yemek tüketim kontrolü
    private void ConsumeRationPack()
    {
        if (rationPacks > 0)
        {
            rationPacks--;
            Debug.Log("You ate a meal. Remaining Ration Packs: " + rationPacks);
        }
        else
        {
            IncreaseExhaustion();
        }
    }

    // Tükenmişlik seviyesi artırılır
    private void IncreaseExhaustion()
    {
        exhaustionLevel++;
        Debug.Log("No food available! Exhaustion level increased to: " + exhaustionLevel);
    }

    // Uyuma işlemi
    public void Sleep()
    {
        // Uyuma sırasında yemek kontrolü yap
        if (currentHour < nextMealTime)
        {
            AdvanceTime(nextMealTime - currentHour, 0); // Yemek saatine kadar zamanı sar
        }

        if (rationPacks > 0)
        {
            rationPacks--;
            exhaustionLevel = 0; // Yemek varsa tükenme sıfırlanır
            Debug.Log("You slept and ate a meal. Exhaustion reset to 0.");
        }
        else
        {
            Debug.Log("You slept but had no food. Exhaustion remains at: " + exhaustionLevel);
        }

        currentHour = HasToSleep();
        currentMinute = 0;
        SetNextMealTime(); // Yeni yemek zamanını ayarla
        UpdateClock();
    }

    // Uyanıştan sonraki ilk yemek saatini belirler
    private void SetNextMealTime()
    {
        nextMealTime = (currentHour + 14) % 24; // Uyanıştan 14 saat sonrası
        Debug.Log("Next meal time set to: " + nextMealTime + ":00");
    }

    // Bir eylem yap ve zamanı ilerlet
    public void PerformAction(string actionName, int hours, int minutes)
    {
        AdvanceTime(hours, minutes); // Zamanı ileri sar
        LogAction(actionName, hours, minutes); // Eylemi kaydet
    }

    // Yapılan eylemleri günceller
    private void LogAction(string actionName, int hours, int minutes)
    {
        actionLogText.text += $"\n{actionName} completed in {hours}h {minutes}m. New time: {currentHour:D2}:{currentMinute:D2}";
    }

    // Saati günceller
    private void UpdateClock()
    {
        clockText.text = $"{currentHour:D2}:{currentMinute:D2}";
    }

    // Saat ve mevcut durumları döndürmek için erişimciler
    public string GetCurrentTime()
    {
        return $"{currentHour:D2}:{currentMinute:D2}";
    }

    public int GetRationPacks()
    {
        return rationPacks;
    }

    public int GetExhaustionLevel()
    {
        return exhaustionLevel;
    }
    public int GetExhaustionTime()
    {
        int exhaustionTime = 0;
        switch (exhaustionLevel)
        {
            case 1:
            exhaustionTime = 2;
            break;
            case 2:
            exhaustionTime = 4;
            break;
            case 3:
            exhaustionTime = 6;
            break;
            case 4:
            exhaustionTime = 8;
            break;
            case 5:
            exhaustionTime = 10;
            break;
            case 6:
            exhaustionTime = 12;
            break;
            case 7:
            exhaustionTime = 14;
            break;
            case 8:
            exhaustionTime = 16;
            break;
            case 9:
            exhaustionTime = 18;
            break;
            case 10:
            exhaustionTime = 18;
            break;
            default: 
            exhaustionTime = 0;
            break;
        }
        return exhaustionTime;
    }
public int HasToSleep()
{
    int sleepTime = 6 + GetExhaustionTime();

    return sleepTime;
}
}
