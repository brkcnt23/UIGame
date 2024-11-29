using System;
using System.Collections.Generic;
using UnityEngine;

public class BattleSimulator
{
    // Birlik türlerinin hangi türlere karşı güçlü olduğunu tanımlayan static readonly bir sözlük.
    private static readonly Dictionary<UnitType, UnitType> StrongAgainst = new Dictionary<UnitType, UnitType>
    {
        { UnitType.Knight, UnitType.Soldier },
        { UnitType.Soldier, UnitType.Shielder },
        { UnitType.Shielder, UnitType.Archer },
        { UnitType.Archer, UnitType.Pikeman },
        { UnitType.Pikeman, UnitType.Knight }
    };

    /// <summary>
    /// Orduların savaş gücünü hesaplar.
    /// </summary>
    /// <param name="ownArmy">Kendi ordunuz.</param>
    /// <param name="enemyArmy">Düşman ordusu.</param>
    /// <returns>Toplam savaş gücü.</returns>
    private float CalculateArmyPower(Army ownArmy, Army enemyArmy)
    {
        float power = 0f;

        foreach (var unit in ownArmy.Units)
        {
            float unitPower = unit.Count;

            // Güçlü olduğu düşman birlikleri için bonus ekle
            if (StrongAgainst.TryGetValue(unit.Type, out UnitType strongAgainstType))
            {
                foreach (var enemyUnit in enemyArmy.Units)
                {
                    if (enemyUnit.Type == strongAgainstType)
                    {
                        unitPower += enemyUnit.Count * 0.5f; // Bonus güç
                    }
                }
            }

            power += unitPower;
        }

        return power;
    }

    /// <summary>
    /// Savaş simülasyonunu gerçekleştirir ve sonucu döndürür.
    /// </summary>
    /// <param name="army1">Birinci ordu.</param>
    /// <param name="army2">İkinci ordu.</param>
    /// <returns>Savaş sonucu.</returns>
    public BattleResult SimulateBattle(Army army1, Army army2)
    {
        float army1Power = CalculateArmyPower(army1, army2);
        float army2Power = CalculateArmyPower(army2, army1);

        float totalPower = army1Power + army2Power;
        float difficultyFactor = CalculateDifficultyFactor(army1,army2);
        // Her iki ordunun kazanma olasılıklarını hesapla
        float army1WinProbability = army1Power / totalPower;

        // Unity'nin random sistemini kullanarak rastgele bir sayı üret
        float randomValue = UnityEngine.Random.Range(0f, 1f);

        bool army1Wins = randomValue < army1WinProbability;
        Army winner = army1Wins ? army1 : army2;
        Army loser = army1Wins ? army2 : army1;

        // Kayıpları hesapla
        Dictionary<UnitType, int> winnerCasualties = CalculateCasualties(winner, true);
        Dictionary<UnitType, int> loserCasualties = CalculateCasualties(loser, false);

        // Kayıpları uygula
        ApplyCasualties(winner, winnerCasualties);
        ApplyCasualties(loser, loserCasualties);

        string resultMessage = army1Wins ? "Army 1 wins!" : "Army 2 wins!";
        Debug.Log(resultMessage);
        army1.DisplayUnits("Army 1");
        army2.DisplayUnits("Army 2");
        ResolveBattle(difficultyFactor);
        return new BattleResult(winner, loser, resultMessage, winnerCasualties, loserCasualties);
    }

    /// <summary>
    /// Kazanan veya kaybeden ordunun kayıplarını hesaplar.
    /// </summary>
    /// <param name="army">Hedef ordu.</param>
    /// <param name="isWinner">Ordu kazandı mı?</param>
    /// <returns>Birlik türüne göre kayıp sayıları.</returns>
    private Dictionary<UnitType, int> CalculateCasualties(Army army, bool isWinner)
    {
        Dictionary<UnitType, int> casualties = new Dictionary<UnitType, int>();

        foreach (var unit in army.Units)
        {
            float lossPercentage = isWinner ? 0.05f : 0.90f; // Kazananlar %5, kaybedenler %90 kaybeder
            int lossCount = Mathf.CeilToInt(unit.Count * lossPercentage);
            lossCount = Mathf.Min(lossCount, unit.Count); // Kayıp sayısı toplam birlik sayısını aşamaz
            casualties.Add(unit.Type, lossCount);
        }

        return casualties;
    }

    /// <summary>
    /// Ordunun birliklerinden belirli sayıda azaltır.
    /// </summary>
    /// <param name="army">Hedef ordu.</param>
    /// <param name="casualties">Birlik türüne göre kayıp sayıları.</param>
    private void ApplyCasualties(Army army, Dictionary<UnitType, int> casualties)
    {
        foreach (var casualty in casualties)
        {
            army.RemoveUnit(casualty.Key, casualty.Value);
        }
    }

    /// <summary>
    /// Rastgele bir düşman ordusu oluşturur.
    /// </summary>
    /// <param name="totalUnits">Toplam asker sayısı.</param>
    /// <returns>Rastgele oluşturulmuş düşman ordusu.</returns>
    public Army GenerateRandomEnemyArmy(int totalUnits)
    {
        Army enemyArmy = new Army();
        List<UnitType> unitTypes = new List<UnitType>((UnitType[])Enum.GetValues(typeof(UnitType)));
        int remainingUnits = totalUnits;
        int unitTypesCount = unitTypes.Count;

        for (int i = 0; i < unitTypesCount; i++)
        {
            if (i == unitTypesCount - 1)
            {
                // Son birlik türü için kalan tüm askerleri ata
                enemyArmy.AddUnit(new Unit(unitTypes[i], remainingUnits));
            }
            else
            {
                // Rastgele bir sayı belirle, kalan asker sayısını aşmayacak şekilde
                int maxUnitsForThisType = Mathf.Max(1, remainingUnits - (unitTypesCount - i - 1));
                int unitsForThisType = UnityEngine.Random.Range(1, maxUnitsForThisType + 1);
                enemyArmy.AddUnit(new Unit(unitTypes[i], unitsForThisType));
                remainingUnits -= unitsForThisType;
            }
        }

        return enemyArmy;
    }
    public float CalculateDifficultyFactor(Army playerArmy, Army enemyArmy)
    {
        // 1. Ordu Gücü Hesaplama
        float playerPower = CalculateArmyPower(playerArmy, enemyArmy);
        float enemyPower = CalculateArmyPower(enemyArmy, playerArmy);

        // 2. Ordu Güç Farkı (Zorluk için normalize edilmiştir)
        float powerDifference = Mathf.Abs(enemyPower - playerPower);
        float powerFactor = Mathf.Clamp01(powerDifference / Mathf.Max(playerPower, enemyPower)); // 0.0 - 1.0 arasında normalize

        // 3. Ordu Boyutu Farkı
        int playerUnits = playerArmy.GetTotalUnits();
        int enemyUnits = enemyArmy.GetTotalUnits();
        float sizeFactor = Mathf.Clamp01(Mathf.Abs(enemyUnits - playerUnits) / (float)Mathf.Max(playerUnits, enemyUnits));

        // 4. Güçlü Birim Oranı
        float strongUnitFactor = 0f;
        foreach (var enemyUnit in enemyArmy.Units)
        {
            foreach (var playerUnit in playerArmy.Units)
            {
                if (StrongAgainst.TryGetValue(enemyUnit.Type, out UnitType strongAgainstType) &&
                    strongAgainstType == playerUnit.Type)
                {
                    strongUnitFactor += enemyUnit.Count * 0.1f; // Güçlü birim başına eklenir
                }
            }
        }
        strongUnitFactor = Mathf.Clamp01(strongUnitFactor / enemyUnits); // Normalize

        // 5. Zorluk Faktörü Toplamı
        float difficultyFactor = 0.4f * powerFactor + 0.3f * sizeFactor + 0.3f * strongUnitFactor;

        Debug.Log($"Difficulty Factor Calculated: {difficultyFactor} (Power: {powerFactor}, Size: {sizeFactor}, Strong Units: {strongUnitFactor})");
        return difficultyFactor;
    }
    public void ResolveBattle(float difficultyFactor)
    {
        // Savaş hesaplamaları...
        int baseExpGain = Mathf.RoundToInt(difficultyFactor * 100);
        PlayerStatHandler.Instance.AddCharacterExperience(baseExpGain);
        Debug.Log($"Battle completed. Character gained {baseExpGain} EXP.");
    }

}

public class BattleResult
{
    public Army Winner { get; private set; }
    public Army Loser { get; private set; }
    public string ResultMessage { get; private set; }
    public Dictionary<UnitType, int> WinnerCasualties { get; private set; }
    public Dictionary<UnitType, int> LoserCasualties { get; private set; }

    public BattleResult(Army winner, Army loser, string message,
                        Dictionary<UnitType, int> winnerCasualties,
                        Dictionary<UnitType, int> loserCasualties)
    {
        Winner = winner;
        Loser = loser;
        ResultMessage = message;
        WinnerCasualties = winnerCasualties;
        LoserCasualties = loserCasualties;
    }
}
