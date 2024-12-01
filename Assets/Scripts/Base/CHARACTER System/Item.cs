using System;

[Serializable]
public class Item
{
    public int ID;             // Unique identifier for the item
    public string Name;        // Name of the item
    public int Value;          // Value of the item in silver
    public int Quantity;       // Quantity for stackable items
    public bool IsStackable;   // If the item can stack
    public ItemCategory Category;

    public Item(int id, string name, int value, int quantity, bool isStackable, ItemCategory category)
    {
        ID = id;
        Name = name;
        Value = value;
        Quantity = quantity;
        IsStackable = isStackable;
        Category = category;
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
