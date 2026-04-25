using System.Collections.Generic;
using UnityEngine;

public class CraftingSystem : MonoBehaviour
{
    private PlayerData playerData;
    private TimeSystem timeSystem;

    public static CraftingSystem Instance { get; private set; }

    [Header("Databases")]
    public ItemDatabase itemDatabase;
    public RecipeDatabaseSO recipeDatabase;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        RefreshReferences();

        if (itemDatabase == null)
        {
            itemDatabase = Resources.Load<ItemDatabase>("ItemDatabase");
            if (itemDatabase == null)
            {
                Debug.LogWarning("CraftingSystem: ItemDatabase not found in Resources.");
            }
        }
    }

    private void RefreshReferences()
    {
        if (PlayerStatHandler.Instance != null)
            playerData = PlayerStatHandler.Instance.pd;

        if (TimeSystem.Instance != null)
            timeSystem = TimeSystem.Instance;
    }

    public RecipeSO GetRecipe(int recipeId)
    {
        if (recipeDatabase == null) return null;
        return recipeDatabase.GetById(recipeId);
    }

    public List<RecipeSO> GetAllRecipes()
    {
        if (recipeDatabase == null || recipeDatabase.recipes == null)
            return new List<RecipeSO>();

        return recipeDatabase.recipes;
    }

    public List<RecipeSO> GetAvailableRecipes(CraftDiscipline discipline)
    {
        List<RecipeSO> result = new List<RecipeSO>();

        if (recipeDatabase == null || itemDatabase == null)
            return result;

        var recipes = recipeDatabase.GetByDiscipline(discipline);
        foreach (var recipe in recipes)
        {
            if (recipe == null) continue;
            if (CanCraftRecipe(recipe))
                result.Add(recipe);
        }

        return result;
    }

    public bool CanCraftRecipe(int recipeId)
    {
        RecipeSO recipe = GetRecipe(recipeId);
        return CanCraftRecipe(recipe);
    }

    public bool CanCraftRecipe(RecipeSO recipe)
    {
        RefreshReferences();

        if (recipe == null || itemDatabase == null || playerData == null)
            return false;

        if (!recipe.IsValid(itemDatabase))
            return false;

        if (!PassLearnedRecipeRequirement(recipe))
            return false;

        if (!PassPlayerSkillRequirement(recipe))
            return false;

        if (!PassPlayerTagRequirement(recipe))
            return false;

        if (!PassSettlementTagRequirement(recipe))
            return false;

        if (!PassStationRequirement(recipe))
            return false;

        if (!PassToolRequirement(recipe))
            return false;

        if (!ItemRewardHelper.HasItems(recipe.ToItemStacks()))
            return false;

        return true;
    }

    public bool CraftRecipe(int recipeId)
    {
        RecipeSO recipe = GetRecipe(recipeId);
        return CraftRecipe(recipe);
    }

    public bool CraftRecipe(RecipeSO recipe)
    {
        RefreshReferences();

        if (!CanCraftRecipe(recipe))
            return false;

        ItemRewardHelper.RemoveItems(recipe.ToItemStacks());

        var outputSo = recipe.GetOutput(itemDatabase);
        if (outputSo == null)
            return false;

        GameBootstrapper.Events?.Dispatch(new AddItemEvent(outputSo.ID, recipe.OutputQuantity));

        GrantCraftDisciplineXP(recipe);
        AdvanceCraftTime(recipe.baseCraftTimeMinutes);
        RefreshUI();

        Debug.Log($"Crafted: {outputSo.itemName} x{recipe.OutputQuantity}");
        return true;
    }

    public void LearnRecipe(int recipeId)
    {
        RefreshReferences();
        if (playerData == null) return;

        if (playerData.LearnedRecipeIds == null)
            playerData.LearnedRecipeIds = new List<int>();

        if (!playerData.LearnedRecipeIds.Contains(recipeId))
            playerData.LearnedRecipeIds.Add(recipeId);
    }

    public void LearnStation(string stationTag)
    {
        RefreshReferences();
        if (playerData == null || string.IsNullOrWhiteSpace(stationTag)) return;

        if (playerData.LearnedStations == null)
            playerData.LearnedStations = new List<string>();

        if (!playerData.LearnedStations.Contains(stationTag))
            playerData.LearnedStations.Add(stationTag);
    }

    public void LearnTool(string toolTag)
    {
        RefreshReferences();
        if (playerData == null || string.IsNullOrWhiteSpace(toolTag)) return;

        if (playerData.LearnedTools == null)
            playerData.LearnedTools = new List<string>();

        if (!playerData.LearnedTools.Contains(toolTag))
            playerData.LearnedTools.Add(toolTag);
    }

    private bool PassLearnedRecipeRequirement(RecipeSO recipe)
    {
        if (recipe.learnedByDefault)
            return true;

        if (playerData.LearnedRecipeIds == null)
            return false;

        return playerData.LearnedRecipeIds.Contains(recipe.RecipeId);
    }

    private bool PassPlayerSkillRequirement(RecipeSO recipe)
    {
        int level = GetDisciplineLevel(recipe.discipline);
        return level >= Mathf.Max(1, recipe.requiredDisciplineLevel);
    }

    private bool PassPlayerTagRequirement(RecipeSO recipe)
    {
        return ContainsAll(GetPlayerTags(), recipe.requiredPlayerTags);
    }

    private bool PassSettlementTagRequirement(RecipeSO recipe)
    {
        return ContainsAll(GetSettlementTags(), recipe.requiredSettlementTags);
    }

    private bool PassStationRequirement(RecipeSO recipe)
    {
        return ContainsAll(GetPlayerTags(), recipe.requiredStations);
    }

    private bool PassToolRequirement(RecipeSO recipe)
    {
        return ContainsAll(GetPlayerTags(), recipe.requiredTools);
    }

    private List<string> GetPlayerTags()
    {
        List<string> tags = new List<string>();

        if (playerData == null)
            return tags;

        if (playerData.HistoryTags != null)
            tags.AddRange(playerData.HistoryTags);

        if (playerData.ActiveTraitTags != null)
            tags.AddRange(playerData.ActiveTraitTags);

        if (playerData.LearnedStations != null)
            tags.AddRange(playerData.LearnedStations);

        if (playerData.LearnedTools != null)
            tags.AddRange(playerData.LearnedTools);

        return tags;
    }

    private List<string> GetSettlementTags()
    {
        List<string> tags = new List<string>();

        if (SettlementHandler.Instance == null || SettlementHandler.Instance.settlement == null)
            return tags;

        if (SettlementHandler.Instance.settlement.SettlementTags != null)
            tags.AddRange(SettlementHandler.Instance.settlement.SettlementTags);

        return tags;
    }

    private bool ContainsAll(List<string> current, List<string> required)
    {
        if (required == null || required.Count == 0)
            return true;

        if (current == null || current.Count == 0)
            return false;

        foreach (var req in required)
        {
            if (string.IsNullOrWhiteSpace(req)) continue;
            if (!current.Contains(req))
                return false;
        }

        return true;
    }

    private int GetDisciplineLevel(CraftDiscipline discipline)
    {
        if (playerData == null) return 1;

        switch (discipline)
        {
            case CraftDiscipline.Blacksmithing: return playerData.SmitherSkillLevel;
            case CraftDiscipline.Tanning: return playerData.TannerSkillLevel;
            case CraftDiscipline.Carpentry: return playerData.CarpenterSkillLevel;
            case CraftDiscipline.Masonry: return playerData.MasonSkillLevel;
            case CraftDiscipline.Alchemy: return playerData.AlchemistSkillLevel;
            case CraftDiscipline.Cooking: return 1;
            default: return 1;
        }
    }

    private void GrantCraftDisciplineXP(RecipeSO recipe)
    {
        if (playerData == null) return;

        int xp = Mathf.Max(5, recipe.requiredDisciplineLevel * 10);

        switch (recipe.discipline)
        {
            case CraftDiscipline.Blacksmithing:
                AddSkillXP(ref playerData.SmitherSkillXP, ref playerData.SmitherSkillLevel, xp);
                break;
            case CraftDiscipline.Tanning:
                AddSkillXP(ref playerData.TannerSkillXP, ref playerData.TannerSkillLevel, xp);
                break;
            case CraftDiscipline.Carpentry:
                AddSkillXP(ref playerData.CarpenterSkillXP, ref playerData.CarpenterSkillLevel, xp);
                break;
            case CraftDiscipline.Masonry:
                AddSkillXP(ref playerData.MasonSkillXP, ref playerData.MasonSkillLevel, xp);
                break;
            case CraftDiscipline.Alchemy:
                AddSkillXP(ref playerData.AlchemistSkillXP, ref playerData.AlchemistSkillLevel, xp);
                break;
        }

        if (PlayerStatHandler.Instance != null)
            PlayerStatHandler.Instance.AddCharacterExperience(xp);
    }

    private void AddSkillXP(ref int xpField, ref int levelField, int amount)
    {
        xpField += amount;

        while (xpField >= 100)
        {
            xpField -= 100;
            levelField += 1;
        }
    }

    private void AdvanceCraftTime(int totalMinutes)
    {
        if (timeSystem == null || totalMinutes <= 0)
            return;

        int hours = totalMinutes / 60;
        int minutes = totalMinutes % 60;
        StartCoroutine(timeSystem.AdvanceTimeCoroutine(0, hours, minutes));
    }

    private void RefreshUI()
    {
        if (PlayerUISystem.Instance != null)
            PlayerUISystem.Instance.UpdateUIObjects();

        // UI updates handled by StateManager listeners
    }
}