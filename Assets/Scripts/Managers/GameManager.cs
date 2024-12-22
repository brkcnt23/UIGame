using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using NEXUS.Utilities;
using System.IO;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public TMP_InputField PlayerNameInput;
    public TMP_InputField VillageNameInput;

    [Header("Save Slot Container")]
    public GameObject saveSlotContainer;
    Button[] saveSlotButtons = new Button[3];
    public Button[] deleteSlotButtons;
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject startGamePanel;
    public GameObject loadGamePanel;
    public GameObject creditsPanel;
    public GameObject settingsPanel;
    public GameObject navPanel;
    public GameObject infoPanel;
    public GameObject InputPanel;
    public GameObject saveSlotsPanel;
    public GameObject homeSettlementPanel;

    public List<PlayerData> playerData = new List<PlayerData>();

    public bool isEnteredSettlement = false;

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
    private void Start()
    {
        DisableAllPanels();
        mainMenuPanel.SetActive(true);
        navPanel.SetActive(false);
        infoPanel.SetActive(false);

        LoadPlayerData();
        PopulateButtonsAndTexts();
    }

    public void LoadPlayerData()
    {
        for (int i = 0; i <= 2; i++)
        {
            JSONDataHandler JSONhandler = new JSONDataHandler(i);
            PlayerDataWrapper wrapper = JSONhandler.LoadData<PlayerDataWrapper>("playerData.json");
            playerData.Add(wrapper != null ? wrapper.pd : new PlayerData());
        }
    }
    public void PopulateButtonsAndTexts()
    {
        saveSlotButtons = saveSlotContainer.GetComponentsInChildren<Button>();

        foreach (Button button in saveSlotButtons)
        {
            int _index = button.gameObject.transform.GetSiblingIndex();
            button.onClick.RemoveAllListeners();

            if (playerData[_index].Name == null)
            {
                button.GetComponentInChildren<TextMeshProUGUI>().text = $"Empty Slot {_index + 1}\nClick to Start New Game";
                button.onClick.AddListener(() =>
                {
                    PlayerPrefs.SetInt("Slot", _index);
                    DisableAllPanels();
                    InputPanel.SetActive(true);
                });
                deleteSlotButtons[_index].gameObject.SetActive(false);
            }
            else
            {
                button.onClick.AddListener(() => LoadGame(_index));
                button.GetComponentInChildren<TextMeshProUGUI>().text = $"{playerData[_index].Name} of {playerData[_index].VillageName}\nDay: {playerData[_index].Day}";

                // Show and assign the delete button
                deleteSlotButtons[_index].gameObject.SetActive(true);
                deleteSlotButtons[_index].onClick.RemoveAllListeners();
                deleteSlotButtons[_index].onClick.AddListener(() => DeleteSaveSlot(_index));
            }
        }
    }

    public void LoadGame(int slot)
    {
        if (playerData[slot] == null)
        {
            print("No Game Data Found... Starting New Game");
            ShowStartGamePanel();
            return;
        }
        else
        {
            print("Loading Game...");
            PlayerPrefs.SetInt("Slot", slot);
            PlayerStatHandler.Instance.LoadPlayerData();
            ShowSettlementPanel();
        }

    }

    public void DeleteSaveSlot(int slotIndex)
    {
        string slotFolderPath = Path.Combine(Application.dataPath, $"SaveSlot{slotIndex}");

        if (Directory.Exists(slotFolderPath))
        {
            Directory.Delete(slotFolderPath, true);
            Debug.Log($"Deleted SaveSlot{slotIndex}");
        }
        else
        {
            Debug.LogWarning($"SaveSlot{slotIndex} does not exist.");
        }

        // Reset the PlayerData for the slot
        playerData[slotIndex] = new PlayerData();

        // Update the UI
        PopulateButtonsAndTexts();
    }

    public void DisableAllPanels()
    {
        mainMenuPanel.SetActive(false);
        startGamePanel.SetActive(false);
        loadGamePanel.SetActive(false);
        creditsPanel.SetActive(false);
        settingsPanel.SetActive(false);
        InputPanel.SetActive(false);
        saveSlotsPanel.SetActive(false);
    }

    public void ShowSettlementPanel()
    {
        DisableAllPanels();
        navPanel.SetActive(true);
        infoPanel.SetActive(true);
        startGamePanel.SetActive(true);

        isEnteredSettlement = true;
    }

    public void ShowMainMenuPanel()
    {
        DisableAllPanels();
        navPanel.SetActive(false);
        infoPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void ShowStartGamePanel()
    {
        DisableAllPanels();
        PopulateButtonsAndTexts();
        saveSlotsPanel.SetActive(true);
    }

    public void ShowCreditsPanel()
    {
        DisableAllPanels();
        startGamePanel.SetActive(true);
        creditsPanel.SetActive(true);
    }

    public void ShowSettingsPanel()
    {
        DisableAllPanels();
        startGamePanel.SetActive(true);
        settingsPanel.SetActive(true);
    }

    public void LoadLastSavedGame()
    {
        int slot = PlayerPrefs.GetInt("Slot");
        LoadGame(slot);
    }

    public void Death()
    {
        Debug.Log("You are Dead...");
    }
    public void StartNewGame()
    {
        SetPlayerData();
        LoadGame(PlayerPrefs.GetInt("Slot"));
    }

    public void SetPlayerData()
    {
        string name, villageName;
        name = PlayerNameInput.text;
        villageName = VillageNameInput.text;

        if (name.Length == 0 || villageName.Length == 0)
            return;

        PlayerData playerData = new PlayerData();
        playerData.Name = name;
        playerData.VillageName = villageName;
        playerData.Day = 1;
        playerData.Hour = 6;
        playerData.Level = 1;
        playerData.Health = 100;
        playerData.MaxHealth = 100;
        playerData.Experience = 0;
        playerData.MaxExperience = 149;
        playerData.Gold = 5;
        playerData.Silver = 0;
        playerData.Alignment = 0;
        playerData.Strength = 1;
        playerData.StrengthXP = 149;
        playerData.Dexterity = 1;
        playerData.DexterityXP = 149;
        playerData.Constitution = 1;
        playerData.ConstitutionXP = 149;
        playerData.Charisma = 1;
        playerData.CharismaXP = 149;
        playerData.SmitherSkillLevel = 1;
        playerData.SmitherSkillXP = 149;
        playerData.TannerSkillLevel = 1;
        playerData.TannerSkillXP = 149;
        playerData.CarpenterSkillLevel = 1;
        playerData.CarpenterSkillXP = 149;
        playerData.MasonSkillLevel = 1;
        playerData.MasonSkillXP = 149;
        playerData.AlchemistSkillLevel = 1;
        playerData.AlchemistSkillXP = 149;
        playerData.TotalBattlesFought = 0;
        playerData.TotalBattlesWon = 0;
        playerData.TotalBattlesLost = 0;
        playerData.MaxExhaustionLevel = 10;
        playerData.CurrentExhaustionLevel = 0;
        playerData.Rations = 10;
        playerData.PlayerArmy = new Army();
        playerData.LastSleepDay = 1;
        playerData.LastSleepHour = 6;
        playerData.LastSleepMinute = 0;
        playerData.LastMealDay = 1;
        playerData.LastMealHour = 6;
        playerData.LastMealMinute = 0;


        playerData.HasDied = false;

        Settlement homeSettlement = new Settlement();
        homeSettlement.ID = 0;
        homeSettlement.Name = villageName;
        homeSettlement.isUnlocked = true;
        homeSettlement.Type = SettlementType.Village;
        homeSettlement.Quality = 1;
        homeSettlement.Population = 10;
        homeSettlement.Wealth.Gold = 100;
        homeSettlement.Wealth.Silver = 0;
        homeSettlement.Tavern = new Taverns();
        homeSettlement.Tavern.Name = "Mükremin's Tavern";
        homeSettlement.Shops = new List<Shops>();
        Shops shop = new Shops();
        shop.Name = "Muhittin's Shop";
        homeSettlement.Shops.Add(shop);
        homeSettlement.TownHall = new TownHalls();
        homeSettlement.TownHall.Name = villageName + "'s Hall";
        homeSettlement.Walls = new Walls();
        homeSettlement.Walls.Name = villageName + "'s Wall";

        PlayerStatHandler.Instance.pd = playerData;
        HomeSettlementHandler.Instance.homeSettlement = homeSettlement;

        EventHandler.Instance.LoadEventsFromSourceData();
        SettlementHandler.Instance.LoadSettlementsFromSourceData();
        TravelSystem.Instance.LoadTravelDataFromSourceData();
        PlayerStatHandler.Instance.SavePlayerData();
    }
}
