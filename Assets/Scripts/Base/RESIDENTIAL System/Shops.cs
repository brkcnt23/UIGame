using System.Collections.Generic;
using UnityEngine;

public enum ShopTypes
{
    Blacksmith,    // Weapons, armors
    Tanner,        // Leather goods
    Carpenter,     // Wooden items
    Alchemist,     // Potions and rare ingredients
    Mason,         // Building materials
    GeneralStore,  // General items and resources
    Mystic,        // Rare, magical items
    defaultShop    // Fallback/default shop
}

[System.Serializable]
public class Shops : Residentials
{
    public ShopTypes ShopType;                  // Type of shop
    public List<Item> Items;                    // For JSON deserialization

    public Shops()
    {
        ShopType = ShopTypes.defaultShop;
        Items = new List<Item>();
    }

    /// <summary>
    /// Add an item to the shop's inventory.
    /// </summary>
    /// <param name="item">The item to add.</param>
    public void AddItem(Item item)
    {
        Items.Add(item);
    }

    /// <summary>
    /// Remove an item from the shop's inventory.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    public void RemoveItem(Item item)
    {
        if (Items.Contains(item))
        {
            Items.Remove(item);
        }
    }


    /// <summary>
    /// Get all items of a specific category.
    /// </summary>
    /// <param name="category">The item category to filter by.</param>
    /// <returns>A list of items in the specified category.</returns>
    public List<Item> GetItemsByCategory(ItemCategory category)
    {
        return Items.FindAll(item => item.Category == category);
    }

    /// <summary>
    /// Display the shop's inventory in the console for debugging.
    /// </summary>
    public void DisplayInventory()
    {
        Debug.Log($"Shop: {Name} (Type: {ShopType}) Inventory:");
        foreach (var item in Items)
        {
            Debug.Log($"- {item.Name} (Category: {item.Category}, Value: {item.Value} silver)");
        }
    }

    public override void LevelUpResidential(ref PlayerData _Player)
    {
        base.LevelUpResidential(ref _Player);
        upgradeHour = CalculateUpgradeHour(_Player);

        foreach (var item in Items)
        {
            //item.AdjustValue(level);
        }
    }
}