using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public float CalculateArmyPower(Army ownArmy, Army enemyArmy, TerrainType terrain, WeatherType weather)
    {
        float power = 0f;

        foreach (var unit in ownArmy.Units)
        {
            float unitPower = unit.Count;

            // Apply terrain bonuses
            if (TerrainAdvantages.TryGetValue(unit.Type, out TerrainType advantageTerrain) && advantageTerrain == terrain)
            {
                unitPower *= 1.2f; // 20% terrain bonus
            }

            // Apply weather penalties
            if (WeatherDisadvantages.TryGetValue(unit.Type, out WeatherType disadvantageWeather) && disadvantageWeather == weather)
            {
                unitPower *= 0.8f; // 20% weather penalty
            }

            // Existing strong against calculations...
            if (StrongAgainst.TryGetValue(unit.Type, out UnitType strongAgainstType))
            {
                foreach (var enemyUnit in enemyArmy.Units)
                {
                    if (enemyUnit.Type == strongAgainstType)
                    {
                        unitPower += enemyUnit.Count * 0.5f;
                    }
                }
            }

            // Add morale system
            float moraleModifier = CalculateMoraleModifier(ownArmy);
            unitPower *= moraleModifier;

            power += unitPower;
        }

        return power;
    }
    private float CalculateMoraleModifier(Army army)
    {
        // Base morale starts at 1.0
        float morale = 1.0f;

        // Recent victories increase morale
        int recentVictories = PlayerStatHandler.Instance.pd.TotalBattlesWon;
        morale += recentVictories * 0.05f; // Each victory adds 5% morale

        // Low health reduces morale
        float healthPercentage = (float)PlayerStatHandler.Instance.pd.Health / PlayerStatHandler.Instance.pd.MaxHealth;
        if (healthPercentage < 0.5f)
        {
            morale *= healthPercentage + 0.5f; // Up to 50% reduction for low health
        }

        // Exhaustion reduces morale
        int exhaustion = PlayerStatHandler.Instance.pd.CurrentExhaustionLevel;
        morale *= (1.0f - (exhaustion * 0.1f)); // Each exhaustion level reduces morale by 10%

        return Mathf.Clamp(morale, 0.5f, 2.0f); // Clamp between 50% and 200%
    }


    /// <summary>
    /// Savaş simülasyonunu gerçekleştirir ve sonucu döndürür.
    /// </summary>
    /// <param name="army1">Birinci ordu.</param>
    /// <param name="army2">İkinci ordu.</param>
    /// <returns>Savaş sonucu.</returns>
    public BattleResult SimulateBattle(Army army1, Army army2, TerrainType terrain = TerrainType.Plains, WeatherType weather = WeatherType.Clear)
    {
        // Randomly determine terrain and weather

        float army1Power = CalculateArmyPower(army1, army2, terrain, weather);
        float army2Power = CalculateArmyPower(army2, army1, terrain, weather);

        float totalPower = army1Power + army2Power;
        //float difficultyFactor = CalculateDifficultyFactor(army1, army2);
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
        //ResolveBattle(difficultyFactor);
        // Add battle events system
        List<string> battleEvents = GenerateBattleEvents(army1, army2, terrain, weather);

        // Return enhanced battle result with events
        return new BattleResult(winner, loser, resultMessage, winnerCasualties, loserCasualties, battleEvents, terrain, weather);
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
                enemyArmy.AddUnit(new Unit() { Type = unitTypes[i], Count = remainingUnits });
            }
            else
            {
                // Rastgele bir sayı belirle, kalan asker sayısını aşmayacak şekilde
                int maxUnitsForThisType = Mathf.Max(1, remainingUnits - (unitTypesCount - i - 1));
                int unitsForThisType = UnityEngine.Random.Range(1, maxUnitsForThisType + 1);
                enemyArmy.AddUnit(new Unit() { Type = unitTypes[i], Count = unitsForThisType });
                remainingUnits -= unitsForThisType;
            }
        }

        return enemyArmy;
    }
    // public float CalculateDifficultyFactor(Army playerArmy, Army enemyArmy)
    // {
    //     // 1. Ordu Gücü Hesaplama
    //     float playerPower = CalculateArmyPower(playerArmy, enemyArmy);
    //     float enemyPower = CalculateArmyPower(enemyArmy, playerArmy);

    //     // 2. Ordu Güç Farkı (Zorluk için normalize edilmiştir)
    //     float powerDifference = Mathf.Abs(enemyPower - playerPower);
    //     float powerFactor = Mathf.Clamp01(powerDifference / Mathf.Max(playerPower, enemyPower)); // 0.0 - 1.0 arasında normalize

    //     // 3. Ordu Boyutu Farkı
    //     int playerUnits = playerArmy.GetTotalUnits();
    //     int enemyUnits = enemyArmy.GetTotalUnits();
    //     float sizeFactor = Mathf.Clamp01(Mathf.Abs(enemyUnits - playerUnits) / (float)Mathf.Max(playerUnits, enemyUnits));

    //     // 4. Güçlü Birim Oranı
    //     float strongUnitFactor = 0f;
    //     foreach (var enemyUnit in enemyArmy.Units)
    //     {
    //         foreach (var playerUnit in playerArmy.Units)
    //         {
    //             if (StrongAgainst.TryGetValue(enemyUnit.Type, out UnitType strongAgainstType) &&
    //                 strongAgainstType == playerUnit.Type)
    //             {
    //                 strongUnitFactor += enemyUnit.Count * 0.1f; // Güçlü birim başına eklenir
    //             }
    //         }
    //     }
    //     strongUnitFactor = Mathf.Clamp01(strongUnitFactor / enemyUnits); // Normalize

    //     // 5. Zorluk Faktörü Toplamı
    //     float difficultyFactor = 0.4f * powerFactor + 0.3f * sizeFactor + 0.3f * strongUnitFactor;

    //     Debug.Log($"Difficulty Factor Calculated: {difficultyFactor} (Power: {powerFactor}, Size: {sizeFactor}, Strong Units: {strongUnitFactor})");
    //     return difficultyFactor;
    // }
    public void ResolveBattle(float difficultyFactor)
    {
        // Savaş hesaplamaları...
        int baseExpGain = Mathf.RoundToInt(difficultyFactor * 100);
        PlayerStatHandler.Instance.AddCharacterExperience(baseExpGain);
        Debug.Log($"Battle completed. Character gained {baseExpGain} EXP.");
    }
    private List<string> GenerateBattleEvents(Army army1, Army army2, TerrainType terrain, WeatherType weather)
    {
        List<string> events = new List<string>
        {
            $"Battle begins in {terrain} terrain under {weather} conditions."
        };

        // Generate strategic advantages
        foreach (var unit in army1.Units)
        {
            if (TerrainAdvantages.TryGetValue(unit.Type, out TerrainType advantageTerrain) && advantageTerrain == terrain)
            {
                events.Add($"{unit.Type}s take advantage of the {terrain} terrain!");
            }
        }

        // Generate weather effects
        foreach (var unit in army1.Units.Concat(army2.Units))
        {
            if (WeatherDisadvantages.TryGetValue(unit.Type, out WeatherType disadvantageWeather) && disadvantageWeather == weather)
            {
                events.Add($"{unit.Type}s struggle in the {weather} conditions!");
            }
        }

        return events;
    }

    private static readonly Dictionary<UnitType, TerrainType> TerrainAdvantages = new Dictionary<UnitType, TerrainType>
    {
        { UnitType.Archer, TerrainType.Hills },
        { UnitType.Knight, TerrainType.Plains },
        { UnitType.Soldier, TerrainType.Forest },
        { UnitType.Pikeman, TerrainType.Mountains }
    };

    // Add weather effects
    private static readonly Dictionary<UnitType, WeatherType> WeatherDisadvantages = new Dictionary<UnitType, WeatherType>
    {
        { UnitType.Archer, WeatherType.Rain },
        { UnitType.Knight, WeatherType.Fog },
        { UnitType.Soldier, WeatherType.Storm }
    };


    /// <summary>
    /// Coroutine to simulate the battle in stages.
    /// </summary>
    /// <param name="army1">First army.</param>
    /// <param name="army2">Second army.</param>
    /// <param name="onUpdate">Action to update the UI after each stage.</param>
    /// <param name="onComplete">Action to call when the battle is complete.</param>
    public IEnumerator SimulateBattleInStages(Army army1, Army army2, System.Action<BattleResult> onUpdate, System.Action<BattleResult> onComplete)
    {
        BattleResult result = new BattleResult();
        // Initialize result with initial values...

        while (army1.GetTotalUnits() > 0 && army2.GetTotalUnits() > 0)
        {
            // Simulate a single stage of the battle...
            SimulateBattleStage(army1, army2, result);

            // Update the total unit counts for each army
            result.TotalUnitsArmy1 = army1.GetTotalUnits();
            result.TotalUnitsArmy2 = army2.GetTotalUnits();

            // Call the onUpdate action to update the UI
            onUpdate(result);

            // Wait for a short duration before the next stage
            yield return new WaitForSeconds(1.0f);
        }

        // Determine the winner and finalize the result...
        DetermineWinner(army1, army2, result);
        onComplete(result);
    }

    private void SimulateBattleStage(Army army1, Army army2, BattleResult result)
    {
        // Simulate a single stage of the battle...
        // Update the result with the current state...
        foreach (var unit1 in army1.Units)
        {
            foreach (var unit2 in army2.Units)
            {
                if (StrongAgainst.TryGetValue(unit1.Type, out UnitType strongAgainstType) && strongAgainstType == unit2.Type)
                 {
                    // Unit1 has an advantage over Unit2
                    int casualties = Mathf.Min(UnityEngine.Random.Range(1, unit2.Count / 10), unit2.Count);
                    unit2.Count -= casualties;
                    if (!result.LoserCasualties.ContainsKey(unit2.Type))
                    {
                        result.LoserCasualties[unit2.Type] = 0;
                    }
                    result.LoserCasualties[unit2.Type] += casualties;
                }
                else if (StrongAgainst.TryGetValue(unit2.Type, out UnitType strongAgainstType2) && strongAgainstType2 == unit1.Type)
                {
                    // Unit2 has an advantage over Unit1
                    int casualties = Mathf.Min(UnityEngine.Random.Range(1, unit1.Count / 10), unit1.Count);
                    unit1.Count -= casualties;
                    if (!result.WinnerCasualties.ContainsKey(unit1.Type))
                    {
                        result.WinnerCasualties[unit1.Type] = 0;
                    }
                    result.WinnerCasualties[unit1.Type] += casualties;
                }
                else
                {
                    // No advantage, both units take casualties
                    int casualties1 = Mathf.Min(UnityEngine.Random.Range(1, unit1.Count / 20), unit1.Count);
                    int casualties2 = Mathf.Min(UnityEngine.Random.Range(1, unit2.Count / 20), unit2.Count);
                    unit1.Count -= casualties1;
                    unit2.Count -= casualties2;
                    if (!result.WinnerCasualties.ContainsKey(unit1.Type))
                    {
                        result.WinnerCasualties[unit1.Type] = 0;
                    }
                    if (!result.LoserCasualties.ContainsKey(unit2.Type))
                    {
                        result.LoserCasualties[unit2.Type] = 0;
                    }
                    result.WinnerCasualties[unit1.Type] += casualties1;
                    result.LoserCasualties[unit2.Type] += casualties2;
                }
            }
        }

        // Add a battle event to the result
        result.BattleEvents.Add($"Battle stage: {army1.GetTotalUnits()} vs {army2.GetTotalUnits()}");
    }

    private void DetermineWinner(Army army1, Army army2, BattleResult result)
    {
        if (army1.GetTotalUnits() > army2.GetTotalUnits())
        {
            result.Winner = army1;
            result.Loser = army2;
            result.ResultMessage = "You won the battle!";
        }
        else
        {
            result.Winner = army2;
            result.Loser = army1;
            result.ResultMessage = "You lost the battle!";
        }
    }

    /// <summary>
    /// Handles retreating from the battle.
    /// </summary>
    /// <param name="army">The army that is retreating.</param>
    /// <returns>Casualties suffered during the retreat.</returns>
    public Dictionary<UnitType, int> HandleRetreat(Army army)
    {
        Dictionary<UnitType, int> retreatCasualties = new Dictionary<UnitType, int>();

        foreach (var unit in army.Units)
        {
            int casualties = UnityEngine.Random.Range(0, unit.Count / 2); // Randomly lose up to half of each unit type
            unit.Count -= casualties;
            retreatCasualties[unit.Type] = casualties;
        }

        return retreatCasualties;
    }
}



public enum TerrainType { Plains, Forest, Mountains, Hills }
public enum WeatherType { Clear, Rain, Fog, Storm }


public class BattleResult
{
    public Army Winner { get; set; }
    public Army Loser { get; set; }
    public string ResultMessage { get; set; }
    public Dictionary<UnitType, int> WinnerCasualties { get; private set; }
    public Dictionary<UnitType, int> LoserCasualties { get; private set; }
    public List<string> BattleEvents { get; private set; }
    public TerrainType Terrain { get; private set; }
    public WeatherType Weather { get; private set; }
    public int TotalUnitsArmy1 { get; set; }
    public int TotalUnitsArmy2 { get; set; }

    public BattleResult(Army winner, Army loser, string message,
                       Dictionary<UnitType, int> winnerCasualties,
                       Dictionary<UnitType, int> loserCasualties,
                       List<string> battleEvents,
                       TerrainType terrain,
                       WeatherType weather)
    {
        Winner = winner;
        Loser = loser;
        ResultMessage = message;
        WinnerCasualties = winnerCasualties;
        LoserCasualties = loserCasualties;
        BattleEvents = battleEvents;
        Terrain = terrain;
        Weather = weather;
    }

    public BattleResult()
    {
        WinnerCasualties = new Dictionary<UnitType, int>();
        LoserCasualties = new Dictionary<UnitType, int>();
        BattleEvents = new List<string>();
    }


}
