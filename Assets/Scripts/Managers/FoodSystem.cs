using UnityEngine;

public class FoodSystem : MonoBehaviour
{
    public static FoodSystem Instance { get; private set; }

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

    public void DailyRationConsumption()
    {
        if (PlayerStatHandler.Instance == null)
        {
            Debug.LogError("FoodSystem: PlayerStatHandler.Instance is null! Cannot consume daily rations.");
            return;
        }

        PlayerStatHandler.Instance.ConsumeDailyRations();

        if (PlayerUISystem.Instance != null)
        {
            PlayerUISystem.Instance.UpdateRationText();
            PlayerUISystem.Instance.UpdateExhaustionText();
            PlayerUISystem.Instance.UpdateUIObjects();
        }

        // UI updates handled by StateManager listeners
        {
        }

        Debug.Log("Günlük rasyon tüketimi tamamlandı.");
    }

    public int GetRationPacks()
    {
        if (PlayerStatHandler.Instance != null && PlayerStatHandler.Instance.pd != null)
        {
            return PlayerStatHandler.Instance.pd.Rations;
        }

        Debug.LogWarning("FoodSystem: PlayerStatHandler.Instance or pd is null! Returning 0.");
        return 0;
    }
}