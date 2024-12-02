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
