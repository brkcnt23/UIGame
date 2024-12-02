using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using NEXUS.Utilities;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public TMP_InputField PlayerNameInput;
    public TMP_InputField VillageNameInput;

    [Header("Save Slot Container")]
    public GameObject saveSlotContainer;
    Button[] saveSlotButtons = new Button[3];

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

    public List<PlayerData> playerData = new List<PlayerData>();

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

        CheckFirstTime();
    }


    void CheckFirstTime()
    {
        if (!PlayerPrefs.HasKey("FirstTime"))
        {
            PlayerPrefs.SetInt("FirstTime", 1);

            for (int i = 0; i <= 2; i++)
            {
                JSONDataHandler JSONhandler = new JSONDataHandler(i);
                PlayerDataWrapper wrapper = new PlayerDataWrapper();
                JSONhandler.SaveData(wrapper, "playerData.json");

                SettlementListWrapper settlementWrapper = new SettlementListWrapper();
                JSONhandler.SaveData(settlementWrapper, "settlements.json");

                PlayerStatHandler.Instance.pd = new PlayerData();
                SettlementHandler.Instance.settlement = new Settlement();
            }
        }
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
        PlayerStatHandler.Instance.Wrappers(slot);

        PlayerPrefs.SetInt("Slot", slot);

        Settlement settlement = PlayerStatHandler.Instance.pd.LastSettlementName == "" ? PlayerStatHandler.Instance.homeSettlement : SettlementHandler.Instance.settlements.Find(x => x.Name == PlayerStatHandler.Instance.pd.LastSettlementName);
        UIHandler.Instance.UpdateSettlementInfo(settlement);

        SettlementHandler.Instance.settlement = settlement;

        SettlementHandler.Instance.OnSettlmentEntered(settlement);

        ShowSettlementPanel();

        MapHandler.Instance.MovePlayerToLastVisitedSettlement();

    }

    public void SaveGame()
    {
        PlayerStatHandler.Instance.EndWrappers();
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
        startGamePanel.SetActive(true);}

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
        saveSlotsPanel.SetActive(true);
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

    public void LoadLastSavedGame()
    {
        // Load the most recent save data.
        int slot = PlayerPrefs.GetInt("Slot");
        LoadGame(slot);
    }

    public void Death()
    {
        Debug.Log("You are Dead...");
    }
    public void StartNewGame()
    {
        string name, villageName;
        name = PlayerNameInput.text;
        villageName = VillageNameInput.text;

        if (name.Length == 0 || villageName.Length == 0)
            return;
        PlayerStatHandler.Instance.pd.VillageName = villageName;
        PlayerStatHandler.Instance.pd.Name = name;
        PlayerStatHandler.Instance.pd.Day = 1;
        PlayerStatHandler.Instance.homeSettlement.Name = villageName;

        SaveGame();

        LoadGame(PlayerPrefs.GetInt("Slot"));
    }
}
