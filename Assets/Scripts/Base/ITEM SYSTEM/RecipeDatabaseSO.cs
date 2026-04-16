using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RecipeDatabase", menuName = "Crafting/RecipeDatabase")]
public class RecipeDatabaseSO : ScriptableObject
{
    public List<RecipeSO> recipes = new();

    private Dictionary<int, RecipeSO> byId;
    private Dictionary<string, RecipeSO> byName;

    private void OnEnable()
    {
        RebuildIndex();
    }

    public void RebuildIndex()
    {
        byId = new Dictionary<int, RecipeSO>();
        byName = new Dictionary<string, RecipeSO>();

        foreach (var recipe in recipes)
        {
            if (recipe == null) continue;

            if (!byId.ContainsKey(recipe.RecipeId))
                byId[recipe.RecipeId] = recipe;
            else
                Debug.LogWarning($"RecipeDatabaseSO: Duplicate RecipeId -> {recipe.RecipeId}");

            if (!string.IsNullOrWhiteSpace(recipe.recipeName))
            {
                if (!byName.ContainsKey(recipe.recipeName))
                    byName[recipe.recipeName] = recipe;
                else
                    Debug.LogWarning($"RecipeDatabaseSO: Duplicate recipeName -> {recipe.recipeName}");
            }
        }
    }

    public RecipeSO GetById(int id)
    {
        if (byId == null) RebuildIndex();
        byId.TryGetValue(id, out var recipe);
        return recipe;
    }

    public RecipeSO GetByName(string recipeName)
    {
        if (byName == null) RebuildIndex();
        byName.TryGetValue(recipeName, out var recipe);
        return recipe;
    }

    public List<RecipeSO> GetByDiscipline(CraftDiscipline discipline)
    {
        List<RecipeSO> result = new();

        foreach (var recipe in recipes)
        {
            if (recipe == null) continue;
            if (recipe.discipline == discipline)
                result.Add(recipe);
        }

        return result;
    }
}