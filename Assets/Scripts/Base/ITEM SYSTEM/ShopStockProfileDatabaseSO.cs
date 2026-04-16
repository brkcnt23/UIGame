using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShopStockProfileDatabase", menuName = "Shop/StockProfileDatabase")]
public class ShopStockProfileDatabaseSO : ScriptableObject
{
    public List<ShopStockProfileSO> profiles = new();

    private Dictionary<string, ShopStockProfileSO> byId;

    private void OnEnable()
    {
        RebuildIndex();
    }

    public void RebuildIndex()
    {
        byId = new Dictionary<string, ShopStockProfileSO>();

        foreach (var profile in profiles)
        {
            if (profile == null) continue;
            if (string.IsNullOrWhiteSpace(profile.ProfileId)) continue;

            if (!byId.ContainsKey(profile.ProfileId))
                byId[profile.ProfileId] = profile;
            else
                Debug.LogWarning($"Duplicate ShopStockProfileId: {profile.ProfileId}");
        }
    }

    public ShopStockProfileSO GetById(string profileId)
    {
        if (byId == null)
            RebuildIndex();

        if (string.IsNullOrWhiteSpace(profileId))
            return null;

        byId.TryGetValue(profileId, out var profile);
        return profile;
    }

    public bool Contains(string profileId)
    {
        if (byId == null)
            RebuildIndex();

        return !string.IsNullOrWhiteSpace(profileId) && byId.ContainsKey(profileId);
    }

    public List<ShopStockProfileSO> GetByShopType(ShopTypes shopType)
    {
        List<ShopStockProfileSO> result = new();

        foreach (var profile in profiles)
        {
            if (profile == null) continue;
            if (profile.shopType == shopType)
                result.Add(profile);
        }

        return result;
    }
}