using UnityEngine;
using UnityEngine.UI;

public class FoodSystem : MonoBehaviour
{
    public Text rationPackText; // Ration Pack sayısını göstermek için
    public Text exhaustionLevelText; // Tükenme seviyesini göstermek için
    public Text clockText; // Saat göstergesi

    private void Update()
    {
        // UI'yi sürekli günceller
        rationPackText.text = $"Ration Packs: {ManualTimeSystem.Instance.GetRationPacks()}";
        exhaustionLevelText.text = $"Exhaustion Level: {ManualTimeSystem.Instance.GetExhaustionLevel()}";
        clockText.text = $"Time: {ManualTimeSystem.Instance.GetCurrentTime()}";
    }

    // Örnek bir çalışma eylemi
    public void Work()
    {
        ManualTimeSystem.Instance.AdvanceTime(8, 0); // 8 saat ileri
        Debug.Log("Worked for 8 hours.");
    }

    // Örnek bir yolculuk eylemi
    public void Travel()
    {
        ManualTimeSystem.Instance.AdvanceTime(2, 30); // 2 saat 30 dakika ileri
        Debug.Log("Traveled for 2 hours 30 minutes.");
    }

    // Uyuma işlemi
    public void Sleep()
    {
        ManualTimeSystem.Instance.Sleep();
    }
}
