using System.Collections.Generic;

/// <summary>
/// The authored facts about every item: what it is, what it costs, what it
/// does. The importer reads this table, matches it against the PNGs in
/// UI Elements, and writes one ItemSO asset per entry.
///
/// Kept as plain C# rather than JSON so a typo is a compile error instead of a
/// silent null at runtime, and so the whole item economy is readable in one
/// file while it is still being balanced.
///
/// Sprite naming convention the importer expects:
///   "Falchion1.png" .. "Falchion4.png"  -> quality tiers Crude..Masterwork
///   "Bread.png"                          -> single sprite, all tiers
/// The spriteBase field is the part before the number.
/// </summary>
public sealed class ItemDef
{
    public int Id;
    public string Name;
    public string SpriteBase;
    public ItemCategory Category;

    public int Gold;
    public int Silver;
    public float Weight = 1f;
    public bool Stackable;
    public int MaxStack = 1;

    // Weapon
    public WeaponClass Weapon = WeaponClass.None;
    public ScalingStat Scaling = ScalingStat.Strength;
    public int DamageDie;
    public int DamageDiceCount = 1;
    public bool TwoHanded;

    // Armour
    public int ArmorValue;
    public ArmorWeight ArmorClass = ArmorWeight.None;

    // Consumable
    public int Health;
    public int ExhaustionReduction;
    public int RationValue;

    // Flags
    public bool Craftable = true;
    public bool Unique;
    public bool Magical;

    public CraftDiscipline Craft = CraftDiscipline.Smither;

    public string Flavor = "";
}

public static class ItemCatalog
{
    private static List<ItemDef> _all;

    public static List<ItemDef> All => _all ??= Build();

    private static List<ItemDef> Build()
    {
        var list = new List<ItemDef>();
        int id = 1000;

        void W(string name, string sprite, WeaponClass wc, ScalingStat sc, int die,
               int gold, int silver, float weight, bool twoHanded = false,
               CraftDiscipline craft = CraftDiscipline.Smither, string flavor = "")
        {
            list.Add(new ItemDef
            {
                Id = id++, Name = name, SpriteBase = sprite,
                Category = ItemCategory.Weapon,
                Weapon = wc, Scaling = sc, DamageDie = die,
                TwoHanded = twoHanded,
                Gold = gold, Silver = silver, Weight = weight,
                Craft = craft, Flavor = flavor
            });
        }

        void A(string name, string sprite, ItemCategory cat, int armor, ArmorWeight aw,
               int gold, int silver, float weight, CraftDiscipline craft, string flavor = "")
        {
            list.Add(new ItemDef
            {
                Id = id++, Name = name, SpriteBase = sprite, Category = cat,
                ArmorValue = armor, ArmorClass = aw,
                Gold = gold, Silver = silver, Weight = weight,
                Craft = craft, Flavor = flavor
            });
        }

        void C(string name, string sprite, int hp, int exh, int ration,
               int silver, float weight, int stack = 20,
               CraftDiscipline craft = CraftDiscipline.Alchemist, string flavor = "")
        {
            list.Add(new ItemDef
            {
                Id = id++, Name = name, SpriteBase = sprite,
                Category = ItemCategory.Consumable,
                Health = hp, ExhaustionReduction = exh, RationValue = ration,
                Silver = silver, Weight = weight,
                Stackable = true, MaxStack = stack,
                Craft = craft, Flavor = flavor
            });
        }

        void R(string name, string sprite, ItemCategory cat, int silver, float weight,
               CraftDiscipline craft = CraftDiscipline.Smither, string flavor = "")
        {
            list.Add(new ItemDef
            {
                Id = id++, Name = name, SpriteBase = sprite, Category = cat,
                Silver = silver, Weight = weight,
                Stackable = true, MaxStack = 99,
                Craft = craft, Flavor = flavor
            });
        }

        void T(string name, string sprite, int gold, float weight, bool magical, string flavor)
        {
            list.Add(new ItemDef
            {
                Id = id++, Name = name, SpriteBase = sprite,
                Category = ItemCategory.Trinket,
                Gold = gold, Weight = weight,
                Craftable = false, Unique = true, Magical = magical,
                Flavor = flavor
            });
        }

        // ---------------- Weapons: STR, one-handed ----------------
        W("Club",          "club",         WeaponClass.Improvised, ScalingStat.Strength, 4,  0,  8,  2f, false, CraftDiscipline.Carpenter,
          "A shaped length of oak. It asks nothing of you but a firm hand.");
        W("Hand Axe",      "handaxe",      WeaponClass.Axe,        ScalingStat.Strength, 6,  0, 45,  2f);
        W("Arming Sword",  "ArmingSword",  WeaponClass.Sword,      ScalingStat.Strength, 8,  1, 20,  3f,  false, CraftDiscipline.Smither,
          "Balanced a thumb's width above the guard. It wants to be swung.");
        W("Mace",          "mace",         WeaponClass.Mace,       ScalingStat.Strength, 8,  1, 10,  4f);
        W("War Hammer",    "warhammer",    WeaponClass.Mace,       ScalingStat.Strength, 8,  1, 40,  5f);
        W("Falchion",      "Falchion",     WeaponClass.Sword,      ScalingStat.Strength, 8,  1, 35,  3f,  false, CraftDiscipline.Smither,
          "Heavy toward the tip. It cuts more than it thrusts.");

        // ---------------- Weapons: STR, two-handed ----------------
        W("Poleaxe",       "poleaxe",      WeaponClass.Polearm,    ScalingStat.Strength, 10, 2, 40,  6f, true);
        W("Halberd",       "halberd",      WeaponClass.Polearm,    ScalingStat.Strength, 10, 2, 60,  7f, true);
        W("Greatsword",    "greatsword",   WeaponClass.Sword,      ScalingStat.Strength, 12, 4,  0,  8f, true, CraftDiscipline.Smither,
          "Two hands and a wide stance. Nothing about it is subtle.");
        W("Battle Axe",    "battleaxe",    WeaponClass.Axe,        ScalingStat.Strength, 12, 3, 50,  8f, true);
        W("Maul",          "maul",         WeaponClass.Mace,       ScalingStat.Strength, 12, 3, 20, 10f, true);

        // ---------------- Weapons: DEX, finesse ----------------
        W("Stiletto",      "Stiletto",     WeaponClass.Dagger,     ScalingStat.Dexterity, 4, 0, 60, 0.5f, false, CraftDiscipline.Smither,
          "Narrow enough to find the gap in a mail shirt.");
        W("Dagger",        "dagger",       WeaponClass.Dagger,     ScalingStat.Dexterity, 4, 0, 35, 0.6f, false, CraftDiscipline.Smither,
          "Light in the palm. Quick to draw, quicker to hide.");
        W("Shortsword",    "Shortsword",   WeaponClass.Sword,      ScalingStat.Dexterity, 6, 0, 90, 1.5f, false, CraftDiscipline.Smither,
          "Short enough for close work, long enough to be taken seriously.");
        W("Rapier",        "rapier",       WeaponClass.Sword,      ScalingStat.Dexterity, 8, 2, 10, 2f);

        // ---------------- Weapons: DEX, ranged ----------------
        W("Hunting Bow",   "huntingbow",          WeaponClass.Bow,        ScalingStat.Dexterity, 6, 0, 70, 2f,  true,  CraftDiscipline.Carpenter,
          "Cut for game, not for war. It will still put an arrow where you look.");
        W("Shortbow",      "yay",     WeaponClass.Bow,        ScalingStat.Dexterity, 6, 0, 95, 2f,  true,  CraftDiscipline.Carpenter);
        W("Longbow",       "longbow",      WeaponClass.Bow,        ScalingStat.Dexterity, 8, 2, 30, 3f,  true,  CraftDiscipline.Carpenter,
          "Draws deep. Your shoulder will remember every shot.");
        W("Crossbow",      "crossbow",     WeaponClass.Crossbow,   ScalingStat.Dexterity, 10, 3, 10, 5f, true,  CraftDiscipline.Carpenter);

        // ---------------- Weapons: Hybrid, thrown ----------------
        W("Throwing Knife","ThrowingKnife",WeaponClass.Thrown,     ScalingStat.Hybrid, 4, 0, 20, 0.4f);
        W("Javelin",       "javelin",      WeaponClass.Thrown,     ScalingStat.Hybrid, 6, 0, 30, 1.5f, false, CraftDiscipline.Carpenter,
          "Thrown from the shoulder. Reach and force in equal measure.");
        W("Throwing Axe",  "ThrowingAxe",  WeaponClass.Thrown,     ScalingStat.Hybrid, 6, 0, 40, 1.5f);
        W("Spear",         "spear",        WeaponClass.Polearm,    ScalingStat.Hybrid, 6, 0, 55, 3f,  false, CraftDiscipline.Carpenter);

        // ---------------- Shields ----------------
        A("Wooden Shield",  "WoodenShield", ItemCategory.Shield, 1, ArmorWeight.Light,  0, 25, 3f, CraftDiscipline.Carpenter);
        A("Buckler",        "Buckler",      ItemCategory.Shield, 1, ArmorWeight.Light,  0, 60, 2f, CraftDiscipline.Smither,
          "Small and fast. It parries more than it blocks.");
        A("Round Shield",   "RoundShield",  ItemCategory.Shield, 2, ArmorWeight.Medium, 1, 20, 5f, CraftDiscipline.Carpenter);
        A("Kite Shield",    "KiteShield",   ItemCategory.Shield, 3, ArmorWeight.Medium, 2, 40, 7f, CraftDiscipline.Smither);
        A("Tower Shield",   "TowerShield",  ItemCategory.Shield, 4, ArmorWeight.Heavy,  3, 60, 12f, CraftDiscipline.Smither,
          "It sits on your shoulder and the world stops arriving so quickly.");

        // ---------------- Helmets ----------------
        A("Leather Cap",  "LeatherCap",  ItemCategory.Helmet, 1, ArmorWeight.Light,  0, 30, 1f, CraftDiscipline.Tanner);
        A("Padded Coif",  "PaddedCoif",  ItemCategory.Helmet, 1, ArmorWeight.Light,  0, 50, 1.5f, CraftDiscipline.Tanner);
        A("Kettle Hat",   "KettleHat",   ItemCategory.Helmet, 2, ArmorWeight.Medium, 1, 10, 2.5f, CraftDiscipline.Smither);
        A("Nasal Helm",   "NasalHelm",   ItemCategory.Helmet, 2, ArmorWeight.Medium, 1, 40, 3f, CraftDiscipline.Smither);
        A("Iron Helm",    "helmet",      ItemCategory.Helmet, 3, ArmorWeight.Medium, 2, 20, 3.5f, CraftDiscipline.Smither,
          "Cold on the brow at dawn. You stop noticing by midday.");
        A("Bascinet",     "Bascinet",    ItemCategory.Helmet, 3, ArmorWeight.Heavy,  3, 30, 4f, CraftDiscipline.Smither);
        A("Great Helm",   "GreatHelm",   ItemCategory.Helmet, 4, ArmorWeight.Heavy,  4, 50, 6f, CraftDiscipline.Smither);

        // ---------------- Body armour ----------------
        A("Peasant Tunic",     "PeasentTunic",     ItemCategory.Armor, 0, ArmorWeight.Light,  0,  8, 1.5f, CraftDiscipline.Tanner);
        A("Gambeson",          "Gambeson",         ItemCategory.Armor, 2, ArmorWeight.Light,  0, 90, 4f,   CraftDiscipline.Tanner,
          "Layered linen, quilted tight. Warmer than it looks and softer than steel.");
        A("Leather Cuirass",   "ChestArmor",   ItemCategory.Armor, 3, ArmorWeight.Light,  1, 60, 6f,   CraftDiscipline.Tanner);
        A("Studded Leather",   "StuddedLeather",   ItemCategory.Armor, 4, ArmorWeight.Light,  2, 40, 8f,   CraftDiscipline.Tanner);
        A("Chainmail Hauberk", "ChainmailHauberk", ItemCategory.Armor, 6, ArmorWeight.Medium, 5,  0, 14f,  CraftDiscipline.Smither,
          "Every ring closed by hand. It hangs on you like weather.");
        A("Scale Mail",        "ScaleMail",        ItemCategory.Armor, 6, ArmorWeight.Medium, 5, 40, 16f,  CraftDiscipline.Smither);
        A("Brigandine",        "Brigandine",       ItemCategory.Armor, 7, ArmorWeight.Medium, 7,  0, 15f,  CraftDiscipline.Smither);
        A("Half Plate",        "HalfPlate",        ItemCategory.Armor, 9, ArmorWeight.Heavy, 12,  0, 22f,  CraftDiscipline.Smither);
        A("Full Plate",        "FullPlate",        ItemCategory.Armor, 12, ArmorWeight.Heavy, 25, 0, 30f,  CraftDiscipline.Smither,
          "Fitted to one body and no other. Worth a farm, and priced like one.");

        // ---------------- Leggings ----------------
        A("Cloth Trousers",  "ClothTrousers",  ItemCategory.Leggings, 0, ArmorWeight.Light,  0,  6, 1f,   CraftDiscipline.Tanner);
        A("Leather Leggings","PantsArmor",ItemCategory.Leggings, 2, ArmorWeight.Light,  0, 70, 3f,   CraftDiscipline.Tanner);
        A("Padded Chausses", "PaddedChausses", ItemCategory.Leggings, 2, ArmorWeight.Light,  1,  0, 3.5f, CraftDiscipline.Tanner);
        A("Mail Chausses",   "MailChausses",   ItemCategory.Leggings, 4, ArmorWeight.Medium, 3, 20, 8f,   CraftDiscipline.Smither);
        A("Plate Greaves",   "PlateGreaves",   ItemCategory.Leggings, 6, ArmorWeight.Heavy,  8,  0, 12f,  CraftDiscipline.Smither);

        // ---------------- Boots ----------------
        A("Worn Shoes",       "WornShoes",       ItemCategory.Boots, 0, ArmorWeight.Light,  0,  4, 0.8f, CraftDiscipline.Tanner);
        A("Leather Boots",    "BootsArmor",      ItemCategory.Boots, 1, ArmorWeight.Light,  0, 55, 2f,   CraftDiscipline.Tanner,
          "Broken in by someone else's feet, but they hold the road well enough.");
        A("Riding Boots",     "RidingBoots",     ItemCategory.Boots, 1, ArmorWeight.Light,  1, 10, 2.5f, CraftDiscipline.Tanner);
        A("Reinforced Boots", "ReinforcedBoots", ItemCategory.Boots, 2, ArmorWeight.Medium, 1, 80, 4f,   CraftDiscipline.Tanner);
        A("Sabatons",         "Sabatons",        ItemCategory.Boots, 3, ArmorWeight.Heavy,  4,  0, 7f,   CraftDiscipline.Smither);

        // ---------------- Gloves ----------------
        A("Cloth Wraps",   "Cloth Wraps",    ItemCategory.Gloves, 0, ArmorWeight.Light,  0,  5, 0.3f, CraftDiscipline.Tanner);
        A("Leather Gloves","Leather Gloves", ItemCategory.Gloves, 1, ArmorWeight.Light,  0, 40, 0.6f, CraftDiscipline.Tanner,
          "Your fingers stay warm in the cold. You could do needlework in these.");
        A("Work Gloves",   "WorkGloves",     ItemCategory.Gloves, 1, ArmorWeight.Light,  0, 35, 0.8f, CraftDiscipline.Tanner);
        A("Mail Mittens",  "Mail Mittens",   ItemCategory.Gloves, 2, ArmorWeight.Medium, 1, 50, 2f,   CraftDiscipline.Smither);
        A("Gauntlets",     "Gauntlets",      ItemCategory.Gloves, 3, ArmorWeight.Heavy,  3,  0, 3.5f, CraftDiscipline.Smither);

        // ---------------- Trinkets: found, never made ----------------
        T("Witch's Charm",            "Witch's Charm",            2, 0.2f, true,
          "A braid of hair from a woman in the north. They say sickness passes it by.");
        T("Saint's Medallion",        "Saint's Medallion",        3, 0.3f, true,
          "Worn smooth at the edge by thumbs that are no longer alive.");
        T("Wolf-Tooth Necklace",      "Wolf-Tooth Necklace",      1, 0.4f, false,
          "Three teeth on a leather cord. The wolf did not give them willingly.");
        T("Signet Ring",              "Signet Ring",              4, 0.1f, false,
          "A crest you do not recognise. Others might.");
        T("Iron Band",                "Iron Band",                1, 0.1f, false,
          "Plain iron, no mark. It has been on a hand longer than yours.");
        T("Widow's Ring",             "Widow's Ring",             2, 0.1f, true,
          "Cold no matter how long you wear it. Bargains go your way. People remember you badly.");
        T("Hooded Traveler's Cloak",  "Hooded Traveler's Cloak",  2, 2f,   false,
          "Oiled at the shoulders, patched at the hem. It has already seen the road.");
        T("Oilskin Mantle",           "Oilskin Mantle",           3, 2.5f, false,
          "Rain runs off it and keeps going.");
        T("Brass Lantern",            "Brass Lantern",            1, 2f,   false,
          "The shutter still works. Light where you point it, dark everywhere else.");
        T("Carved Wooden Idol",       "Carved Wooden Idol",       1, 0.8f, true,
          "A figure with no face. Farmers keep them by the door and do not explain why.");
        T("Embroidered Handkerchief", "Embroidered Handkerchief", 1, 0.1f, false,
          "Initials in the corner, stitched by someone with time and affection.");
        T("Field Surgeon's Kit",      "Field Surgeon's Kit",      5, 1.5f, false,
          "Needle, gut thread, a small saw. You hope the saw stays clean.");
        T("Merchant's Ledger",        "Merchant's Ledger",        3, 1f,   false,
          "Someone else's numbers. You can read a fair price out of them now.");

        // ---------------- Consumables ----------------
        C("Minor Healing Draught",  "Minor Healing Draught",  15, 0, 0, 25, 0.4f);
        C("Healing Draught",        "Healing Draught",        35, 0, 0, 70, 0.5f, 20, CraftDiscipline.Alchemist,
          "Bitter enough that you know it is working.");
        C("Strong Healing Draught", "Strong Healing Draught", 70, 0, 0, 160, 0.6f);
        C("Antidote",               "Antidote",                0, 0, 0, 60, 0.3f);
        C("Fever Tonic",            "Fever Tonic",            10, 1, 0, 55, 0.3f);
        C("Bread",                  "Bread",                   2, 0, 1,  4, 0.5f, 30, CraftDiscipline.Alchemist,
          "Yesterday's loaf. It is still bread.");
        C("Dried Meat",             "Dried Meat",              4, 0, 2, 12, 0.4f, 30);
        C("Cheese Wheel",           "Cheese Wheel",            5, 0, 2, 18, 1.5f, 10);
        C("Salted Fish",            "Salted Fish",             3, 0, 1,  9, 0.5f, 30);
        C("Travel Ration",          "RationPack",           3, 1, 3, 20, 0.8f, 30,  CraftDiscipline.Alchemist,
          "Packed for the road: hard bread, salt meat, something dried and unidentifiable.");
        C("Ale",                    "Ale",                     2, 1, 0,  6, 1f,  12);
        C("Wine",                   "Wine",                    3, 1, 0, 22, 1f,  12);
        C("Bandage",                "Bandage",                 8, 0, 0, 10, 0.2f, 20, CraftDiscipline.Tanner);
        C("Clean Linen",            "Clean Linen",            12, 0, 0, 18, 0.2f, 20, CraftDiscipline.Tanner);
        C("Herbal Poultice",        "Herbal Poultice",        20, 1, 0, 35, 0.3f);
        C("Splint",                 "Splint",                  6, 0, 0, 14, 0.6f, 10, CraftDiscipline.Carpenter);

        // ---------------- Raw resources ----------------
        R("Iron Ore",     "Iron Ore",     ItemCategory.Resource,  6, 2f);
        R("Copper Ore",   "Copper Ore",   ItemCategory.Resource,  5, 2f);
        R("Tin Ore",      "Tin Ore",      ItemCategory.Resource,  5, 2f);
        R("Silver Ore",   "Silver Ore",   ItemCategory.Resource, 20, 2f);
        R("Coal",         "Coal",         ItemCategory.Resource,  3, 1.5f);
        R("Raw Hide",     "Raw Hide",     ItemCategory.Resource,  8, 2f,   CraftDiscipline.Tanner);
        R("Pelt",         "Pelt",         ItemCategory.Resource, 14, 1.5f, CraftDiscipline.Tanner);
        R("Wool",         "Wool",         ItemCategory.Resource,  6, 1f,   CraftDiscipline.Tanner);
        R("Bone",         "Bone",         ItemCategory.Resource,  3, 0.8f);
        R("Sinew",        "Sinew",        ItemCategory.Resource,  5, 0.2f, CraftDiscipline.Tanner);
        R("Horn",         "Horn",         ItemCategory.Resource,  7, 0.8f);
        R("Timber Log",   "Timber Log",   ItemCategory.Resource,  4, 6f,   CraftDiscipline.Carpenter);
        R("Ash Log",      "Ash Log",      ItemCategory.Resource,  6, 6f,   CraftDiscipline.Carpenter);
        R("Yew Branch",   "Yew Branch",   ItemCategory.Resource, 12, 2f,   CraftDiscipline.Carpenter);
        R("Rough Stone",  "Rough Stone",  ItemCategory.Resource,  2, 8f,   CraftDiscipline.Mason);
        R("Limestone",    "Limestone",    ItemCategory.Resource,  3, 8f,   CraftDiscipline.Mason);
        R("Clay",         "Clay",         ItemCategory.Resource,  2, 4f,   CraftDiscipline.Mason);
        R("Common Herbs", "herb",        ItemCategory.Resource,  4, 0.2f, CraftDiscipline.Alchemist);
        R("Bitterroot",   "Bitterroot",   ItemCategory.Resource,  9, 0.2f, CraftDiscipline.Alchemist);
        R("Marshbloom",   "Marshbloom",   ItemCategory.Resource, 11, 0.2f, CraftDiscipline.Alchemist);
        R("Fireleaf",     "Fireleaf",     ItemCategory.Resource, 15, 0.2f, CraftDiscipline.Alchemist);

        // ---------------- Processed materials ----------------
        // The metal ladder is real metallurgy, not fantasy alloys:
        //   copper -> bronze (needs tin) -> iron -> steel -> meteoric iron.
        // Each step is gated by heat and knowledge rather than by a rarer rock,
        // which keeps the world grounded and gives tin a reason to exist.
        R("Iron Ingot",      "Iron_ıngot",      ItemCategory.CraftingMaterial, 18, 2f);
        R("Steel Ingot",     "steel_ingot",     ItemCategory.CraftingMaterial, 40, 2f);
        R("Copper Ingot",    "Copper Ingot",    ItemCategory.CraftingMaterial, 14, 2f);
        R("Tin Ingot",       "Tin Ingot",       ItemCategory.CraftingMaterial, 16, 2f);
        R("Bronze Ingot",    "Bronze Ingot",    ItemCategory.CraftingMaterial, 22, 2f);
        R("Silver Ingot",    "Silver Ingot",    ItemCategory.CraftingMaterial, 60, 2f);

        // Found, never smelted. This is the legendary tier without inventing
        // a magic metal — historically it was prized above gold for exactly
        // this reason.
        list.Add(new ItemDef
        {
            Id = id++, Name = "Meteoric Iron", SpriteBase = "Meteoric Iron",
            Category = ItemCategory.CraftingMaterial,
            Gold = 4, Weight = 2f, Stackable = true, MaxStack = 20,
            Craftable = false,
            Flavor = "Heavier than it looks and already dark before the forge touches it. " +
                     "It fell out of the sky, and everyone who handles it knows that."
        });
        R("Rivets",          "Rivets",          ItemCategory.CraftingMaterial,  6, 0.2f);
        R("Buckle",          "Buckle",          ItemCategory.CraftingMaterial,  8, 0.2f);
        R("Tanned Leather",  "Leather",         ItemCategory.CraftingMaterial, 20, 1.5f, CraftDiscipline.Tanner);
        R("Cured Hide",      "Cured Hide",      ItemCategory.CraftingMaterial, 16, 1.8f, CraftDiscipline.Tanner);
        R("Leather Strap",   "Leather Strap",   ItemCategory.CraftingMaterial,  7, 0.3f, CraftDiscipline.Tanner);
        R("Thread",          "Thread",          ItemCategory.CraftingMaterial,  4, 0.1f, CraftDiscipline.Tanner);
        R("Cloth Bolt",      "Cloth Bolt",      ItemCategory.CraftingMaterial, 15, 2f,   CraftDiscipline.Tanner);
        R("Plank",           "WOODEN PLANK",           ItemCategory.CraftingMaterial, 10, 3f,   CraftDiscipline.Carpenter);
        R("Beam",            "Beam",            ItemCategory.CraftingMaterial, 16, 8f,   CraftDiscipline.Carpenter);
        R("Bowstring",       "Bowstring",       ItemCategory.CraftingMaterial, 12, 0.1f, CraftDiscipline.Carpenter);
        R("Charcoal",        "Charcoal",        ItemCategory.CraftingMaterial,  6, 1f,   CraftDiscipline.Carpenter);
        R("Pitch",           "Pitch",           ItemCategory.CraftingMaterial,  8, 1f,   CraftDiscipline.Carpenter);
        R("Cut Stone",       "Cut Stone",       ItemCategory.CraftingMaterial,  9, 10f,  CraftDiscipline.Mason);
        R("Mortar",          "Mortar",          ItemCategory.CraftingMaterial,  7, 4f,   CraftDiscipline.Mason);
        R("Brick",           "STONE BRICK",           ItemCategory.CraftingMaterial,  5, 3f,   CraftDiscipline.Mason);
        R("Distilled Spirit","Distilled Spirit",ItemCategory.CraftingMaterial, 25, 0.5f, CraftDiscipline.Alchemist);
        R("Herbal Extract",  "Herbal Extract",  ItemCategory.CraftingMaterial, 30, 0.3f, CraftDiscipline.Alchemist);
        R("Glass Vial",      "Glass Vial",      ItemCategory.CraftingMaterial, 12, 0.2f, CraftDiscipline.Alchemist);

        // ---------------- Trade goods ----------------
        R("Salt",        "Salt",        ItemCategory.TradeGood,  30, 1f);
        R("Spice Pouch", "Spice Pouch", ItemCategory.TradeGood,  90, 0.5f);
        R("Silk Bolt",   "Silk Bolt",   ItemCategory.TradeGood, 140, 1.5f);
        R("Dyed Cloth",  "Dyed Cloth",  ItemCategory.TradeGood,  70, 1.5f);
        R("Amber",       "Amber",       ItemCategory.TradeGood, 120, 0.3f);
        R("Fine Furs",   "Fine Furs",   ItemCategory.TradeGood, 110, 3f);
        R("Wine Cask",   "Wine Cask",   ItemCategory.TradeGood,  80, 12f);
        R("Honey Jar",   "Honey Jar",   ItemCategory.TradeGood,  45, 2f);
        R("Beeswax",     "Beeswax",     ItemCategory.TradeGood,  35, 1f);
        R("Ivory Comb",  "Ivory Comb",  ItemCategory.TradeGood, 100, 0.3f);

        // ---------------- Quest items ----------------
        R("Sealed Letter",   "Sealed Letter",   ItemCategory.QuestItem, 0, 0.1f);
        R("Lord's Signet",   "Lord's Signet",   ItemCategory.QuestItem, 0, 0.1f);
        R("Stolen Ledger",   "Stolen Ledger",   ItemCategory.QuestItem, 0, 1f);
        R("Family Heirloom", "Family Heirloom", ItemCategory.QuestItem, 0, 0.5f);
        R("Map Fragment",    "Map Fragment",    ItemCategory.QuestItem, 0, 0.1f);
        R("Bandit's Token",  "Bandit's Token",  ItemCategory.QuestItem, 0, 0.1f);

        // ---------------- Misc ----------------
        R("Rope",            "Rope",            ItemCategory.Misc, 18, 4f,   CraftDiscipline.Tanner);
        R("Torch",           "Torch",           ItemCategory.Misc,  4, 1f,   CraftDiscipline.Carpenter);
        R("Flint & Steel",   "Flint and Steel", ItemCategory.Misc, 14, 0.3f);
        R("Cooking Pot",     "Cooking Pot",     ItemCategory.Misc, 22, 3f);
        R("Bedroll",         "Bedroll",         ItemCategory.Misc, 25, 4f,   CraftDiscipline.Tanner);
        R("Waterskin",       "Waterskin",       ItemCategory.Misc, 12, 1f,   CraftDiscipline.Tanner);
        R("Fishing Line",    "Fishing Line",    ItemCategory.Misc,  9, 0.2f, CraftDiscipline.Tanner);
        R("Lockpick",        "Lockpick",        ItemCategory.Misc, 30, 0.1f);
        R("Empty Sack",      "Empty Sack",      ItemCategory.Misc,  5, 0.4f, CraftDiscipline.Tanner);
        R("Whetstone",       "Whetstone",       ItemCategory.Misc, 16, 1f,   CraftDiscipline.Mason);

        return list;
    }
}
