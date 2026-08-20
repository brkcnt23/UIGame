using System.Collections.Generic;
using UnityEngine;
using NEXUS.Utilities;

/// <summary>
/// Reworking a piece of gear at a forge: raising its quality and rerolling the
/// two hidden properties.
///
/// The rule that keeps upgrading worth doing: a reroll never hands back
/// something worse. Each new roll is compared against the one it replaces and
/// the better of the two is kept. Losing a good blade to a bad reroll teaches
/// the player to stop upgrading, which kills the system.
///
/// What the player risks instead is the material and the hours. A failed
/// attempt consumes both and leaves the item untouched — expensive, but never
/// destructive. Gear that can be destroyed by improving it is gear nobody
/// improves.
/// </summary>
public sealed class ItemUpgradeSystem : GameSystemBase
{
    public override int Priority => SystemPriority.Crafting + 5;

    public static ItemUpgradeSystem Instance { get; private set; }

    [Header("Cost")]
    [Tooltip("Ingots or leather consumed per attempt, before the tier multiplier.")]
    [SerializeField] private int baseMaterialCost = 2;

    [Tooltip("Silver charged per attempt when using someone else's workshop.")]
    [SerializeField] private int baseFeeSilver = 40;

    [Tooltip("In-game minutes per attempt, before the station multiplier.")]
    [SerializeField] private int baseMinutes = 90;

    [Header("Odds")]
    [Tooltip("Chance of success at exactly the required level.")]
    [Range(0, 100)]
    [SerializeField] private int baseSuccessChance = 55;

    [Tooltip("Added per craft level above the requirement.")]
    [SerializeField] private int chancePerLevelOver = 6;

    [SerializeField] private bool verbose;

    protected override void OnInitialize() => Instance = this;

    protected override void OnShutdown()
    {
        if (Instance == this) Instance = null;
    }

    // -----------------------------------------------------------------
    // Queries the UI asks before showing the button
    // -----------------------------------------------------------------

    public sealed class UpgradeQuote
    {
        public bool Possible;
        public string Reason;

        public ItemQuality CurrentQuality;
        public ItemQuality NextQuality;

        public int RequiredCraftLevel;
        public int PlayerCraftLevel;
        public int SuccessChance;

        public int MaterialCost;
        public int FeeSilver;
        public int Minutes;

        public CraftDiscipline Discipline;
    }

    /// <summary>Everything the upgrade screen needs to describe the attempt.</summary>
    public UpgradeQuote GetQuote(Item item, CraftStation station)
    {
        var quote = new UpgradeQuote();

        if (item == null)
        {
            quote.Reason = "Nothing selected.";
            return quote;
        }

        var template = LookupTemplate(item.ID);
        if (template == null || !template.IsEquippable)
        {
            quote.Reason = "Only gear can be reworked.";
            return quote;
        }

        quote.CurrentQuality = (ItemQuality)Mathf.Clamp(item.Quality, 0, 4);

        if (quote.CurrentQuality >= ItemQuality.Masterwork)
        {
            // Legendary is placed in the world, never forged. A smith can
            // reach Masterwork and no further.
            quote.Reason = quote.CurrentQuality == ItemQuality.Legendary
                ? "Nothing can be added to this."
                : "This is already the finest work a forge can manage.";
            return quote;
        }

        quote.NextQuality = quote.CurrentQuality + 1;
        quote.Discipline = template.category == ItemCategory.Weapon
                        || template.category == ItemCategory.Armor
                        || template.category == ItemCategory.Helmet
                        || template.category == ItemCategory.Shield
            ? CraftDiscipline.Smither
            : CraftDiscipline.Tanner;

        quote.RequiredCraftLevel = CraftedStatRoller.RequiredLevelFor(quote.NextQuality);
        quote.PlayerCraftLevel = GetCraftLevel(quote.Discipline);

        int tier = (int)quote.NextQuality;
        quote.MaterialCost = baseMaterialCost * Mathf.Max(1, tier);
        quote.FeeSilver = station != null && !station.PlayerOwned
            ? baseFeeSilver * Mathf.Max(1, tier)
            : 0;

        float stationSpeed = station != null ? station.TimeMultiplier : 1f;
        quote.Minutes = Mathf.RoundToInt(baseMinutes * tier * stationSpeed);

        if (quote.PlayerCraftLevel < quote.RequiredCraftLevel)
        {
            quote.Reason = $"Needs {DisciplineName(quote.Discipline)} {quote.RequiredCraftLevel}. " +
                           $"You are {quote.PlayerCraftLevel}.";
            return quote;
        }

        if (station != null && !station.CanCraft(quote.RequiredCraftLevel))
        {
            quote.Reason = "This workshop is not equipped for work that fine.";
            return quote;
        }

        int over = quote.PlayerCraftLevel - quote.RequiredCraftLevel;
        int stationBonus = station != null ? station.QualityBonus : 0;
        quote.SuccessChance = Mathf.Clamp(baseSuccessChance + over * chancePerLevelOver + stationBonus, 5, 95);

        quote.Possible = true;
        return quote;
    }

    // -----------------------------------------------------------------
    // The attempt
    // -----------------------------------------------------------------

    public sealed class UpgradeResult
    {
        public bool Attempted;
        public bool Succeeded;
        public string Message;

        public ItemQuality NewQuality;
        public List<GameplayEffect> NewEffects;
    }

    /// <summary>
    /// Runs one attempt. Materials and time are spent either way; the item is
    /// only changed on success, and never for the worse.
    /// </summary>
    public UpgradeResult TryUpgrade(Item item, CraftStation station)
    {
        var result = new UpgradeResult();
        var quote = GetQuote(item, station);

        if (!quote.Possible)
        {
            result.Message = quote.Reason;
            return result;
        }

        var pd = PlayerStatHandler.Instance?.pd;
        if (pd == null)
        {
            result.Message = "No player data.";
            return result;
        }

        if (quote.FeeSilver > 0 && !pd.TrySpendMoney(0, quote.FeeSilver))
        {
            result.Message = $"The smith wants {quote.FeeSilver} silver for the use of his fire.";
            return result;
        }

        result.Attempted = true;

        TimeSystem.Instance?.AdvanceTime(quote.Minutes);

        // The dice decide, and skill has already tilted them.
        result.Succeeded = Dice.RollD100() <= quote.SuccessChance;

        if (!result.Succeeded)
        {
            result.Message = "The work did not take. The metal is sound, but the day is gone.";

            // Failure still teaches something.
            ExperienceSystem.UpdateCraftLevel(pd, quote.Discipline, 8);

            if (verbose) Log($"Upgrade failed on {item.Name}.");
            return result;
        }

        // Raise the tier, then reroll — keeping whichever properties were
        // already better.
        item.Quality = (int)quote.NextQuality;
        item.HiddenEffects = CraftedStatRoller.Reroll(item, quote.NextQuality,
                                                      quote.PlayerCraftLevel, keepBest: true);

        result.NewQuality = quote.NextQuality;
        result.NewEffects = item.HiddenEffects;
        result.Message = $"{item.Name} is now {ItemRules.Name(quote.NextQuality)}.";

        ExperienceSystem.UpdateCraftLevel(pd, quote.Discipline, 25);

        Events?.Dispatch(new ItemUpgradedEvent(item.ID, (int)quote.NextQuality));
        PlayerStatHandler.Instance?.RefreshPlayerUI();

        if (verbose) Log(result.Message);
        return result;
    }

    // -----------------------------------------------------------------

    private static int GetCraftLevel(CraftDiscipline discipline)
    {
        var pd = PlayerStatHandler.Instance?.pd;
        return pd == null ? 1 : ExperienceSystem.GetCraftLevel(pd, discipline);
    }

    private static string DisciplineName(CraftDiscipline d)
    {
        switch (d)
        {
            case CraftDiscipline.Smither:   return "Smithing";
            case CraftDiscipline.Tanner:    return "Tanning";
            case CraftDiscipline.Carpenter: return "Carpentry";
            case CraftDiscipline.Mason:     return "Masonry";
            case CraftDiscipline.Alchemist: return "Alchemy";
            default:                        return d.ToString();
        }
    }

    private static ItemSO LookupTemplate(int itemId)
    {
        var db = GameBootstrapper.Resources != null
            ? GameBootstrapper.Resources.GetItemDatabase()
            : null;

        return db != null ? db.GetByID(itemId) : null;
    }
}

/// <summary>Raised when a piece of gear is successfully reworked.</summary>
public sealed class ItemUpgradedEvent : GameEvent
{
    public int ItemId { get; }
    public int NewQuality { get; }

    public ItemUpgradedEvent(int itemId, int newQuality)
    {
        ItemId = itemId;
        NewQuality = newQuality;
    }
}
