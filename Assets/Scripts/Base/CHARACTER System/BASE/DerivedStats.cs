using UnityEngine;

/// <summary>
/// The numbers that are computed, never stored.
///
/// Attack, Defense, Accuracy and Critical are not fields on PlayerData —
/// they fall out of the four attributes plus equipment plus traits. Keeping
/// them derived means there is no second copy to fall out of sync when a
/// trait expires or a sword is swapped.
///
/// Follows the D&amp;D convention the design settled on:
///   Mod(stat) = floor((stat - 10) / 2)     10 is an ordinary adult
/// </summary>
public static class DerivedStats
{
    public static int Mod(int statValue) => Mathf.FloorToInt((statValue - 10) / 2f);

    public static int StrMod(PlayerData pd) => Mod(pd.Strength);
    public static int DexMod(PlayerData pd) => Mod(pd.Dexterity);
    public static int ConMod(PlayerData pd) => Mod(pd.Constitution);
    public static int ChaMod(PlayerData pd) => Mod(pd.Charisma);

    /// <summary>
    /// Attack bonus. Scales from whichever attribute the equipped weapon uses,
    /// so a dagger reads DEX and a maul reads STR without the player having to
    /// be told twice.
    /// </summary>
    public static int Attack(PlayerData pd)
    {
        if (pd == null) return 0;

        int fromWeapon = WeaponScalingMod(pd);
        int fromTraits = TraitSystem.Instance != null
            ? TraitSystem.Instance.GetFlatBonus(EffectType.Attack)
            : 0;

        return fromWeapon + fromTraits + EquipmentBonus(pd, EffectType.Attack);
    }

    /// <summary>
    /// Defence. Armour value plus however much dexterity the armour's weight
    /// still allows — plate gives none, leather gives all of it.
    /// </summary>
    public static int Defense(PlayerData pd)
    {
        if (pd == null) return 0;

        int armor = 0;

        // The heaviest worn piece decides how much dexterity still counts —
        // full plate on the chest cancels the benefit of light boots.
        ArmorWeight heaviest = ArmorWeight.Light;

        if (pd.Items != null)
        {
            foreach (var item in pd.Items)
            {
                if (item == null || !item.IsEquipped) continue;

                armor += item.GetArmorValue();

                if (item.ArmorClass > heaviest)
                    heaviest = item.ArmorClass;
            }
        }

        int dexAllowed = ItemRules.DexContribution(heaviest, DexMod(pd));

        int fromTraits = TraitSystem.Instance != null
            ? TraitSystem.Instance.GetFlatBonus(EffectType.Defense)
            : 0;

        return 10 + armor + dexAllowed + fromTraits;
    }

    /// <summary>Target number the player hits against. Higher is better.</summary>
    public static int Accuracy(PlayerData pd)
    {
        if (pd == null) return 0;

        int fromTraits = TraitSystem.Instance != null
            ? TraitSystem.Instance.GetFlatBonus(EffectType.Accuracy)
            : 0;

        return 10 + DexMod(pd) + fromTraits;
    }

    public static int Initiative(PlayerData pd)
    {
        return pd == null ? 0 : DexMod(pd);
    }

    /// <summary>Percent chance of a critical hit.</summary>
    public static int CriticalChance(PlayerData pd)
    {
        if (pd == null) return 0;

        // A natural 20 on a d20 is 5% before anything modifies it.
        int baseChance = 5;

        int fromTraits = TraitSystem.Instance != null
            ? TraitSystem.Instance.GetPercentBonus(EffectType.CriticalChance)
            : 0;

        return Mathf.Clamp(baseChance + fromTraits + Mathf.Max(0, DexMod(pd)), 0, 50);
    }

    /// <summary>Max health from constitution and level.</summary>
    public static int MaxHealth(PlayerData pd)
    {
        if (pd == null) return 0;

        int fromTraits = TraitSystem.Instance != null
            ? TraitSystem.Instance.GetFlatBonus(EffectType.MaxHealth)
            : 0;

        return Mathf.Max(1, 60 + (ConMod(pd) + 4) * Mathf.Max(1, pd.Level) + fromTraits);
    }

    // -----------------------------------------------------------------

    /// <summary>
    /// The modifier the equipped weapon scales from. Falls back to strength
    /// when nothing is equipped — an unarmed swing is a strength swing.
    /// </summary>
    private static int WeaponScalingMod(PlayerData pd)
    {
        var weapon = FindEquipped(pd, ItemCategory.Weapon);
        if (weapon == null) return StrMod(pd);

        var db = GameBootstrapper.Resources != null
            ? GameBootstrapper.Resources.GetItemDatabase()
            : null;

        var so = db != null ? db.GetByID(weapon.ID) : null;
        if (so == null) return StrMod(pd);

        switch (so.scaling)
        {
            case ScalingStat.Dexterity: return DexMod(pd);
            case ScalingStat.Hybrid:    return Mathf.RoundToInt((StrMod(pd) + DexMod(pd)) / 2f);
            default:                    return StrMod(pd);
        }
    }

    private static int EquipmentBonus(PlayerData pd, EffectType type)
    {
        if (pd?.Items == null) return 0;

        int total = 0;
        StatType mapped = MapToStatType(type);

        foreach (var item in pd.Items)
        {
            if (item == null || !item.IsEquipped || item.Modifiers == null) continue;

            foreach (var m in item.Modifiers)
                if (m.Type == mapped)
                    total += m.Value;
        }

        return total;
    }

    private static StatType MapToStatType(EffectType type)
    {
        switch (type)
        {
            case EffectType.Strength:     return StatType.Strength;
            case EffectType.Dexterity:    return StatType.Dexterity;
            case EffectType.Constitution: return StatType.Constitution;
            case EffectType.Charisma:     return StatType.Charisma;
            default:                      return StatType.Strength;
        }
    }

    private static Item FindEquipped(PlayerData pd, ItemCategory category)
    {
        if (pd?.Items == null) return null;

        foreach (var item in pd.Items)
            if (item != null && item.IsEquipped && item.Category == category)
                return item;

        return null;
    }
}
