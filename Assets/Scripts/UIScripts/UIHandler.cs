using Unity;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
public class UIHandler : MonoBehaviour
{
    public static UIHandler Instance { get; private set; }

    [Header("Settlement Info")]
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


    public void Awake()
    {
        // Check if instance already exists
        if (Instance == null)
        {
            // If not, set instance to this
            Instance = this;
        }
        else
        {
            // If instance already exists, destroy this
            Destroy(gameObject);
        }
    }

    public void UpdateSettlementInfo(Settlement settlement)
    {
        if(OnHome(settlement))
        {
            HomePanelBG.SetActive(true);
            QuestPanelBG.SetActive(false);
            SettlementPanelBG.SetActive(false);
        }
        else if(OnQuest(settlement))
        {
            QuestPanelBG.SetActive(true);
            HomePanelBG.SetActive(false);
            SettlementPanelBG.SetActive(false);
        }
        else
        {
            HomePanelBG.SetActive(false);
            QuestPanelBG.SetActive(false);
            SettlementPanelBG.SetActive(true);
        }

        SettlementName.text = settlement.Name;
        SettlementDescription.text = $"Population: {settlement.Population}\nQuality:{settlement.Quality}\nWealth:{settlement.Wealth}";
    }

    public bool OnHome(Settlement settlement)
    {
        return settlement == HomeSettlementHandler.Instance.homeSettlement;
    }

    public bool OnQuest(Settlement settlement)
    {
        return settlement.Type == SettlementType.Quest;
    }
}