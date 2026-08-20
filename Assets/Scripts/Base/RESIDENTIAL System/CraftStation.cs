using UnityEngine;

/// <summary>
/// A production building the player can work at: a forge, a tannery, a
/// carpenter's yard.
///
/// The station's level is a ceiling. A level 2 forge cannot smelt steel even
/// for a master smith — the fire does not get hot enough. That is what makes
/// a settlement's development matter to a travelling craftsman, and what
/// gives the player a reason to improve their own village rather than only
/// their own skills.
/// </summary>
[System.Serializable]
public class CraftStation : Residentials
{
    /// <summary>Which trade this station serves.</summary>
    public CraftDiscipline Discipline = CraftDiscipline.Smither;

    /// <summary>
    /// Station keywords a recipe can require: forge, anvil, tanning_rack,
    /// workbench, alchemy_table, mason_yard.
    /// </summary>
    public string StationTag = "forge";

    /// <summary>Fee in silver to use someone else's workshop for one job.</summary>
    public int UseFeeSilver = 10;

    /// <summary>True in the player's own settlement once they own the building.</summary>
    public bool PlayerOwned;

    /// <summary>
    /// Highest recipe level this station supports. Recipes above it are shown
    /// but locked, so the player can see what a better workshop would unlock.
    /// </summary>
    public int MaxRecipeLevel => Mathf.Max(1, level * 2);

    public bool CanCraft(int recipeLevel) => recipeLevel <= MaxRecipeLevel;

    /// <summary>
    /// A better workshop is faster. Level 1 is the baseline; each level takes
    /// another 8% off, floored so a great forge never makes work instant.
    /// </summary>
    public float TimeMultiplier => Mathf.Max(0.5f, 1f - (level - 1) * 0.08f);

    /// <summary>
    /// Better tools, better odds of a high quality result. Percentage points
    /// added to the player's own quality chance.
    /// </summary>
    public int QualityBonus => (level - 1) * 5;

    public int FeeFor(int recipeLevel)
    {
        if (PlayerOwned) return 0;
        return Mathf.Max(1, UseFeeSilver * Mathf.Max(1, recipeLevel / 2));
    }
}
