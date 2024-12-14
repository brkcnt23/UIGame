using System;

[Serializable]
public class Item
{
    public int ID;                 // Unique identifier for the item
    public string Name;            // Name of the item
    public int Value;              // Value of the item in silver
    public ItemCategory Category;  // Category of the item
    public int Quantity;           // Quantity for stackable items like resources

    // Stat modifiers for weapons and armor
    public int StrengthModifier { get; set; }
    public int ConstitutionModifier { get; set; }
    public int DexterityModifier { get; set; }
    public int CharismaModifier { get; set; }

    // Potion-specific effects
    public int HealthRecovery { get; set; }      // Health recovery for potions
    public int ExhaustionReduction { get; set; } // Exhaustion reduction for potions
    // Constructor for weapons and armor
    public Item(int id, string name, int value, ItemCategory category,
                int strengthModifier, int constitutionModifier, int dexterityModifier, int charismaModifier, int quantity = 1)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));

        ID = id;
        Name = name;
        Value = value;
        Category = category;
        StrengthModifier = strengthModifier;
        ConstitutionModifier = constitutionModifier;
        DexterityModifier = dexterityModifier;
        CharismaModifier = charismaModifier;
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

    public void TestItems()
    {
        Item ironSword = new Item(1, "Iron Sword", 150, ItemCategory.Weapon, 5, 0, 0, 0);
        Item leatherArmor = new Item(2, "Leather Armor", 200, ItemCategory.Armor, 0, 5, 0, 0);

        Item healthPotion = new Item(3, "Health Potion", 50, healthRecovery: 20, exhaustionReduction: 0);
        Item energyPotion = new Item(4, "Energy Potion", 75, healthRecovery: 0, exhaustionReduction: 10);

        Item ironIngot = new Item(5, "Iron Ingot", 100, ItemCategory.CraftingMaterial, quantity: 1);
        Item leather = new Item(6, "Leather", 50, ItemCategory.CraftingMaterial, quantity: 5);
        Item herbs = new Item(7, "Herbs", 30, ItemCategory.CraftingMaterial, quantity: 10);

        Item stone = new Item(8, "Stone", 40, ItemCategory.Resource, quantity: 3);
        Item wood = new Item(9, "Wood", 30, ItemCategory.Resource, quantity: 4);

    }
}


public enum ItemCategory
{
    Weapon,
    Armor,
    Potion,
    CraftingMaterial,
    Resource,
    Misc
}
