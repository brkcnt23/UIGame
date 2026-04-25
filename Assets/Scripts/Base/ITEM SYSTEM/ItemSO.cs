using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemSO", menuName = "Items/ItemSO")]
public class ItemSO : ScriptableObject
{
    [Header("Identity")]
    public int ID;
    public string itemName;
    [TextArea] public string description;

    [Header("Appearance")]
    public Sprite icon;

    [Header("Quality Tiers")]
    [Range(0, 2)] public int quality = 0; // 0=Common, 1=Rare, 2=Epic
    public List<Sprite> spritesByQuality = new(); // [0]=Common, [1]=Rare, [2]=Epic
    public List<int> qualityMultipliers = new() { 100, 150, 200 }; // stat/value multipliers %

    [Header("Economy")]
    public int goldValue;
    public int silverValue;

    [Header("Logistics")]
    [Min(0f)] public float weight = 1f;

    [Header("Stacking")]
    public bool stackable = true;
    [Min(1)] public int maxStack = 99;

    [Header("Classification")]
    public ItemCategory category = ItemCategory.Misc;

    [Header("Modifiers / Stats")]
    public List<StatModifier> modifiers = new List<StatModifier>();

    [Header("Potion (optional)")]
    public int healthRecovery = 0;
    public int exhaustionReduction = 0;

    public bool IsPotion => category == ItemCategory.Potion;
    public bool IsEquippable =>
        category == ItemCategory.Weapon ||
        category == ItemCategory.Armor ||
        category == ItemCategory.Boots ||
        category == ItemCategory.Leggings ||
        category == ItemCategory.Misc;

    public Currency GetBaseValue()
    {
        return new Currency(goldValue, silverValue);
    }

    public Sprite GetSpriteForQuality(int quality)
    {
        if (spritesByQuality.Count == 0) return icon;
        quality = Mathf.Clamp(quality, 0, spritesByQuality.Count - 1);
        return spritesByQuality[quality];
    }

    public int GetMultiplier(int quality)
    {
        if (qualityMultipliers.Count == 0) return 100;
        quality = Mathf.Clamp(quality, 0, qualityMultipliers.Count - 1);
        return qualityMultipliers[quality];
    }

    // Create a runtime Item instance from this ScriptableObject
    public Item ToItem(int quantity = 1)
    {
        int finalQuantity = Mathf.Max(1, quantity);
        Sprite spriteForQuality = GetSpriteForQuality(quality);

        if (category == ItemCategory.Potion)
        {
            return new Item(
                ID,
                itemName,
                goldValue,
                silverValue,
                healthRecovery,
                exhaustionReduction,
                spriteForQuality,
                quality,
                finalQuantity,
                stackable,
                maxStack,
                weight
            );
        }

        // Equippable / modifier-based items
        if (modifiers != null && modifiers.Count > 0)
        {
            List<StatModifier> modsCopy = new List<StatModifier>(modifiers);

            return new Item(
                ID,
                itemName,
                goldValue,
                silverValue,
                category,
                modsCopy,
                spriteForQuality,
                quality,
                finalQuantity,
                stackable,
                maxStack,
                weight
            );
        }

        // Resources / crafting materials / simple items
        return new Item(
            ID,
            itemName,
            goldValue,
            silverValue,
            category,
            finalQuantity,
            stackable,
            maxStack,
            weight
        );
    }
}