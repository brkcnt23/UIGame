using System;

[Serializable]
public class Item
{
    public int ID;                 // Unique identifier for the item
    public string Name;            // Name of the item
    public int Value;              // Value of the item in silver
    public ItemCategory Category;  // Category of the item
    public int StrengthModifier;   // Strength buff
    public int ConstitutionModifier; // Constitution buff
    public int DexterityModifier;  // Dexterity buff
    public int CharismaModifier;   // Charisma buff
    public int Quantity;           // Quantity for stackable items like resources
    private int ıD;

    public Item(int ıD, string name, int value, ItemCategory category, int strengthModifier, int constitutionModifier, int dexterityModifier, int charismaModifier)
    {
        this.ıD = ıD;
        Name = name;
        Value = value;
        Category = category;
        StrengthModifier = strengthModifier;
        ConstitutionModifier = constitutionModifier;
        DexterityModifier = dexterityModifier;
        CharismaModifier = charismaModifier;
    }

    public Item(int id, string name, int value, ItemCategory category, 
                int strengthMod, int constitutionMod, int dexterityMod, int charismaMod, int quantity = 1)
    {
        ID = id;
        Name = name;
        Value = value;
        Category = category;
        StrengthModifier = strengthMod;
        ConstitutionModifier = constitutionMod;
        DexterityModifier = dexterityMod;
        CharismaModifier = charismaMod;
        Quantity = quantity;
    }

    /// <summary>
    /// Change the value of an item in the shop's inventory based on its quantity.
    /// </summary>
    /// <param name="quantityChange">The change in quantity.</param>
    /// <returns>The updated item.</returns>
    public void AdjustValue(int quantityChange)
    {
        int previousQuantity = Quantity; // Track before the change
        Quantity += quantityChange;

        // Ensure Quantity doesn't drop below zero
        Quantity = Math.Max(0, Quantity);

        // Calculate the percentage change
        float percentageChange = (float)Math.Abs(quantityChange) / Math.Max(previousQuantity, 1);

        // Determine the adjustment factor (5% change for 100% stock change)
        float adjustmentFactor = 1 + percentageChange * 0.05f;

        // Adjust value based on stock increase or decrease
        if (quantityChange > 0) // Quantity increased
        {
            Value = Math.Max((int)(Value / adjustmentFactor), 1);
        }
        else if (quantityChange < 0) // Quantity decreased
        {
            Value = Math.Min((int)(Value * adjustmentFactor), int.MaxValue);
        }
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
