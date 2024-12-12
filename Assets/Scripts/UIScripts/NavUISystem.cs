using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NavUISystem : MonoBehaviour
{
    [Header("Panels")]
    public GameObject profilePanel;
    public GameObject jobsPanel;
    public GameObject townPanel;
    //home button
    public GameObject shopPanel;
    public GameObject battlePanel;
    public Button ProfileButton;
    public Button JobsButton;
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
    private void Start()
    {
        ProfileButton.onClick.AddListener(() => OnProfileButtonClick());
        JobsButton.onClick.AddListener(() => OnJobButtonClick());
        TownButton.onClick.AddListener(() => OnHomeButtonClick());
        ShopButton.onClick.AddListener(() => OnShopButtonClick());
        BattleButton.onClick.AddListener(() => OnBattleButtonClick());
    }
    /// <summary>
    /// Handles the Profile Button click event.
    /// </summary>
    public void OnProfileButtonClick()
    {
        UpdateProfileData();
        //OpenUpPanel(profilePanel);
    }

    /// <summary>
    /// Handles the Smithing Button click event.
    /// </summary>
    public void OnJobButtonClick()
    {
        //OpenUpPanel(jobsPanel);
    }

    /// <summary>
    /// Handles the Home Button click event.
    /// </summary>
    public void OnHomeButtonClick()
    {
        //OpenUpPanel(townPanel);
    }
    /// <summary>
    /// Handles the Shop Button click event.
    /// </summary>
    public void OnShopButtonClick()
    {
        //OpenUpPanel(shopPanel);
    }

    /// <summary>
    /// Handles the War Button click event.
    /// </summary>
    public void OnBattleButtonClick()
    {
        //OpenUpPanel(battlePanel);
    }

    /// <summary>
    /// Updates the Profile Panel UI with the player's stats.
    /// </summary>
    private void UpdateProfileData()
    {
        PlayerData pd = PlayerStatHandler.Instance.pd;

        levelText.text = $"Level: {pd.Level}";
        experienceText.text = $"Experience: {pd.Experience} / {pd.MaxExperience}";
        strengthText.text = $"Strength: {pd.Strength}";
        dexterityText.text = $"Dexterity: {pd.Dexterity}";
        constitutionText.text = $"Constitution: {pd.Constitution}";
        charismaText.text = $"Charisma: {pd.Charisma}";

        smitherSkillLevelText.text = $"Smither Skill Level: {pd.SmitherSkillLevel}";
        tannerSkillLevelText.text = $"Tanner Skill Level: {pd.TannerSkillLevel}";
        carpenterSkillLevelText.text = $"Carpenter Skill Level: {pd.CarpenterSkillLevel}";
        masonSkillLevelText.text = $"Mason Skill Level: {pd.MasonSkillLevel}";
        alchemistSkillLevelText.text = $"Alchemist Skill Level: {pd.AlchemistSkillLevel}";

        totalBattlesFoughtText.text = $"Total Battles Fought: {pd.TotalBattlesFought}";
        totalBattlesWonText.text = $"Total Battles Won: {pd.TotalBattlesWon}";
        totalBattlesLostText.text = $"Total Battles Lost: {pd.TotalBattlesLost}";

        companionsText.text = "Companions:\n";
        foreach (var companion in pd.Companions)
        {
            companionsText.text += $"{companion.Name} (Level {companion.Level})\n";
        }
    }
    public void OpenUpPanel(GameObject PanelWhichWillOpen)
    {
        DisableAllNavPanels();
        PanelWhichWillOpen.SetActive(true);
    }
    public void DisableAllNavPanels()
    {
        profilePanel.SetActive(false);
        jobsPanel.SetActive(false);
        townPanel.SetActive(false);
        shopPanel.SetActive(false);
        battlePanel.SetActive(false);
    }
}
