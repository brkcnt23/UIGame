using System.Collections.Generic;

/// <summary>
/// Who sells what, and why a town has more than one shop.
///
/// A swordsmith sells swords. Refusing to stock finished goods would push
/// every player through the crafting system whether they wanted it or not,
/// and a player who would rather fight than forge should be able to buy a
/// blade and get on with it.
///
/// Crafting stays worthwhile through quality rather than exclusivity:
///
///   Buy    fast, costs coin, Crude or Common
///   Craft  slow, costs materials and hours, Fine and above
///
/// A shop blade is serviceable. The one you make with a good hand and good
/// steel is better and cheaper — you paid in time instead of silver. Both are
/// valid ways to play, which is the point.
///
/// Shops still specialise, so the trade loop survives:
///   sell raw -> buy processed -> craft -> sell finished elsewhere
/// and each step passes through a different shop. That is why a settlement
/// needs several.
/// </summary>
public sealed class ShopProfileDef
{
    public string ProfileId;
    public string DisplayName;
    public ShopTypes Type;

    /// <summary>Item names, resolved against ItemCatalog by the importer.</summary>
    public List<(string item, int min, int max, int weight)> Stock = new();

    /// <summary>Categories this shop will take off the player's hands.</summary>
    public List<ItemCategory> Buys = new();

    /// <summary>Fraction of base value paid to the player when selling to it.</summary>
    public float BuyMultiplier = 0.55f;

    /// <summary>Multiple of base value charged to the player.</summary>
    public float SellMultiplier = 1.15f;

    /// <summary>Highest quality this shop will ever stock.</summary>
    public int MaxQuality = 2;

    public string Description = "";
}

public static class ShopCatalog
{
    private static List<ShopProfileDef> _all;
    public static List<ShopProfileDef> All => _all ??= Build();

    private static List<ShopProfileDef> Build()
    {
        var list = new List<ShopProfileDef>();

        void S(string id, string name, ShopTypes type, float buy, float sell, int maxQuality,
               (string, int, int, int)[] stock, ItemCategory[] buys, string description)
        {
            list.Add(new ShopProfileDef
            {
                ProfileId = id,
                DisplayName = name,
                Type = type,
                BuyMultiplier = buy,
                SellMultiplier = sell,
                MaxQuality = maxQuality,
                Stock = new List<(string, int, int, int)>(stock),
                Buys = new List<ItemCategory>(buys),
                Description = description
            });
        }

        // =============================================================
        // BLACKSMITH — sells metal, buys ore. Never sells finished weapons.
        // =============================================================
        S("smith_common", "Blacksmith", ShopTypes.Blacksmith, 0.50f, 1.20f, 1,
          new[]
          {
              // Materials — the smith's own trade
              ("Iron Ingot",   2, 8, 10),
              ("Copper Ingot", 2, 6, 8),
              ("Tin Ingot",    1, 4, 6),
              ("Bronze Ingot", 1, 4, 5),
              ("Steel Ingot",  0, 3, 3),
              ("Rivets",       4, 16, 10),
              ("Buckle",       2, 8, 6),
              ("Coal",         5, 20, 10),
              ("Charcoal",     2, 10, 6),
              ("Whetstone",    1, 3, 4),
              ("Flint & Steel", 1, 3, 3),

              // Finished work. Honest, serviceable, nothing special — which is
              // exactly what a shop blade should be next to a crafted one.
              ("Dagger",       1, 4, 8),
              ("Hand Axe",     1, 3, 7),
              ("Shortsword",   1, 3, 7),
              ("Mace",         1, 3, 6),
              ("Arming Sword", 0, 2, 5),
              ("Falchion",     0, 2, 4),
              ("Spear",        1, 3, 6),
              ("Kettle Hat",   1, 3, 6),
              ("Iron Helm",    0, 2, 4),
              ("Buckler",      0, 2, 5),
          },
          new[] { ItemCategory.Resource, ItemCategory.CraftingMaterial, ItemCategory.Weapon,
                  ItemCategory.Armor, ItemCategory.Helmet, ItemCategory.Shield, ItemCategory.Gloves },
          "Ingots, fittings and honest ironwork. Nothing on the rack will win a " +
          "tournament, but it will hold an edge.");

        // =============================================================
        // TANNER — leather and cloth, buys hides
        // =============================================================
        S("tanner_common", "Tanner", ShopTypes.Tanner, 0.50f, 1.15f, 2,
          new[]
          {
              ("Tanned Leather", 3, 10, 10),
              ("Cured Hide",     2, 6, 7),
              ("Leather Strap",  4, 12, 10),
              ("Thread",         5, 20, 10),
              ("Cloth Bolt",     2, 8, 8),
              ("Bowstring",      1, 4, 4),
              ("Rope",           1, 4, 5),
              ("Waterskin",      1, 3, 4),
              ("Bedroll",        1, 2, 3),
              ("Empty Sack",     2, 6, 5),
              ("Bandage",        3, 10, 8),

              // Finished leatherwork
              ("Cloth Wraps",      1, 3, 5),
              ("Worn Shoes",       1, 3, 5),
              ("Leather Gloves",   1, 3, 6),
              ("Leather Boots",    1, 3, 6),
              ("Leather Cap",      1, 3, 6),
              ("Gambeson",         0, 2, 4),
              ("Leather Leggings", 0, 2, 4),
              ("Leather Cuirass",  0, 2, 3),
          },
          new[] { ItemCategory.Resource, ItemCategory.CraftingMaterial, ItemCategory.Boots,
                  ItemCategory.Leggings, ItemCategory.Gloves, ItemCategory.Armor,
                  ItemCategory.Helmet },
          "Leather, cord and cloth, and a rack of workaday gear. Bring hides; " +
          "she pays fairly for anything still supple.");

        // =============================================================
        // CARPENTER — wood, and the charcoal the smith needs
        // =============================================================
        S("carpenter_common", "Carpenter", ShopTypes.Carpenter, 0.50f, 1.15f, 2,
          new[]
          {
              ("Plank",       5, 20, 10),
              ("Beam",        2, 8, 8),
              ("Timber Log",  4, 15, 9),
              ("Ash Log",     2, 8, 6),
              ("Yew Branch",  0, 3, 3),
              ("Charcoal",    3, 12, 9),
              ("Pitch",       2, 6, 6),
              ("Torch",       3, 10, 7),
              ("Splint",      1, 4, 4),

              // Wooden weapons and shields
              ("Club",          1, 3, 5),
              ("Javelin",       1, 4, 5),
              ("Hunting Bow",   0, 2, 4),
              ("Shortbow",      0, 2, 3),
              ("Wooden Shield", 1, 3, 5),
              ("Round Shield",  0, 2, 3),
          },
          new[] { ItemCategory.Resource, ItemCategory.CraftingMaterial,
                  ItemCategory.Weapon, ItemCategory.Shield },
          "Timber, planks and charcoal, plus bows and shields. " +
          "The smith buys half his fuel here.");

        // =============================================================
        // MASON — stone, brick, and the alchemist's glass
        // =============================================================
        S("mason_common", "Stonemason", ShopTypes.Mason, 0.50f, 1.15f, 2,
          new[]
          {
              ("Cut Stone",   4, 14, 10),
              ("Rough Stone", 6, 20, 10),
              ("Limestone",   3, 10, 8),
              ("Clay",        4, 14, 8),
              ("Brick",       4, 16, 9),
              ("Mortar",      2, 8, 7),
              ("Glass Vial",  2, 10, 6),
              ("Whetstone",   1, 4, 5),
          },
          new[] { ItemCategory.Resource, ItemCategory.CraftingMaterial },
          "Stone, brick and mortar — and the only reliable source of glass vials.");

        // =============================================================
        // ALCHEMIST — potions and herbs
        // =============================================================
        S("alchemist_common", "Apothecary", ShopTypes.Alchemist, 0.55f, 1.25f, 2,
          new[]
          {
              ("Common Herbs",           4, 15, 10),
              ("Bitterroot",             1, 6, 6),
              ("Marshbloom",             1, 5, 5),
              ("Fireleaf",               0, 3, 3),
              ("Herbal Extract",         2, 8, 8),
              ("Distilled Spirit",       1, 4, 4),
              ("Glass Vial",             3, 12, 8),
              ("Minor Healing Draught",  2, 8, 9),
              ("Healing Draught",        1, 4, 6),
              ("Strong Healing Draught", 0, 2, 2),
              ("Antidote",               1, 3, 5),
              ("Fever Tonic",            1, 3, 5),
              ("Herbal Poultice",        1, 5, 5),
              ("Clean Linen",            2, 6, 5),
          },
          new[] { ItemCategory.Resource, ItemCategory.Potion, ItemCategory.Consumable,
                  ItemCategory.CraftingMaterial },
          "Draughts, tonics and the plants they come from. Pays well for rare herbs.");

        // =============================================================
        // GENERAL STORE — food, sundries, and cheap starter gear
        // =============================================================
        S("general_common", "General Store", ShopTypes.GeneralStore, 0.45f, 1.30f, 1,
          new[]
          {
              ("Bread",          5, 20, 10),
              ("Dried Meat",     4, 15, 10),
              ("Cheese Wheel",   2, 8, 7),
              ("Salted Fish",    3, 12, 8),
              ("Travel Ration",  3, 12, 10),
              ("Ale",            4, 15, 7),
              ("Wine",           2, 8, 5),
              ("Salt",           3, 10, 8),
              ("Rope",           1, 4, 5),
              ("Torch",          3, 10, 6),
              ("Cooking Pot",    1, 3, 4),
              ("Waterskin",      1, 4, 5),
              ("Bedroll",        1, 3, 4),
              ("Empty Sack",     2, 8, 5),
              ("Bandage",        2, 8, 6),
              ("Fishing Line",   1, 4, 4),
              ("Peasant Tunic",  1, 3, 4),
              ("Cloth Trousers", 1, 3, 4),
              ("Worn Shoes",     1, 3, 4),
              ("Club",           1, 2, 2),
              ("Dagger",         0, 2, 2),
          },
          new[] { ItemCategory.Misc, ItemCategory.Consumable, ItemCategory.Potion,
                  ItemCategory.TradeGood, ItemCategory.Resource, ItemCategory.CraftingMaterial,
                  ItemCategory.Weapon, ItemCategory.Armor, ItemCategory.Boots,
                  ItemCategory.Leggings, ItemCategory.Helmet, ItemCategory.Gloves,
                  ItemCategory.Shield, ItemCategory.Trinket },
          "Food, rope, and whatever else travellers forget. Buys anything, " +
          "pays the least for it.");

        // =============================================================
        // MERCHANT — trade goods only. The long-distance profit game.
        // =============================================================
        S("merchant_town", "Merchant House", ShopTypes.GeneralStore, 0.70f, 1.10f, 3,
          new[]
          {
              ("Salt",        4, 15, 10),
              ("Spice Pouch", 1, 6, 7),
              ("Silk Bolt",   0, 4, 4),
              ("Dyed Cloth",  1, 6, 6),
              ("Amber",       0, 3, 3),
              ("Fine Furs",   1, 5, 5),
              ("Wine Cask",   1, 4, 5),
              ("Honey Jar",   2, 8, 6),
              ("Beeswax",     2, 8, 6),
              ("Ivory Comb",  0, 3, 3),
          },
          new[] { ItemCategory.TradeGood },
          "Only trade goods, at a narrow margin. Buy where a thing is common, " +
          "sell where it is not.");

        // =============================================================
        // MYSTIC — rare, town-and-above, the only place trinkets surface
        // =============================================================
        S("mystic_rare", "The Curio", ShopTypes.Mystic, 0.60f, 1.60f, 4,
          new[]
          {
              ("Witch's Charm",            0, 1, 2),
              ("Saint's Medallion",        0, 1, 2),
              ("Wolf-Tooth Necklace",      0, 1, 3),
              ("Iron Band",                0, 1, 3),
              ("Signet Ring",              0, 1, 2),
              ("Carved Wooden Idol",       0, 1, 3),
              ("Embroidered Handkerchief", 0, 1, 3),
              ("Brass Lantern",            0, 1, 3),
              ("Merchant's Ledger",        0, 1, 2),
              ("Field Surgeon's Kit",      0, 1, 2),
              ("Meteoric Iron",            0, 1, 1),
          },
          new[] { ItemCategory.Trinket, ItemCategory.QuestItem },
          "Odds and ends nobody can account for. Expensive, and rarely twice " +
          "the same stock.");

        return list;
    }

    /// <summary>
    /// Which profiles a settlement of this tier can host. A hamlet has one
    /// general store and nothing else; a city has everything.
    /// </summary>
    public static List<string> ProfilesForTier(SettlementTier tier)
    {
        switch (tier)
        {
            case SettlementTier.Hamlet:
                return new List<string> { "general_common" };

            case SettlementTier.Village:
                return new List<string> { "general_common", "smith_common", "carpenter_common" };

            case SettlementTier.Town:
                return new List<string>
                {
                    "general_common", "smith_common", "carpenter_common",
                    "tanner_common", "mason_common", "alchemist_common",
                    "merchant_town"
                };

            case SettlementTier.City:
                return new List<string>
                {
                    "general_common", "smith_common", "carpenter_common",
                    "tanner_common", "mason_common", "alchemist_common",
                    "merchant_town", "mystic_rare"
                };

            default:
                return new List<string> { "general_common" };
        }
    }
}
