using System.Collections.Generic;
using UnityEngine;

public enum SettlementType
{
    Village,
    Castle,
    Town,
    Quest,
    defaultSettlement
}

[System.Serializable]
public class Settlement
{
    public string Name;
    public int ID;
    public bool isUnlocked;
    public int levelToUnlock;
    public int Population;
    public int Wealth;
    public int Quality;
    public List<Shops> Shops;
    public Taverns Tavern;
    public TownHalls TownHall;
    public Walls Walls;

    public SettlementType Type;

    public delegate void SettlementEntered(Settlement settlement);
    public event SettlementEntered OnSettlementEntered;

    public delegate void SettlementExited();
    public event SettlementExited OnSettlementExited;

    public delegate void SettlementUnlocked(Settlement settlement);
    public event SettlementUnlocked OnSettlementUnlocked;

    public delegate void PopulationChanged(int population);
    public event PopulationChanged OnPopulationChanged;

    public delegate void WealthChanged(int wealth);
    public event WealthChanged OnWealthChanged;

    public delegate void QualityChanged(int quality);
    public event QualityChanged OnQualityChanged;

    public delegate void ShopAdded(Shops shop);
    public event ShopAdded OnShopAdded;

    public delegate void ShopEntered(Shops shop);
    public event ShopEntered OnShopEntered;

    public delegate void TavernEntered(Taverns tavern);
    public event TavernEntered OnTavernEntered;

    public delegate void TownHallEntered();
    public event TownHallEntered OnTownHallEntered;

    public delegate void WallEntered(Walls wall);
    public event WallEntered OnWallEntered;

    public delegate void SettlementUpgraded();
    public event SettlementUpgraded OnSettlementUpgraded;

    public Settlement()
    {
        Name = "";
        Population = 0;
        Wealth = 0;
        Quality = 0;
        Shops = new List<Shops>();
        Shops shop = new Shops();
        shop.Name = "Shop";
        Shops.Add(shop);
        Tavern = new Taverns();
        Tavern.Name = "Tavern";
        TownHall = new TownHalls();
        TownHall.Name = "Town Hall";
        Walls = new Walls();
        Walls.Name = "Wall";

        Type = SettlementType.defaultSettlement;
    }
    public Settlement(Quest_SO_Constructor quest)
    {
        Type = SettlementType.Quest;
        Name = quest.questLocation;
        Population = 0;
        Wealth = 0;
        Quality = 0;
        Tavern = new Taverns();
        Tavern.Quests.Add(quest);
    }

    public void AddPopulation(int population)
    {
        Population += population;
        OnPopulationChanged?.Invoke(Population);
    }

    public void AddWealth(int wealth)
    {
        Wealth += wealth;
        OnWealthChanged?.Invoke(Wealth);
    }

    public void AddQuality(int quality)
    {
        Quality += quality;
        OnQualityChanged?.Invoke(Quality);
    }

    public void AddShop(Shops shop)
    {
        Shops.Add(shop);
        OnShopAdded?.Invoke(shop);
    }

    public void EnterTavern(Taverns tavern)
    {
        Tavern = tavern;
        OnTavernEntered?.Invoke(tavern);
    }

    public void EnterShop(Shops shop)
    {
        OnShopEntered?.Invoke(shop);
    }

    public void EnterTownHall()
    {
        OnTownHallEntered?.Invoke();
    }

    public void EnterWall(Walls wall)
    {
        Walls = wall;
        OnWallEntered?.Invoke(wall);
    }

    public void UnlockSettlement(Settlement settlement)
    {
        settlement.isUnlocked = true;
        OnSettlementUnlocked?.Invoke(settlement);
    }

    public void ExitSettlement()
    {
        OnSettlementExited?.Invoke();
    }

    public void EnterSettlement()
    {
        OnSettlementEntered?.Invoke(this);
    }

    void Print(string message)
    {
        Debug.Log($"{message}\nSender:\"{this.GetType().Name}\" class in \"{this.Name}\"");
    }

    public void PrintResidentialInfo()
    {
        Print($"Name: {Name}\nPopulation: {Population}\nWealth: {Wealth}\nQuality: {Quality}\nTavern: {Tavern.Name}\nTownHall: {TownHall.Name}\nWall: {Walls.Name}\nHas {Shops.Count} shops:");
        foreach (var shop in Shops)
        {
            Debug.Log($"Shop: {shop.Name} Level: {shop.level}");
        }
    }
}
