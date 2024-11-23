using System.Collections.Generic;
using UnityEngine;

public enum SettlementType
{
    Village,
    Castle,
    Town,
    defaultSettlement
}

[System.Serializable]
public class Settlement
{
    public string Name;
    public int Population;
    public int Wealth;
    public int Quality;
    public List<Shops> Shops;
    public Taverns Tavern;
    public TownHalls TownHall;
    public Walls Walls;

    public SettlementType Type;

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

    public delegate void TownHallEntered(TownHalls townHall);
    public event TownHallEntered OnTownHallEntered;

    public delegate void WallEntered(Walls wall);
    public event WallEntered OnWallEntered;

    public delegate void SettlementUpgraded();
    public event SettlementUpgraded OnSettlementUpgraded;

    public Settlement()
    {
        Name = "Settlement";
        Population = 0;
        Wealth = 0;
        Quality = 0;
        Shops = new List<Shops>();
        Shops shops = new Shops();
        shops.Name = "Shop";
        Shops.Add(shops);
        Tavern = new Taverns();
        Tavern.Name = "Tavern";
        TownHall = new TownHalls();
        TownHall.Name = "Town Hall";
        Walls = new Walls();
        Walls.Name = "Wall";

        Type = SettlementType.defaultSettlement;
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

    public void EnterTownHall(TownHalls townHall)
    {
        TownHall = townHall;
        OnTownHallEntered?.Invoke(townHall);
    }

    public void EnterWall(Walls wall)
    {
        Walls = wall;
        OnWallEntered?.Invoke(wall);
    }

    public void UpgradeSettlement()
    {
        switch (Type)
        {
            case SettlementType.Village:
                Type = SettlementType.Castle;
                break;
            case SettlementType.Castle:
                Type = SettlementType.Town;
                break;
        }

        OnSettlementUpgraded?.Invoke();
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
