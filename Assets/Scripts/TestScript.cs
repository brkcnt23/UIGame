using UnityEngine;

public class TestScript : MonoBehaviour
{
    private PlayerStatHandler playerStatHandler;
    private TimeSystem timeSystem;
    private CraftingSystem craftingSystem;

    private void Start()
    {
        // PlayerStatHandler'ı al
        playerStatHandler = PlayerStatHandler.Instance;
        timeSystem = TimeSystem.Instance;

        // CraftingSystem'ı başlat
        craftingSystem = new CraftingSystem(playerStatHandler.pd, timeSystem);
    }

    // Bu metodu butona bağlayacağız
    public void TestButtonPressed()
    {

        timeSystem.Sleep();
        Debug.Log("Karakter uyudu.");

        // Smithing çıraklığı yapsın
        craftingSystem.WorkAsApprentice(CraftType.Smither, 1);
        Debug.Log("Karakter smithing çıraklığı yaptı.");

        // UI güncellensin
        PlayerUISystem.Instance.UpdateClockText();
        PlayerUISystem.Instance.UpdateExhaustionText();
        PlayerUISystem.Instance.UpdateRationText();
    }
}
