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

        saveSlotButtons = saveSlotContainer.GetComponentsInChildren<Button>();

        if (saveSlotButtons == null || saveSlotButtons.Length == 0)
        {
            Debug.LogWarning("GameManager: No save slot buttons found!");
            return;
        }

        foreach (Button button in saveSlotButtons)
        {
            int index = button.gameObject.transform.GetSiblingIndex();
            button.onClick.RemoveAllListeners();

            PlayerData pd = playerData[index];

            if (string.IsNullOrEmpty(pd.Name))
            {
                button.GetComponentInChildren<TextMeshProUGUI>().text = $"Empty Slot {index + 1}\nClick to Start New Game";
                button.onClick.AddListener(() =>
                {
                    PlayerPrefs.SetInt("Slot", index);
                    DisableAllPanels();
                    if (InputPanel != null) InputPanel.SetActive(true);
                });

                if (deleteSlotButtons != null && index < deleteSlotButtons.Length && deleteSlotButtons[index] != null)
                    deleteSlotButtons[index].gameObject.SetActive(false);
            }
            else
            {
                button.GetComponentInChildren<TextMeshProUGUI>().text =
                    $"{pd.Name} of {pd.VillageName}\nDay: {pd.Day}\nMoney: {pd.Money.Gold}g {pd.Money.Silver}s";

                button.onClick.AddListener(() => LoadGame(index));

                if (deleteSlotButtons != null && index < deleteSlotButtons.Length && deleteSlotButtons[index] != null)
                {
                    deleteSlotButtons[index].gameObject.SetActive(true);
                    deleteSlotButtons[index].onClick.RemoveAllListeners();
                    deleteSlotButtons[index].onClick.AddListener(() => DeleteSaveSlot(index));
                }
            }
        }
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
    }

    public void ShowSettlementPanel()
    {
        DisableAllPanels();

        if (navPanel != null) navPanel.SetActive(true);
        if (infoPanel != null) infoPanel.SetActive(true);
        if (startGamePanel != null) startGamePanel.SetActive(true);

        // NAV içindeki alt panelleri kapat
        NavUISystem navUi = FindObjectOfType<NavUISystem>();
        if (navUi != null)
        {
            navUi.DisableAllNavPanels();
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
        SetPlayerData();
        LoadPlayerData();
        LoadGame(PlayerPrefs.GetInt("Slot"));
    }

    public void SetPlayerData()
    {
        string name = PlayerNameInput != null ? PlayerNameInput.text.Trim() : "";
        string villageName = VillageNameInput != null ? VillageNameInput.text.Trim() : "";

        if (name.Length == 0 || villageName.Length == 0)
            return;

        PlayerData pd = new PlayerData
        {
            Name = name,
            VillageName = villageName,
            Day = 1,
            Hour = 6,
            Minute = 0,

            Level = 1,
            Health = 100,
            MaxHealth = 100,
            Experience = 0,
            MaxExperience = 149,

            Alignment = 0,

            Strength = 1,
            StrengthXP = 149,
            Dexterity = 1,
            DexterityXP = 149,
            Constitution = 1,
            ConstitutionXP = 149,
            Charisma = 1,
            CharismaXP = 149,

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

            MaxExhaustionLevel = 10,
            CurrentExhaustionLevel = 0,

            Rations = 10,
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
        pd.SetMoney(5, 0);

        // Home settlement
        Settlement homeSettlement = new Settlement
        {
            ID = 0,
            Name = villageName,
            isUnlocked = true,
            Type = SettlementType.Village,
            Quality = 1,
            Population = 10,
            Wealth = new Currency(100, 0),
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
            8f,
            false,
            1
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
            return;
        }

        if (HomeSettlementHandler.Instance != null)
        {
            HomeSettlementHandler.Instance.homeSettlement = homeSettlement;
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

        PlayerStatHandler.Instance.SavePlayerData();
    }
}