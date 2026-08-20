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

        [Tooltip("0=Crude 1=Common 2=Fine 3=Masterwork 4=Legendary")]
        [Range(0, 4)] public int MinItemQuality = 0;
        [Range(0, 4)] public int MaxItemQuality = 4;

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

            // quality is now the ItemQuality enum (Crude..Legendary); the
            // entry bounds stay plain ints so existing profile assets keep
            // their values.
            int itemQuality = (int)itemSo.quality;

            if (itemQuality < entry.MinItemQuality) continue;
            if (itemQuality > entry.MaxItemQuality) continue;
            if (itemQuality > shopMaxAffordableItemQuality) continue;

            // Uniques are placed by hand in the world, never stocked.
            if (itemSo.isUnique) continue;

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