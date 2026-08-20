using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Every trait in the game, in one asset. Lookup is by id.
/// </summary>
[CreateAssetMenu(fileName = "TraitDatabase", menuName = "UIGame/Trait Database")]
public class TraitDatabaseSO : ScriptableObject
{
    public List<TraitSO> traits = new();

    private Dictionary<string, TraitSO> _byId;

    public void RebuildIndex()
    {
        _byId = new Dictionary<string, TraitSO>();

        foreach (var t in traits)
        {
            if (t == null || string.IsNullOrEmpty(t.traitId))
                continue;

            if (_byId.ContainsKey(t.traitId))
            {
                Debug.LogWarning($"[TraitDatabase] Duplicate trait id '{t.traitId}'. Keeping the first.");
                continue;
            }

            _byId[t.traitId] = t;
        }
    }

    public TraitSO Get(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        if (_byId == null) RebuildIndex();

        return _byId.TryGetValue(id, out var t) ? t : null;
    }

    public List<TraitSO> GetByKind(TraitKind kind)
    {
        var result = new List<TraitSO>();
        foreach (var t in traits)
            if (t != null && t.kind == kind)
                result.Add(t);
        return result;
    }

    public bool Contains(string id) => Get(id) != null;
}
