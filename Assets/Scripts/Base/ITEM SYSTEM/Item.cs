using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Item
{
    public int ID;
    public string Name;
    public Currency Value;
    public ItemCategory Category;
    public int Quantity;

    public List<StatModifier> Modifiers;
    public Sprite ItemImage;
    public int Quality;

    public bool Stackable;
    public int MaxStack;

    // Tekil ağırlık
    public float UnitWeight;

    // Alias for backward compatibility
    public float Weight { get => UnitWeight; set => UnitWeight = value; }

    // Toplam ağırlık
    public float TotalWeight => UnitWeight * Mathf.Max(1, Quantity);

    // Total value
    public Currency TotalValue => Value * Quantity;

    // Potion-specific effects
    public int HealthRecovery { get; set; }
    public int ExhaustionReduction { get; set; }

    // Magical properties
    public bool IsMagical { get; set; } = false;

    /// <summary>Worn or wielded right now. Derived stats only count equipped items.</summary>
    public bool IsEquipped;

    /// <summary>Which slot this occupies while equipped.</summary>
    public EquipSlot EquippedSlot = EquipSlot.None;

    /// <summary>Armour points this contributes. 0 for anything that is not armour.</summary>
    public int ArmorValue;

    /// <summary>Governs how much dexterity still counts toward defence.</summary>
    public ArmorWeight ArmorClass = ArmorWeight.None;

    /// <summary>
    /// The two rolled properties a crafted or upgraded piece carries.
    ///
    /// Every item made at a forge comes out slightly different: the smith
    /// cannot promise which way it will go, only that a better smith is more
    /// often right. These are what make one shortsword worth keeping and the
    /// next worth selling.
    ///
    /// Shop stock and starting gear have none — mass-produced work is
    /// unremarkable by definition.
    /// </summary>
    public List<GameplayEffect> HiddenEffects = new List<GameplayEffect>();

    public bool HasHiddenEffects => HiddenEffects != null && HiddenEffects.Count > 0;

    public int GetArmorValue() => IsEquipped ? ArmorValue : 0;

    // Full constructor (weapon / armor / misc with modifiers) - backward compatible
    public Item(
        int id,
        string name,
        int gold,
        int silver,
        ItemCategory category,
        List<StatModifier> modifiers,
        Sprite itemImage,
        int quality,
        int quantity = 1)
        : this(id, name, gold, silver, category, modifiers, itemImage, quality, quantity, true, 99, 1f)
    {
    }

    // Full constructor with weight/stack support
    public Item(
        int id,
        string name,
        int gold,
        int silver,
        ItemCategory category,
        List<StatModifier> modifiers,
        Sprite itemImage,
        int quality,
        int quantity,
        bool stackable,
        int maxStack,
        float unitWeight)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));

        ID = id;
        Name = name;
        Value = new Currency(gold, silver);
        Category = category;
        Modifiers = modifiers ?? new List<StatModifier>();
        ItemImage = itemImage;
        Quality = quality;
        Quantity = Mathf.Max(1, quantity);

        Stackable = stackable;
        MaxStack = Mathf.Max(1, maxStack);
        UnitWeight = Mathf.Max(0f, unitWeight);
    }

    // Potion constructor - backward compatible
    public Item(
        int id,
        string name,
        int gold,
        int silver,
        int healthRecovery,
        int exhaustionReduction,
        Sprite itemImage,
        int quality,
        int quantity = 1)
        : this(id, name, gold, silver, healthRecovery, exhaustionReduction, itemImage, quality, quantity, true, 99, 0.5f)
    {
    }

    // Potion constructor with weight/stack support
    public Item(
        int id,
        string name,
        int gold,
        int silver,
        int healthRecovery,
        int exhaustionReduction,
        Sprite itemImage,
        int quality,
        int quantity,
        bool stackable,
        int maxStack,
        float unitWeight)
    {
        if (string.IsNullOrWhiteSpace(name))
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

        Modifiers = new List<StatModifier>();
        Stackable = stackable;
        MaxStack = Mathf.Max(1, maxStack);
        UnitWeight = Mathf.Max(0f, unitWeight);
    }

    // Simple constructor (resource / crafting material) - backward compatible
    public Item(int id, string name, int gold, int silver, ItemCategory category, int quantity = 1)
        : this(id, name, gold, silver, category, quantity, true, 99, 1f)
    {
    }

    // Simple constructor with weight/stack support
    public Item(
        int id,
        string name,
        int gold,
        int silver,
        ItemCategory category,
        int quantity,
        bool stackable,
        int maxStack,
        float unitWeight)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));

        ID = id;
        Name = name;
        Value = new Currency(gold, silver);
        Category = category;
        Quantity = Mathf.Max(1, quantity);

        Modifiers = new List<StatModifier>();
        ItemImage = null;
        Quality = 1;

        Stackable = stackable;
        MaxStack = Mathf.Max(1, maxStack);
        UnitWeight = Mathf.Max(0f, unitWeight);
    }

    public bool CheckRequiredAmount(int requiredAmount)
    {
        return Quantity >= requiredAmount;
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
                Stackable,
                MaxStack,
                UnitWeight
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
                Stackable,
                MaxStack,
                UnitWeight
            );
        }

        return new Item(
            ID,
            Name,
            Value.Gold,
            Value.Silver,
            Category,
            finalQuantity,
            Stackable,
            MaxStack,
            UnitWeight
        );
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

    public Currency GetSingleValue()
    {
        return Value;
    }

    public Currency GetTotalValue()
    {
        return TotalValue;
    }

    public override string ToString()
    {
        return $"{Name} (ID: {ID}, Category: {Category}, Value: {Value}, Quantity: {Quantity}, UnitWeight: {UnitWeight}, TotalWeight: {TotalWeight})";
    }
}

[Serializable]
public class StatModifier
{
    public StatType Type;
    public int Value;
    public string Source { get; set; }

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

/// <summary>
/// What an item IS. Where it goes is EquipSlot — a two-handed sword is one
/// category but occupies two slots, and a shield and an off-hand dagger share
/// a slot while being different categories.
///
/// Existing members keep their original order so saved data and Inspector
/// references stay valid; new members are appended.
/// </summary>
public enum ItemCategory
{
    Weapon,
    Armor,              // chest / body armour
    Boots,
    Leggings,
    Potion,
    CraftingMaterial,   // processed: ingots, tanned leather, planks
    Resource,           // raw: ore, hide, timber, herbs
    Misc,

    Shield,
    Helmet,
    Gloves,
    Trinket,            // amulet, ring, cloak, lantern — never craftable
    Consumable,         // food, bandages; Potion stays for existing data
    TradeGood,          // no use, sells high
    QuestItem           // cannot be sold or dropped
}