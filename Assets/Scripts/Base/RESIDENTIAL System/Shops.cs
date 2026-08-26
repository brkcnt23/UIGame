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
    public string ShopId;
    public string StockProfileId;
    public string OwnerNpcId;
    public List<string> ShopTags = new List<string>();

    public ShopTypes ShopType;

    // Runtime / legacy
    public List<Item> Items;
    public List<ShopItemEntry> ItemEntries;

    // Economy
    /// <summary>
    /// The settlement this shop stands in.
    ///
    /// Prices depend on the town - its wealth, what is mined or grown nearby -
    /// and a shop deserialised from JSON has no way to look upward on its own.
    /// SettlementHandler fills this in on entry. When it is null pricing still
    /// works, it just loses the local half of the market.
    /// </summary>
    [System.NonSerialized] public Settlement Owner;

    public Currency Cash;
    public float BuyMultiplier = 0.6f;
    public float SellMultiplier = 1.0f;
    public int MaxAffordableItemQuality = 10;
    public List<ItemCategory> AcceptedCategories = new List<ItemCategory>();

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
        if (Items == null)
            Items = new List<Item>();
        else
            Items.Clear();
    }

    public bool CanAcceptCategory(ItemCategory category)
    {
        if (AcceptedCategories == null || AcceptedCategories.Count == 0)
            return true;

        return AcceptedCategories.Contains(category);
    }

    public void DisplayInventory()
    {
        Debug.Log($"Shop: {Name} (Type: {ShopType}) Inventory:");
        foreach (var item in Items)
        {
            if (item == null) continue;
            Debug.Log($"- {item.Name} (Category: {item.Category}, Value: {item.Value}, Quantity: {item.Quantity})");
        }
    }

    public bool AcceptsItem(Item item)
    {
        return item != null && CanAcceptCategory(item.Category);
    }

    /// <summary>What the shop pays the player for one unit.</summary>
    public Currency GetSellPrice(Item item)
        => Priced(item, buying: false, fallbackMultiplier: BuyMultiplier);

    /// <summary>What the player pays the shop for one unit.</summary>
    public Currency GetBuyPrice(Item item)
        => Priced(item, buying: true, fallbackMultiplier: SellMultiplier);

    /// <summary>
    /// Every price in the game comes from PricingSystem, which knows about the
    /// town's wealth, what is abundant nearby, this shop's level and till, and
    /// the player's haggling. This class used to multiply the item's face value
    /// by a flat number, which meant the whole trade economy that had been
    /// written was never what a player actually paid.
    ///
    /// The flat calculation survives only as a fallback for an item with no
    /// catalogue entry behind it - generated shop stock, mostly - because those
    /// have no ItemSO for the real formula to read.
    /// </summary>
    private Currency Priced(Item item, bool buying, float fallbackMultiplier)
    {
        if (item == null)
            return new Currency(0, 0);

        var template = GameBootstrapper.Resources != null
            ? GameBootstrapper.Resources.GetItemDatabase()?.GetByID(item.ID)
            : null;

        if (template == null)
            return CalculatePrice(item.GetSingleValue(), fallbackMultiplier);

        var player = PlayerStatHandler.Instance != null ? PlayerStatHandler.Instance.pd : null;

        int silver = buying
            ? PricingSystem.GetBuyPrice(item, template, this, Owner, player)
            : PricingSystem.GetSellPrice(item, template, this, Owner, player);

        return new Currency(silver / 100, silver % 100);
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