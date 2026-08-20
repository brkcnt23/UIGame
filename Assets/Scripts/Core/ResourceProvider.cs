using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Centralized resource loading. Load once, share everywhere.
/// Prevents multiple systems from trying to load same resources independently.
/// </summary>
public sealed class ResourceProvider : MonoBehaviour
{
    private ItemSpriteDatabase _itemSpriteDatabase;
    private ItemDatabase _itemDatabase;
    private TraitDatabaseSO _traitDatabase;
    private TitleDatabaseSO _titleDatabase;

    private readonly Dictionary<string, ScriptableObject> _cachedResources = new();

    /// <summary>
    /// Load all critical game resources. Called once during bootstrap.
    /// Returns false if any critical resource is missing.
    /// </summary>
    public bool Initialize()
    {
        LoadItemDatabase();
        LoadItemSpriteDatabase();
        LoadTraitDatabase();
        LoadTitleDatabase();
        return true; // Continue even if optional resources fail
    }

    private void LoadTraitDatabase()
    {
        _traitDatabase = Resources.Load<TraitDatabaseSO>("TraitDatabase");

        if (_traitDatabase == null)
        {
            Debug.LogWarning("[ResourceProvider] TraitDatabase not found in Resources/TraitDatabase. " +
                             "Run Tools > UIGame > Traits > Generate trait assets.");
            return;
        }

        _traitDatabase.RebuildIndex();
        Debug.Log($"[ResourceProvider] TraitDatabase loaded ({_traitDatabase.traits.Count} traits)");
    }

    private void LoadTitleDatabase()
    {
        _titleDatabase = Resources.Load<TitleDatabaseSO>("TitleDatabase");

        if (_titleDatabase == null)
        {
            Debug.LogWarning("[ResourceProvider] TitleDatabase not found in Resources/TitleDatabase (optional)");
            return;
        }

        Debug.Log($"[ResourceProvider] TitleDatabase loaded ({_titleDatabase.titles.Count} titles)");
    }

    public TraitDatabaseSO GetTraitDatabase() => _traitDatabase;
    public TitleDatabaseSO GetTitleDatabase() => _titleDatabase;

    private void LoadItemDatabase()
    {
        // Try standard path first
        _itemDatabase = Resources.Load<ItemDatabase>("ItemDatabase");

        if (_itemDatabase == null)
        {
            // Try alt paths - scene-based fallback
            _itemDatabase = FindFirstObjectByType<ItemDatabase>();
        }

        if (_itemDatabase == null)
        {
            Debug.LogWarning("[ResourceProvider] ItemDatabase not found. Inventory features may be limited.");
        }
        else
        {
            Debug.Log("[ResourceProvider] ItemDatabase loaded");
        }
    }

    private bool LoadItemSpriteDatabase()
    {
        _itemSpriteDatabase = Resources.Load<ItemSpriteDatabase>("ItemSpriteDatabase");
        if (_itemSpriteDatabase == null)
        {
            Debug.LogWarning("[ResourceProvider] ItemSpriteDatabase not found in Resources/ItemSpriteDatabase (optional)");
            return true; // Not critical, but log warning
        }
        Debug.Log("[ResourceProvider] ItemSpriteDatabase loaded");
        return true;
    }

    /// <summary>
    /// Get ItemDatabase. Guaranteed to be loaded (or returns null if not found).
    /// </summary>
    public ItemDatabase GetItemDatabase()
    {
        if (_itemDatabase == null)
            Debug.LogError("[ResourceProvider] ItemDatabase not initialized. Call Initialize() first.");
        return _itemDatabase;
    }

    /// <summary>
    /// Get ItemSpriteDatabase. May return null if not found.
    /// </summary>
    public ItemSpriteDatabase GetItemSpriteDatabase()
    {
        return _itemSpriteDatabase;
    }

    /// <summary>
    /// Generic resource cache. Load once, reuse.
    /// </summary>
    public T GetResource<T>(string resourcePath) where T : ScriptableObject
    {
        if (_cachedResources.TryGetValue(resourcePath, out var cached))
            return cached as T;

        var resource = Resources.Load<T>(resourcePath);
        if (resource != null)
            _cachedResources[resourcePath] = resource;

        return resource;
    }
}
