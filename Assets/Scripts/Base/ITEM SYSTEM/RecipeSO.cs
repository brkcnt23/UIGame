using System.Collections.Generic;
using UnityEngine;

public enum CraftDiscipline
{
    Blacksmithing,
    Tanning,
    Carpentry,
    Masonry,
    Alchemy,
    Cooking,
    Misc
}

[System.Serializable]
public class RecipeIngredient
{
    public int ItemId;
    public int Quantity = 1;
}

[CreateAssetMenu(fileName = "Recipe", menuName = "Crafting/Recipe")]
public class RecipeSO : ScriptableObject
{
    [Header("Identity")]
    public int RecipeId;
    public string recipeName;
    [TextArea] public string description;

    [Header("Output")]
    public int OutputItemId;
    public int OutputQuantity = 1;

    [Header("Ingredients")]
    public List<RecipeIngredient> ingredients = new();

    [Header("Craft Rules")]
    public CraftDiscipline discipline = CraftDiscipline.Misc;
    public int requiredDisciplineLevel = 1;
    public int baseCraftTimeMinutes = 30;
    [Range(0f, 1f)] public float baseSuccessChance = 1f;

    [Header("Unlock / Context")]
    public bool learnedByDefault = false;
    public List<string> requiredPlayerTags = new();
    public List<string> requiredSettlementTags = new();
    public List<string> requiredNpcTags = new();

    [Header("Stations / Tools")]
    public List<string> requiredStations = new(); // forge, anvil, tanning_rack, alchemy_table...
    public List<string> requiredTools = new();    // hammer, tongs, saw, mortar...

    public bool IsValid(ItemDatabase db)
    {
        if (db == null) return false;
        if (!db.ContainsID(OutputItemId)) return false;

        foreach (var ing in ingredients)
        {
            if (ing == null) continue;
            if (ing.Quantity <= 0) return false;
            if (!db.ContainsID(ing.ItemId)) return false;
        }

        return true;
    }

    public ItemSO GetOutput(ItemDatabase db)
    {
        return db == null ? null : db.GetByID(OutputItemId);
    }

    public List<ItemStackData> ToItemStacks()
    {
        List<ItemStackData> result = new();

        foreach (var ing in ingredients)
        {
            if (ing == null || ing.Quantity <= 0) continue;

            result.Add(new ItemStackData
            {
                ItemId = ing.ItemId,
                Quantity = ing.Quantity
            });
        }

        return result;
    }
}