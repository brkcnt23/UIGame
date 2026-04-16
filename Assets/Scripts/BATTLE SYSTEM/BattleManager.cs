using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class BattleManager : MonoBehaviour
{
    [Header("Battle Settings")]
    public int enemyTotalUnits = 500;

    [Header("Battle Simulator")]
    private BattleSimulator simulator;

    [Header("Pre-Battle UI Elements")]
    public TMP_Text terrainText;
    public TMP_Text weatherText;
    public TMP_Text playerArmyInfoText;
    public TMP_Text enemyArmyInfoText;
    public TMP_Text winProbabilityText;
    public Button negotiateButton;
    public Button preBattleRetreatButton;

    [Header("UI Elements")]
    public GameObject battleSimulationPanel;
    public Button startBattleButton;
    public Button continueBattleButton;
    public Button retreatButton;
    public TMP_Text battleResultText;
    public TMP_Text PlayerbattleCasualtiesText;
    public TMP_Text EnemybattleCasualtiesText;
    public Transform battleEventsContainer;
    public GameObject battleEventPrefab;

    private Coroutine battleCoroutine;

    private Army currentPlayerArmy;
    private Army currentEnemyArmy;

    private TerrainType terrainType;
    private WeatherType weatherType;

    private void Start()
    {
        simulator = new BattleSimulator();

        if (startBattleButton != null)
        {
            startBattleButton.onClick.RemoveAllListeners();
            startBattleButton.onClick.AddListener(StartBattle);
        }

        if (continueBattleButton != null)
        {
            continueBattleButton.onClick.RemoveAllListeners();
            continueBattleButton.onClick.AddListener(ContinueBattle);
        }

        if (retreatButton != null)
        {
            retreatButton.onClick.RemoveAllListeners();
            retreatButton.onClick.AddListener(Retreat);
        }

        if (negotiateButton != null)
        {
            negotiateButton.onClick.RemoveAllListeners();
            negotiateButton.onClick.AddListener(Negotiate);
        }

        if (preBattleRetreatButton != null)
        {
            preBattleRetreatButton.onClick.RemoveAllListeners();
            preBattleRetreatButton.onClick.AddListener(PreBattleRetreat);
        }
    }

    public void OpenPanelAndDisplayPreBattleInfo()
    {
        DisplayPreBattleInfo();
    }

    private void DisplayPreBattleInfo()
    {
        if (battleSimulationPanel != null)
            battleSimulationPanel.SetActive(true);

        terrainType = (TerrainType)Random.Range(0, System.Enum.GetValues(typeof(TerrainType)).Length);
        weatherType = (WeatherType)Random.Range(0, System.Enum.GetValues(typeof(WeatherType)).Length);

        currentPlayerArmy = GetPlayerArmy();
        currentEnemyArmy = simulator.GenerateRandomEnemyArmy(enemyTotalUnits);

        if (currentPlayerArmy == null || currentEnemyArmy == null)
        {
            Debug.LogError("BattleManager: Could not prepare armies.");
            return;
        }

        float playerPower = simulator.CalculateArmyPower(currentPlayerArmy, currentEnemyArmy, terrainType, weatherType);
        float enemyPower = simulator.CalculateArmyPower(currentEnemyArmy, currentPlayerArmy, terrainType, weatherType);

        int playerUnitCount = Mathf.Max(1, currentPlayerArmy.GetTotalUnits());
        int armyNumberDifference = Mathf.Abs(playerUnitCount - currentEnemyArmy.GetTotalUnits());
        float differencePercentage = (100f * armyNumberDifference) / playerUnitCount;

        if (differencePercentage > 70)
            playerPower += 1000;
        else if (differencePercentage > 50)
            playerPower += 600;
        else if (differencePercentage > 35)
            playerPower += 450;
        else if (differencePercentage > 20)
            playerPower += 350;

        float winProbability = (playerPower + enemyPower) > 0f
            ? (playerPower / (playerPower + enemyPower)) * 100f
            : 0f;

        if (terrainText != null) terrainText.text = $"Terrain: {terrainType}";
        if (weatherText != null) weatherText.text = $"Weather: {weatherType}";
        if (playerArmyInfoText != null) playerArmyInfoText.text = $"Player Army: {currentPlayerArmy.GetTotalUnits()} units";
        if (enemyArmyInfoText != null) enemyArmyInfoText.text = $"Enemy Army: {currentEnemyArmy.GetTotalUnits()} units";
        if (winProbabilityText != null) winProbabilityText.text = $"Win Probability: {winProbability:F2}%";
    }

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

    public void StartBattle()
    {
        currentPlayerArmy = GetPlayerArmy();

        if (currentEnemyArmy == null || currentEnemyArmy.GetTotalUnits() <= 0)
        {
            currentEnemyArmy = simulator.GenerateRandomEnemyArmy(enemyTotalUnits);
        }

        if (currentPlayerArmy == null || currentEnemyArmy == null)
        {
            Debug.LogError("BattleManager: armies are null, cannot start battle.");
            return;
        }

        if (battleCoroutine != null)
        {
            StopCoroutine(battleCoroutine);
        }

        battleCoroutine = StartCoroutine(
            simulator.SimulateBattleInStages(
                currentPlayerArmy,
                currentEnemyArmy,
                UpdateBattleUI,
                CompleteBattle,
                terrainType,
                weatherType
            )
        );
    }

    public void ContinueBattle()
    {
        if (battleCoroutine == null)
        {
            StartBattle();
        }
    }

    private void UpdateBattleUI(BattleResult result)
    {
        if (result == null) return;

        if (battleResultText != null)
        {
            battleResultText.text = result.ResultMessage + "\n";
            battleResultText.text += $"Total Units - Player: {result.TotalUnitsArmy1}, Enemy: {result.TotalUnitsArmy2}\n";
        }

        UpdateCasualtyTexts(result);
        DrawBattleEvents(result);
    }

    private void CompleteBattle(BattleResult result)
    {
        if (result == null) return;

        if (battleResultText != null)
        {
            battleResultText.text = result.ResultMessage + "\n";
            battleResultText.text += $"Total Units - Player: {result.TotalUnitsArmy1}, Enemy: {result.TotalUnitsArmy2}\n";
        }

        UpdateCasualtyTexts(result);
        DrawBattleEvents(result);

        battleCoroutine = null;

        if (PlayerUISystem.Instance != null)
        {
            PlayerUISystem.Instance.UpdateUIObjects();
        }
    }

    private void UpdateCasualtyTexts(BattleResult result)
    {
        if (result == null) return;

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

        if (PlayerbattleCasualtiesText != null)
            PlayerbattleCasualtiesText.text = playerCasualtiesMessage;

        if (EnemybattleCasualtiesText != null)
            EnemybattleCasualtiesText.text = enemyCasualtiesMessage;
    }

    private void DrawBattleEvents(BattleResult result)
    {
        if (battleEventsContainer == null || battleEventPrefab == null || result == null)
            return;

        foreach (Transform child in battleEventsContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (var battleEvent in result.BattleEvents)
        {
            GameObject eventEntry = Instantiate(battleEventPrefab, battleEventsContainer);
            TMP_Text txt = eventEntry.GetComponent<TMP_Text>();
            if (txt != null)
            {
                txt.text = battleEvent;
            }
        }
    }

    public void Retreat()
    {
        if (battleCoroutine != null)
        {
            StopCoroutine(battleCoroutine);
            battleCoroutine = null;
        }

        Army playerArmy = GetPlayerArmy();
        if (playerArmy == null) return;

        Dictionary<UnitType, int> retreatCasualties = simulator.HandleRetreat(playerArmy);

        string retreatMessage = "Retreat successful! Casualties:\n";
        foreach (var casualty in retreatCasualties)
        {
            retreatMessage += $"{casualty.Key}: {casualty.Value}\n";
        }

        if (battleResultText != null)
            battleResultText.text = retreatMessage;

        if (PlayerUISystem.Instance != null)
            PlayerUISystem.Instance.UpdateUIObjects();
    }

    public void Negotiate()
    {
        if (PlayerStatHandler.Instance == null || PlayerStatHandler.Instance.pd == null)
            return;

        float diplomacy = PlayerStatHandler.Instance.pd.Charisma;
        float successChance = diplomacy / 100.0f;

        if (Random.value <= successChance)
        {
            if (battleResultText != null)
                battleResultText.text = "Negotiation successful! The battle is avoided.";
        }
        else
        {
            if (battleResultText != null)
                battleResultText.text = "Negotiation failed! Prepare for battle.";
        }
    }

    public void PreBattleRetreat()
    {
        if (PlayerStatHandler.Instance == null || PlayerStatHandler.Instance.pd == null)
            return;

        float agility = PlayerStatHandler.Instance.pd.Dexterity;
        float successChance = agility / 100.0f;

        if (Random.value <= successChance)
        {
            if (battleResultText != null)
                battleResultText.text = "Retreat successful! You avoided the battle.";
        }
        else
        {
            Army playerArmy = GetPlayerArmy();
            if (playerArmy == null) return;

            Dictionary<UnitType, int> retreatCasualties = simulator.HandleRetreat(playerArmy);

            string retreatMessage = "Retreat failed! Casualties:\n";
            foreach (var casualty in retreatCasualties)
            {
                retreatMessage += $"{casualty.Key}: {casualty.Value}\n";
            }

            if (battleResultText != null)
                battleResultText.text = retreatMessage;
        }

        if (PlayerUISystem.Instance != null)
            PlayerUISystem.Instance.UpdateUIObjects();
    }

    private Army GetPlayerArmy()
    {
        if (PlayerStatHandler.Instance == null || PlayerStatHandler.Instance.pd == null)
        {
            Debug.LogError("BattleManager: PlayerStatHandler or PlayerData is null.");
            return null;
        }

        if (PlayerStatHandler.Instance.pd.PlayerArmy == null)
        {
            PlayerStatHandler.Instance.pd.PlayerArmy = new Army();
        }

        return PlayerStatHandler.Instance.pd.PlayerArmy;
    }
}