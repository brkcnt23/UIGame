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
    public Currency Wealth;
    public int Quality;
    public List<Shops> Shops;
    public Taverns Tavern;
    public TownHalls TownHall;
    public Walls Walls;

    public string RulerNpcId;

    /// <summary>
    /// What this place is good at: mining, forestry, pastoral, quarry,
    /// farming, trade_hub, remote. Pricing reads these — a mining town sells
    /// ore cheap and pays badly for more of it, which is what makes a trade
    /// route worth walking.
    /// </summary>
    public List<string> SettlementTags = new();

    public List<string> CultureTags = new();

    /// <summary>
    /// Production buildings. A crafter's level is the ceiling on what can be
    /// made here, so a level 2 forge cannot produce steel no matter how good
    /// the player's own smithing is.
    /// </summary>
    public List<CraftStation> Crafters = new();

    /// <summary>
    /// Map position. Lives in data rather than in the scene so a settlement
    /// can be added by editing JSON instead of by placing a GameObject.
    /// </summary>
    public float MapX;
    public float MapY;

    public SettlementType Type;

    public delegate void SettlementEntered(Settlement settlement);
    public event SettlementEntered OnSettlementEntered;

    public delegate void SettlementExited();
    public event SettlementExited OnSettlementExited;

    public delegate void SettlementUnlocked(Settlement settlement);
    public event SettlementUnlocked OnSettlementUnlocked;

    public delegate void PopulationChanged(int population);
    public event PopulationChanged OnPopulationChanged;

    public delegate void WealthChanged(Currency wealth);
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

    // TODO: Remove if not used
    // public delegate void SettlementUpgraded();
    // public event SettlementUpgraded OnSettlementUpgraded;

    public Settlement()
    {
        Name = "";
        Population = 0;
        Wealth = new Currency(0, 0);
        Quality = 0;

        Shops = new List<Shops>(); // default boş kalsın, otomatik dummy shop ekleme

        Tavern = new Taverns();
        Tavern.Name = "Tavern";

        TownHall = new TownHalls();
        TownHall.Name = "Town Hall";

        Walls = new Walls();
        Walls.Name = "Wall";

        Type = SettlementType.defaultSettlement;
        RulerNpcId = string.Empty;
        SettlementTags = new List<string>();
        CultureTags = new List<string>();
    }

    public Settlement(Quest_SO_Constructor quest)
    {
        Type = SettlementType.Quest;
        Name = quest.questLocation;
        ID = quest.settlementID;
        Population = 0;
        Wealth = new Currency(0, 0);
        Quality = 0;
        Shops = new List<Shops>();
        Tavern = new Taverns();
        Tavern.Quests.Add(quest);
        TownHall = new TownHalls();
        Walls = new Walls();
        RulerNpcId = string.Empty;
        SettlementTags = new List<string>();
        CultureTags = new List<string>();
    }

    public void AddPopulation(int population)
    {
        Population += population;
        OnPopulationChanged?.Invoke(Population);
    }

    public void AddWealth(int wealth)
    {
        Wealth.Gold += wealth;
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
