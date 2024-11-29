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
    public void ConsumeArmyRations()
    {
        // Oyuncunun ordusunu al
        Army playerArmy = PlayerStatHandler.Instance.pd.PlayerArmy;

        // Ordudaki toplam asker sayısını al
        int totalUnits = playerArmy.GetTotalUnits();

        // Yeterli rasyon varsa tüket
        if (PlayerStatHandler.Instance.GetRations() >= totalUnits)
        {
            PlayerStatHandler.Instance.DecreaseRations(totalUnits);
            Debug.Log($"Ordunuz {totalUnits} rasyon tüketti.");
        }
        else
        {
            // Rasyon yetersizse yorgunluk seviyesi artırılır
            int missingRations = totalUnits - PlayerStatHandler.Instance.GetRations();
            PlayerStatHandler.Instance.DecreaseRations(PlayerStatHandler.Instance.GetRations()); // Kalan rasyonları tüket
            PlayerStatHandler.Instance.IncreaseExhaustion();
            Debug.Log($"Rasyon yetersiz! Eksik kalan rasyon: {missingRations}. Yorgunluk seviyesi artırıldı.");
        }
    }

    public void DailyRationConsumption()
    {
        ConsumeArmyRations();
        PlayerUISystem.Instance.UpdateRationText();
        Debug.Log("Günlük rasyon tüketimi tamamlandı.");
    }

    public int GetRationPacks()
    {
        return PlayerStatHandler.Instance.pd.Rations;
    }

}
