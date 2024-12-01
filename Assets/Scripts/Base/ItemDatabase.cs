using System.Collections.Generic;

public static class ItemDatabase
{
    private static Dictionary<int, Item> items = new Dictionary<int, Item>();

    static ItemDatabase()
    {
        // Predefined items
        AddItem(new Item(1, "Iron Sword", 150, 1, false, ItemCategory.Weapon));
        AddItem(new Item(2, "Health Potion", 50, 1, true, ItemCategory.Potion));
        AddItem(new Item(3, "Leather Armor", 200, 1, false, ItemCategory.Armor));
        AddItem(new Item(4, "Wooden Plank", 30, 0, true, ItemCategory.CraftingMaterial));
        AddItem(new Item(5, "Stone Brick", 40, 0, true, ItemCategory.CraftingMaterial));
        AddItem(new Item(6, "Gold Nugget", 300, 0, true, ItemCategory.Resource));
        AddItem(new Item(7, "Iron Ore", 100, 0, true, ItemCategory.CraftingMaterial));
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
