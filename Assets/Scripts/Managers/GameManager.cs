using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public bool isSaveSlotEmpty;
    public TMP_InputField PlayerNameInput;
    public TMP_InputField VillageNameInput;

    [Header("Save Slot Buttons")]
    public Button slot1Button;
    public Button slot2Button;
    public Button slot3Button;

    [Header("Slot Texts")]
    public TMP_Text slot1Text;
    public TMP_Text slot2Text;
    public TMP_Text slot3Text;


    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject startGamePanel;
    public GameObject loadGamePanel;
    public GameObject creditsPanel;
    public GameObject settingsPanel;
    public GameObject navPanel;
    public GameObject infoPanel;
    public GameObject InputPanel;

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
        UpdateSlotUI(1, slot1Text, slot1Button);
        UpdateSlotUI(2, slot2Text, slot2Button);
        UpdateSlotUI(3, slot3Text, slot3Button);

    }
    private void UpdateSlotUI(int slot, TMP_Text slotText, Button slotButton)
    {
        SaveSlot saveSlot = SaveManager.Instance.LoadGame(slot);

        if (saveSlot != null)
        {
            slotText.text = $"{saveSlot.PlayerName}\nDay {saveSlot.Day}\n{saveSlot.VillageName}";
            slotButton.interactable = true;
            slotButton.onClick.AddListener(() => LoadGame(slot));
        }
        else
        {
            slotText.text = "Empty Slot";
            slotButton.interactable = false;
        }
    }
    public void LoadGame(int slot)
    {
        SaveSlot saveSlot = SaveManager.Instance.LoadGame(slot);
        if (saveSlot != null)
        {
            PlayerStatHandler.Instance.pd = saveSlot.PlayerData;
            Debug.Log($"Loaded game from slot {slot}");
            ShowMainMenuPanel();
        }
    }
    public void SaveToSlot(int slot)
    {
        PlayerData currentPlayerData = PlayerStatHandler.Instance.pd;

        SaveSlot saveSlot = new SaveSlot
        {
            PlayerName = currentPlayerData.Name,
            VillageName = currentPlayerData.VillageName,
            Day = currentPlayerData.Day,
            PlayerData = currentPlayerData
        };

        SaveManager.Instance.SaveGame(slot, saveSlot);
        Debug.Log($"Saved game to slot {slot}");
        UpdateSlotUI(slot, GetSlotText(slot), GetSlotButton(slot));
    }
    private TMP_Text GetSlotText(int slot)
    {
        return slot switch
        {
            1 => slot1Text,
            2 => slot2Text,
            3 => slot3Text,
            _ => null
        };
    }

    private Button GetSlotButton(int slot)
    {
        return slot switch
        {
            1 => slot1Button,
            2 => slot2Button,
            3 => slot3Button,
            _ => null
        };
    }

    public void DisableAllPanels()
    {
        mainMenuPanel.SetActive(false);
        startGamePanel.SetActive(false);
        loadGamePanel.SetActive(false);
        creditsPanel.SetActive(false);
        settingsPanel.SetActive(false);
        InputPanel.SetActive(false);

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
        InputPanel.SetActive(true);
    }

    public void ShowLoadGamePanel()
    {
        DisableAllPanels();
        startGamePanel.SetActive(true);
        loadGamePanel.SetActive(true);
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

    public void ExitGame()
    {
        Debug.Log("Exiting Game...");
        Application.Quit();
    }

    public bool HasSavedGame()
    {
        // Check for saved game data. Replace with your actual save/load implementation.
        return PlayerPrefs.HasKey("SavedGameSlot");
    }

    public void LoadLastSavedGame()
    {
        // Load the most recent save data.
        Debug.Log("Loading last saved game...");
        // Add your loading logic here.
        ShowMainMenuPanel(); // Assuming you'll switch to the main menu after loading.
    }

    public void Death()
    {
        Debug.Log("You are Dead...");
    }
    public void StartNewGameButton()
    {

        string name, villageName;
        name = PlayerNameInput.text;
        villageName = VillageNameInput.text;

        if (name.Length == 0 || villageName.Length == 0)
            return;
        PlayerStatHandler.Instance.pd.VillageName = villageName;
        PlayerStatHandler.Instance.pd.Name = name;
        PlayerStatHandler.Instance.JSONhandler.SaveData(new PlayerDataWrapper { pd = PlayerStatHandler.Instance.pd }, "playerData.json");
        DisableAllPanels();
        navPanel.SetActive(true);
        infoPanel.SetActive(true);
        startGamePanel.SetActive(true);

    }
}
