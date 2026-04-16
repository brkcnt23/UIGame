using TMPro;
using UnityEngine;

public class ResidentalUIHnadler : MonoBehaviour
{
    public static ResidentalUIHnadler Instance;

    [Header("TownHall")]
    public TMP_Text TownHallLevel;
    public TMP_Text TownHallCost;

    [Header("Tavern")]
    public TMP_Text TavernLevel;
    public TMP_Text TavernCost;

    [Header("Wall")]
    public TMP_Text WallLevel;
    public TMP_Text WallCost;

    [Header("Shop")]
    public TMP_Text ShopLevel;
    public TMP_Text ShopCost;

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

    public void UpgradeTownHall()
    {
        if (HomeSettlementHandler.Instance == null) return;

        HomeSettlementHandler.Instance.UpgradeTownHall();
        UpdateUI();
    }

    public void UpgradeTavern()
    {
        if (HomeSettlementHandler.Instance == null) return;

        HomeSettlementHandler.Instance.UpgradeTavern();
        UpdateUI();
    }

    public void UpgradeWall()
    {
        if (HomeSettlementHandler.Instance == null) return;

        HomeSettlementHandler.Instance.UpgradeWalls();
        UpdateUI();
    }

    public void UpgradeShop()
    {
        if (HomeSettlementHandler.Instance == null) return;

        HomeSettlementHandler.Instance.UpgradeShop();
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (HomeSettlementHandler.Instance == null || HomeSettlementHandler.Instance.homeSettlement == null)
        {
            Debug.LogWarning("ResidentalUIHnadler: HomeSettlementHandler or homeSettlement is null.");
            return;
        }

        var home = HomeSettlementHandler.Instance.homeSettlement;

        if (home.TownHall != null)
        {
            if (TownHallLevel != null)
                TownHallLevel.text = home.TownHall.level.ToString();

            if (TownHallCost != null)
                TownHallCost.text = home.TownHall.RequiredResourceAmount().ToString();
        }

        if (home.Tavern != null)
        {
            if (TavernLevel != null)
                TavernLevel.text = home.Tavern.level.ToString();

            if (TavernCost != null)
                TavernCost.text = home.Tavern.RequiredResourceAmount().ToString();
        }

        if (home.Walls != null)
        {
            if (WallLevel != null)
                WallLevel.text = home.Walls.level.ToString();

            if (WallCost != null)
                WallCost.text = home.Walls.RequiredResourceAmount().ToString();
        }

        if (home.Shops != null && home.Shops.Count > 0 && home.Shops[0] != null)
        {
            if (ShopLevel != null)
                ShopLevel.text = home.Shops[0].level.ToString();

            if (ShopCost != null)
                ShopCost.text = home.Shops[0].RequiredResourceAmount().ToString();
        }
    }
}