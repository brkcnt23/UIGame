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

        // CraftingSystem'ı al
        craftingSystem = CraftingSystem.Instance;
    }

    // Bu metodu butona bağlayacağız
    public void TestButtonPressed()
    {

        timeSystem.Sleep();
        Debug.Log("Karakter uyudu.");

        // UI güncellensin
        PlayerUISystem.Instance.UpdateClockText();
        PlayerUISystem.Instance.UpdateExhaustionText();
        PlayerUISystem.Instance.UpdateRationText();
    }
}
