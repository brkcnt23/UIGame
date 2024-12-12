using TMPro;
using UnityEngine;

public class ResidentalUIHnadler : MonoBehaviour
{
    public static ResidentalUIHnadler Instance;

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

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

    public void UpgradeTownHall()
    {
        HomeSettlementHandler.Instance.UpgradeTownHall();
    }

    public void UpgradeTavern()
    {
        HomeSettlementHandler.Instance.UpgradeTavern();
    }

    public void UpgradeWall()
    {
        HomeSettlementHandler.Instance.UpgradeWalls();
    }

    public void UpgradeShop()
    {
        HomeSettlementHandler.Instance.UpgradeShop();
    }

    public void UpdateUI()
    {
        TownHallLevel.text = HomeSettlementHandler.Instance.homeSettlement.TownHall.level.ToString();
        TownHallCost.text = HomeSettlementHandler.Instance.homeSettlement.TownHall.RequiredResourceAmount().ToString();

        TavernLevel.text = HomeSettlementHandler.Instance.homeSettlement.Tavern.level.ToString();
        TavernCost.text = HomeSettlementHandler.Instance.homeSettlement.Tavern.RequiredResourceAmount().ToString();

        WallLevel.text = HomeSettlementHandler.Instance.homeSettlement.Walls.level.ToString();
        WallCost.text = HomeSettlementHandler.Instance.homeSettlement.Walls.RequiredResourceAmount().ToString();

        ShopLevel.text = HomeSettlementHandler.Instance.homeSettlement.Shops[0].level.ToString();
        ShopCost.text = HomeSettlementHandler.Instance.homeSettlement.Shops[0].RequiredResourceAmount().ToString();
    }
}
