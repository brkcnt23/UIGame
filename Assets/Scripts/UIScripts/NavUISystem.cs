using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

public class NavUISystem : MonoBehaviour
{
    [Header("Panels")]
    public GameObject profilePanel;

    [FormerlySerializedAs("jobsPanel")]
    public GameObject activitiesPanel;

    public GameObject townPanel;
    public GameObject shopPanel;
    public GameObject battlePanel;

    public Button ProfileButton;
    [FormerlySerializedAs("JobsButton")]
    public Button ActivitiesButton;
    public Button TownButton;
    public Button ShopButton;
    public Button BattleButton;

    [Header("Profile UI Elements")]
    public TMP_Text levelText;
    public TMP_Text experienceText;
    public TMP_Text strengthText;
    public TMP_Text dexterityText;
    public TMP_Text constitutionText;
    public TMP_Text charismaText;

    public TMP_Text smitherSkillLevelText;
    public TMP_Text tannerSkillLevelText;
    public TMP_Text carpenterSkillLevelText;
    public TMP_Text masonSkillLevelText;
    public TMP_Text alchemistSkillLevelText;

    public TMP_Text totalBattlesFoughtText;
    public TMP_Text totalBattlesWonText;
    public TMP_Text totalBattlesLostText;
    public TMP_Text companionsText;

    public TMP_Text weightText;
    public TMP_Text moneyText;

    private void Start()
    {
        if (ProfileButton != null)
        {
            ProfileButton.onClick.RemoveAllListeners();
            ProfileButton.onClick.AddListener(OnProfileButtonClick);
        }

        if (ActivitiesButton != null)
        {
            ActivitiesButton.onClick.RemoveAllListeners();
            ActivitiesButton.onClick.AddListener(OnActivitiesButtonClick);
        }

        if (TownButton != null)
        {
            TownButton.onClick.RemoveAllListeners();
            TownButton.onClick.AddListener(OnHomeButtonClick);
        }

        if (ShopButton != null)
        {
            ShopButton.onClick.RemoveAllListeners();
            ShopButton.onClick.AddListener(OnShopButtonClick);
        }

        if (BattleButton != null)
        {
            BattleButton.onClick.RemoveAllListeners();
            BattleButton.onClick.AddListener(OnBattleButtonClick);
        }
    }

    public void OnProfileButtonClick()
    {
        UpdateProfileData();
        OpenUpPanel(profilePanel);
    }

    public void OnActivitiesButtonClick()
    {
        OpenUpPanel(activitiesPanel);
    }

    public void OnHomeButtonClick()
    {
        DisableAllNavPanels();

        if (SettlementHandler.Instance != null && SettlementHandler.Instance.settlement != null)
        {
            UIHandler.Instance.UpdateSettlementInfo(SettlementHandler.Instance.settlement);
        }
    }

    public void OnShopButtonClick()
    {
        OpenUpPanel(shopPanel);
    }

    public void OnBattleButtonClick()
    {
        OpenUpPanel(battlePanel);
    }

    private void UpdateProfileData()
    {
        if (PlayerStatHandler.Instance == null || PlayerStatHandler.Instance.pd == null)
        {
            Debug.LogError("NavUISystem: PlayerStatHandler.Instance or pd is null! Cannot update profile data.");
            return;
        }

        PlayerData pd = PlayerStatHandler.Instance.pd;
        Currency money = pd.GetMoney();

        if (levelText != null) levelText.text = $"Level: {pd.Level}";
        if (experienceText != null) experienceText.text = $"Experience: {pd.Experience} / {pd.MaxExperience}";
        if (strengthText != null) strengthText.text = $"Strength: {pd.Strength}";
        if (dexterityText != null) dexterityText.text = $"Dexterity: {pd.Dexterity}";
        if (constitutionText != null) constitutionText.text = $"Constitution: {pd.Constitution}";
        if (charismaText != null) charismaText.text = $"Charisma: {pd.Charisma}";

        if (smitherSkillLevelText != null) smitherSkillLevelText.text = $"Smither Skill Level: {pd.SmitherSkillLevel}";
        if (tannerSkillLevelText != null) tannerSkillLevelText.text = $"Tanner Skill Level: {pd.TannerSkillLevel}";
        if (carpenterSkillLevelText != null) carpenterSkillLevelText.text = $"Carpenter Skill Level: {pd.CarpenterSkillLevel}";
        if (masonSkillLevelText != null) masonSkillLevelText.text = $"Mason Skill Level: {pd.MasonSkillLevel}";
        if (alchemistSkillLevelText != null) alchemistSkillLevelText.text = $"Alchemist Skill Level: {pd.AlchemistSkillLevel}";

        if (totalBattlesFoughtText != null) totalBattlesFoughtText.text = $"Total Battles Fought: {pd.TotalBattlesFought}";
        if (totalBattlesWonText != null) totalBattlesWonText.text = $"Total Battles Won: {pd.TotalBattlesWon}";
        if (totalBattlesLostText != null) totalBattlesLostText.text = $"Total Battles Lost: {pd.TotalBattlesLost}";

        if (moneyText != null) moneyText.text = $"Money: {money.Gold}g {money.Silver}s";

        if (weightText != null)
        {
            float currentWeight = pd.GetCurrentInventoryWeight();
            float carryCapacity = pd.GetCarryCapacity();
            weightText.text = $"Load: {currentWeight:0.0} / {carryCapacity:0.0}";
        }

        if (companionsText != null)
        {
            companionsText.text = "Companions:\n";

            if (pd.Companions != null && pd.Companions.Count > 0)
            {
                foreach (var companion in pd.Companions)
                {
                    companionsText.text += $"{companion.Name} (Level {companion.Level})\n";
                }
            }
            else
            {
                companionsText.text += "None";
            }
        }
    }

    public void OpenUpPanel(GameObject panelWhichWillOpen)
    {
        if (UIHandler.Instance != null)
        {
            UIHandler.Instance.HideHomeUI();
        }

        DisableAllNavPanels();

        if (panelWhichWillOpen != null)
            panelWhichWillOpen.SetActive(true);
    }

    public void DisableAllNavPanels()
    {
        if (profilePanel != null) profilePanel.SetActive(false);
        if (activitiesPanel != null) activitiesPanel.SetActive(false);
        if (townPanel != null) townPanel.SetActive(false);
        if (shopPanel != null) shopPanel.SetActive(false);
        if (battlePanel != null) battlePanel.SetActive(false);
    }
}