using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimeSystemTest : MonoBehaviour
{
    public Button testButton; // Testi başlatmak için bir buton
    public TMP_Text logText; // Sonuçları göstermek için bir UI Text

    private void Start()
    {
        // Butona tıklanınca TestTimeSystem fonksiyonunu çağır
        testButton.onClick.AddListener(TestTimeSystem);

        // Başlangıç mesajı
        if (logText != null)
            logText.text = "Time System Test Initialized.";
    }

public void TestTimeSystem()
{
    string log = "==== Time System Test Start ====\n";

    // Test 1: Yemek varken uyuma
    log += "Test 1: Sleep with Rations...\n";
    PlayerStatHandler.Instance.IncreaseRations(1); // Oyuncuya 1 rasyon ekle
    ManualTimeSystem.Instance.Sleep();
    log += $"After Sleep with Rations: {ManualTimeSystem.Instance.GetCurrentTime()}, Exhaustion: {PlayerStatHandler.Instance.GetExhaustionLevel()}\n";

    // Test 2: Yemek yokken uyuma
    log += "\nTest 2: Sleep without Rations...\n";
    PlayerStatHandler.Instance.DecreaseRations(PlayerStatHandler.Instance.GetRations()); // Rasyonları sıfırla
    ManualTimeSystem.Instance.Sleep();
    log += $"After Sleep without Rations: {ManualTimeSystem.Instance.GetCurrentTime()}, Exhaustion: {PlayerStatHandler.Instance.GetExhaustionLevel()}\n";

    log += "==== Time System Test End ====";

    // Sonuçları göster
    if (logText != null)
    {
        logText.text = log;
    }

    Debug.Log(log);
}

}
