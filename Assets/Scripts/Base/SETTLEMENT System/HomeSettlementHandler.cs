using UnityEngine;
using NEXUS.Utilities;
using Unity.Mathematics;
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

        homeSettlement = PlayerStatHandler.Instance.homeSettlement;
    }

    //this will be the home settlement
    public Settlement homeSettlement = new Settlement();

    //this will be the home settlement wrapper
    JSONDataHandler JSONhandler;

    void OnEnable()
    {
        homeSettlement.OnTownHallEntered += HandleTownHallEntered;
        Wrappers();
    }

    void OnDisable()
    {
        homeSettlement.OnTownHallEntered -= HandleTownHallEntered;
        SaveHomeSettlement();
    }

    //this will be the function that will load the home settlement data
    public void Wrappers()
    {
        JSONhandler = new JSONDataHandler(PlayerPrefs.GetInt("Slot"));
        HomeSettlementWrapper wrapper = JSONhandler.LoadData<HomeSettlementWrapper>("homeSettlement.json");
        homeSettlement = wrapper != null ? wrapper.homeSettlement : new Settlement();
    }

    //this will be the function that will save the home settlement data
    public void SaveHomeSettlement()
    {
        JSONhandler.SaveData(new HomeSettlementWrapper { homeSettlement = homeSettlement }, "homeSettlement.json");
    }

    //this will be the function that will handle the home settlements upgrades
    public void UpgradeHomeSettlement(int _quality = 1, int _population = 10, int _wealth = 100)
    {
        homeSettlement.Quality += _quality;
        homeSettlement.Population += _population;
        homeSettlement.Wealth += _wealth;

        switch (homeSettlement.Quality)
        {
            case 9:
                homeSettlement.Type = SettlementType.Castle;
                break;
            case 15:
                homeSettlement.Type = SettlementType.Town;
                break;
            default:
                homeSettlement.Type = SettlementType.Village;
                break;
        }

        SaveHomeSettlement();
    }

    public void UpgradeHomeSettlementHelper()
    {
        UpgradeHomeSettlement();
    }

    public void HandleTownHallEntered()
    {
        homeSettlement.EnterTownHall();
        GameManager.Instance.ShowSettlementPanel();
        GameManager.Instance.homeSettlementPanel.SetActive(true);
    }

    public void OnSettlmentEntered()
    {
        GenerateRandomHappenings();
    }

    public void GenerateRandomHappenings()
    {
        int dice = Dice.RollD100();

        if (dice <= 10)
        {
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

        SaveHomeSettlement();
    }

    public void RandomWealthEvent()
    {
        int wealth = Dice.Roll(-300, 300);

        homeSettlement.Wealth += wealth;

        print(wealth > 0 ? "Merchants sold their goods" : "Merchants bought goods" + " when you were away");

        SaveHomeSettlement();
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

        SaveHomeSettlement();
    }

    public void RandomShopEvent()
    {
        int selectedShop = Dice.Roll(homeSettlement.Shops.Count);
        Shops shop = homeSettlement.Shops[selectedShop];
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
        if (quantity != 0)
        {
            item.AdjustValue(quantity); // Adjust value based on quantity change

            if (quantity > 0)
            {
                print($"Shop {shop.Name} bought {item.Name} x{quantity} when you were away and spent {item.Value * quantity} silver");

                homeSettlement.Wealth -= item.Value * quantity;
            }
            else
            {
                print($"Shop {shop.Name} sold {item.Name} x{-quantity} when you were away and made {item.Value * -quantity} silver");

                homeSettlement.Wealth += item.Value * -quantity;
            }
        }

        SaveHomeSettlement();
    }

    public void RandomTavernEvent()
    {
        int selectedQuest = Dice.Roll(homeSettlement.Tavern.Quests.Count);
        Quest_SO_Constructor quest = homeSettlement.Tavern.Quests[selectedQuest];
        if (quest == null)
        {
            return;
        }

        int reward = Dice.Roll(quest.Silver / 2, quest.Silver * 2);
        int dice = Dice.RollD100();

        if (dice <= 10)
        {
            homeSettlement.Wealth += reward;
            print($"Tavern quest {quest.Name} was completed when you were away and your settlement earned {reward} silver");
        }
        else
        {
            homeSettlement.Wealth -= reward;
            print($"Tavern quest {quest.Name} was failed when you were away and you settlement lost {reward} silver");
        }

        SaveHomeSettlement();
    }

}