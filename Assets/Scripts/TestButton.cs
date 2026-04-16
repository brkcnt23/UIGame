using System.Collections.Generic;
using UnityEngine;

public class TestButton : MonoBehaviour
{
    [Header("Databases")]
    public ItemDatabase itemDatabase;
    public RecipeDatabaseSO recipeDatabase;

    [Header("Test Recipe")]
    public int testRecipeId = 1;

    [Header("Test Material IDs")]
    public int ironIngotItemId = 6;
    public int leatherItemId = 7;
    public int herbItemId = 8;

    [Header("Test Quantities")]
    public int ironIngotQuantity = 20;
    public int leatherQuantity = 20;
    public int herbQuantity = 20;

    [Header("Craft Access")]
    public bool unlockForge = true;
    public bool unlockHammer = true;
    public bool learnRecipe = true;

    [ContextMenu("TEST/Give Materials")]
    public void GiveMaterials()
    {
        if (InventorySystem.Instance == null)
        {
            Debug.LogWarning("TestButton: InventorySystem.Instance is null.");
            return;
        }

        List<ItemStackData> stacks = new List<ItemStackData>
        {
            new ItemStackData { ItemId = ironIngotItemId, Quantity = ironIngotQuantity },
            new ItemStackData { ItemId = leatherItemId, Quantity = leatherQuantity },
            new ItemStackData { ItemId = herbItemId, Quantity = herbQuantity }
        };

        ItemRewardHelper.GiveItems(stacks);
        Debug.Log("TestButton: Test materials given.");
    }

    [ContextMenu("TEST/Give Craft Access")]
    public void GiveCraftAccess()
    {
        if (PlayerStatHandler.Instance == null || PlayerStatHandler.Instance.pd == null)
        {
            Debug.LogWarning("TestButton: PlayerStatHandler or PlayerData is null.");
            return;
        }

        if (CraftingSystem.Instance == null)
        {
            Debug.LogWarning("TestButton: CraftingSystem.Instance is null.");
            return;
        }

        PlayerData pd = PlayerStatHandler.Instance.pd;

        if (pd.HistoryTags == null) pd.HistoryTags = new List<string>();
        if (pd.ActiveTraitTags == null) pd.ActiveTraitTags = new List<string>();
        if (pd.LearnedRecipeIds == null) pd.LearnedRecipeIds = new List<int>();
        if (pd.LearnedStations == null) pd.LearnedStations = new List<string>();
        if (pd.LearnedTools == null) pd.LearnedTools = new List<string>();

        if (unlockForge && !pd.LearnedStations.Contains("forge"))
            pd.LearnedStations.Add("forge");

        if (unlockHammer && !pd.LearnedTools.Contains("hammer"))
            pd.LearnedTools.Add("hammer");

        if (learnRecipe && !pd.LearnedRecipeIds.Contains(testRecipeId))
            pd.LearnedRecipeIds.Add(testRecipeId);

        // test için skill aç
        pd.SmitherSkillLevel = Mathf.Max(pd.SmitherSkillLevel, 10);
        pd.TannerSkillLevel = Mathf.Max(pd.TannerSkillLevel, 10);
        pd.CarpenterSkillLevel = Mathf.Max(pd.CarpenterSkillLevel, 10);
        pd.MasonSkillLevel = Mathf.Max(pd.MasonSkillLevel, 10);
        pd.AlchemistSkillLevel = Mathf.Max(pd.AlchemistSkillLevel, 10);

        Debug.Log("TestButton: Craft access granted.");
    }

    [ContextMenu("TEST/Can Craft Recipe")]
    public void CheckCanCraftRecipe()
    {
        if (CraftingSystem.Instance == null)
        {
            Debug.LogWarning("TestButton: CraftingSystem.Instance is null.");
            return;
        }

        bool result = CraftingSystem.Instance.CanCraftRecipe(testRecipeId);
        Debug.Log($"TestButton: Can craft recipe {testRecipeId} = {result}");
    }

    [ContextMenu("TEST/Craft Recipe")]
    public void CraftRecipe()
    {
        if (CraftingSystem.Instance == null)
        {
            Debug.LogWarning("TestButton: CraftingSystem.Instance is null.");
            return;
        }

        bool result = CraftingSystem.Instance.CraftRecipe(testRecipeId);
        Debug.Log($"TestButton: Craft recipe {testRecipeId} result = {result}");
    }

    [ContextMenu("TEST/Give All And Craft")]
    public void GiveAllAndCraft()
    {
        GiveMaterials();
        GiveCraftAccess();
        CheckCanCraftRecipe();
        CraftRecipe();
    }

    [ContextMenu("TEST/Clear Learned Recipe")]
    public void ClearLearnedRecipe()
    {
        if (PlayerStatHandler.Instance == null || PlayerStatHandler.Instance.pd == null)
            return;

        if (PlayerStatHandler.Instance.pd.LearnedRecipeIds == null)
            return;

        PlayerStatHandler.Instance.pd.LearnedRecipeIds.Remove(testRecipeId);
        Debug.Log($"TestButton: Removed learned recipe {testRecipeId}");
    }

    [ContextMenu("TEST/Log Recipe Info")]
    public void LogRecipeInfo()
    {
        if (recipeDatabase == null)
        {
            Debug.LogWarning("TestButton: recipeDatabase is null.");
            return;
        }

        RecipeSO recipe = recipeDatabase.GetById(testRecipeId);
        if (recipe == null)
        {
            Debug.LogWarning($"TestButton: Recipe not found -> {testRecipeId}");
            return;
        }

        Debug.Log(
            $"Recipe: {recipe.recipeName}\n" +
            $"OutputItemId: {recipe.OutputItemId}\n" +
            $"OutputQuantity: {recipe.OutputQuantity}\n" +
            $"Discipline: {recipe.discipline}\n" +
            $"RequiredLevel: {recipe.requiredDisciplineLevel}\n" +
            $"LearnedByDefault: {recipe.learnedByDefault}"
        );
    }
}