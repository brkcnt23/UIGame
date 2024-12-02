using System.Collections.Generic;

public static class ItemDatabase
{
    private static Dictionary<int, Item> items = new Dictionary<int, Item>();

    static ItemDatabase()
    {
        // Predefined items
        AddItem(new Item(1, "Iron Sword", 150, ItemCategory.Weapon, 5, 0, 0, 0));
        AddItem(new Item(2, "Health Potion", 50, ItemCategory.Potion, 0, 0, 0, 0));
        AddItem(new Item(3, "Leather Armor", 200, ItemCategory.Armor, 0, 5, 0, 0));
        AddItem(new Item(4, "Wooden Plank", 30, ItemCategory.CraftingMaterial, 0, 0, 0, 0));
        AddItem(new Item(5, "Stone Brick", 40, ItemCategory.CraftingMaterial, 0, 0, 0, 0));
        AddItem(new Item(6, "Gold Nugget", 300, ItemCategory.Resource, 0, 0, 0, 0));
        AddItem(new Item(7, "Iron Ore", 100, ItemCategory.CraftingMaterial, 0, 0, 0, 0));
    }

    public static void AddItem(Item item)
    {
        if (!items.ContainsKey(item.ID))
        {
            items.Add(item.ID, item);
        }
    }

    public static Item GetItemByID(int id)
    {
        return items.ContainsKey(id) ? items[id] : null;
    }

    public static List<Item> GetAllItems()
    {
        return new List<Item>(items.Values);
    }
}
