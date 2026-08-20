using System.Collections.Generic;

/// <summary>
/// The production economy.
///
/// Three principles hold the whole thing together:
///
/// 1. Nothing is made from raw ore directly. Ore becomes an ingot, the ingot
///    becomes a part, parts become a sword. That chain is what makes a
///    greatsword feel like a project rather than a purchase.
///
/// 2. The metal ladder is real metallurgy — copper, bronze, iron, steel — so
///    each tier is gated by heat and skill rather than by a rarer rock. Tin is
///    useless alone and essential for bronze, which is exactly its historical
///    role.
///
/// 3. Every discipline feeds the others. A smith cannot finish a sword without
///    the tanner's strap, and the carpenter's charcoal is what turns iron into
///    steel. Specialising is possible; self-sufficiency is not.
///
/// Times are in in-game minutes. A greatsword takes four hours — long enough
/// that the player plans around it.
/// </summary>
public sealed class RecipeDef
{
    public int Id;
    public string Name;
    public string Output;
    public int OutputQty = 1;

    public CraftDiscipline Discipline = CraftDiscipline.Smither;
    public int Level = 1;
    public int Minutes = 30;
    public float SuccessChance = 1f;
    public bool KnownByDefault;

    /// <summary>Ingredient name -> quantity. Names resolve against ItemCatalog.</summary>
    public List<(string item, int qty)> Ingredients = new();

    public List<string> Stations = new();
    public List<string> Tools = new();

    public string Description = "";
}

public static class RecipeCatalog
{
    private static List<RecipeDef> _all;
    public static List<RecipeDef> All => _all ??= Build();

    private static List<RecipeDef> Build()
    {
        var list = new List<RecipeDef>();
        int id = 5000;

        void R(string output, CraftDiscipline disc, int level, int minutes,
               (string, int)[] ingredients, string[] stations = null,
               int outQty = 1, bool known = false, string desc = "")
        {
            list.Add(new RecipeDef
            {
                Id = id++,
                Name = output,
                Output = output,
                OutputQty = outQty,
                Discipline = disc,
                Level = level,
                Minutes = minutes,
                KnownByDefault = known,
                Ingredients = new List<(string, int)>(ingredients),
                Stations = new List<string>(stations ?? new string[0]),
                Description = desc
            });
        }

        const string Forge = "forge";
        const string Anvil = "anvil";
        const string Rack  = "tanning_rack";
        const string Bench = "workbench";
        const string Table = "alchemy_table";
        const string Yard  = "mason_yard";

        // =============================================================
        // SMELTING — the base of everything
        // =============================================================

        R("Copper Ingot", CraftDiscipline.Smither, 1, 45,
          new[] { ("Copper Ore", 2), ("Coal", 1) }, new[] { Forge }, 1, true,
          "Copper runs early and cheap. It is what you learn on.");

        R("Tin Ingot", CraftDiscipline.Smither, 1, 45,
          new[] { ("Tin Ore", 2), ("Coal", 1) }, new[] { Forge }, 1, true,
          "Soft and nearly useless on its own. Everything it is good for, it is good for with copper.");

        R("Bronze Ingot", CraftDiscipline.Smither, 2, 60,
          new[] { ("Copper Ingot", 2), ("Tin Ingot", 1), ("Coal", 1) }, new[] { Forge }, 1, false,
          "Two parts copper to one of tin. Harder than either, and the first alloy anyone learns.");

        R("Iron Ingot", CraftDiscipline.Smither, 3, 75,
          new[] { ("Iron Ore", 2), ("Coal", 2) }, new[] { Forge }, 1, false,
          "Iron needs more heat than copper ever asked for. The bellows do half the work.");

        R("Steel Ingot", CraftDiscipline.Smither, 5, 120,
          new[] { ("Iron Ingot", 2), ("Charcoal", 2) }, new[] { Forge }, 1, false,
          "Carbon from the charcoal goes into the iron. Too little and it stays soft; too much and it shatters.");

        R("Silver Ingot", CraftDiscipline.Smither, 4, 60,
          new[] { ("Silver Ore", 2), ("Coal", 1) }, new[] { Forge });

        // =============================================================
        // COMPONENTS — small parts, made in batches
        // =============================================================

        R("Rivets", CraftDiscipline.Smither, 2, 20,
          new[] { ("Iron Ingot", 1) }, new[] { Forge, Anvil }, 8, true);

        R("Buckle", CraftDiscipline.Smither, 2, 25,
          new[] { ("Bronze Ingot", 1) }, new[] { Forge, Anvil }, 4);

        R("Whetstone", CraftDiscipline.Mason, 1, 20,
          new[] { ("Rough Stone", 1) }, new[] { Yard }, 1, true);

        // =============================================================
        // CARPENTER — wood, and the charcoal that makes steel possible
        // =============================================================

        R("Charcoal", CraftDiscipline.Carpenter, 2, 90,
          new[] { ("Timber Log", 3) }, new[] { Bench }, 4, false,
          "Burned slow and starved of air. This is what the smith actually needs, not firewood.");

        R("Plank", CraftDiscipline.Carpenter, 1, 20,
          new[] { ("Timber Log", 1) }, new[] { Bench }, 3, true);

        R("Beam", CraftDiscipline.Carpenter, 2, 40,
          new[] { ("Timber Log", 2) }, new[] { Bench });

        R("Pitch", CraftDiscipline.Carpenter, 2, 60,
          new[] { ("Timber Log", 2), ("Coal", 1) }, new[] { Bench }, 2);

        R("Torch", CraftDiscipline.Carpenter, 1, 10,
          new[] { ("Plank", 1), ("Pitch", 1) }, new[] { Bench }, 3, true);

        // =============================================================
        // TANNER — leather, cloth, cord
        // =============================================================

        R("Tanned Leather", CraftDiscipline.Tanner, 1, 90,
          new[] { ("Raw Hide", 2), ("Salt", 1) }, new[] { Rack }, 1, true,
          "Salt, time and patience. Rush it and it rots on the rack.");

        R("Cured Hide", CraftDiscipline.Tanner, 2, 60,
          new[] { ("Pelt", 2) }, new[] { Rack });

        R("Thread", CraftDiscipline.Tanner, 1, 20,
          new[] { ("Wool", 1) }, new[] { Rack }, 4, true);

        R("Leather Strap", CraftDiscipline.Tanner, 1, 15,
          new[] { ("Tanned Leather", 1) }, new[] { Rack }, 3, true);

        R("Cloth Bolt", CraftDiscipline.Tanner, 2, 60,
          new[] { ("Wool", 3), ("Thread", 1) }, new[] { Rack });

        R("Bowstring", CraftDiscipline.Tanner, 3, 45,
          new[] { ("Sinew", 2), ("Thread", 1) }, new[] { Rack }, 1, false,
          "Twisted wet and dried under tension. A bad string ruins a good bow.");

        R("Rope", CraftDiscipline.Tanner, 2, 40,
          new[] { ("Sinew", 3), ("Thread", 2) }, new[] { Rack });

        R("Waterskin", CraftDiscipline.Tanner, 2, 35,
          new[] { ("Tanned Leather", 1), ("Thread", 1), ("Pitch", 1) }, new[] { Rack });

        R("Bedroll", CraftDiscipline.Tanner, 2, 50,
          new[] { ("Cured Hide", 1), ("Wool", 2), ("Thread", 2) }, new[] { Rack });

        R("Empty Sack", CraftDiscipline.Tanner, 1, 15,
          new[] { ("Cloth Bolt", 1), ("Thread", 1) }, new[] { Rack }, 2, true);

        R("Bandage", CraftDiscipline.Tanner, 1, 15,
          new[] { ("Cloth Bolt", 1) }, new[] { Rack }, 4, true);

        R("Clean Linen", CraftDiscipline.Tanner, 3, 30,
          new[] { ("Cloth Bolt", 1), ("Distilled Spirit", 1) }, new[] { Rack }, 3);

        // =============================================================
        // MASON — stone and building material
        // =============================================================

        R("Cut Stone", CraftDiscipline.Mason, 1, 40,
          new[] { ("Rough Stone", 2) }, new[] { Yard }, 1, true);

        R("Brick", CraftDiscipline.Mason, 1, 50,
          new[] { ("Clay", 2), ("Coal", 1) }, new[] { Yard }, 4, true);

        R("Mortar", CraftDiscipline.Mason, 2, 45,
          new[] { ("Limestone", 2), ("Clay", 1) }, new[] { Yard }, 2);

        R("Glass Vial", CraftDiscipline.Mason, 3, 40,
          new[] { ("Rough Stone", 2), ("Coal", 2) }, new[] { Yard, Forge }, 4, false,
          "Sand melted until it runs clear. The alchemist will take every one you make.");

        // =============================================================
        // ALCHEMIST — extraction, then the draughts
        // =============================================================

        R("Herbal Extract", CraftDiscipline.Alchemist, 1, 40,
          new[] { ("Common Herbs", 3) }, new[] { Table }, 1, true,
          "Crushed, steeped, strained. Most of the plant is thrown away.");

        R("Distilled Spirit", CraftDiscipline.Alchemist, 3, 90,
          new[] { ("Common Herbs", 4), ("Fireleaf", 1) }, new[] { Table }, 2);

        R("Minor Healing Draught", CraftDiscipline.Alchemist, 1, 30,
          new[] { ("Herbal Extract", 1), ("Glass Vial", 1) }, new[] { Table }, 1, true);

        R("Healing Draught", CraftDiscipline.Alchemist, 3, 50,
          new[] { ("Herbal Extract", 2), ("Bitterroot", 1), ("Glass Vial", 1) }, new[] { Table });

        R("Strong Healing Draught", CraftDiscipline.Alchemist, 5, 80,
          new[] { ("Herbal Extract", 3), ("Marshbloom", 2), ("Distilled Spirit", 1), ("Glass Vial", 1) },
          new[] { Table }, 1, false,
          "The difference between this and the weak one is not more herbs. It is knowing when to stop heating it.");

        R("Antidote", CraftDiscipline.Alchemist, 4, 60,
          new[] { ("Bitterroot", 2), ("Herbal Extract", 1), ("Glass Vial", 1) }, new[] { Table });

        R("Fever Tonic", CraftDiscipline.Alchemist, 3, 55,
          new[] { ("Marshbloom", 1), ("Distilled Spirit", 1), ("Glass Vial", 1) }, new[] { Table });

        R("Herbal Poultice", CraftDiscipline.Alchemist, 2, 35,
          new[] { ("Common Herbs", 2), ("Cloth Bolt", 1) }, new[] { Table }, 2);

        R("Splint", CraftDiscipline.Carpenter, 1, 20,
          new[] { ("Plank", 1), ("Leather Strap", 1) }, new[] { Bench }, 2, true);

        // =============================================================
        // WEAPONS — the chain pays off here
        // =============================================================

        R("Club", CraftDiscipline.Carpenter, 1, 20,
          new[] { ("Plank", 2) }, new[] { Bench }, 1, true);

        R("Dagger", CraftDiscipline.Smither, 2, 45,
          new[] { ("Iron Ingot", 1), ("Leather Strap", 1) }, new[] { Forge, Anvil });

        R("Stiletto", CraftDiscipline.Smither, 4, 60,
          new[] { ("Steel Ingot", 1), ("Leather Strap", 1) }, new[] { Forge, Anvil });

        R("Hand Axe", CraftDiscipline.Smither, 2, 50,
          new[] { ("Iron Ingot", 1), ("Plank", 1) }, new[] { Forge, Anvil });

        R("Shortsword", CraftDiscipline.Smither, 3, 75,
          new[] { ("Iron Ingot", 2), ("Leather Strap", 1) }, new[] { Forge, Anvil });

        R("Mace", CraftDiscipline.Smither, 3, 70,
          new[] { ("Iron Ingot", 2), ("Beam", 1) }, new[] { Forge, Anvil });

        R("Falchion", CraftDiscipline.Smither, 4, 100,
          new[] { ("Iron Ingot", 3), ("Leather Strap", 1), ("Rivets", 2) }, new[] { Forge, Anvil });

        R("Arming Sword", CraftDiscipline.Smither, 4, 110,
          new[] { ("Iron Ingot", 3), ("Leather Strap", 1), ("Rivets", 2) }, new[] { Forge, Anvil });

        R("War Hammer", CraftDiscipline.Smither, 4, 105,
          new[] { ("Iron Ingot", 3), ("Beam", 1), ("Rivets", 2) }, new[] { Forge, Anvil });

        R("Rapier", CraftDiscipline.Smither, 6, 150,
          new[] { ("Steel Ingot", 2), ("Leather Strap", 1), ("Buckle", 1) }, new[] { Forge, Anvil });

        R("Maul", CraftDiscipline.Smither, 5, 160,
          new[] { ("Steel Ingot", 3), ("Beam", 1), ("Rivets", 3) }, new[] { Forge, Anvil });

        R("Battle Axe", CraftDiscipline.Smither, 6, 180,
          new[] { ("Steel Ingot", 3), ("Beam", 1), ("Leather Strap", 1) }, new[] { Forge, Anvil });

        R("Poleaxe", CraftDiscipline.Smither, 6, 190,
          new[] { ("Steel Ingot", 3), ("Beam", 1), ("Rivets", 3) }, new[] { Forge, Anvil });

        R("Halberd", CraftDiscipline.Smither, 7, 200,
          new[] { ("Steel Ingot", 3), ("Beam", 1), ("Rivets", 4) }, new[] { Forge, Anvil });

        R("Greatsword", CraftDiscipline.Smither, 8, 240,
          new[] { ("Steel Ingot", 4), ("Leather Strap", 2), ("Rivets", 3), ("Whetstone", 1) },
          new[] { Forge, Anvil }, 1, false,
          "Four ingots, a day at the forge, and a grip wrapped twice. " +
          "A smith is judged on one of these more than on a hundred nails.");

        // ---- Ranged: carpenter's work, with the tanner's string ----

        R("Javelin", CraftDiscipline.Carpenter, 2, 30,
          new[] { ("Plank", 1), ("Iron Ingot", 1) }, new[] { Bench }, 3);

        R("Spear", CraftDiscipline.Carpenter, 2, 45,
          new[] { ("Beam", 1), ("Iron Ingot", 1) }, new[] { Bench });

        R("Hunting Bow", CraftDiscipline.Carpenter, 3, 90,
          new[] { ("Ash Log", 2), ("Bowstring", 1) }, new[] { Bench });

        R("Shortbow", CraftDiscipline.Carpenter, 4, 110,
          new[] { ("Yew Branch", 1), ("Bowstring", 1), ("Leather Strap", 1) }, new[] { Bench });

        R("Longbow", CraftDiscipline.Carpenter, 6, 160,
          new[] { ("Yew Branch", 2), ("Bowstring", 1), ("Leather Strap", 1) }, new[] { Bench }, 1, false,
          "Yew, because it bends and returns without complaint. Anything else takes a set and stays there.");

        R("Crossbow", CraftDiscipline.Carpenter, 7, 200,
          new[] { ("Beam", 1), ("Steel Ingot", 1), ("Bowstring", 1), ("Rivets", 2) }, new[] { Bench, Forge });

        // =============================================================
        // ARMOUR
        // =============================================================

        R("Peasant Tunic", CraftDiscipline.Tanner, 1, 30,
          new[] { ("Cloth Bolt", 2), ("Thread", 1) }, new[] { Rack }, 1, true);

        R("Cloth Trousers", CraftDiscipline.Tanner, 1, 30,
          new[] { ("Cloth Bolt", 2), ("Thread", 1) }, new[] { Rack }, 1, true);

        R("Cloth Wraps", CraftDiscipline.Tanner, 1, 15,
          new[] { ("Cloth Bolt", 1) }, new[] { Rack }, 1, true);

        R("Worn Shoes", CraftDiscipline.Tanner, 1, 25,
          new[] { ("Tanned Leather", 1), ("Thread", 1) }, new[] { Rack }, 1, true);

        R("Gambeson", CraftDiscipline.Tanner, 3, 120,
          new[] { ("Cloth Bolt", 3), ("Wool", 4), ("Thread", 3) }, new[] { Rack }, 1, false,
          "Layer on layer, quilted through. It stops more than people expect.");

        R("Leather Cap", CraftDiscipline.Tanner, 2, 40,
          new[] { ("Tanned Leather", 1), ("Thread", 1) }, new[] { Rack });

        R("Leather Gloves", CraftDiscipline.Tanner, 2, 35,
          new[] { ("Tanned Leather", 1), ("Thread", 2) }, new[] { Rack });

        R("Work Gloves", CraftDiscipline.Tanner, 2, 35,
          new[] { ("Cured Hide", 1), ("Thread", 2) }, new[] { Rack });

        R("Leather Boots", CraftDiscipline.Tanner, 2, 55,
          new[] { ("Tanned Leather", 2), ("Thread", 2), ("Leather Strap", 1) }, new[] { Rack });

        R("Leather Leggings", CraftDiscipline.Tanner, 3, 70,
          new[] { ("Tanned Leather", 3), ("Thread", 2) }, new[] { Rack });

        R("Leather Cuirass", CraftDiscipline.Tanner, 4, 110,
          new[] { ("Tanned Leather", 4), ("Leather Strap", 2), ("Buckle", 1) }, new[] { Rack });

        R("Studded Leather", CraftDiscipline.Tanner, 5, 140,
          new[] { ("Tanned Leather", 4), ("Rivets", 6), ("Leather Strap", 2) }, new[] { Rack, Forge });

        R("Padded Chausses", CraftDiscipline.Tanner, 3, 70,
          new[] { ("Cloth Bolt", 2), ("Wool", 3), ("Thread", 2) }, new[] { Rack });

        R("Reinforced Boots", CraftDiscipline.Tanner, 4, 80,
          new[] { ("Tanned Leather", 2), ("Iron Ingot", 1), ("Rivets", 4) }, new[] { Rack, Forge });

        // ---- Mail and plate: the smith's long jobs ----

        R("Mail Mittens", CraftDiscipline.Smither, 5, 120,
          new[] { ("Iron Ingot", 2), ("Leather Strap", 1) }, new[] { Forge, Anvil });

        R("Mail Chausses", CraftDiscipline.Smither, 6, 180,
          new[] { ("Iron Ingot", 4), ("Leather Strap", 2), ("Buckle", 1) }, new[] { Forge, Anvil });

        R("Chainmail Hauberk", CraftDiscipline.Smither, 6, 300,
          new[] { ("Iron Ingot", 6), ("Rivets", 8), ("Leather Strap", 2) }, new[] { Forge, Anvil }, 1, false,
          "Every ring drawn, bent and closed by hand. Weeks of work in a real shop, and it shows.");

        R("Scale Mail", CraftDiscipline.Smither, 6, 260,
          new[] { ("Iron Ingot", 5), ("Tanned Leather", 3), ("Rivets", 6) }, new[] { Forge, Anvil });

        R("Kettle Hat", CraftDiscipline.Smither, 4, 90,
          new[] { ("Iron Ingot", 2), ("Leather Strap", 1) }, new[] { Forge, Anvil });

        R("Nasal Helm", CraftDiscipline.Smither, 5, 110,
          new[] { ("Iron Ingot", 2), ("Leather Strap", 1), ("Rivets", 2) }, new[] { Forge, Anvil });

        R("Iron Helm", CraftDiscipline.Smither, 5, 130,
          new[] { ("Iron Ingot", 3), ("Leather Strap", 1), ("Rivets", 2) }, new[] { Forge, Anvil });

        R("Brigandine", CraftDiscipline.Smither, 7, 320,
          new[] { ("Steel Ingot", 4), ("Tanned Leather", 3), ("Rivets", 10) }, new[] { Forge, Anvil });

        R("Bascinet", CraftDiscipline.Smither, 7, 180,
          new[] { ("Steel Ingot", 2), ("Leather Strap", 1), ("Buckle", 1) }, new[] { Forge, Anvil });

        R("Great Helm", CraftDiscipline.Smither, 8, 220,
          new[] { ("Steel Ingot", 3), ("Leather Strap", 2), ("Rivets", 4) }, new[] { Forge, Anvil });

        R("Gauntlets", CraftDiscipline.Smither, 7, 160,
          new[] { ("Steel Ingot", 2), ("Leather Strap", 2), ("Rivets", 4) }, new[] { Forge, Anvil });

        R("Sabatons", CraftDiscipline.Smither, 7, 170,
          new[] { ("Steel Ingot", 2), ("Leather Strap", 2), ("Buckle", 1) }, new[] { Forge, Anvil });

        R("Plate Greaves", CraftDiscipline.Smither, 8, 220,
          new[] { ("Steel Ingot", 3), ("Leather Strap", 2), ("Buckle", 2) }, new[] { Forge, Anvil });

        R("Half Plate", CraftDiscipline.Smither, 8, 380,
          new[] { ("Steel Ingot", 6), ("Leather Strap", 3), ("Buckle", 2), ("Rivets", 6) },
          new[] { Forge, Anvil });

        R("Full Plate", CraftDiscipline.Smither, 10, 600,
          new[] { ("Steel Ingot", 10), ("Leather Strap", 4), ("Buckle", 4), ("Rivets", 12) },
          new[] { Forge, Anvil }, 1, false,
          "Fitted to one body and no other. Ten ingots and the better part of a month. " +
          "A smith who can make this does not need to advertise.");

        // =============================================================
        // SHIELDS
        // =============================================================

        R("Wooden Shield", CraftDiscipline.Carpenter, 1, 40,
          new[] { ("Plank", 3), ("Leather Strap", 1) }, new[] { Bench }, 1, true);

        R("Buckler", CraftDiscipline.Smither, 3, 70,
          new[] { ("Iron Ingot", 1), ("Leather Strap", 1), ("Rivets", 2) }, new[] { Forge, Anvil });

        R("Round Shield", CraftDiscipline.Carpenter, 3, 90,
          new[] { ("Plank", 4), ("Iron Ingot", 1), ("Leather Strap", 2) }, new[] { Bench, Forge });

        R("Kite Shield", CraftDiscipline.Carpenter, 5, 130,
          new[] { ("Plank", 5), ("Iron Ingot", 2), ("Tanned Leather", 2), ("Rivets", 4) },
          new[] { Bench, Forge });

        R("Tower Shield", CraftDiscipline.Carpenter, 6, 180,
          new[] { ("Beam", 2), ("Plank", 4), ("Steel Ingot", 1), ("Rivets", 6) },
          new[] { Bench, Forge });

        // =============================================================
        // FOOD AND SUNDRIES
        // =============================================================

        R("Travel Ration", CraftDiscipline.Alchemist, 2, 40,
          new[] { ("Dried Meat", 1), ("Bread", 1), ("Salt", 1) }, null, 2, true,
          "Packed for the road: hard bread, salt meat, and something dried past recognition.");

        R("Cooking Pot", CraftDiscipline.Smither, 3, 60,
          new[] { ("Copper Ingot", 2), ("Rivets", 2) }, new[] { Forge, Anvil });

        R("Flint & Steel", CraftDiscipline.Smither, 2, 25,
          new[] { ("Steel Ingot", 1), ("Rough Stone", 1) }, new[] { Forge }, 2);

        R("Fishing Line", CraftDiscipline.Tanner, 2, 25,
          new[] { ("Sinew", 2), ("Thread", 1) }, new[] { Rack }, 2);

        R("Lockpick", CraftDiscipline.Smither, 5, 40,
          new[] { ("Steel Ingot", 1) }, new[] { Forge, Anvil }, 3);

        return list;
    }
}
