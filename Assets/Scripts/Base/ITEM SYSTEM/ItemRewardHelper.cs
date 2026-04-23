using System.Collections.Generic;
using UnityEngine;

public static class ItemRewardHelper
{
    private static ItemDatabase cachedDb;

    private static ItemDatabase GetDatabase()
    {
        if (cachedDb == null)
        {
            cachedDb = Resources.Load<ItemDatabase>("ItemDatabase");
            if (cachedDb == null)
            {
                Debug.LogWarning("ItemRewardHelper: ItemDatabase not found in Resources.");
            }
        }

        return cachedDb;
    }

    public static bool HasItems(List<ItemStackData> stacks)
    {
        if (stacks == null || stacks.Count == 0)
            return true;

        if (InventorySystem.Instance == null)
            return false;

        foreach (var stack in stacks)
        {
            if (stack == null || stack.Quantity <= 0)
                continue;

            if (!InventorySystem.Instance.HasItem(stack.ItemId, stack.Quantity))
            {
                return false;
            }
        }

        return true;
    }

    public static void GiveItems(List<ItemStackData> stacks)
    {
        if (stacks == null || stacks.Count == 0)
            return;

        if (InventorySystem.Instance == null)
        {
            Debug.LogWarning("ItemRewardHelper: InventorySystem.Instance is null. Cannot give items.");
            return;
        }

        var db = GetDatabase();

        foreach (var stack in stacks)
        {
            if (stack == null || stack.Quantity <= 0)
                continue;

            if (db != null)
            {
                var so = db.GetByID(stack.ItemId);
                if (so != null)
                {
                    InventorySystem.Instance.AddItem(so, stack.Quantity);
                    continue;
                }
            }

            // Fallback
            var fallback = new Item(
                stack.ItemId,
                "Unknown Item",
                0,
                0,
                ItemCategory.Misc,
                stack.Quantity,
                true,
                99,
                1f
            );

            InventorySystem.Instance.AddItem(fallback);
        }

        RefreshUI();
    }

    public static void RemoveItems(List<ItemStackData> stacks)
    {
        if (stacks == null || stacks.Count == 0)
            return;

        if (InventorySystem.Instance == null)
        {
            Debug.LogWarning("ItemRewardHelper: InventorySystem.Instance is null. Cannot remove items.");
            return;
        }

        foreach (var stack in stacks)
        {
            if (stack == null || stack.Quantity <= 0)
                continue;

            InventorySystem.Instance.RemoveItemById(stack.ItemId, stack.Quantity);
        }

        RefreshUI();
    }

    private static void RefreshUI()
    {
        if (InventoryUI.Instance != null)
        {
            InventoryUI.Instance.UpdateInventoryUI();
        }

        if (PlayerUISystem.Instance != null)
        {
            PlayerUISystem.Instance.UpdateUIObjects();
        }
    }
}