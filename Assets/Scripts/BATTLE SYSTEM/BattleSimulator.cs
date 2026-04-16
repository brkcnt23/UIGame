using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleSimulator
{
    private static readonly Dictionary<UnitType, UnitType> StrongAgainst = new Dictionary<UnitType, UnitType>
    {
        { UnitType.Knight, UnitType.Soldier },
        { UnitType.Soldier, UnitType.Shielder },
        { UnitType.Shielder, UnitType.Archer },
        { UnitType.Archer, UnitType.Pikeman },
        { UnitType.Pikeman, UnitType.Knight }
    };

    private static readonly Dictionary<UnitType, TerrainType> TerrainAdvantages = new Dictionary<UnitType, TerrainType>
    {
        { UnitType.Archer, TerrainType.Hills },
        { UnitType.Knight, TerrainType.Plains },
        { UnitType.Soldier, TerrainType.Forest },
        { UnitType.Pikeman, TerrainType.Mountains }
    };

    private static readonly Dictionary<UnitType, WeatherType> WeatherDisadvantages = new Dictionary<UnitType, WeatherType>
    {
        { UnitType.Archer, WeatherType.Rain },
        { UnitType.Knight, WeatherType.Fog },
        { UnitType.Soldier, WeatherType.Storm }
    };

    // -----------------------------
    // POWER
    // -----------------------------

    public float CalculateArmyPower(Army ownArmy, Army enemyArmy, TerrainType terrain, WeatherType weather)
    {
        if (ownArmy == null || ownArmy.Units == null)
            return 0f;

        float power = 0f;

        foreach (var unit in ownArmy.Units)
        {
            if (unit == null || unit.Count <= 0) continue;

            float unitPower = unit.Count;

            if (TerrainAdvantages.TryGetValue(unit.Type, out TerrainType advantageTerrain) && advantageTerrain == terrain)
            {
                unitPower *= 1.2f;
            }

            if (WeatherDisadvantages.TryGetValue(unit.Type, out WeatherType disadvantageWeather) && disadvantageWeather == weather)
            {
                unitPower *= 0.8f;
            }

            if (enemyArmy != null && enemyArmy.Units != null &&
                StrongAgainst.TryGetValue(unit.Type, out UnitType strongAgainstType))
            {
                foreach (var enemyUnit in enemyArmy.Units)
                {
                    if (enemyUnit != null && enemyUnit.Type == strongAgainstType)
                    {
                        unitPower += enemyUnit.Count * 0.5f;
                    }
                }
            }

            float moraleModifier = CalculateMoraleModifier(ownArmy);
            unitPower *= moraleModifier;

            power += unitPower;
        }

        return power;
    }

    private float CalculateMoraleModifier(Army army)
    {
        if (PlayerStatHandler.Instance == null || PlayerStatHandler.Instance.pd == null)
            return 1.0f;

        float morale = 1.0f;

        int recentVictories = PlayerStatHandler.Instance.pd.TotalBattlesWon;
        morale += recentVictories * 0.05f;

        float maxHealth = Mathf.Max(1, PlayerStatHandler.Instance.pd.MaxHealth);
        float healthPercentage = PlayerStatHandler.Instance.pd.Health / maxHealth;

        if (healthPercentage < 0.5f)
        {
            morale *= healthPercentage + 0.5f;
        }

        int exhaustion = PlayerStatHandler.Instance.pd.CurrentExhaustionLevel;
        morale *= (1.0f - (exhaustion * 0.1f));

        return Mathf.Clamp(morale, 0.5f, 2.0f);
    }

    // -----------------------------
    // SINGLE SIMULATION
    // -----------------------------

    public BattleResult SimulateBattle(Army army1, Army army2, TerrainType terrain = TerrainType.Plains, WeatherType weather = WeatherType.Clear)
    {
        float army1Power = CalculateArmyPower(army1, army2, terrain, weather);
        float army2Power = CalculateArmyPower(army2, army1, terrain, weather);

        float totalPower = Mathf.Max(1f, army1Power + army2Power);
        float army1WinProbability = army1Power / totalPower;

        float randomValue = UnityEngine.Random.Range(0f, 1f);

        bool army1Wins = randomValue < army1WinProbability;
        Army winner = army1Wins ? army1 : army2;
        Army loser = army1Wins ? army2 : army1;

        Dictionary<UnitType, int> winnerCasualties = CalculateCasualties(winner, true);
        Dictionary<UnitType, int> loserCasualties = CalculateCasualties(loser, false);

        ApplyCasualties(winner, winnerCasualties);
        ApplyCasualties(loser, loserCasualties);

        string resultMessage = army1Wins ? "Army 1 wins!" : "Army 2 wins!";
        List<string> battleEvents = GenerateBattleEvents(army1, army2, terrain, weather);

        return new BattleResult(winner, loser, resultMessage, winnerCasualties, loserCasualties, battleEvents, terrain, weather)
        {
            TotalUnitsArmy1 = army1 != null ? army1.GetTotalUnits() : 0,
            TotalUnitsArmy2 = army2 != null ? army2.GetTotalUnits() : 0
        };
    }

    private Dictionary<UnitType, int> CalculateCasualties(Army army, bool isWinner)
    {
        Dictionary<UnitType, int> casualties = new Dictionary<UnitType, int>();

        if (army == null || army.Units == null)
            return casualties;

        foreach (var unit in army.Units)
        {
            if (unit == null || unit.Count <= 0) continue;

            float lossPercentage = isWinner ? 0.05f : 0.90f;
            int lossCount = Mathf.CeilToInt(unit.Count * lossPercentage);
            lossCount = Mathf.Min(lossCount, unit.Count);

            casualties[unit.Type] = lossCount;
        }

        return casualties;
    }

    private void ApplyCasualties(Army army, Dictionary<UnitType, int> casualties)
    {
        if (army == null || casualties == null) return;

        foreach (var casualty in casualties)
        {
            army.RemoveUnit(casualty.Key, casualty.Value);
        }
    }

    // -----------------------------
    // ENEMY GENERATION
    // -----------------------------

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
                enemyArmy.AddUnit(new Unit() { Type = unitTypes[i], Count = remainingUnits });
            }
            else
            {
                int maxUnitsForThisType = Mathf.Max(1, remainingUnits - (unitTypesCount - i - 1));
                int unitsForThisType = UnityEngine.Random.Range(1, maxUnitsForThisType + 1);

                enemyArmy.AddUnit(new Unit() { Type = unitTypes[i], Count = unitsForThisType });
                remainingUnits -= unitsForThisType;
            }
        }

        return enemyArmy;
    }

    // -----------------------------
    // STAGED BATTLE
    // -----------------------------

    public IEnumerator SimulateBattleInStages(
        Army army1,
        Army army2,
        Action<BattleResult> onUpdate,
        Action<BattleResult> onComplete,
        TerrainType terrain,
        WeatherType weather)
    {
        BattleResult result = new BattleResult
        {
            Terrain = terrain,
            Weather = weather,
            ResultMessage = $"Battle begins in {terrain} terrain under {weather} conditions."
        };

        result.BattleEvents.Add(result.ResultMessage);

        while (army1.GetTotalUnits() > 0 && army2.GetTotalUnits() > 0)
        {
            SimulateBattleStage(army1, army2, result, terrain, weather);

            result.TotalUnitsArmy1 = army1.GetTotalUnits();
            result.TotalUnitsArmy2 = army2.GetTotalUnits();

            onUpdate?.Invoke(result);

            yield return new WaitForSeconds(1.0f);
        }

        DetermineWinner(army1, army2, result);
        onComplete?.Invoke(result);
    }

    private void SimulateBattleStage(Army army1, Army army2, BattleResult result, TerrainType terrain, WeatherType weather)
    {
        if (army1 == null || army2 == null) return;

        List<Unit> army1Units = army1.Units.Where(u => u != null && u.Count > 0).ToList();
        List<Unit> army2Units = army2.Units.Where(u => u != null && u.Count > 0).ToList();

        foreach (var unit1 in army1Units)
        {
            foreach (var unit2 in army2Units)
            {
                if (unit1.Count <= 0 || unit2.Count <= 0) continue;

                int casualtiesToArmy2;
                int casualtiesToArmy1;

                float attackBonus1 = GetCombatModifier(unit1.Type, terrain, weather);
                float attackBonus2 = GetCombatModifier(unit2.Type, terrain, weather);

                if (StrongAgainst.TryGetValue(unit1.Type, out UnitType strongAgainstType1) && strongAgainstType1 == unit2.Type)
                {
                    casualtiesToArmy2 = Mathf.Min(
                        Mathf.Max(1, Mathf.RoundToInt(UnityEngine.Random.Range(1, Mathf.Max(2, unit2.Count / 10f)) * attackBonus1)),
                        unit2.Count
                    );

                    unit2.Count -= casualtiesToArmy2;
                    AddCasualty(result.LoserCasualties, unit2.Type, casualtiesToArmy2);
                }
                else if (StrongAgainst.TryGetValue(unit2.Type, out UnitType strongAgainstType2) && strongAgainstType2 == unit1.Type)
                {
                    casualtiesToArmy1 = Mathf.Min(
                        Mathf.Max(1, Mathf.RoundToInt(UnityEngine.Random.Range(1, Mathf.Max(2, unit1.Count / 10f)) * attackBonus2)),
                        unit1.Count
                    );

                    unit1.Count -= casualtiesToArmy1;
                    AddCasualty(result.WinnerCasualties, unit1.Type, casualtiesToArmy1);
                }
                else
                {
                    casualtiesToArmy1 = Mathf.Min(
                        Mathf.Max(1, Mathf.RoundToInt(UnityEngine.Random.Range(1, Mathf.Max(2, unit1.Count / 20f)) * attackBonus2)),
                        unit1.Count
                    );

                    casualtiesToArmy2 = Mathf.Min(
                        Mathf.Max(1, Mathf.RoundToInt(UnityEngine.Random.Range(1, Mathf.Max(2, unit2.Count / 20f)) * attackBonus1)),
                        unit2.Count
                    );

                    unit1.Count -= casualtiesToArmy1;
                    unit2.Count -= casualtiesToArmy2;

                    AddCasualty(result.WinnerCasualties, unit1.Type, casualtiesToArmy1);
                    AddCasualty(result.LoserCasualties, unit2.Type, casualtiesToArmy2);
                }
            }
        }

        army1.Units.RemoveAll(u => u == null || u.Count <= 0);
        army2.Units.RemoveAll(u => u == null || u.Count <= 0);

        result.BattleEvents.Add($"Battle stage: {army1.GetTotalUnits()} vs {army2.GetTotalUnits()}");
    }

    private float GetCombatModifier(UnitType unitType, TerrainType terrain, WeatherType weather)
    {
        float modifier = 1f;

        if (TerrainAdvantages.TryGetValue(unitType, out TerrainType advantageTerrain) && advantageTerrain == terrain)
        {
            modifier += 0.2f;
        }

        if (WeatherDisadvantages.TryGetValue(unitType, out WeatherType disadvantageWeather) && disadvantageWeather == weather)
        {
            modifier -= 0.2f;
        }

        return Mathf.Clamp(modifier, 0.5f, 2f);
    }

    private void AddCasualty(Dictionary<UnitType, int> dict, UnitType type, int value)
    {
        if (!dict.ContainsKey(type))
            dict[type] = 0;

        dict[type] += value;
    }

    private void DetermineWinner(Army army1, Army army2, BattleResult result)
    {
        if (army1.GetTotalUnits() > army2.GetTotalUnits())
        {
            result.Player = army1;
            result.Enemy = army2;
            result.ResultMessage = "You won the battle!";
        }
        else
        {
            result.Player = army2;
            result.Enemy = army1;
            result.ResultMessage = "You lost the battle!";
        }
    }

    // -----------------------------
    // RETREAT
    // -----------------------------

    public Dictionary<UnitType, int> HandleRetreat(Army army)
    {
        Dictionary<UnitType, int> retreatCasualties = new Dictionary<UnitType, int>();

        if (army == null || army.Units == null)
            return retreatCasualties;

        foreach (var unit in army.Units)
        {
            if (unit == null || unit.Count <= 0) continue;

            int casualties = UnityEngine.Random.Range(0, Mathf.Max(1, unit.Count / 2));
            unit.Count -= casualties;
            retreatCasualties[unit.Type] = casualties;
        }

        army.Units.RemoveAll(u => u == null || u.Count <= 0);

        return retreatCasualties;
    }

    // -----------------------------
    // EVENTS / LOG
    // -----------------------------

    public void ResolveBattle(float difficultyFactor)
    {
        int baseExpGain = Mathf.RoundToInt(difficultyFactor * 100);

        if (PlayerStatHandler.Instance != null)
        {
            PlayerStatHandler.Instance.AddCharacterExperience(baseExpGain);
            Debug.Log($"Battle completed. Character gained {baseExpGain} EXP.");
        }
    }

    private List<string> GenerateBattleEvents(Army army1, Army army2, TerrainType terrain, WeatherType weather)
    {
        List<string> events = new List<string>
        {
            $"Battle begins in {terrain} terrain under {weather} conditions."
        };

        if (army1 != null)
        {
            foreach (var unit in army1.Units)
            {
                if (unit == null) continue;

                if (TerrainAdvantages.TryGetValue(unit.Type, out TerrainType advantageTerrain) && advantageTerrain == terrain)
                {
                    events.Add($"{unit.Type}s take advantage of the {terrain} terrain!");
                }
            }
        }

        IEnumerable<Unit> allUnits = Enumerable.Empty<Unit>();

        if (army1 != null && army1.Units != null)
            allUnits = allUnits.Concat(army1.Units);

        if (army2 != null && army2.Units != null)
            allUnits = allUnits.Concat(army2.Units);

        foreach (var unit in allUnits)
        {
            if (unit == null) continue;

            if (WeatherDisadvantages.TryGetValue(unit.Type, out WeatherType disadvantageWeather) && disadvantageWeather == weather)
            {
                events.Add($"{unit.Type}s struggle in the {weather} conditions!");
            }
        }

        return events;
    }
}

public enum TerrainType { Plains, Forest, Mountains, Hills }
public enum WeatherType { Clear, Rain, Fog, Storm }

public class BattleResult
{
    public Army Player { get; set; }
    public Army Enemy { get; set; }
    public string ResultMessage { get; set; }

    public Dictionary<UnitType, int> WinnerCasualties { get; private set; }
    public Dictionary<UnitType, int> LoserCasualties { get; private set; }
    public List<string> BattleEvents { get; private set; }

    public TerrainType Terrain { get; set; }
    public WeatherType Weather { get; set; }

    public int TotalUnitsArmy1 { get; set; }
    public int TotalUnitsArmy2 { get; set; }

    public BattleResult(Army player, Army enemy, string message,
        Dictionary<UnitType, int> winnerCasualties,
        Dictionary<UnitType, int> loserCasualties,
        List<string> battleEvents,
        TerrainType terrain,
        WeatherType weather)
    {
        Player = player;
        Enemy = enemy;
        ResultMessage = message;
        WinnerCasualties = winnerCasualties ?? new Dictionary<UnitType, int>();
        LoserCasualties = loserCasualties ?? new Dictionary<UnitType, int>();
        BattleEvents = battleEvents ?? new List<string>();
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