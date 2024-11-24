using System;

[Serializable]
public class Item
{
    public int ID;         // Unique identifier for the item
    public string Name;    // Name of the item
    public int Value;      // Value of the item in silver
    public ItemCategory Category; 

    public Item(int id, string name, int value,ItemCategory category)
    {
        ID = id;
        Name = name;
        Value = value;
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
