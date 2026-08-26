using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleSimulator
{
    /// <summary>
    /// Who beats whom, by role rather than by unit.
    ///
    ///   Pike beats Horse - a hedge of points stops a charge
    ///   Horse beats Archer - bowmen caught in the open are ridden down
    ///   Archer beats Foot - shot at before they arrive
    ///   Foot beats Pike - once past the points, a pike is a stick
    ///
    /// Keeping the cycle on four roles rather than ten unit types means a new
    /// portrait joins the fight correctly the moment it has a role, instead of
    /// quietly fighting nobody because somebody forgot a matrix entry. Five of
    /// the ten troops in this game did exactly that.
    /// </summary>
    private static readonly Dictionary<UnitRole, UnitRole> RoleBeats = new Dictionary<UnitRole, UnitRole>
    {
        { UnitRole.Pike,   UnitRole.Horse  },
        { UnitRole.Horse,  UnitRole.Archer },
        { UnitRole.Archer, UnitRole.Foot   },
        { UnitRole.Foot,   UnitRole.Pike   }
    };

    private static readonly Dictionary<UnitRole, TerrainType> TerrainAdvantages = new Dictionary<UnitRole, TerrainType>
    {
        { UnitRole.Archer, TerrainType.Hills },
        { UnitRole.Horse,  TerrainType.Plains },
        { UnitRole.Foot,   TerrainType.Forest },
        { UnitRole.Pike,   TerrainType.Mountains }
    };

    private static readonly Dictionary<UnitRole, WeatherType> WeatherDisadvantages = new Dictionary<UnitRole, WeatherType>
    {
        { UnitRole.Archer, WeatherType.Rain },   // wet strings
        { UnitRole.Horse,  WeatherType.Fog },    // you cannot charge what you cannot see
        { UnitRole.Foot,   WeatherType.Storm }
    };

    /// <summary>What this troop does on a field. Unknown types fight as foot.</summary>
    private static UnitRole RoleOf(UnitType type)
        => UnitCatalog.Get(type)?.Role ?? UnitRole.Foot;

    // -----------------------------
    // POWER
    // -----------------------------

    public float CalculateArmyPower(Army ownArmy, Army enemyArmy, TerrainType terrain, WeatherType weather)
    {
        if (ownArmy == null || ownArmy.Units == null)
            return 0f;

        // Morale is a property of the army, not of each stack in it. It used to
        // be recomputed inside the loop, which cost nothing but said the wrong
        // thing about what it measures.
        float morale = CalculateMoraleModifier(ownArmy);

        float power = 0f;

        foreach (var unit in ownArmy.Units)
        {
            if (unit == null || unit.Count <= 0) continue;

            var def = UnitCatalog.Get(unit.Type);

            // Headcount alone made a knight the equal of a levy, so five hundred
            // gold of cavalry fought exactly like ten gold of farmhands.
            float unitPower = unit.Count * (def?.CombatValue ?? 1f);

            UnitRole role = def?.Role ?? UnitRole.Foot;

            if (TerrainAdvantages.TryGetValue(role, out TerrainType goodGround) && goodGround == terrain)
                unitPower *= 1.2f;

            if (WeatherDisadvantages.TryGetValue(role, out WeatherType badWeather) && badWeather == weather)
                unitPower *= 0.8f;

            if (enemyArmy?.Units != null && RoleBeats.TryGetValue(role, out UnitRole prey))
            {
                foreach (var enemyUnit in enemyArmy.Units)
                {
                    if (enemyUnit == null || enemyUnit.Count <= 0) continue;

                    var enemyDef = UnitCatalog.Get(enemyUnit.Type);

                    if (enemyDef != null && enemyDef.Role == prey)
                        unitPower += enemyUnit.Count * enemyDef.CombatValue * 0.25f;
                }
            }

            power += unitPower * morale;
        }

        return power;
    }

    private float CalculateMoraleModifier(Army army)
    {
        if (PlayerStatHandler.Instance == null || PlayerStatHandler.Instance.pd == null)
            return 1.0f;

        var pd = PlayerStatHandler.Instance.pd;

        // Reputation for winning, read as a record rather than a running total.
        // The old line added five percent per lifetime victory, so a commander
        // twenty battles in sat permanently at the cap and nothing they did
        // afterwards could move it - including losing.
        int fought = pd.TotalBattlesWon + pd.TotalBattlesLost;
        float record = fought > 0 ? pd.TotalBattlesWon / (float)fought : 0.5f;

        float morale = 1.0f + (record - 0.5f) * 0.4f;

        float maxHealth = Mathf.Max(1, pd.MaxHealth);
        float healthPercentage = pd.Health / maxHealth;

        if (healthPercentage < 0.5f)
            morale *= healthPercentage + 0.5f;

        morale *= 1.0f - pd.CurrentExhaustionLevel * 0.1f;

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

                if (RoleBeats.TryGetValue(RoleOf(unit1.Type), out UnitRole prey1) && prey1 == RoleOf(unit2.Type))
                {
                    casualtiesToArmy2 = Mathf.Min(
                        Mathf.Max(1, Mathf.RoundToInt(UnityEngine.Random.Range(1, Mathf.Max(2, unit2.Count / 10f)) * attackBonus1)),
                        unit2.Count
                    );

                    unit2.Count -= casualtiesToArmy2;
                    AddCasualty(result.LoserCasualties, unit2.Type, casualtiesToArmy2);
                }
                else if (RoleBeats.TryGetValue(RoleOf(unit2.Type), out UnitRole prey2) && prey2 == RoleOf(unit1.Type))
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

        UnitRole role = RoleOf(unitType);

        if (TerrainAdvantages.TryGetValue(role, out TerrainType advantageTerrain) && advantageTerrain == terrain)
        {
            modifier += 0.2f;
        }

        if (WeatherDisadvantages.TryGetValue(role, out WeatherType disadvantageWeather) && disadvantageWeather == weather)
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

                if (TerrainAdvantages.TryGetValue(RoleOf(unit.Type), out TerrainType advantageTerrain) && advantageTerrain == terrain)
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

            if (WeatherDisadvantages.TryGetValue(RoleOf(unit.Type), out WeatherType disadvantageWeather) && disadvantageWeather == weather)
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