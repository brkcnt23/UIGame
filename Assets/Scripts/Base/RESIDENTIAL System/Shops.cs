using System.Collections.Generic;
using UnityEngine;

public enum ShopTypes
{
    Blacksmith,
    Tanner,
    Carpenter,
    Alchemist,
    Mason,
    GeneralStore,
    Mystic,
    defaultShop
}

[System.Serializable]
public class Shops : Residentials
{
    public string ShopId;                  // Unique per-settlement shop id
    public string StockProfileId;          // SO profile lookup key
    public string OwnerNpcId;              // NPC binding
    public List<string> ShopTags = new();  // rare_stock, noble_clientele, frontier, etc.

    public ShopTypes ShopType;

    // Runtime / legacy
    public List<Item> Items;
    public List<ShopItemEntry> ItemEntries;

    // Economy
    public Currency Cash;
    public float BuyMultiplier = 0.6f;
    public float SellMultiplier = 1.0f;
    public int MaxAffordableItemQuality = 10;
    public List<ItemCategory> AcceptedCategories = new();

    public Shops()
    {
        ShopId = string.Empty;
        StockProfileId = string.Empty;
        OwnerNpcId = string.Empty;

        ShopType = ShopTypes.defaultShop;

        Items = new List<Item>();
        ItemEntries = new List<ShopItemEntry>();
        ShopTags = new List<string>();

        Cash = new Currency(0, 0);
        BuyMultiplier = 0.6f;
        SellMultiplier = 1.0f;
        MaxAffordableItemQuality = 10;
        AcceptedCategories = new List<ItemCategory>();
    }

    public void AddItem(Item item)
    {
        if (item == null) return;
        Items.Add(item);
    }

    public void RemoveItem(Item item)
    {
        if (item == null) return;
        Items.Remove(item);
    }

    public List<Item> GetItemsByCategory(ItemCategory category)
    {
        return Items.FindAll(item => item != null && item.Category == category);
    }

    public void ClearRuntimeItems()
    {
        Items.Clear();
    }

    public bool CanAcceptCategory(ItemCategory category)
    {
        if (AcceptedCategories == null || AcceptedCategories.Count == 0)
            return true;

        return AcceptedCategories.Contains(category);
    }

    public bool AcceptsItem(Item item)
    {
        return item != null && CanAcceptCategory(item.Category);
    }

    public Currency GetSellPrice(Item item)
    {
        if (item == null)
            return new Currency(0, 0);

        return CalculatePrice(item.GetSingleValue(), SellMultiplier);
    }

    public Currency GetBuyPrice(Item item)
    {
        if (item == null)
            return new Currency(0, 0);

        return CalculatePrice(item.GetSingleValue(), BuyMultiplier);
    }

    public bool CanAfford(Currency amount)
    {
        return Cash.HasEnough(amount.Gold, amount.Silver);
    }

    public void AddCash(int gold, int silver)
    {
        Cash.Add(gold, silver);
    }

    public bool TrySpendCash(int gold, int silver)
    {
        if (!CanAfford(new Currency(gold, silver)))
            return false;

        Cash.Subtract(gold, silver);
        return true;
    }

    private Currency CalculatePrice(Currency baseValue, float multiplier)
    {
        int totalSilver = baseValue.Gold * 100 + baseValue.Silver;
        int finalSilver = Mathf.RoundToInt(totalSilver * multiplier);
        return new Currency(finalSilver / 100, finalSilver % 100);
    }

    public override void LevelUpResidential(ref PlayerData player)
    {
        base.LevelUpResidential(ref player);
        upgradeHour = CalculateUpgradeHour(player);
    }
}

[System.Serializable]
public class ShopItemEntry
{
    public int ItemId;
    public int Quantity = 1;
    public int GoldOverride = -1;
    public int SilverOverride = -1;
}