using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Helper for item rewards/costs. Uses new EventDispatcher pattern (no direct state mutation).
/// </summary>
public static class ItemRewardHelper
{
    private static ItemDatabase GetDatabase()
    {
        return GameBootstrapper.Resources.GetItemDatabase();
    }

    public static bool HasItems(List<ItemStackData> stacks)
    {
        if (stacks == null || stacks.Count == 0)
            return true;

        var stateManager = GameBootstrapper.State;
        if (stateManager == null)
            return false;

        foreach (var stack in stacks)
        {
            if (stack == null || stack.Quantity <= 0)
                continue;

            var count = stateManager.GetValue(state =>
            {
                var item = state.Inventory.Items.Find(i => i.ItemId == stack.ItemId);
                return item?.Quantity ?? 0;
            });

            if (count < stack.Quantity)
                return false;
        }

        return true;
    }

    public static void GiveItems(List<ItemStackData> stacks)
    {
        if (stacks == null || stacks.Count == 0)
            return;

        var eventBus = GameBootstrapper.Events;
        if (eventBus == null)
        {
            Debug.LogError("ItemRewardHelper: EventDispatcher not initialized");
            return;
        }

        var db = GetDatabase();

        foreach (var stack in stacks)
        {
            if (stack == null || stack.Quantity <= 0)
                continue;

            eventBus.Dispatch(new AddItemEvent(stack.ItemId, stack.Quantity));
        }
    }

    public static void RemoveItems(List<ItemStackData> stacks)
    {
        if (stacks == null || stacks.Count == 0)
            return;

        var eventBus = GameBootstrapper.Events;
        if (eventBus == null)
        {
            Debug.LogError("ItemRewardHelper: EventDispatcher not initialized");
            return;
        }

        foreach (var stack in stacks)
        {
            if (stack == null || stack.Quantity <= 0)
                continue;

            eventBus.Dispatch(new RemoveItemEvent(stack.ItemId, stack.Quantity));
        }
    }
}