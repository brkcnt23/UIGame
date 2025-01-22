using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class BattleManager : MonoBehaviour
{
    [Header("Battle Settings")]
    public int enemyTotalUnits = 500; // Total units in the randomly generated enemy army

    [Header("Battle Simulator")]
    private BattleSimulator simulator;

    [Header("Pre-Battle UI Elements")]
    public TMP_Text terrainText; // Text to display terrain information
    public TMP_Text weatherText; // Text to display weather information
    public TMP_Text playerArmyInfoText; // Text to display player army information
    public TMP_Text enemyArmyInfoText; // Text to display enemy army information
    public TMP_Text winProbabilityText; // Text to display win probability
    public Button negotiateButton; // Button to negotiate before the battle
    public Button preBattleRetreatButton; // Button to retreat before the battle


    [Header("UI Elements")]
    public GameObject battleSimulationPanel; // Reference to the Battle Simulation Panel
    public Button startBattleButton; // Button to start the battle
    public Button continueBattleButton; // Button to continue the battle
    public Button retreatButton; // Button to retreat from the battle
    public TMP_Text battleResultText; // Text to display the battle result
    public TMP_Text PlayerbattleCasualtiesText; // Text to display the Player's battle casualties
    public TMP_Text EnemybattleCasualtiesText; // Text to display the Enemy's battle casualties
    public Transform battleEventsContainer; // Container to display battle events
    public GameObject battleEventPrefab; // Prefab for battle event entries

    private Coroutine battleCoroutine; // Coroutine to simulate the battle

    private void Start()
    {
        simulator = new BattleSimulator();
        startBattleButton.onClick.AddListener(StartBattle);
        continueBattleButton.onClick.AddListener(ContinueBattle);
        retreatButton.onClick.AddListener(Retreat);
        negotiateButton.onClick.AddListener(Negotiate);
        preBattleRetreatButton.onClick.AddListener(PreBattleRetreat);
    }

    public void OpenPanelAndDisplayPreBattleInfo()
    {
        // Generate and display pre-battle information
        DisplayPreBattleInfo();
    }

    /// <summary>
    /// Generates and displays pre-battle information.
    /// </summary>
    private void DisplayPreBattleInfo()
    {
        battleSimulationPanel.SetActive(true);

        terrainType = (TerrainType)Random.Range(0, System.Enum.GetValues(typeof(TerrainType)).Length);
        weatherType = (WeatherType)Random.Range(0, System.Enum.GetValues(typeof(WeatherType)).Length);
        // Generate terrain and weather
        TerrainType terrain = terrainType;
        WeatherType weather = weatherType;

        // Get the player's army from PlayerStatHandler
        Army playerArmy = PlayerStatHandler.Instance.pd.PlayerArmy;

        // Generate a random enemy army
        Army enemyArmy = simulator.GenerateRandomEnemyArmy(enemyTotalUnits);

        // Calculate win probability (simplified example)
        float playerPower = simulator.CalculateArmyPower(playerArmy, enemyArmy, terrain, weather);
        float enemyPower = simulator.CalculateArmyPower(enemyArmy, playerArmy, terrain, weather);
        float winProbability = playerPower / (playerPower + enemyPower) * 100;

        // Update the UI with pre-battle information
        terrainText.text = $"Terrain: {terrain}";
        weatherText.text = $"Weather: {weather}";
        playerArmyInfoText.text = $"Player Army: {playerArmy.GetTotalUnits()} units";
        enemyArmyInfoText.text = $"Enemy Army: {enemyArmy.GetTotalUnits()} units";
        winProbabilityText.text = $"Win Probability: {winProbability:F2}%";
    }

    private TerrainType terrainType;
    private WeatherType weatherType;

    public void SetTerrainType(int terrainTypeIndex)
    {
        terrainType = (TerrainType)terrainTypeIndex;
    }

    public void SetWeatherType(int weatherTypeIndex)
    {
        weatherType = (WeatherType)weatherTypeIndex;
    }

    public TerrainType GetTerrainType()
    {
        return terrainType;
    }

    public WeatherType GetWeatherType()
    {
        return weatherType;
    }

    /// <summary>
    /// Starts the battle when called from the UI.
    /// </summary>
    public void StartBattle()
    {
        // Get the player's army from PlayerStatHandler
        Army playerArmy = PlayerStatHandler.Instance.pd.PlayerArmy;

        // Generate a random enemy army
        Army enemyArmy = simulator.GenerateRandomEnemyArmy(enemyTotalUnits);

        // Start the battle simulation
        battleCoroutine = StartCoroutine(simulator.SimulateBattleInStages(playerArmy, enemyArmy, UpdateBattleUI, CompleteBattle));
    }

    /// <summary>
    /// Continues the battle when called from the UI.
    /// </summary>
    public void ContinueBattle()
    {
        // Resume the battle simulation...
    }

    /// <summary>
    /// Updates the battle UI after each stage.
    /// </summary>
    /// <param name="result">The current result of the battle.</param>
    private void UpdateBattleUI(BattleResult result)
    {
        // Update the UI with the current battle result...
        battleResultText.text = result.ResultMessage + "\n";

        // Display total units remaining
        string totalUnitsMessage = $"Total Units - Player: {result.TotalUnitsArmy1}, Enemy: {result.TotalUnitsArmy2}\n";
        battleResultText.text += totalUnitsMessage;

        // Display casualties
        string playerCasualtiesMessage = "Player Casualties:\n";
        string enemyCasualtiesMessage = "Enemy Casualties:\n";
        foreach (var casualty in result.WinnerCasualties)
        {
            playerCasualtiesMessage += $"{casualty.Key}: {casualty.Value}\n";
        }
        foreach (var casualty in result.LoserCasualties)
        {
            enemyCasualtiesMessage += $"{casualty.Key}: {casualty.Value}\n";
        }

        PlayerbattleCasualtiesText.text = playerCasualtiesMessage;
        EnemybattleCasualtiesText.text = enemyCasualtiesMessage;

        // Display battle events
        foreach (Transform child in battleEventsContainer)
        {
            Destroy(child.gameObject);
        }
        foreach (var battleEvent in result.BattleEvents)
        {
            GameObject eventEntry = Instantiate(battleEventPrefab, battleEventsContainer);
            eventEntry.GetComponent<TMP_Text>().text = battleEvent;
        }
    }

    private void CompleteBattle(BattleResult result)
    {
        // Update the UI with the final battle result...
        battleResultText.text = result.ResultMessage + "\n";

        // Display total units remaining
        string totalUnitsMessage = $"Total Units - Player: {result.TotalUnitsArmy1}, Enemy: {result.TotalUnitsArmy2}\n";
        battleResultText.text += totalUnitsMessage;

        // Display casualties
        // Display casualties
        string playerCasualtiesMessage = "Player Casualties:\n";
        string enemyCasualtiesMessage = "Enemy Casualties:\n";
        foreach (var casualty in result.WinnerCasualties)
        {
            playerCasualtiesMessage += $"{casualty.Key}: {casualty.Value}\n";
        }
        foreach (var casualty in result.LoserCasualties)
        {
            enemyCasualtiesMessage += $"{casualty.Key}: {casualty.Value}\n";
        }

        PlayerbattleCasualtiesText.text = playerCasualtiesMessage;
        EnemybattleCasualtiesText.text = enemyCasualtiesMessage;

        // Display battle events
        foreach (Transform child in battleEventsContainer)
        {
            Destroy(child.gameObject);
        }
        foreach (var battleEvent in result.BattleEvents)
        {
            GameObject eventEntry = Instantiate(battleEventPrefab, battleEventsContainer);
            eventEntry.GetComponent<TMP_Text>().text = battleEvent;
        }
    }

    /// <summary>
    /// Allows the player to retreat from the battle.
    /// </summary>
    public void Retreat()
    {
        if (battleCoroutine != null)
        {
            StopCoroutine(battleCoroutine);

            // Get the player's army from PlayerStatHandler
            Army playerArmy = PlayerStatHandler.Instance.pd.PlayerArmy;

            // Handle retreat and get casualties
            Dictionary<UnitType, int> retreatCasualties = simulator.HandleRetreat(playerArmy);

            // Update the UI with retreat information
            string retreatMessage = "Retreat successful! Casualties:\n";
            foreach (var casualty in retreatCasualties)
            {
                retreatMessage += $"{casualty.Key}: {casualty.Value}\n";
            }
            battleResultText.text = retreatMessage;
        }
    }

    /// <summary>
    /// Allows the player to negotiate before the battle.
    /// </summary>
    public void Negotiate()
    {
        // Calculate negotiation success based on a stat (e.g., charisma)
        float diplomacy = PlayerStatHandler.Instance.pd.Charisma;
        float successChance = diplomacy / 100.0f; // Simplified example

        if (Random.value <= successChance)
        {
            // Negotiation successful
            battleResultText.text = "Negotiation successful! The battle is avoided.";
        }
        else
        {
            // Negotiation failed
            battleResultText.text = "Negotiation failed! Prepare for battle.";
        }
    }

    /// <summary>
    /// Allows the player to retreat before the battle.
    /// </summary>
    public void PreBattleRetreat()
    {
        // Calculate retreat success based on a stat (e.g., dexterity)
        float agility = PlayerStatHandler.Instance.pd.Dexterity;
        float successChance = agility / 100.0f; // Simplified example

        if (Random.value <= successChance)
        {
            // Retreat successful
            battleResultText.text = "Retreat successful! You avoided the battle.";
        }
        else
        {
            // Retreat failed, apply random casualties
            Army playerArmy = PlayerStatHandler.Instance.pd.PlayerArmy;
            Dictionary<UnitType, int> retreatCasualties = simulator.HandleRetreat(playerArmy);

            // Update the UI with retreat information
            string retreatMessage = "Retreat failed! Casualties:\n";
            foreach (var casualty in retreatCasualties)
            {
                retreatMessage += $"{casualty.Key}: {casualty.Value}\n";
            }
            battleResultText.text = retreatMessage;
        }
    }

    /// <summary>
    /// Displays the battle result in the UI.
    /// </summary>
    /// <param name="result">The result of the battle.</param>
    private void DisplayBattleResult(BattleResult result)
    {
        battleResultText.text = result.ResultMessage;

        // Display casualties
        // Display casualties
        string playerCasualtiesMessage = "Player Casualties:\n";
        string enemyCasualtiesMessage = "Enemy Casualties:\n";
        foreach (var casualty in result.WinnerCasualties)
        {
            playerCasualtiesMessage += $"{casualty.Key}: {casualty.Value}\n";
        }
        foreach (var casualty in result.LoserCasualties)
        {
            enemyCasualtiesMessage += $"{casualty.Key}: {casualty.Value}\n";
        }

        PlayerbattleCasualtiesText.text = playerCasualtiesMessage;
        EnemybattleCasualtiesText.text = enemyCasualtiesMessage;

        // Display battle events
        foreach (Transform child in battleEventsContainer)
        {
            Destroy(child.gameObject);
        }
        foreach (var battleEvent in result.BattleEvents)
        {
            GameObject eventEntry = Instantiate(battleEventPrefab, battleEventsContainer);
            eventEntry.GetComponent<TMP_Text>().text = battleEvent;
        }
    }


}