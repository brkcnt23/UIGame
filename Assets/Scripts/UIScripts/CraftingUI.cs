using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;

public class CraftingUI : MonoBehaviour
{
    private int SettlementBlacksmithLevel;
    private int SettlementTannerLevel;
    private int SettlementAlchemistLevel;

    public TMP_Text PlayerBlacksmithLevelText;
    public TMP_Text PlayerTannerLevelText;
    public TMP_Text PlayerAlchemistLevelText;

    public TMP_Text BlacksmithLevelText;
    public TMP_Text TannerLevelText;
    public TMP_Text AlchemistLevelText;

    public Button StartSmithingButton;
    public Button StartTanningButton;
    public Button StartAlchemyButton;

    private PlayerData playerData;

    public GameObject CraftingPanel;

    void Start()
    {
        StartSmithingButton.onClick.AddListener(StartSmithing);
        StartTanningButton.onClick.AddListener(StartTanning);
        StartAlchemyButton.onClick.AddListener(StartAlchemy);

        UpdateSettlementShopLevels();
        RefreshUI();
    }

    void UpdateSettlementShopLevels()
    {
        if (SettlementHandler.Instance == null)
        {
            Debug.LogWarning("SettlementHandler.Instance is null.");
            return;
        }

        var currentSettlement = SettlementHandler.Instance.GetCurrentSettlement();
        if (currentSettlement == null)
        {
            Debug.LogWarning("No current settlement found.");
            return;
        }

        if (currentSettlement.Shops == null || currentSettlement.Shops.Count == 0)
        {
            Debug.LogWarning("Current settlement has no shops.");
            return;
        }

        var blacksmithShop = currentSettlement.Shops.FirstOrDefault(s => s.ShopType == ShopTypes.Blacksmith);
        var tannerShop = currentSettlement.Shops.FirstOrDefault(s => s.ShopType == ShopTypes.Tanner);
        var alchemistShop = currentSettlement.Shops.FirstOrDefault(s => s.ShopType == ShopTypes.Alchemist);

        // Check that these shops are not null before accessing their properties
        if (blacksmithShop == null)
        {
            Debug.LogWarning("No blacksmith shop found in the current settlement.");
        }
        else
        {
            SettlementBlacksmithLevel = blacksmithShop.level;
            Debug.Log($" this settlement's blacksmith level : {blacksmithShop.level}");
        }

        if (tannerShop == null)
        {
            Debug.LogWarning("No tanner shop found in the current settlement.");
        }
        else
        {
            SettlementTannerLevel = tannerShop.level;
            Debug.Log($" this settlement's tanner level : {tannerShop.level}");
        }

        if (alchemistShop == null)
        {
            Debug.LogWarning("No alchemist shop found in the current settlement.");
        }
        else
        {
            SettlementAlchemistLevel = alchemistShop.level;
            Debug.Log($" this settlement's alchemist level : {alchemistShop.level}");
        }
    }



    void RefreshUI()
    {
        // Settlement levels
        if (BlacksmithLevelText != null)
            BlacksmithLevelText.text = "Blacksmith Level: " + SettlementBlacksmithLevel;
        if (TannerLevelText != null)
            TannerLevelText.text = "Tanner Level: " + SettlementTannerLevel;
        if (AlchemistLevelText != null)
            AlchemistLevelText.text = "Alchemist Level: " + SettlementAlchemistLevel;

        Debug.Log($" this player's blacksmith level : {PlayerStatHandler.Instance.pd.SmitherSkillLevel}");
        Debug.Log($" this player's tanner level : {PlayerStatHandler.Instance.pd.TannerSkillLevel}");
        Debug.Log($" this player's alchemist level : {PlayerStatHandler.Instance.pd.AlchemistSkillLevel}");

        if (PlayerBlacksmithLevelText != null)
            PlayerBlacksmithLevelText.text = "Player Blacksmith Level: " + PlayerStatHandler.Instance.pd.SmitherSkillLevel;
        if (PlayerTannerLevelText != null)
            PlayerTannerLevelText.text = "Player Tanner Level: " + PlayerStatHandler.Instance.pd.TannerSkillLevel;
        if (PlayerAlchemistLevelText != null)
            PlayerAlchemistLevelText.text = "Player Alchemist Level: " + PlayerStatHandler.Instance.pd.AlchemistSkillLevel;
    }

    public void StartSmithing()
    {
        // Pass the obtained level, or just call the function
        CraftingSystem.Instance.WorkAsBlacksmith(SettlementBlacksmithLevel, "weapon");
    }

    public void StartTanning()
    {
        CraftingSystem.Instance.WorkAsTanner(SettlementTannerLevel);
    }

    public void StartAlchemy()
    {
        CraftingSystem.Instance.WorkAsAlchemist(SettlementAlchemistLevel);
    }
}
