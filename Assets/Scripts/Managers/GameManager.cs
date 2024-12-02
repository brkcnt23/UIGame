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
    }

    public void LoadPlayerData()
    {
        playerData.Clear();
        for (int i = 0; i <= 2; i++)
        {
            JSONDataHandler JSONhandler = new JSONDataHandler(i);
            PlayerDataWrapper wrapper = JSONhandler.LoadData<PlayerDataWrapper>("playerData.json");
            playerData.Add(wrapper.pd);
        }
    }
    public void PopulateButtonsAndTexts()
    {
        saveSlotButtons = saveSlotContainer.GetComponentsInChildren<Button>();

        LoadPlayerData();
        

        foreach (Button button in saveSlotButtons)
        {
            
            int _index = button.gameObject.transform.GetSiblingIndex();
            button.onClick.RemoveAllListeners();
            if (playerData[_index].Name == "")
            {
                button.GetComponentInChildren<TextMeshProUGUI>().text = $"Empty Slot {_index}\nClick to Start New Game";
                button.onClick.AddListener(() =>
                {
                    PlayerPrefs.SetInt("Slot", _index);
                    DisableAllPanels();
                    InputPanel.SetActive(true);
                }
                );
            }
            else
            {
                button.onClick.AddListener(() => LoadGame(_index));
                button.GetComponentInChildren<TextMeshProUGUI>().text = $"{playerData[_index].Name} of {playerData[_index].VillageName}\nDay: {playerData[_index].Day}";
            }
        }
    }

    public void LoadGame(int slot)
    {
        JSONDataHandler JSONhandler = new JSONDataHandler(slot);
        PlayerDataWrapper wrapper = JSONhandler.LoadData<PlayerDataWrapper>("playerData.json");
        PlayerStatHandler.Instance.pd = wrapper.pd;

        PlayerPrefs.SetInt("Slot", slot);

        DisableAllPanels();
        navPanel.SetActive(true);
        infoPanel.SetActive(true);
        startGamePanel.SetActive(true);

    }

    public void SaveGame()
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
