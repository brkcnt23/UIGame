using UnityEngine;

/// <summary>
/// Turns carried weight into a felt consequence.
///
/// Weight is the inventory limit rather than a slot count, because a slot
/// limit is arbitrary — the player cannot tell why twenty is the number.
/// Weight explains itself: full plate is thirty kilos and a potion is half a
/// one. It also finally gives Strength a use outside combat.
///
/// Four bands, each a trait so the effects flow through the same stacking and
/// capping rules as everything else:
///
///   below 75%   nothing
///   75-100%     Burdened      travel +15%, exhaustion +15%
///   100-125%    Overburdened  travel +40%, exhaustion +40%, defence -1
///   above 125%  Overloaded    travel +100%, and travel is refused
///
/// The last band deliberately does not simply slow the player down. Being
/// unable to leave until something is dropped is the moment the system starts
/// making decisions for the player, which is the point of a survival economy.
/// </summary>
public sealed class EncumbranceSystem : GameSystemBase
{
    public override int Priority => SystemPriority.Inventory + 5;

    public static EncumbranceSystem Instance { get; private set; }

    public enum Band { Normal, Burdened, Overburdened, Overloaded }

    [Header("Thresholds (fraction of carry capacity)")]
    [SerializeField] private float burdenedAt = 0.75f;
    [SerializeField] private float overburdenedAt = 1.00f;
    [SerializeField] private float overloadedAt = 1.25f;

    [Tooltip("Overloaded characters cannot start a journey.")]
    [SerializeField] private bool blockTravelWhenOverloaded = true;

    [SerializeField] private bool verbose;

    private Band _current = Band.Normal;

    /// <summary>Raised when the band changes, so the UI can react once.</summary>
    public System.Action<Band> OnBandChanged;

    public Band CurrentBand => _current;

    protected override void OnInitialize()
    {
        Instance = this;

        Events.Subscribe<ItemAddedEvent>(OnInventoryChanged);
        Events.Subscribe<ItemRemovedEvent>(OnInventoryChanged);

        Evaluate();
    }

    protected override void OnShutdown()
    {
        Events.Unsubscribe<ItemAddedEvent>(OnInventoryChanged);
        Events.Unsubscribe<ItemRemovedEvent>(OnInventoryChanged);

        if (Instance == this)
            Instance = null;
    }

    // Weight can also change without an inventory event — equipping, a quest
    // reward written straight into PlayerData — so re-check on the hour too.
    protected override void OnHourTick(int day, int hour) => Evaluate();

    private void OnInventoryChanged(ItemAddedEvent _) => Evaluate();
    private void OnInventoryChanged(ItemRemovedEvent _) => Evaluate();

    // -----------------------------------------------------------------

    public float GetRatio()
    {
        var pd = PlayerStatHandler.Instance?.pd;
        if (pd == null) return 0f;

        float capacity = pd.GetCarryCapacity();
        if (capacity <= 0f) return 0f;

        return pd.GetCurrentInventoryWeight() / capacity;
    }

    public Band GetBand(float ratio)
    {
        if (ratio >= overloadedAt)    return Band.Overloaded;
        if (ratio >= overburdenedAt)  return Band.Overburdened;
        if (ratio >= burdenedAt)      return Band.Burdened;
        return Band.Normal;
    }

    /// <summary>Recomputes the band and swaps the trait if it changed.</summary>
    public void Evaluate()
    {
        var band = GetBand(GetRatio());
        if (band == _current) return;

        _current = band;
        ApplyTrait(band);
        OnBandChanged?.Invoke(band);

        if (verbose)
            Log($"Encumbrance is now {band} ({GetRatio():P0} of capacity).");
    }

    private void ApplyTrait(Band band)
    {
        var traits = TraitSystem.Instance;
        if (traits == null) return;

        // The three weight traits list each other under removesTraitIds, so
        // granting one clears the others. Normal just clears all three.
        switch (band)
        {
            case Band.Burdened:     traits.Grant("cond_burdened"); break;
            case Band.Overburdened: traits.Grant("cond_overburdened"); break;
            case Band.Overloaded:   traits.Grant("cond_overloaded"); break;

            default:
                traits.Remove("cond_burdened");
                traits.Remove("cond_overburdened");
                traits.Remove("cond_overloaded");
                break;
        }
    }

    /// <summary>
    /// Called before a journey starts. Returns false with a reason when the
    /// player is carrying too much to move.
    /// </summary>
    public bool CanTravel(out string reason)
    {
        if (!blockTravelWhenOverloaded || _current != Band.Overloaded)
        {
            reason = null;
            return true;
        }

        var pd = PlayerStatHandler.Instance?.pd;
        float over = pd != null
            ? pd.GetCurrentInventoryWeight() - pd.GetCarryCapacity()
            : 0f;

        reason = $"You are carrying {Mathf.CeilToInt(over)} too much to travel. " +
                 "Sell something, store it, or leave it behind.";
        return false;
    }

    /// <summary>
    /// Multiplier applied to a journey's duration. Reads the trait total, so
    /// companions and other sources that reduce travel time are included.
    /// </summary>
    public float GetTravelTimeMultiplier()
    {
        var traits = TraitSystem.Instance;
        if (traits == null) return 1f;

        int percent = traits.GetPercentBonus(EffectType.TravelTime);
        return Mathf.Max(0.25f, 1f + percent / 100f);
    }

    /// <summary>Colour for the weight readout: white, amber, orange, red.</summary>
    public static Color BandColor(Band band)
    {
        switch (band)
        {
            case Band.Burdened:     return new Color(0.90f, 0.76f, 0.36f);
            case Band.Overburdened: return new Color(0.90f, 0.56f, 0.28f);
            case Band.Overloaded:   return new Color(0.86f, 0.34f, 0.30f);
            default:                return Color.white;
        }
    }

    public static string BandLabel(Band band)
    {
        switch (band)
        {
            case Band.Burdened:     return "Burdened";
            case Band.Overburdened: return "Overburdened";
            case Band.Overloaded:   return "Overloaded";
            default:                return "";
        }
    }
}
