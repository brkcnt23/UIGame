using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using NEXUS.Utilities;
using System.IO;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public ItemSpriteDatabase spriteDatabase;
    public TMP_InputField PlayerNameInput;
    public TMP_InputField VillageNameInput;

    [Tooltip("Optional. Shows why a new game could not be started (empty name etc.).")]
    public TMP_Text newGameWarningText;

    [Header("Character Creation")]
    [Tooltip("Optional. The eight scenario questions asked between the name input and the game start.")]
    public PersonalityQuestionPanel personalityQuestions;
    public GameObject personalityPanel;

    [Header("Save Slot Container")]
    public GameObject saveSlotContainer;
    private Button[] saveSlotButtons = new Button[3];
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
            return;
        }
    }

    private void Start()
    {
        DisableAllPanels();

        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (navPanel != null) navPanel.SetActive(false);
        if (infoPanel != null) infoPanel.SetActive(false);

        LoadPlayerData();
        PopulateButtonsAndTexts();
    }

    // -----------------------------
    // SAVE SLOT UI
    // -----------------------------

    public void LoadPlayerData()
    {
        playerData.Clear();

        for (int i = 0; i <= 2; i++)
        {
            JSONDataHandler jsonHandler = new JSONDataHandler(i);
            PlayerDataWrapper wrapper = jsonHandler.LoadData<PlayerDataWrapper>("playerData.json");

            PlayerData pd = wrapper != null ? wrapper.pd : new PlayerData();
            pd.InitializeMoneyFromLegacyIfNeeded();

            if (pd.Items == null) pd.Items = new List<Item>();
            if (pd.ItemStacks == null) pd.ItemStacks = new List<ItemStackData>();
            if (pd.Companions == null) pd.Companions = new List<Companion>();
            if (pd.Quests == null) pd.Quests = new List<Quest_SO_Constructor>();
            if (pd.Units == null) pd.Units = new List<Unit>();
            if (pd.PlayerArmy == null) pd.PlayerArmy = new Army();

            playerData.Add(pd);
        }
    }

    public void PopulateButtonsAndTexts()
    {
        if (saveSlotContainer == null)
        {
            Debug.LogError("GameManager: saveSlotContainer is null!");
            return;
        }

        saveSlotButtons = CollectSaveSlotButtons();

        if (saveSlotButtons == null || saveSlotButtons.Length == 0)
        {
            Debug.LogWarning("GameManager: No save slot buttons found!");
            return;
        }

        for (int index = 0; index < saveSlotButtons.Length; index++)
        {
            Button button = saveSlotButtons[index];

            if (button == null)
                continue;

            if (index >= playerData.Count)
            {
                // More buttons than save slots: leave the extra ones alone.
                Debug.LogWarning($"GameManager: Save slot button {index} has no matching save slot ({playerData.Count} slots).");
                continue;
            }

            int slot = index;
            button.onClick.RemoveAllListeners();

            PlayerData pd = playerData[slot];
            TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>(true);

            if (pd == null || string.IsNullOrEmpty(pd.Name))
            {
                if (label != null)
                    label.text = $"Empty Slot {slot + 1}\nClick to Start New Game";

                button.onClick.AddListener(() => ShowNewGameInputPanel(slot));

                if (deleteSlotButtons != null && slot < deleteSlotButtons.Length && deleteSlotButtons[slot] != null)
                    deleteSlotButtons[slot].gameObject.SetActive(false);
            }
            else
            {
                if (label != null)
                    label.text =
                        $"{pd.Name} of {pd.VillageName}\nDay: {pd.Day}\nMoney: {pd.Money.Gold}g {pd.Money.Silver}s";

                button.onClick.AddListener(() => LoadGame(slot));

                if (deleteSlotButtons != null && slot < deleteSlotButtons.Length && deleteSlotButtons[slot] != null)
                {
                    deleteSlotButtons[slot].gameObject.SetActive(true);
                    deleteSlotButtons[slot].onClick.RemoveAllListeners();
                    deleteSlotButtons[slot].onClick.AddListener(() => DeleteSaveSlot(slot));
                }
            }
        }
    }

    /// <summary>
    /// Only the buttons that are direct children of the container are save slots.
    /// Buttons nested inside them (delete buttons, icons) are skipped.
    /// </summary>
    private Button[] CollectSaveSlotButtons()
    {
        List<Button> buttons = new List<Button>();

        foreach (Transform child in saveSlotContainer.transform)
        {
            Button button = child.GetComponent<Button>();

            if (button != null)
                buttons.Add(button);
        }

        if (buttons.Count == 0)
        {
            // Fallback for layouts where the slots are nested deeper.
            buttons.AddRange(saveSlotContainer.GetComponentsInChildren<Button>(true));
        }

        return buttons.ToArray();
    }

    public void ShowNewGameInputPanel(int slot)
    {
        PlayerPrefs.SetInt("Slot", slot);
        PlayerPrefs.Save();

        DisableAllPanels();

        if (PlayerNameInput != null) PlayerNameInput.text = "";
        if (VillageNameInput != null) VillageNameInput.text = "";

        ShowNewGameWarning("");

        if (InputPanel != null) InputPanel.SetActive(true);
    }

    public void LoadGame(int slot)
    {
        if (slot < 0 || slot >= playerData.Count)
        {
            Debug.LogError($"GameManager: Invalid slot index {slot}");
            return;
        }

        if (playerData[slot] == null || string.IsNullOrEmpty(playerData[slot].Name))
        {
            Debug.Log("No Game Data Found... Starting New Game");
            ShowStartGamePanel();
            return;
        }

        Debug.Log("Loading Game...");
        PlayerPrefs.SetInt("Slot", slot);

        if (PlayerStatHandler.Instance == null)
        {
            Debug.LogError("GameManager: PlayerStatHandler.Instance is null!");
            return;
        }

        PlayerStatHandler.Instance.LoadPlayerData();
        ShowSettlementPanel();
    }

    public void DeleteSaveSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= playerData.Count)
        {
            Debug.LogError($"GameManager: Invalid slot index {slotIndex}");
            return;
        }

        bool deletedAnything = false;

        // Delete both the current location and the old one (Assets/SaveSlotX).
        string[] slotFolders =
        {
            JSONDataHandler.GetSlotDirectory(slotIndex),
            JSONDataHandler.GetLegacySlotDirectory(slotIndex)
        };

        foreach (string slotFolderPath in slotFolders)
        {
            if (!Directory.Exists(slotFolderPath))
                continue;

            try
            {
                Directory.Delete(slotFolderPath, true);
                deletedAnything = true;
                Debug.Log($"Deleted save slot folder {slotFolderPath}");
            }
            catch (IOException e)
            {
                Debug.LogError($"GameManager: Could not delete '{slotFolderPath}': {e.Message}");
            }
        }

        if (!deletedAnything)
        {
            Debug.LogWarning($"SaveSlot{slotIndex} does not exist.");
        }

        playerData[slotIndex] = new PlayerData();
        PopulateButtonsAndTexts();
    }

    // -----------------------------
    // PANEL CONTROL
    // -----------------------------

    public void DisableAllPanels()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (startGamePanel != null) startGamePanel.SetActive(false);
        if (loadGamePanel != null) loadGamePanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (InputPanel != null) InputPanel.SetActive(false);
        if (personalityPanel != null) personalityPanel.SetActive(false);
        if (saveSlotsPanel != null) saveSlotsPanel.SetActive(false);
        if (homeSettlementPanel != null) homeSettlementPanel.SetActive(false);
        if (navPanel != null) navPanel.SetActive(false);
        if (infoPanel != null) infoPanel.SetActive(false);
    }

    public void ShowSettlementPanel()
    {
        DisableAllPanels();

        // Show main game panel (MainGamePanel)
        if (startGamePanel != null) startGamePanel.SetActive(true);

        // Also show navigation and info UI
        if (navPanel != null) navPanel.SetActive(true);
        if (infoPanel != null) infoPanel.SetActive(true);

        NavUISystem navUi = FindFirstObjectByType<NavUISystem>();
        if (navUi != null)
        {
            navUi.DisableAllNavPanels();
        }

        // Show settlement info by default (like Home button pressed)
        if (SettlementHandler.Instance != null && SettlementHandler.Instance.settlement != null)
        {
            UIHandler uiHandler = FindFirstObjectByType<UIHandler>();
            if (uiHandler != null)
            {
                uiHandler.UpdateSettlementInfo(SettlementHandler.Instance.settlement);
            }
        }

        isEnteredSettlement = true;
    }

    public void ShowMainMenuPanel()
    {
        DisableAllPanels();

        if (navPanel != null) navPanel.SetActive(false);
        if (infoPanel != null) infoPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }

    public void ShowStartGamePanel()
    {
        DisableAllPanels();
        LoadPlayerData();
        PopulateButtonsAndTexts();

        if (startGamePanel != null) startGamePanel.SetActive(true);
        if (saveSlotsPanel != null) saveSlotsPanel.SetActive(true);
    }

    public void ShowCreditsPanel()
    {
        DisableAllPanels();
        if (startGamePanel != null) startGamePanel.SetActive(true);
        if (creditsPanel != null) creditsPanel.SetActive(true);
    }

    public void ShowSettingsPanel()
    {
        DisableAllPanels();
        if (startGamePanel != null) startGamePanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(true);
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

    // -----------------------------
    // NEW GAME
    // -----------------------------

    public void StartNewGame()
    {
        if (!HasValidNewGameInput())
        {
            // The input was not valid, stay on the input panel.
            ShowNewGameWarning("Please enter both a player name and a village name.");
            return;
        }

        ShowNewGameWarning("");

        if (personalityQuestions != null)
        {
            // Name -> Village -> the eight scenarios -> the game starts.
            DisableAllPanels();

            if (personalityPanel != null) personalityPanel.SetActive(true);

            personalityQuestions.Begin(OnPersonalityCompleted, OnPersonalityCancelled);
            return;
        }

        CreateAndEnterNewGame(null);
    }

    private bool HasValidNewGameInput()
    {
        string name = PlayerNameInput != null ? PlayerNameInput.text.Trim() : "";
        string villageName = VillageNameInput != null ? VillageNameInput.text.Trim() : "";

        return name.Length > 0 && villageName.Length > 0;
    }

    private void OnPersonalityCompleted(PersonalityResult result)
    {
        if (personalityPanel != null) personalityPanel.SetActive(false);

        CreateAndEnterNewGame(result);
    }

    private void OnPersonalityCancelled()
    {
        // The player backed out of the first scenario, return to the name input.
        if (personalityPanel != null) personalityPanel.SetActive(false);

        DisableAllPanels();

        if (InputPanel != null) InputPanel.SetActive(true);
    }

    private void CreateAndEnterNewGame(PersonalityResult personality)
    {
        if (!SetPlayerData(personality))
            return;

        LoadPlayerData();
        LoadGame(PlayerPrefs.GetInt("Slot"));
    }

    /// <summary>
    /// Writes the result of the character creation scenarios onto the new save.
    /// The trait id is also added to ActiveTraitTags so trait driven systems see it.
    /// </summary>
    private void ApplyPersonality(PlayerData pd, PersonalityResult personality)
    {
        if (pd == null || personality == null)
            return;

        pd.Personality = personality.Scores;
        pd.PersonalityTrait = personality.TraitId;

        if (pd.ActiveTraitTags == null)
            pd.ActiveTraitTags = new List<string>();

        if (!string.IsNullOrEmpty(personality.TraitId) && !pd.ActiveTraitTags.Contains(personality.TraitId))
            pd.ActiveTraitTags.Add(personality.TraitId);

        Debug.Log($"New character personality: {personality.TraitDisplayName} ({personality.Scores})");
    }

    private void ShowNewGameWarning(string message)
    {
        if (newGameWarningText != null)
            newGameWarningText.text = message;

        if (!string.IsNullOrEmpty(message))
            Debug.LogWarning($"GameManager: {message}");
    }

    /// <summary>
    /// Builds a brand new save for the selected slot.
    /// Returns false when the player name / village name is missing.
    /// </summary>
    public bool SetPlayerData()
    {
        return SetPlayerData(null);
    }

    /// <summary>
    /// Builds a brand new save for the selected slot, with the personality trait
    /// that came out of the character creation scenarios (may be null).
    /// </summary>
    public bool SetPlayerData(PersonalityResult personality)
    {
        string name = PlayerNameInput != null ? PlayerNameInput.text.Trim() : "";
        string villageName = VillageNameInput != null ? VillageNameInput.text.Trim() : "";

        if (name.Length == 0 || villageName.Length == 0)
        {
            ShowNewGameWarning("Please enter both a player name and a village name.");
            return false;
        }

        ShowNewGameWarning("");

        PlayerData pd = new PlayerData
        {
            Name = name,
            VillageName = villageName,
            Day = NewGameDefaults.Day,
            Hour = NewGameDefaults.Hour,
            Minute = NewGameDefaults.Minute,

            Level = NewGameDefaults.Level,
            Health = NewGameDefaults.Health,
            MaxHealth = NewGameDefaults.Health,
            Experience = 0,
            MaxExperience = NewGameDefaults.MaxExperience,

            Alignment = 0,

            Strength = NewGameDefaults.StatValue,
            StrengthXP = NewGameDefaults.StatXP,
            Dexterity = NewGameDefaults.StatValue,
            DexterityXP = NewGameDefaults.StatXP,
            Constitution = NewGameDefaults.StatValue,
            ConstitutionXP = NewGameDefaults.StatXP,
            Charisma = NewGameDefaults.StatValue,
            CharismaXP = NewGameDefaults.StatXP,

            SmitherSkillLevel = NewGameDefaults.SkillLevel,
            SmitherSkillXP = NewGameDefaults.SkillXP,
            TannerSkillLevel = NewGameDefaults.SkillLevel,
            TannerSkillXP = NewGameDefaults.SkillXP,
            CarpenterSkillLevel = NewGameDefaults.SkillLevel,
            CarpenterSkillXP = NewGameDefaults.SkillXP,
            MasonSkillLevel = NewGameDefaults.SkillLevel,
            MasonSkillXP = NewGameDefaults.SkillXP,
            AlchemistSkillLevel = NewGameDefaults.SkillLevel,
            AlchemistSkillXP = NewGameDefaults.SkillXP,

            TotalBattlesFought = 0,
            TotalBattlesWon = 0,
            TotalBattlesLost = 0,

            MaxExhaustionLevel = NewGameDefaults.MaxExhaustionLevel,
            CurrentExhaustionLevel = 0,

            Rations = NewGameDefaults.Rations,
            PlayerArmy = new Army(),

            LastSleepDay = NewGameDefaults.Day,
            LastSleepHour = NewGameDefaults.Hour,
            LastSleepMinute = NewGameDefaults.Minute,

            LastMealDay = NewGameDefaults.Day,
            LastMealHour = NewGameDefaults.Hour,
            LastMealMinute = NewGameDefaults.Minute,

            HasDied = false,

            Companions = new List<Companion>(),
            Items = new List<Item>(),
            ItemStacks = new List<ItemStackData>(),
            Units = new List<Unit>(),
            Quests = new List<Quest_SO_Constructor>()
        };

        // Yeni para sistemi
        pd.SetMoney(NewGameDefaults.Gold, NewGameDefaults.Silver);

        ApplyPersonality(pd, personality);

        // Home settlement
        Settlement homeSettlement = new Settlement
        {
            ID = 0,
            Name = villageName,
            isUnlocked = true,
            Type = SettlementType.Village,
            Quality = NewGameDefaults.VillageQuality,
            Population = NewGameDefaults.VillagePopulation,
            Wealth = new Currency(NewGameDefaults.VillageWealthGold, NewGameDefaults.VillageWealthSilver),
            Tavern = new Taverns { Name = "Mükremin's Tavern" },
            Shops = new List<Shops>(),
            TownHall = new TownHalls { Name = $"{villageName}'s Hall" },
            Walls = new Walls { Name = $"{villageName}'s Wall" }
        };

        // Blacksmith
        Shops blacksmithShop = new Shops
        {
            Name = "Muhittin's Blacksmith",
            ShopType = ShopTypes.Blacksmith,
            level = 1
        };

        blacksmithShop.Items.AddRange(ItemGenerator.GenerateItems(ShopTypes.Blacksmith, blacksmithShop.level, spriteDatabase));
        blacksmithShop.Cash = new Currency(3, 50);

        List<StatModifier> modifiers = new List<StatModifier>
        {
            new StatModifier(StatType.Strength, 10, "Crafting"),
            new StatModifier(StatType.Constitution, 5, "Crafting")
        };

        Sprite armorSprite = spriteDatabase != null ? spriteDatabase.GetSprite(ItemCategory.Armor, 1) : null;

        Item customArmor = new Item(
            1500,
            "Armor",
            54,
            80,
            ItemCategory.Armor,
            modifiers,
            armorSprite,
            1,
            1,
            false,
            1,
            8f
        );

        blacksmithShop.Items.Add(customArmor);
        homeSettlement.Shops.Add(blacksmithShop);

        // General store
        Shops generalStore = new Shops
        {
            Name = "General Store",
            ShopType = ShopTypes.GeneralStore,
            level = 1,
            Cash = new Currency(5, 0)
        };

        generalStore.Items.AddRange(ItemGenerator.GenerateItems(ShopTypes.GeneralStore, generalStore.level, spriteDatabase));
        homeSettlement.Shops.Add(generalStore);

        if (PlayerStatHandler.Instance != null)
        {
            PlayerStatHandler.Instance.pd = pd;
        }
        else
        {
            Debug.LogError("GameManager: PlayerStatHandler.Instance is null during new game creation!");
            ShowNewGameWarning("The game could not be started, player systems are not ready.");
            return false;
        }

        if (HomeSettlementHandler.Instance != null)
        {
            HomeSettlementHandler.Instance.SetHomeSettlement(homeSettlement);
        }
        else
        {
            Debug.LogError("GameManager: HomeSettlementHandler.Instance is null!");
        }

        if (EventHandler.Instance != null)
        {
            EventHandler.Instance.LoadEventsFromSourceData();
        }
        else
        {
            Debug.LogWarning("GameManager: EventHandler.Instance is null!");
        }

        if (SettlementHandler.Instance != null)
        {
            SettlementHandler.Instance.LoadSettlementsFromSourceData();
        }
        else
        {
            Debug.LogWarning("GameManager: SettlementHandler.Instance is null!");
        }

        if (TravelSystem.Instance != null)
        {
            TravelSystem.Instance.LoadTravelDataFromSourceData();
        }
        else
        {
            Debug.LogWarning("GameManager: TravelSystem.Instance is null!");
        }

        // The player starts in the home village.
        pd.LastSettlementName = homeSettlement.Name;

        if (SettlementHandler.Instance != null)
            SettlementHandler.Instance.settlement = homeSettlement;

        PlayerStatHandler.Instance.SavePlayerData();
        return true;
    }
}