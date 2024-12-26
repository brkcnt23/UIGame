using UnityEngine;
using NEXUS.Utilities;
using System.Collections.Generic;

public class HomeSettlementHandler : MonoBehaviour
{
    //this will be the instance of the HomeSettlementHandler, we will handle this home settlemnets upgrades and other stuff here
    public static HomeSettlementHandler Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    //this will be the home settlement
    public Settlement homeSettlement;

    //this will be the home settlement wrapper
    JSONDataHandler JSONhandler;

    void OnEnable()
    {
        homeSettlement.OnTownHallEntered += HandleTownHallEntered;
    }

    void OnDisable()
    {
        homeSettlement.OnTownHallEntered -= HandleTownHallEntered;
    }

    public void LoadHomeSettlement()
    {
        JSONhandler = new JSONDataHandler(PlayerPrefs.GetInt("Slot"));
        HomeSettlementWrapper wrapper = JSONhandler.LoadData<HomeSettlementWrapper>("homeSettlement.json");
        homeSettlement = wrapper != null ? wrapper.homeSettlement : new Settlement();
    }

    //this will be the function that will save the home settlement data
    public void SaveHomeSettlement()
    {
        JSONhandler = new JSONDataHandler(PlayerPrefs.GetInt("Slot"));
        JSONhandler.SaveData(new HomeSettlementWrapper { homeSettlement = homeSettlement }, "homeSettlement.json");
    }



    public void HandleTownHallEntered()
    {
        homeSettlement.EnterTownHall();
        GameManager.Instance.ShowSettlementPanel();
    }

    public void OnSettlmentEntered()
    {
        UIHandler.Instance.HomePanelBG.SetActive(true);
        //GenerateRandomHappenings();

        ResidentalUIHnadler.Instance.UpdateUI();
    }

    public void UpgradeResidental(Residentials residential)
    {
        residential.LevelUpResidential(ref PlayerStatHandler.Instance.pd);

        TimeSystem.Instance.AdvanceTimeCoroutine(0, residential.upgradeHour, 0);
    }

    public void UpgradeTavern()
    {
        UpgradeResidental(homeSettlement.Tavern);
    }

    public void UpgradeTownHall()
    {
        UpgradeResidental(homeSettlement.TownHall);
    }

    public void UpgradeWalls()
    {
        UpgradeResidental(homeSettlement.Walls);
    }

    public void UpgradeShop()
    {
        UpgradeResidental(homeSettlement.Shops[0]);
    }

    public void GenerateRandomHappenings()
    {
        int dice = Dice.RollD100();

        if (dice <= 10)
        {
            print("%10 chance of random event");
            dice = Dice.RollD6();

            switch (dice)
            {
                case 1:
                    RandomPopulationEvent();
                    break;
                case 2:
                    RandomWealthEvent();
                    break;
                case 3:
                    RandomQualityEvent();
                    break;
                case 4:
                    RandomShopEvent();
                    break;
                case 5:
                    RandomTavernEvent();
                    break;
                default:
                    break;
            }
        }
    }

    public void RandomPopulationEvent()
    {
        int population = Dice.Roll(-10, 10);

        homeSettlement.Population += population;

        print(population > 0 ? "People moved in" : "People moved out" + " when you were away");
    }

    public void RandomWealthEvent()
    {
        int wealth = Dice.Roll(-300, 300);

        if (wealth > 0)
        {
            homeSettlement.Wealth.Add(0, wealth); // Add positive wealth as silver
            print($"Merchants sold their goods and brought {wealth} silver to the settlement while you were away.");
        }
        else
        {
            homeSettlement.Wealth.Subtract(0, Mathf.Abs(wealth)); // Subtract negative wealth as silver
            print($"Merchants bought goods and took {Mathf.Abs(wealth)} silver from the settlement while you were away.");
        }

        print(wealth > 0 ? "Merchants sold their goods" : "Merchants bought goods" + " when you were away");
    }

    public void RandomQualityEvent()
    {
        int dice = Dice.RollD100();
        int quality = Dice.Roll(-2, 2);

        if (dice <= 10)
        {
            homeSettlement.Quality += quality;
        }
        else
        {
            homeSettlement.Quality -= quality;
        }

        if (quality > 0)
        {
            print("Settlements members worked hard to improve the quality of the settlement when you were away");
        }
        else if (quality < 0)
        {
            print("Settlements members slacked off when you were away");
        }

        if (homeSettlement.Quality < 0)
        {
            homeSettlement.Quality = 0;
        }

        //UpgradeHomeSettlement(quality, 0, 0);
    }

    public void RandomShopEvent()
    {
        Shops shop = homeSettlement.Shops[0];
        if (shop == null)
        {
            return;
        }

        int itemCategory = Dice.Roll(2);
        List<Item> items = itemCategory == 1
            ? shop.GetItemsByCategory(ItemCategory.Resource)
            : shop.GetItemsByCategory(ItemCategory.CraftingMaterial);

        if (items.Count == 0)
        {
            return;
        }

        int itemIndex = Dice.Roll(items.Count);
        int quantity = Dice.Roll(-shop.level * 10, shop.level * 10);

        Item item = items[itemIndex];

        Currency itemCost = new Currency(0, item.Value.Silver);
        Currency transactionAmount = itemCost * Mathf.Abs(quantity);
        if (quantity > 0)
        {

            print($"Shop {shop.Name} bought {item.Name} x{quantity} when you were away and spent {transactionAmount}");
            homeSettlement.Wealth.Subtract(transactionAmount.Gold, transactionAmount.Silver);
        }
        else
        {
            // Shop sold items
            print($"Shop {shop.Name} sold {item.Name} x{-quantity} when you were away and made {transactionAmount}");
            homeSettlement.Wealth.Add(transactionAmount.Gold, transactionAmount.Silver);
        }
    }

    public void RandomTavernEvent()
    {
        int selectedQuest = Dice.Roll(homeSettlement.Tavern.Quests.Count);
        Quest_SO_Constructor quest = homeSettlement.Tavern.Quests[selectedQuest];
        if (quest == null)
        {
            return;
        }

        int reward = Dice.Roll(quest.Silver / 2, quest.Silver * 2); // Reward in silver
        int dice = Dice.RollD100();

        if (dice <= 10)
        {
            homeSettlement.Wealth.Add(0, reward); // Add reward to the settlement's wealth
            print($"Tavern quest {quest.Name} was completed when you were away and your settlement earned {reward} silver");
        }
        else
        {
            homeSettlement.Wealth.Subtract(0, reward); // Subtract reward from the settlement's wealth
            print($"Tavern quest {quest.Name} was failed when you were away and your settlement lost {reward} silver");
        }

        homeSettlement.Tavern.Quests.Remove(quest);
    }


    #region Upgrade Settlement
    public void UpgradeSettlement()
    {
        //we will check series of conditions to upgrade the settlement
        //1. Check if the at least one of the residentials is at max level(0-10 for village, 11-15 for castle, 16-20 for town)
        //2. Check if the settlemnts residentials levels are not far between each other(max 2 levels)
        //3. Check if the any residential is not far from the max level(max 5 levels)
        //4. Check if the wealth is enough to upgrade the settlement (1000 silver for village, 5000 silver for castle, 10000 silver for town) can be changed
        //5. Check if the population is enough to upgrade the settlement (100 for village, 500 for castle, 1000 for town) can be changed

        //if all the conditions are met, we will upgrade the settlement
        //if not, we will return and do nothing

        //we will check the conditions here but return if the conditions are not met, put an order to the conditions
        if (!CheckResidentialLevels())
        {
            return;
        }

        if (!CheckResidentialDistance())
        {
            return;
        }

        if (!CheckResidentialMaxLevel())
        {
            return;
        }

        if (!CheckWealth())
        {
            return;
        }

        if (!CheckPopulation())
        {
            return;
        }

        //if all the conditions are met, we will upgrade the settlement
        UpgradeHomeSettlement();
        print("Settlement upgraded");

    }

    public void UpgradeHomeSettlement(int _quality = 1, int _population = 10, int _wealth = 100)
    {
        homeSettlement.Quality += _quality;

        switch (homeSettlement.Quality)
        {
            case > 10 and <= 15:
                homeSettlement.Type = SettlementType.Castle;
                print("Settlement upgraded to Castle");
                GetResidentials().ForEach(residential => residential.ChangeMaxLevel(15));
                break;
            case > 15 and <= 20:
                homeSettlement.Type = SettlementType.Town;
                print("Settlement upgraded to Town");
                GetResidentials().ForEach(residential => residential.ChangeMaxLevel(20));
                break;
            default:
                homeSettlement.Type = SettlementType.Village;
                GetResidentials().ForEach(residential => residential.ChangeMaxLevel(10));
                break;
        }
    }

    public List<Residentials> GetResidentials()
    {
        List<Residentials> residentials = new List<Residentials>
        {
            homeSettlement.TownHall,
            homeSettlement.Walls,
            homeSettlement.Tavern
        };
        residentials.AddRange(homeSettlement.Shops);

        return residentials;
    }

    public bool CheckResidentialLevels()
    {
        //we will check if the at least one of the residentials is at max level(0-10 for village, 11-15 for castle, 16-20 for town)
        //if not, we will return false
        List<Residentials> residentials = GetResidentials();

        foreach (var residential in residentials)
        {
            if (residential.level == residential.maxLevel)
            {
                return true;
            }
        }

        return false;
    }

    public bool CheckResidentialDistance()
    {
        //we will check if the settlemnts residentials levels are not far between each other(max 2 levels)
        //if not, we will return false
        List<Residentials> residentials = GetResidentials();

        int minLevel = residentials[0].level;
        int maxLevel = residentials[0].level;

        foreach (var residential in residentials)
        {
            if (residential.level < minLevel)
            {
                minLevel = residential.level;
            }

            if (residential.level > maxLevel)
            {
                maxLevel = residential.level;
            }
        }

        if (maxLevel - minLevel > 2)
        {
            return false;
        }

        return true;
    }

    public bool CheckResidentialMaxLevel()
    {
        //we will check if the any residential is not far from the max level(max 5 levels)
        //if not, we will return false
        List<Residentials> residentials = GetResidentials();

        foreach (var residential in residentials)
        {
            if (residential.maxLevel - residential.level > 5)
            {
                return false;
            }
        }

        return true;
    }

    public bool CheckWealth()
    {
        //we will check if the wealth is enough to upgrade the settlement (1000 silver for village, 5000 silver for castle, 10000 silver for town) can be changed
        //if not, we will return false
        Currency requiredWealth = new Currency(0, 0);

        switch (homeSettlement.Type)
        {
            case SettlementType.Village:
                requiredWealth = new Currency(0, 1000); // 1000 silver
                break;
            case SettlementType.Castle:
                requiredWealth = new Currency(0, 5000); // 5000 silver
                break;
            case SettlementType.Town:
                requiredWealth = new Currency(0, 10000); // 10000 silver
                break;
            default:
                break;
        }

        return homeSettlement.Wealth.HasEnough(requiredWealth.Gold, requiredWealth.Silver);
    }

    public bool CheckPopulation()
    {
        //we will check if the population is enough to upgrade the settlement (100 for village, 500 for castle, 1000 for town) can be changed
        //if not, we will return false
        int requiredPopulation = 0;

        switch (homeSettlement.Type)
        {
            case SettlementType.Village:
                requiredPopulation = 100;
                break;
            case SettlementType.Castle:
                requiredPopulation = 500;
                break;
            case SettlementType.Town:
                requiredPopulation = 1000;
                break;
            default:
                break;
        }

        if (homeSettlement.Population < requiredPopulation)
        {
            return false;
        }

        return true;
    }

    #endregion

}