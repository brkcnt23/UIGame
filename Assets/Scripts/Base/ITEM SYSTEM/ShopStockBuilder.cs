using System.Collections.Generic;
using UnityEngine;

public static class ShopStockBuilder
{
    public static void RebuildShopStock(
        Shops shop,
        ItemDatabase itemDatabase,
        ShopStockProfileDatabaseSO profileDatabase)
    {
        if (shop == null || itemDatabase == null || profileDatabase == null)
        {
            Debug.LogWarning("ShopStockBuilder: Missing dependency.");
            return;
        }

        if (string.IsNullOrWhiteSpace(shop.StockProfileId))
        {
            Debug.LogWarning($"ShopStockBuilder: Shop '{shop.Name}' has no StockProfileId.");
            return;
        }

        var profile = profileDatabase.GetById(shop.StockProfileId);
        if (profile == null)
        {
            Debug.LogWarning($"ShopStockBuilder: Stock profile not found -> {shop.StockProfileId}");
            return;
        }

        shop.ItemEntries = profile.BuildStock(itemDatabase, Mathf.Max(1, shop.MaxAffordableItemQuality));
        BuildRuntimeItemsFromEntries(shop, itemDatabase);
    }

    public static void BuildRuntimeItemsFromEntries(Shops shop, ItemDatabase itemDatabase)
    {
        if (shop == null || itemDatabase == null)
            return;

        shop.ClearRuntimeItems();

        if (shop.ItemEntries == null)
            return;

        foreach (var entry in shop.ItemEntries)
        {
            if (entry == null || entry.Quantity <= 0)
                continue;

            var item = itemDatabase.GetItemInstanceByID(entry.ItemId, entry.Quantity);
            if (item == null)
                continue;

            if (entry.GoldOverride >= 0 || entry.SilverOverride >= 0)
            {
                int gold = entry.GoldOverride >= 0 ? entry.GoldOverride : item.Value.Gold;
                int silver = entry.SilverOverride >= 0 ? entry.SilverOverride : item.Value.Silver;
                item.Value = new Currency(gold, silver);
            }

            shop.AddItem(item);
        }
    }
}