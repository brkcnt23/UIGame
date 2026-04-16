using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShopStockProfile", menuName = "Shop/StockProfile")]
public class ShopStockProfileSO : ScriptableObject
{
    [System.Serializable]
    public class StockEntry
    {
        public int ItemId;
        public int MinQuantity = 1;
        public int MaxQuantity = 3;
        [Min(1)] public int SpawnWeight = 1;

        public int GoldOverride = -1;
        public int SilverOverride = -1;

        [Range(1, 10)] public int MinItemQuality = 1;
        [Range(1, 10)] public int MaxItemQuality = 10;

        public List<string> RequiredSettlementTags = new();
        public List<string> RequiredOwnerTags = new();
        public List<string> BlockedSettlementTags = new();
    }

    [Header("Identity")]
    public string ProfileId;
    public string displayName;

    [Header("Shop Compatibility")]
    public ShopTypes shopType = ShopTypes.defaultShop;
    public List<string> requiredSettlementTags = new();
    public List<string> blockedSettlementTags = new();

    [Header("Stock")]
    public List<StockEntry> entries = new();

    public List<ShopItemEntry> BuildStock(ItemDatabase db, int shopMaxAffordableItemQuality)
    {
        List<ShopItemEntry> result = new();

        if (db == null)
        {
            Debug.LogWarning($"ShopStockProfileSO[{name}]: ItemDatabase is null.");
            return result;
        }

        foreach (var entry in entries)
        {
            if (entry == null) continue;
            if (!db.ContainsID(entry.ItemId)) continue;

            var itemSo = db.GetByID(entry.ItemId);
            if (itemSo == null) continue;

            if (itemSo.quality < entry.MinItemQuality) continue;
            if (itemSo.quality > entry.MaxItemQuality) continue;
            if (itemSo.quality > shopMaxAffordableItemQuality) continue;

            int qty = Random.Range(entry.MinQuantity, entry.MaxQuantity + 1);
            if (qty <= 0) continue;

            result.Add(new ShopItemEntry
            {
                ItemId = entry.ItemId,
                Quantity = qty,
                GoldOverride = entry.GoldOverride,
                SilverOverride = entry.SilverOverride
            });
        }

        return result;
    }
}