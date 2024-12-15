using System;
using System.Collections.Generic;

[Serializable]
public class Item
{
    public int ID;                 // Unique identifier for the item
    public string Name;            // Name of the item
    public int Value;              // Value of the item in silver
    public ItemCategory Category;  // Category of the item
    public int Quantity;           // Quantity for stackable items like resources

    // Stat modifiers for weapons and armor
    public List<StatModifier> Modifiers;  // Stat modifiers list

    // Potion-specific effects
    public int HealthRecovery { get; set; }      // Health recovery for potions
    public int ExhaustionReduction { get; set; } // Exhaustion reduction for potions
    // Constructor for weapons and armor
    public Item(int id, string name, int value, ItemCategory category,
                List<StatModifier> modifiers, int quantity = 1)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));

        ID = id;
        Name = name;
        Value = value;
        Category = category;
        Modifiers = modifiers;
        Quantity = quantity;
    }

    // Constructor for potions
    public Item(int id, string name, int value, int healthRecovery, int exhaustionReduction, int quantity = 1)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));

        ID = id;
        Name = name;
        Value = value;
        Category = ItemCategory.Potion;
        HealthRecovery = healthRecovery;
        ExhaustionReduction = exhaustionReduction;
        Quantity = quantity;
    }

    // Constructor for crafting materials and resources
    public Item(int id, string name, int value, ItemCategory category, int quantity = 1)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));

        ID = id;
        Name = name;
        Value = value;
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
    public StatType Stat;
    public int Value;
}

public enum ItemCategory
{
    Weapon,
    Armor,
    Boots,
    Potion,
    CraftingMaterial,
    Resource,
    Misc
}
