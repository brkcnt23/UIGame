using UnityEngine;
using UnityEngine.UI;

public class FoodSystem : MonoBehaviour
{
    //instance
    public static FoodSystem Instance;
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

    public void ConsumeRationPack()
    {
        if (PlayerStatHandler.Instance.GetRations() > 0)
        {
            PlayerStatHandler.Instance.DecreaseRations(1);

        }
        else
        {
            PlayerStatHandler.Instance.IncreaseExhaustion();
        }
    }

    public void DailyRationConsumption()
    {
        PlayerStatHandler.Instance.ConsumeDailyRations();
        PlayerUISystem.Instance.UpdateRationText();
        Debug.Log("Günlük rasyon tüketimi tamamlandı.");
    }

    public int GetRationPacks()
    {
        return PlayerStatHandler.Instance.pd.Rations;
    }

}
