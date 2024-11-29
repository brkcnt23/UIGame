using UnityEngine;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour
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
        InitializeArmies();
    }
    private void InitializeArmies()
    {
        // Oyuncu ordusunu oluştur
        playerArmy = new Army();
        playerArmy.AddUnit(new Unit(UnitType.Knight, 100));
        playerArmy.AddUnit(new Unit(UnitType.Soldier, 200));
        playerArmy.AddUnit(new Unit(UnitType.Archer, 150));
        playerArmy.AddUnit(new Unit(UnitType.Pikeman, 100));
        playerArmy.AddUnit(new Unit(UnitType.Shielder, 50));

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
        PlayerStatHandler.Instance.ConsumeDailyRations();

        // Savaş sonucuna göre yorgunluk seviyesini ve diğer istatistikleri güncelle
        UpdatePlayerStats(result);

        // Zaman sistemini güncelle (isteğe bağlı)
        TimeSystem.Instance.AdvanceTime(60); // Örneğin, savaşı 1 saat olarak kabul et
    }

    /// <summary>
    /// Savaş sonucuna bağlı olarak oyuncunun istatistiklerini günceller.
    /// </summary>
    /// <param name="result">Savaş sonucu.</param>
    private void UpdatePlayerStats(BattleResult result)
    {
        PlayerData playerData = PlayerStatHandler.Instance.pd;

        playerData.TotalBattlesFought += 1;

        if (result.Winner == playerData.PlayerArmy)
        {
            playerData.TotalBattlesWon += 1;
            // Örnek: Kazanılan deneyim ve altın
            playerData.Experience += 50;
            playerData.Gold += 100;
            Debug.Log("Battle Won! Experience and Gold increased.");

            // Bonus: Sağlık artırımı veya diğer avantajlar ekleyebilirsiniz
            playerData.Health = Mathf.Min(playerData.Health + 10, playerData.MaxHealth);
            Debug.Log("Health increased by 10.");
        }
        else
        {
            playerData.TotalBattlesLost += 1;
            // Örnek: Kaybedilen birlikler sonrası yorgunluk
            playerData.CurrentExhaustionLevel += 1;
            Debug.Log("Battle Lost! Exhaustion level increased.");

            // Bonus: Sağlık kaybı veya diğer cezalar ekleyebilirsiniz
            playerData.Health = Mathf.Max(playerData.Health - 20, 0);
            Debug.Log("Health decreased by 20.");

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
