using UnityEngine;

/// <summary>
/// Where an item is worn. Distinct from ItemCategory: a greatsword is one
/// category but blocks two slots; a shield and an off-hand dagger share the
/// off-hand while being different categories.
/// </summary>
public enum EquipSlot
{
    None,
    MainHand,
    OffHand,
    Head,
    Chest,
    Legs,
    Feet,
    Hands,
    Trinket1,
    Trinket2
}

/// <summary>Which attribute drives a weapon's attack and damage.</summary>
public enum ScalingStat
{
    Strength,
    Dexterity,
    /// <summary>Thrown weapons — javelin, spear. Uses the average of STR and DEX.</summary>
    Hybrid
}

public enum WeaponClass
{
    None,
    Sword,
    Axe,
    Mace,
    Dagger,
    Polearm,
    Bow,
    Crossbow,
    Thrown,
    Improvised
}

/// <summary>
/// How much armour lets dexterity contribute to defence.
/// Light: full DEX modifier. Medium: capped at +2. Heavy: none.
/// </summary>
public enum ArmorWeight
{
    None,
    Light,
    Medium,
    Heavy
}

/// <summary>
/// Craftsmanship, not magic. Legendary is unique — exactly one of each
/// legendary item exists in the world.
/// </summary>
public enum ItemQuality
{
    Crude = 0,
    Common = 1,
    Fine = 2,
    Masterwork = 3,
    Legendary = 4
}

/// <summary>
/// Shared rules about quality and slots. Kept in one place so the importer,
/// the crafting system and the tooltip all agree.
/// </summary>
public static class ItemRules
{
    /// <summary>Value and stat multiplier per quality tier, in percent.</summary>
    public static readonly int[] QualityMultipliers = { 80, 100, 130, 170, 220 };

    public static readonly string[] QualityNames =
        { "Crude", "Common", "Fine", "Masterwork", "Legendary" };

    /// <summary>Colour used for the quality label in tooltips and lists.</summary>
    public static Color QualityColor(ItemQuality q)
    {
        switch (q)
        {
            case ItemQuality.Crude:      return new Color(0.60f, 0.58f, 0.54f);
            case ItemQuality.Common:     return new Color(0.88f, 0.86f, 0.80f);
            case ItemQuality.Fine:       return new Color(0.42f, 0.68f, 0.90f);
            case ItemQuality.Masterwork: return new Color(0.70f, 0.50f, 0.90f);
            case ItemQuality.Legendary:  return new Color(0.94f, 0.75f, 0.30f);
            default:                     return Color.white;
        }
    }

    public static int Multiplier(ItemQuality q)
    {
        int i = Mathf.Clamp((int)q, 0, QualityMultipliers.Length - 1);
        return QualityMultipliers[i];
    }

    public static string Name(ItemQuality q)
    {
        int i = Mathf.Clamp((int)q, 0, QualityNames.Length - 1);
        return QualityNames[i];
    }

    /// <summary>The slot a category occupies by default.</summary>
    public static EquipSlot DefaultSlot(ItemCategory category)
    {
        switch (category)
        {
            case ItemCategory.Weapon:   return EquipSlot.MainHand;
            case ItemCategory.Shield:   return EquipSlot.OffHand;
            case ItemCategory.Helmet:   return EquipSlot.Head;
            case ItemCategory.Armor:    return EquipSlot.Chest;
            case ItemCategory.Leggings: return EquipSlot.Legs;
            case ItemCategory.Boots:    return EquipSlot.Feet;
            case ItemCategory.Gloves:   return EquipSlot.Hands;
            case ItemCategory.Trinket:  return EquipSlot.Trinket1;
            default:                    return EquipSlot.None;
        }
    }

    public static bool IsEquippable(ItemCategory category)
    {
        return DefaultSlot(category) != EquipSlot.None;
    }

    /// <summary>
    /// Only daggers and shortswords may be dual-wielded. Everything else in
    /// the off-hand must be a shield.
    /// </summary>
    public static bool CanDualWield(WeaponClass weaponClass, bool twoHanded)
    {
        if (twoHanded) return false;
        return weaponClass == WeaponClass.Dagger || weaponClass == WeaponClass.Sword;
    }

    /// <summary>How much of the DEX modifier a given armour weight allows.</summary>
    public static int DexContribution(ArmorWeight weight, int dexModifier)
    {
        switch (weight)
        {
            case ArmorWeight.Light:  return dexModifier;
            case ArmorWeight.Medium: return Mathf.Min(dexModifier, 2);
            case ArmorWeight.Heavy:  return 0;
            default:                 return dexModifier;
        }
    }

    /// <summary>D&amp;D-style ability modifier. 10 is average, giving 0.</summary>
    public static int Modifier(int statValue)
    {
        return Mathf.FloorToInt((statValue - 10) / 2f);
    }
}
