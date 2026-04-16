using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIHandler : MonoBehaviour
{
    public static UIHandler Instance { get; private set; }

    [Header("Settlement Info")]
    public GameObject SettlementInfoPanel;
    public TMP_Text SettlementName;
    public TMP_Text SettlementDescription;

    [Header("Settlement Panels")]
    public GameObject SettlementPanelBG;
    public GameObject TownHallPanel;
    public GameObject TavernPanel;
    public GameObject ShopsPanel;
    public GameObject WallsPanel;

    [Header("Home Settlement Panels")]
    public GameObject HomePanelBG;
    public GameObject HomeTownHallPanel;
    public GameObject HomeTavernPanel;
    public GameObject HomeShopsPanel;
    public GameObject HomeWallsPanel;

    [Header("Quest Settlement Panels")]
    public GameObject QuestPanelBG;
    public GameObject ResultsPanel;
    public TMP_Text QuestInfo;
    public Button[] GoBackButtons;
    public Button FightButton;

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

    public void UpdateSettlementInfo(Settlement settlement)
    {
        if (settlement == null)
        {
            Debug.LogWarning("UIHandler: settlement is null.");
            return;
        }

        if (SettlementName != null)
            SettlementName.text = settlement.Name;

        if (SettlementDescription != null)
            SettlementDescription.text =
                $"Population: {settlement.Population}\n" +
                $"Quality: {settlement.Quality}\n" +
                $"Wealth: {settlement.Wealth}";

        if (SettlementInfoPanel != null)
            SettlementInfoPanel.SetActive(true);

        if (OnHome(settlement))
        {
            if (HomePanelBG != null) HomePanelBG.SetActive(true);
            if (QuestPanelBG != null) QuestPanelBG.SetActive(false);
            if (SettlementPanelBG != null) SettlementPanelBG.SetActive(false);
        }
        else if (OnQuest(settlement))
        {
            if (QuestPanelBG != null) QuestPanelBG.SetActive(true);
            if (HomePanelBG != null) HomePanelBG.SetActive(false);
            if (SettlementPanelBG != null) SettlementPanelBG.SetActive(false);

            if (SettlementDescription != null)
                SettlementDescription.text = "";
        }
        else
        {
            if (HomePanelBG != null) HomePanelBG.SetActive(false);
            if (QuestPanelBG != null) QuestPanelBG.SetActive(false);
            if (SettlementPanelBG != null) SettlementPanelBG.SetActive(true);
        }
    }

    public bool OnHome(Settlement settlement)
    {
        return HomeSettlementHandler.Instance != null &&
               settlement == HomeSettlementHandler.Instance.homeSettlement;
    }

    public bool OnQuest(Settlement settlement)
    {
        return settlement != null && settlement.Type == SettlementType.Quest;
    }

    public void HideHomeUI()
    {
        if (SettlementInfoPanel != null) SettlementInfoPanel.SetActive(false);

        if (SettlementPanelBG != null) SettlementPanelBG.SetActive(false);
        if (HomePanelBG != null) HomePanelBG.SetActive(false);
        if (QuestPanelBG != null) QuestPanelBG.SetActive(false);

        if (TownHallPanel != null) TownHallPanel.SetActive(false);
        if (TavernPanel != null) TavernPanel.SetActive(false);
        if (ShopsPanel != null) ShopsPanel.SetActive(false);
        if (WallsPanel != null) WallsPanel.SetActive(false);

        if (HomeTownHallPanel != null) HomeTownHallPanel.SetActive(false);
        if (HomeTavernPanel != null) HomeTavernPanel.SetActive(false);
        if (HomeShopsPanel != null) HomeShopsPanel.SetActive(false);
        if (HomeWallsPanel != null) HomeWallsPanel.SetActive(false);

        if (ResultsPanel != null) ResultsPanel.SetActive(false);
    }
}