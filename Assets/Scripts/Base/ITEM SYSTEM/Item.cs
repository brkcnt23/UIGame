using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Item
{
    public int ID;                 // Unique identifier for the item
    public string Name;            // Name of the item
    public Currency Value;                // Value of the item in silver
    public ItemCategory Category;  // Category of the item
    public int Quantity;           // Quantity for stackable items like resources

    // Stat modifiers for weapons and armor
    public List<StatModifier> Modifiers;  // Stat modifiers list
    public Sprite ItemImage;       // Image representing the item
    public int Quality;            // Quality or level of the item (e.g., 1, 2, 3)

    // Potion-specific effects
    public int HealthRecovery { get; set; }      // Health recovery for potions
    public int ExhaustionReduction { get; set; } // Exhaustion reduction for potions
                                                 // Constructor for weapons and armor

    public Item(int id, string name, int gold, int silver, ItemCategory category,
                List<StatModifier> modifiers, Sprite itemImage, int quality, int quantity = 1)
    {
        if (string.IsNullOrEmpty(name))
            throw new System.ArgumentException("Name cannot be null or empty.", nameof(name));

        ID = id;
        Name = name;
        Value = new Currency(gold, silver);
        Category = category;
        Modifiers = modifiers;
        ItemImage = itemImage;
        Quality = quality;
        Quantity = quantity;
    }
    // Constructor for potions
    public Item(int id, string name, int gold, int silver, int healthRecovery, int exhaustionReduction,Sprite itemImage,int quality, int quantity = 1)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));

        ID = id;
        Name = name;
        Value = new Currency(gold, silver);
        Category = ItemCategory.Potion;
        HealthRecovery = healthRecovery;
        ExhaustionReduction = exhaustionReduction;
        ItemImage = itemImage;
        Quality = quality;
        Quantity = quantity;
    }

    // Constructor for crafting materials and resources
    public Item(int id, string name, int gold, int silver, ItemCategory category, int quantity = 1)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));

        ID = id;
        Name = name;
        Value = new Currency(gold, silver);
        Category = category;
        Quantity = quantity;
    }

    public bool CheckRequiredAmount(int requiredAmount)
    {
        return Quantity >= requiredAmount;
    }

    public override string ToString()
    {
        return $"{Name} (ID: {ID}, Category: {Category}, Value: {Value}, Quantity: {Quantity})";
    }

}

[Serializable]
public class StatModifier
{
    public StatType Type;
    public int Value;
    public string Source { get; set; } // Optional, e.g., "Blacksmith Item"

    public StatModifier(StatType type, int value)
    {
        Type = type;
        Value = value;
    }
    public StatModifier(StatType type, int value, string source = "")
    {
        Type = type;
        Value = value;
        Source = source;
    }
}

public enum ItemCategory
{
    Weapon,
    Armor,
    Boots,
    Leggings,
    Potion,
    CraftingMaterial,
    Resource,
    Misc
}
