using UnityEngine;
using UnityEngine.UI;

public class BattleManagerOLD : MonoBehaviour
{
    [Header("Battle Settings")]
    public int enemyTotalUnits = 500; // Rastgele oluşturulacak düşman ordusundaki toplam asker sayısı

    [Header("Battle Simulator")]
    private BattleSimulator simulator;

    [Header("UI Elements")]
    public Text battleResultText; // Savaş sonucunu göstermek için UI Text bileşeni
    public Text battleCasualtiesText; // Savaş kayıplarını göstermek için ek UI Text bileşeni
    public Army playerArmy;
    private void Start()
    {
        simulator = new BattleSimulator();
    }

    /// <summary>
    /// UI'den çağrılarak savaşı başlatır.
    /// </summary>
    public void StartBattle()
    {
        // Oyuncunun ordusunu PlayerStatHandler üzerinden al
        Army playerArmy = PlayerStatHandler.Instance.pd.PlayerArmy;

        // Rastgele bir düşman ordusu oluştur
        Army enemyArmy = simulator.GenerateRandomEnemyArmy(enemyTotalUnits);

        // Savaşı simüle et ve sonucu al
        BattleResult result = simulator.SimulateBattle(playerArmy, enemyArmy);

        // Sonucu UI'de göster
        battleResultText.text = result.ResultMessage;

        // Savaş kayıplarını UI'de göster
        DisplayCasualties(result);

        // Savaş sonucuna göre rasyon tüketimini uygula
        FoodSystem.Instance.DailyRationConsumption();

        // Savaş sonucuna göre yorgunluk seviyesini ve diğer istatistikleri güncelle
        UpdatePlayerStats(result);

        // Zaman sistemini güncelle (isteğe bağlı)
        TimeSystem.Instance.AdvanceTimeCoroutine(0,1,0); // Örneğin, savaşı 1 saat olarak kabul et
    }

    /// <summary>
    /// Savaş sonucuna bağlı olarak oyuncunun istatistiklerini günceller.
    /// </summary>
    /// <param name="result">Savaş sonucu.</param>
    private void UpdatePlayerStats(BattleResult result)
    {
        PlayerData playerData = PlayerStatHandler.Instance.pd;

        playerData.TotalBattlesFought += 1;

        if (result.Player == playerData.PlayerArmy)
        {
            playerData.TotalBattlesWon += 1;
            playerData.Experience += 50;
            playerData.AddMoney(100, 0);
            Debug.Log("Battle Won! Experience and Gold increased.");

            // Bonus: Sağlık artırımı veya diğer avantajlar ekleyebilirsiniz
            playerData.Health -= 10;
            Debug.Log("Health increased by 10.");
        }
        else
        {
            playerData.TotalBattlesLost += 1;
            // Örnek: Kaybedilen birlikler sonrası yorgunluk
            playerData.CurrentExhaustionLevel += 1;
            Debug.Log("Battle Lost! Exhaustion level increased.");

            // Bonus: Sağlık kaybı veya diğer cezalar ekleyebilirsiniz
            playerData.Health -= 50;
            Debug.Log("Health decreased by 50.");

            // Yorgunluk seviyesinin maksimum seviyeyi aşıp aşmadığını kontrol et
            PlayerStatHandler.Instance.CheckExhaustionMaxed();
        }

    }


    /// <summary>
    /// Savaş kayıplarını UI'de gösterir.
    /// </summary>
    /// <param name="result">Savaş sonucu.</param>
    private void DisplayCasualties(BattleResult result)
    {
        string casualtiesMessage = "Battle Casualties:\n";

        // Kazanan ordunun kayıpları
        casualtiesMessage += "Winner Casualties:\n";
        foreach (var casualty in result.WinnerCasualties)
        {
            casualtiesMessage += $"{casualty.Key}: {casualty.Value}\n";
        }

        // Kaybeden ordunun kayıpları
        casualtiesMessage += "Loser Casualties:\n";
        foreach (var casualty in result.LoserCasualties)
        {
            casualtiesMessage += $"{casualty.Key}: {casualty.Value}\n";
        }

        battleCasualtiesText.text = casualtiesMessage;
    }
}
