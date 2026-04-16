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

    public GameObject CraftingPanel;

    private void Start()
    {
        if (StartSmithingButton != null)
        {
            StartSmithingButton.onClick.RemoveAllListeners();
            StartSmithingButton.onClick.AddListener(StartSmithing);
        }

        if (StartTanningButton != null)
        {
            StartTanningButton.onClick.RemoveAllListeners();
            StartTanningButton.onClick.AddListener(StartTanning);
        }

        if (StartAlchemyButton != null)
        {
            StartAlchemyButton.onClick.RemoveAllListeners();
            StartAlchemyButton.onClick.AddListener(StartAlchemy);
        }

        UpdateSettlementShopLevels();
        RefreshUI();
    }

    public void UpdateSettlementShopLevels()
    {
        SettlementBlacksmithLevel = 0;
        SettlementTannerLevel = 0;
        SettlementAlchemistLevel = 0;

        if (SettlementHandler.Instance == null)
        {
            Debug.LogWarning("CraftingUI: SettlementHandler.Instance is null.");
            return;
        }

        var currentSettlement = SettlementHandler.Instance.GetCurrentSettlement();
        if (currentSettlement == null)
        {
            Debug.LogWarning("CraftingUI: No current settlement found.");
            return;
        }

        if (currentSettlement.Shops == null || currentSettlement.Shops.Count == 0)
        {
            Debug.LogWarning("CraftingUI: Current settlement has no shops.");
            return;
        }

        var blacksmithShop = currentSettlement.Shops.FirstOrDefault(s => s.ShopType == ShopTypes.Blacksmith);
        var tannerShop = currentSettlement.Shops.FirstOrDefault(s => s.ShopType == ShopTypes.Tanner);
        var alchemistShop = currentSettlement.Shops.FirstOrDefault(s => s.ShopType == ShopTypes.Alchemist);

        if (blacksmithShop != null)
        {
            SettlementBlacksmithLevel = blacksmithShop.level;
            Debug.Log($"Settlement blacksmith level: {blacksmithShop.level}");
        }

        if (tannerShop != null)
        {
            SettlementTannerLevel = tannerShop.level;
            Debug.Log($"Settlement tanner level: {tannerShop.level}");
        }

        if (alchemistShop != null)
        {
            SettlementAlchemistLevel = alchemistShop.level;
            Debug.Log($"Settlement alchemist level: {alchemistShop.level}");
        }
    }

    public void RefreshUI()
    {
        if (PlayerStatHandler.Instance == null || PlayerStatHandler.Instance.pd == null)
        {
            Debug.LogWarning("CraftingUI: Player data is not ready.");
            return;
        }

        if (BlacksmithLevelText != null)
            BlacksmithLevelText.text = $"Blacksmith Level: {SettlementBlacksmithLevel}";

        if (TannerLevelText != null)
            TannerLevelText.text = $"Tanner Level: {SettlementTannerLevel}";

        if (AlchemistLevelText != null)
            AlchemistLevelText.text = $"Alchemist Level: {SettlementAlchemistLevel}";

        if (PlayerBlacksmithLevelText != null)
            PlayerBlacksmithLevelText.text = $"Player Blacksmith Level: {PlayerStatHandler.Instance.pd.SmitherSkillLevel}";

        if (PlayerTannerLevelText != null)
            PlayerTannerLevelText.text = $"Player Tanner Level: {PlayerStatHandler.Instance.pd.TannerSkillLevel}";

        if (PlayerAlchemistLevelText != null)
            PlayerAlchemistLevelText.text = $"Player Alchemist Level: {PlayerStatHandler.Instance.pd.AlchemistSkillLevel}";

        if (StartSmithingButton != null)
            StartSmithingButton.interactable = SettlementBlacksmithLevel > 0 && CraftingSystem.Instance != null;

        if (StartTanningButton != null)
            StartTanningButton.interactable = SettlementTannerLevel > 0 && CraftingSystem.Instance != null;

        if (StartAlchemyButton != null)
            StartAlchemyButton.interactable = SettlementAlchemistLevel > 0 && CraftingSystem.Instance != null;
    }

    public void StartSmithing()
    {
        if (CraftingSystem.Instance == null)
        {
            Debug.LogWarning("CraftingUI: CraftingSystem.Instance is null.");
            return;
        }

        CraftingSystem.Instance.WorkAsBlacksmith(SettlementBlacksmithLevel, "weapon");
        RefreshUI();
    }

    public void StartTanning()
    {
        if (CraftingSystem.Instance == null)
        {
            Debug.LogWarning("CraftingUI: CraftingSystem.Instance is null.");
            return;
        }

        CraftingSystem.Instance.WorkAsTanner(SettlementTannerLevel);
        RefreshUI();
    }

    public void StartAlchemy()
    {
        if (CraftingSystem.Instance == null)
        {
            Debug.LogWarning("CraftingUI: CraftingSystem.Instance is null.");
            return;
        }

        CraftingSystem.Instance.WorkAsAlchemist(SettlementAlchemistLevel);
        RefreshUI();
    }
}