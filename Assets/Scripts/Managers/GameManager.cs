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

    [Tooltip("Optional. Says why a new game could not be started (empty name etc.).")]
    public TMP_Text newGameWarningText;

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

    [Tooltip("Optional. The creation questions. Without one a new game starts " +
             "straight away, exactly as it did before.")]
    public GameObject characterCreationPanel;

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
                // More buttons than save slots: leave the extra ones alone rather
                // than indexing past the end of the list.
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
    /// Buttons nested inside them (the delete button, icons) are skipped — they
    /// have their own sibling index and used to be treated as slots.
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
            // Fallback for layouts where the slots sit deeper in the hierarchy.
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

        // Saves are written to persistentDataPath; older ones may still sit in
        // Assets/SaveSlotX, which is where this used to delete from (and only
        // there, so deleting a slot never removed the real save).
        foreach (string slotFolderPath in JSONDataHandler.GetSlotDirectories(slotIndex))
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
            Debug.LogWarning($"SaveSlot{slotIndex} does not exist.");

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
        if (saveSlotsPanel != null) saveSlotsPanel.SetActive(false);
        if (homeSettlementPanel != null) homeSettlementPanel.SetActive(false);
        if (characterCreationPanel != null) characterCreationPanel.SetActive(false);
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
        if (!SetPlayerData())
        {
            // Nothing was created, stay on the input screen instead of dropping
            // the player back on the slot list with no explanation.
            return;
        }

        // The character exists but has not been shaped yet. Creation edits the
        // PlayerData in place and calls FinishNewGame when the player is done,
        // so the save is written once, after the answers, not before them.
        if (characterCreationPanel != null)
        {
            DisableAllPanels();
            characterCreationPanel.SetActive(true);
            return;
        }

        // Said out loud. Skipping creation silently looks identical to creation
        // being broken, and the only way to tell them apart was to read this file.
        Debug.LogWarning("GameManager: no Character Creation Panel assigned, " +
                         "starting the game without the creation questions.");

        FinishNewGame();
    }

    /// <summary>
    /// Writes the finished character and enters the world.
    ///
    /// Separate from StartNewGame because character creation sits between the
    /// two: the save has to be written after the answers have moved the stats
    /// and granted the traits, otherwise the first load undoes all of it.
    /// </summary>
    public void FinishNewGame()
    {
        if (PlayerStatHandler.Instance != null)
            PlayerStatHandler.Instance.SavePlayerData();

        LoadPlayerData();
        LoadGame(PlayerPrefs.GetInt("Slot"));
    }

    private void ShowNewGameWarning(string message)
    {
        if (newGameWarningText != null)
            newGameWarningText.text = message;

        if (!string.IsNullOrEmpty(message))
            Debug.LogWarning($"GameManager: {message}");
    }

    /// <summary>
    /// Builds a brand new save in the selected slot.
    /// Returns false when the player name or village name is missing.
    /// </summary>
    public bool SetPlayerData()
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

            // 10 = an average adult (D&D convention our combat formulas assume).
            // Starting at 1 made every event requirement (5-15) unreachable and
            // gave -5 combat modifiers. Character creation nudges these up or
            // down from here.
            Strength = NewGameDefaults.StatValue,
            StrengthXP = NewGameDefaults.StatXP,
            Dexterity = NewGameDefaults.StatValue,
            DexterityXP = NewGameDefaults.StatXP,
            Constitution = NewGameDefaults.StatValue,
            ConstitutionXP = NewGameDefaults.StatXP,
            Charisma = NewGameDefaults.StatValue,
            CharismaXP = NewGameDefaults.StatXP,

            SmitherSkillLevel = 1,
            SmitherSkillXP = 149,
            TannerSkillLevel = 1,
            TannerSkillXP = 149,
            CarpenterSkillLevel = 1,
            CarpenterSkillXP = 149,
            MasonSkillLevel = 1,
            MasonSkillXP = 149,
            AlchemistSkillLevel = 1,
            AlchemistSkillXP = 149,

            TotalBattlesFought = 0,
            TotalBattlesWon = 0,
            TotalBattlesLost = 0,

            MaxExhaustionLevel = NewGameDefaults.MaxExhaustionLevel,
            CurrentExhaustionLevel = 0,

            Rations = NewGameDefaults.Rations,
            PlayerArmy = new Army(),

            LastSleepDay = 1,
            LastSleepHour = 6,
            LastSleepMinute = 0,

            LastMealDay = 1,
            LastMealHour = 6,
            LastMealMinute = 0,

            HasDied = false,

            Companions = new List<Companion>(),
            Items = new List<Item>(),
            ItemStacks = new List<ItemStackData>(),
            Units = new List<Unit>(),
            Quests = new List<Quest_SO_Constructor>()
        };

        // Yeni para sistemi
        pd.SetMoney(NewGameDefaults.Gold, NewGameDefaults.Silver);

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
            ShowNewGameWarning("The game could not be started, the player systems are not ready.");
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

        // The player starts at home.
        pd.LastSettlementName = homeSettlement.Name;

        if (SettlementHandler.Instance != null)
            SettlementHandler.Instance.settlement = homeSettlement;

        PlayerStatHandler.Instance.SavePlayerData();
        return true;
    }
}