using System.Collections.Generic;

public static class ItemDatabase
{
    private static Dictionary<int, Item> items = new Dictionary<int, Item>();

    static ItemDatabase()
    {
        // Predefined items
        // AddItem(new Item(1, "Iron Sword", 150, ItemCategory.Weapon, 5, 0, 0, 0));
        // AddItem(new Item(2, "Health Potion", 50, ItemCategory.Potion, 0, 0, 0, 0));
        // AddItem(new Item(3, "Leather Armor", 200, ItemCategory.Armor, 0, 5, 0, 0));
        // AddItem(new Item(4, "Wooden Plank", 30, ItemCategory.CraftingMaterial, 0, 0, 0, 0));
        // AddItem(new Item(5, "Stone Brick", 40, ItemCategory.CraftingMaterial, 0, 0, 0, 0));
        // AddItem(new Item(6, "Gold Nugget", 300, ItemCategory.Resource, 0, 0, 0, 0));
        // AddItem(new Item(7, "Iron Ore", 100, ItemCategory.CraftingMaterial, 0, 0, 0, 0));
    }

    public static void AddItem(Item item)
    {
        if (!items.ContainsKey(item.ID))
        {
            items.Add(item.ID, item);
        }
    }

    public static void TestItems()
    {
        Item ironSword = new Item(1, "Iron Sword", 150, ItemCategory.Weapon,
            new List<StatModifier> { new StatModifier { Stat = StatType.Strength, Value = 5 } }, 1);
        Item leatherArmor = new Item(2, "Leather Armor", 200, ItemCategory.Armor,
            new List<StatModifier> { new StatModifier { Stat = StatType.Constitution, Value = 5 } }, 1);

        Item enchantedSword = new Item(3, "Enchanted Sword", 300, ItemCategory.Weapon,
            new List<StatModifier>{new StatModifier { Stat = StatType.Strength, Value = 7 },
                                   new StatModifier { Stat = StatType.Dexterity, Value = 3 }}, 1);

        Item healthPotion = new Item(3, "Health Potion", 50, healthRecovery: 20, exhaustionReduction: 0);
        Item energyPotion = new Item(4, "Energy Potion", 75, healthRecovery: 0, exhaustionReduction: 10);

        Item ironIngot = new Item(5, "Iron Ingot", 100, ItemCategory.CraftingMaterial, quantity: 1);
        Item leather = new Item(6, "Leather", 50, ItemCategory.CraftingMaterial, quantity: 5);
        Item herbs = new Item(7, "Herbs", 30, ItemCategory.CraftingMaterial, quantity: 10);

        Item stone = new Item(8, "Stone", 40, ItemCategory.Resource, quantity: 3);
        Item wood = new Item(9, "Wood", 30, ItemCategory.Resource, quantity: 4);

        Item cursedAmulet = new Item(4, "Cursed Amulet", 100, ItemCategory.Misc,
        new List<StatModifier>{new StatModifier { Stat = StatType.Constitution, Value = -5 },
                               new StatModifier { Stat = StatType.Dexterity, Value = -4 },
                               new StatModifier { Stat = StatType.Strength, Value = +12 }});

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
