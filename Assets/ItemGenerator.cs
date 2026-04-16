using System.Collections.Generic;
using UnityEngine;

public static class ItemGenerator
{
    public static List<Item> GenerateItems(ShopTypes shopType, int shopLevel, ItemSpriteDatabase spriteDatabase)
    {
        List<Item> items = new List<Item>();
        int itemCount = Random.Range(3, 6); // Number of items to generate

        for (int i = 0; i < itemCount; i++)
        {
            int id = Random.Range(1000, 9999);
            string name = $"{shopType} Item {i + 1}";
            int gold = Random.Range(1, 10) * shopLevel;
            int silver = Random.Range(10, 100);
            int quality = Mathf.Clamp(shopLevel, 1, 3);
            int quantity = Random.Range(1, 5);

            ItemCategory category = shopType == ShopTypes.Blacksmith ? ItemCategory.Weapon : ItemCategory.Armor;
            Sprite itemSprite = spriteDatabase.GetSprite(category, quality);

            Item item;
            switch (shopType)
            {
                case ShopTypes.Blacksmith:
                    item = new Item(
                        id,
                        name,
                        gold,
                        silver,
                        ItemCategory.Weapon,
                        new List<StatModifier>
                        {
                            new StatModifier(StatType.Strength, Random.Range(1, 5), "Blacksmith"),
                            new StatModifier(StatType.Constitution, Random.Range(1, 5), "Blacksmith")
                        },
                        itemSprite, 
                        quality,
                        quantity
                    );
                    break;

                case ShopTypes.Tanner:
                    item = new Item(
                        id,
                        name,
                        gold,
                        silver,
                        ItemCategory.Armor,
                        new List<StatModifier>
                        {
                            new StatModifier(StatType.Dexterity, Random.Range(1, 3), "Tanner"),
                            new StatModifier(StatType.Charisma, Random.Range(1, 2), "Tanner")
                        },
                        itemSprite, 
                        quality,
                        quantity
                    );
                    break;

                case ShopTypes.Alchemist:
                    item = new Item(
                        id,
                        name,
                        gold,
                        silver,
                        Random.Range(10, 50), // Health recovery
                        Random.Range(5, 20),   // Exhaustion reduction
                        itemSprite,
                        quality,
                        quantity
                    );
                    break;

                case ShopTypes.Carpenter:
                case ShopTypes.Mason:
                case ShopTypes.GeneralStore:
                    item = new Item(
                        id,
                        name,
                        gold,
                        silver,
                        shopType == ShopTypes.Carpenter ? ItemCategory.Resource : ItemCategory.CraftingMaterial,
                        quantity
                    );
                    break;

                default:
                    item = new Item(
                        id,
                        name,
                        gold,
                        silver,
                        ItemCategory.Misc,
                        quantity
                    );
                    break;
            }

            items.Add(item);
        }

        return items;
    }
}
