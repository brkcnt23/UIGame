using System.Collections.Generic;
using UnityEngine;

public static class RecipeService
{
    public static bool CanCraft(RecipeSO recipe, ItemDatabase itemDb, RecipeContext context = null)
    {
        if (recipe == null || itemDb == null)
            return false;

        if (!recipe.IsValid(itemDb))
            return false;

        if (!PassContextRequirements(recipe, context))
            return false;

        if (!PassPlayerSkillRequirement(recipe, context != null ? context.Player : null))
            return false;

        if (!PassLearnedRecipeRequirement(recipe, context != null ? context.Player : null))
            return false;

        if (!ItemRewardHelper.HasItems(recipe.ToItemStacks()))
            return false;

        return true;
    }

    public static bool Craft(RecipeSO recipe, ItemDatabase itemDb, RecipeContext context = null)
    {
        if (!CanCraft(recipe, itemDb, context))
            return false;

        ItemRewardHelper.RemoveItems(recipe.ToItemStacks());

        var outputSo = recipe.GetOutput(itemDb);
        if (outputSo == null)
            return false;

        var eventBus = GameBootstrapper.Events;
        if (eventBus == null)
            return false;

        // Dispatch add item event (StateManager listeners handle UI updates)
        eventBus.Dispatch(new AddItemEvent(outputSo.ID, recipe.OutputQuantity));

        GrantCraftDisciplineXP(recipe, context != null ? context.Player : null);

        return true;
    }

    private static bool PassLearnedRecipeRequirement(RecipeSO recipe, PlayerData player)
    {
        if (recipe.learnedByDefault)
            return true;

        if (player == null || player.LearnedRecipeIds == null)
            return false;

        return player.LearnedRecipeIds.Contains(recipe.RecipeId);
    }

    private static bool PassContextRequirements(RecipeSO recipe, RecipeContext context)
    {
        if (context == null)
        {
            return recipe.requiredPlayerTags.Count == 0 &&
                   recipe.requiredSettlementTags.Count == 0 &&
                   recipe.requiredNpcTags.Count == 0 &&
                   recipe.requiredStations.Count == 0 &&
                   recipe.requiredTools.Count == 0;
        }

        List<string> playerTags = context.GetPlayerTags();
        List<string> settlementTags = context.GetSettlementTags();
        List<string> npcTags = context.GetNpcTags();

        if (!ContainsAll(playerTags, recipe.requiredPlayerTags))
            return false;

        if (!ContainsAll(settlementTags, recipe.requiredSettlementTags))
            return false;

        if (!ContainsAll(npcTags, recipe.requiredNpcTags))
            return false;

        if (!ContainsAll(playerTags, recipe.requiredStations))
            return false;

        if (!ContainsAll(playerTags, recipe.requiredTools))
            return false;

        return true;
    }

    private static bool PassPlayerSkillRequirement(RecipeSO recipe, PlayerData player)
    {
        if (player == null)
            return recipe.requiredDisciplineLevel <= 1;

        int level = GetDisciplineLevel(player, recipe.discipline);
        return level >= Mathf.Max(1, recipe.requiredDisciplineLevel);
    }

    private static int GetDisciplineLevel(PlayerData player, CraftDiscipline discipline)
    {
        switch (discipline)
        {
            case CraftDiscipline.Blacksmithing: return player.SmitherSkillLevel;
            case CraftDiscipline.Tanning: return player.TannerSkillLevel;
            case CraftDiscipline.Carpentry: return player.CarpenterSkillLevel;
            case CraftDiscipline.Masonry: return player.MasonSkillLevel;
            case CraftDiscipline.Alchemy: return player.AlchemistSkillLevel;
            case CraftDiscipline.Cooking: return 1;
            default: return 1;
        }
    }

    private static void GrantCraftDisciplineXP(RecipeSO recipe, PlayerData player)
    {
        if (player == null) return;

        int xp = Mathf.Max(1, recipe.requiredDisciplineLevel * 5);

        switch (recipe.discipline)
        {
            case CraftDiscipline.Blacksmithing:
                player.SmitherSkillXP += xp;
                break;
            case CraftDiscipline.Tanning:
                player.TannerSkillXP += xp;
                break;
            case CraftDiscipline.Carpentry:
                player.CarpenterSkillXP += xp;
                break;
            case CraftDiscipline.Masonry:
                player.MasonSkillXP += xp;
                break;
            case CraftDiscipline.Alchemy:
                player.AlchemistSkillXP += xp;
                break;
        }
    }

    private static bool ContainsAll(List<string> current, List<string> required)
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
}