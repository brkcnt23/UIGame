using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Owns the player's traits: grants them, expires them, and answers the one
/// question every other system asks — "how much does this change my number?"
///
/// Conditions expire on hour ticks. Nothing polls Update().
///
/// Percent effects from all held traits are summed per type and then capped
/// (TraitRules.PercentCap), so a player who stacks four crafting traits gets
/// a strong bonus rather than free items.
/// </summary>
public sealed class TraitSystem : GameSystemBase
{
    public override int Priority => SystemPriority.Trait;

    public static TraitSystem Instance { get; private set; }

    [SerializeField] private TraitDatabaseSO database;
    [SerializeField] private bool verbose;

    private readonly List<ActiveTrait> _active = new();

    /// <summary>Raised whenever the trait list changes, so UI can redraw.</summary>
    public System.Action OnTraitsChanged;

    private int _absoluteHour;

    public IReadOnlyList<ActiveTrait> Active => _active;
    public TraitDatabaseSO Database => database;

    protected override void OnInitialize()
    {
        Instance = this;

        if (database == null)
        {
            database = Resources != null ? Resources.GetTraitDatabase() : null;

            if (database == null)
                LogWarning("No TraitDatabase assigned. Traits will not resolve.");
        }

        database?.RebuildIndex();
        LoadFromPlayerData();
    }

    protected override void OnShutdown()
    {
        SaveToPlayerData();
        if (Instance == this)
            Instance = null;
    }

    protected override void OnHourTick(int day, int hour)
    {
        _absoluteHour = day * 24 + hour;
        ExpireConditions();
    }

    // -----------------------------------------------------------------
    // Grant / remove
    // -----------------------------------------------------------------

    public bool Has(string traitId)
    {
        return _active.Exists(a => a.traitId == traitId);
    }

    public int StacksOf(string traitId)
    {
        var a = _active.Find(x => x.traitId == traitId);
        return a?.stacks ?? 0;
    }

    /// <summary>
    /// Adds a trait. Returns false when it is blocked, already held and not
    /// stackable, or unknown to the database.
    /// </summary>
    public bool Grant(string traitId)
    {
        var def = database?.Get(traitId);
        if (def == null)
        {
            LogWarning($"Unknown trait '{traitId}'.");
            return false;
        }

        foreach (var blocker in def.blockedByTraitIds)
        {
            if (Has(blocker))
            {
                if (verbose) Log($"'{traitId}' blocked by '{blocker}'.");
                return false;
            }
        }

        // Opposites clear out first: gaining Nourished removes Starving.
        foreach (var removed in def.removesTraitIds)
            Remove(removed, silent: true);

        var existing = _active.Find(a => a.traitId == traitId);

        if (existing != null)
        {
            if (def.stackable && existing.stacks < def.maxStacks)
                existing.stacks++;
            else if (!def.refreshOnReapply)
                return false;

            if (def.Expires)
                existing.expiresAtHour = _absoluteHour + def.durationHours;
        }
        else
        {
            _active.Add(new ActiveTrait
            {
                traitId = traitId,
                stacks = 1,
                expiresAtHour = def.Expires ? _absoluteHour + def.durationHours : -1,
                gainedOnDay = _absoluteHour / 24
            });
        }

        if (verbose) Log($"Gained '{def.displayName}'.");

        SaveToPlayerData();
        OnTraitsChanged?.Invoke();
        return true;
    }

    public bool Remove(string traitId, bool silent = false)
    {
        int index = _active.FindIndex(a => a.traitId == traitId);
        if (index < 0) return false;

        _active.RemoveAt(index);

        if (verbose && !silent) Log($"Lost '{traitId}'.");

        if (!silent)
        {
            SaveToPlayerData();
            OnTraitsChanged?.Invoke();
        }

        return true;
    }

    private void ExpireConditions()
    {
        bool changed = false;

        for (int i = _active.Count - 1; i >= 0; i--)
        {
            if (!_active[i].IsExpired(_absoluteHour))
                continue;

            if (verbose) Log($"'{_active[i].traitId}' expired.");
            _active.RemoveAt(i);
            changed = true;
        }

        if (!changed) return;

        SaveToPlayerData();
        OnTraitsChanged?.Invoke();
    }

    // -----------------------------------------------------------------
    // Queries — the interface every other system uses
    // -----------------------------------------------------------------

    /// <summary>Sum of flat bonuses of this type across all held traits.</summary>
    public int GetFlatBonus(EffectType type, string qualifier = null)
    {
        int total = 0;

        foreach (var a in _active)
        {
            var def = database?.Get(a.traitId);
            if (def == null) continue;

            total += def.GetFlat(type, qualifier) * Mathf.Max(1, a.stacks);
        }

        return total;
    }

    /// <summary>Capped sum of percent bonuses of this type.</summary>
    public int GetPercentBonus(EffectType type, string qualifier = null)
    {
        int total = 0;

        foreach (var a in _active)
        {
            var def = database?.Get(a.traitId);
            if (def == null) continue;

            total += def.GetPercent(type, qualifier) * Mathf.Max(1, a.stacks);
        }

        return TraitRules.Cap(total);
    }

    /// <summary>Applies both flat and percent to a base value.</summary>
    public int Apply(EffectType type, int baseValue, string qualifier = null)
    {
        int flat = GetFlatBonus(type, qualifier);
        int percent = GetPercentBonus(type, qualifier);

        return Mathf.RoundToInt((baseValue + flat) * (1f + percent / 100f));
    }

    /// <summary>
    /// Static shim for the crafting code, which reaches for the system without
    /// holding a reference and has to work when no trait system is present.
    /// </summary>
    public static int ApplyOrPass(EffectType type, int baseValue, string qualifier = null)
        => Instance != null ? Instance.Apply(type, baseValue, qualifier) : baseValue;

    /// <summary>All tags contributed by held traits, for recipes and events.</summary>
    public List<string> GetAllTags()
    {
        var tags = new List<string>();

        foreach (var a in _active)
        {
            var def = database?.Get(a.traitId);
            if (def?.grantsTags == null) continue;

            tags.AddRange(def.grantsTags);
        }

        return tags;
    }

    public bool IsCompanionUnlocked(string companionId)
    {
        bool blocked = false;
        bool unlocked = false;

        foreach (var a in _active)
        {
            var def = database?.Get(a.traitId);
            if (def == null) continue;

            if (def.blocksCompanionIds.Contains(companionId)) blocked = true;
            if (def.unlocksCompanionIds.Contains(companionId)) unlocked = true;
        }

        // A block always wins: someone who refuses you does not change their
        // mind because you also impressed them elsewhere.
        return unlocked && !blocked;
    }

    public List<TraitSO> GetHeldOfKind(TraitKind kind)
    {
        var result = new List<TraitSO>();

        foreach (var a in _active)
        {
            var def = database?.Get(a.traitId);
            if (def != null && def.kind == kind)
                result.Add(def);
        }

        return result;
    }

    // -----------------------------------------------------------------
    // Persistence — rides along in PlayerData
    // -----------------------------------------------------------------

    private void LoadFromPlayerData()
    {
        var pd = PlayerStatHandler.Instance?.pd;
        if (pd == null) return;

        _active.Clear();

        if (pd.ActiveTraits != null && pd.ActiveTraits.Count > 0)
        {
            _active.AddRange(pd.ActiveTraits);
        }
        else if (pd.ActiveTraitTags != null)
        {
            // Migration from the old plain-string list.
            foreach (var tag in pd.ActiveTraitTags)
                _active.Add(new ActiveTrait { traitId = tag, expiresAtHour = -1 });
        }

        _absoluteHour = pd.Day * 24 + pd.Hour;

        if (verbose) Log($"Loaded {_active.Count} traits.");
    }

    private void SaveToPlayerData()
    {
        var pd = PlayerStatHandler.Instance?.pd;
        if (pd == null) return;

        pd.ActiveTraits = new List<ActiveTrait>(_active);

        // Keep the legacy tag list in sync — recipes still read it.
        pd.ActiveTraitTags = new List<string>();
        foreach (var a in _active)
        {
            pd.ActiveTraitTags.Add(a.traitId);

            var def = database?.Get(a.traitId);
            if (def?.grantsTags != null)
                pd.ActiveTraitTags.AddRange(def.grantsTags);
        }
    }
}
