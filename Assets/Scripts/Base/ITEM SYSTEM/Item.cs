using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Item
{
    public int ID;                 // Unique identifier for the item
    public string Name;            // Name of the item
    public Currency Value;         // Single item value
    public ItemCategory Category;  // Category of the item
    public int Quantity;           // Quantity for stackable items like resources

    // New logistics fields
    public float Weight;           // Single item weight
    public bool Stackable;         // Can this item stack?
    public int MaxStack;           // Maximum stack size

    // Stat modifiers for weapons and armor
    public List<StatModifier> Modifiers;
    public Sprite ItemImage;
    public int Quality;

    // Potion-specific effects
    public int HealthRecovery { get; set; }
    public int ExhaustionReduction { get; set; }

    // Computed helpers
    public float TotalWeight => Weight * Quantity;
    public Currency TotalValue => Value * Quantity;

    // -----------------------------
    // CONSTRUCTORS
    // -----------------------------

    // Constructor for equippable items (weapon, armor, boots, leggings, misc with modifiers, etc.)
    public Item(
        int id,
        string name,
        int gold,
        int silver,
        ItemCategory category,
        List<StatModifier> modifiers,
        Sprite itemImage,
        int quality,
        int quantity = 1,
        float weight = 1f,
        bool stackable = false,
        int maxStack = 1)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));

        ID = id;
        Name = name;
        Value = new Currency(gold, silver);
        Category = category;
        Modifiers = modifiers ?? new List<StatModifier>();
        ItemImage = itemImage;
        Quality = quality;
        Quantity = Mathf.Max(1, quantity);

        Weight = Mathf.Max(0f, weight);
        Stackable = stackable;
        MaxStack = Mathf.Max(1, maxStack);
    }

    // Constructor for potions
    public Item(
        int id,
        string name,
        int gold,
        int silver,
        int healthRecovery,
        int exhaustionReduction,
        Sprite itemImage,
        int quality,
        int quantity = 1,
        float weight = 0.5f,
        bool stackable = true,
        int maxStack = 10)
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
        Quantity = Mathf.Max(1, quantity);

        Weight = Mathf.Max(0f, weight);
        Stackable = stackable;
        MaxStack = Mathf.Max(1, maxStack);

        Modifiers = new List<StatModifier>();
    }

    // Constructor for resources / crafting materials / simple stackables
    public Item(
        int id,
        string name,
        int gold,
        int silver,
        ItemCategory category,
        int quantity = 1,
        float weight = 1f,
        bool stackable = true,
        int maxStack = 99)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));

        ID = id;
        Name = name;
        Value = new Currency(gold, silver);
        Category = category;
        Quantity = Mathf.Max(1, quantity);

        Weight = Mathf.Max(0f, weight);
        Stackable = stackable;
        MaxStack = Mathf.Max(1, maxStack);

        Modifiers = new List<StatModifier>();
        Quality = 1;
    }

    // -----------------------------
    // HELPERS
    // -----------------------------

    public bool CheckRequiredAmount(int requiredAmount)
    {
        return Quantity >= requiredAmount;
    }

    public bool IsEquippable()
    {
        return Category == ItemCategory.Weapon ||
               Category == ItemCategory.Armor ||
               Category == ItemCategory.Boots ||
               Category == ItemCategory.Leggings ||
               Category == ItemCategory.Misc;
    }

    public bool IsConsumable()
    {
        return Category == ItemCategory.Potion;
    }

    public bool IsStackableItem()
    {
        return Stackable;
    }

    public bool CanStackWith(Item other)
    {
        if (other == null) return false;

        return Stackable &&
               other.Stackable &&
               ID == other.ID &&
               Category == other.Category &&
               Quality == other.Quality;
    }

    public int GetRemainingStackSpace()
    {
        return MaxStack - Quantity;
    }

    public Currency GetSingleValue()
    {
        return Value;
    }

    public Currency GetTotalValue()
    {
        return TotalValue;
    }

    public Item Clone(int quantityOverride = -1)
    {
        int finalQuantity = quantityOverride > 0 ? quantityOverride : Quantity;

        if (Category == ItemCategory.Potion)
        {
            return new Item(
                ID,
                Name,
                Value.Gold,
                Value.Silver,
                HealthRecovery,
                ExhaustionReduction,
                ItemImage,
                Quality,
                finalQuantity,
                Weight,
                Stackable,
                MaxStack
            );
        }

        if (Modifiers != null && Modifiers.Count > 0)
        {
            return new Item(
                ID,
                Name,
                Value.Gold,
                Value.Silver,
                Category,
                new List<StatModifier>(Modifiers),
                ItemImage,
                Quality,
                finalQuantity,
                Weight,
                Stackable,
                MaxStack
            );
        }

        return new Item(
            ID,
            Name,
            Value.Gold,
            Value.Silver,
            Category,
            finalQuantity,
            Weight,
            Stackable,
            MaxStack
        );
    }

    public override string ToString()
    {
        return $"{Name} (ID: {ID}, Category: {Category}, Value: {Value}, Quantity: {Quantity}, Weight: {Weight}, TotalWeight: {TotalWeight})";
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