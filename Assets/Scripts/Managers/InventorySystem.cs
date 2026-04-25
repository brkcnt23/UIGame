using UnityEngine;
using System;

/// <summary>
/// Refactored InventorySystem - example of how to convert old systems.
///
/// OLD: Awake() { Resources.Load("ItemDatabase") }
/// NEW: Initialize() { Use GameBootstrapper.Resources }
///
/// OLD: Direct setter on inventory
/// NEW: UpdateState() triggers all subscribers
/// </summary>
public sealed partial class InventorySystem : GameSystem
{
    private ItemDatabase _itemDatabase;

    public override void Initialize(EventDispatcher eventDispatcher, StateManager stateManager)
    {
        base.Initialize(eventDispatcher, stateManager);

        // Load resources from provider (one-time, shared)
        _itemDatabase = Resources.GetItemDatabase();
        if (_itemDatabase == null)
        {
            Debug.LogError("[InventorySystem] ItemDatabase not found!");
            return;
        }

        // Subscribe to events that affect inventory
        EventDispatcher.Subscribe<AddItemEvent>(OnAddItem);
        EventDispatcher.Subscribe<RemoveItemEvent>(OnRemoveItem);

        // Subscribe to state changes (for UI refresh)
        StateManager.Subscribe(new InventoryUIUpdater());
    }

    private void OnAddItem(AddItemEvent evt)
    {
        StateManager.UpdateState(state =>
        {
            var newState = state.Clone();

            var itemDef = _itemDatabase.GetByID(evt.ItemId);
            if (itemDef == null)
            {
                Debug.LogWarning($"[InventorySystem] Item not found: {evt.ItemId}");
                return state; // No change
            }

            // Find existing stack or add new
            var existingItem = newState.Inventory.Items.Find(i => i.ItemId == evt.ItemId);
            if (existingItem != null)
            {
                existingItem.Quantity += evt.Quantity;
            }
            else
            {
                if (newState.Inventory.FreeSlots <= 0)
                {
                    Debug.LogWarning("[InventorySystem] Inventory full!");
                    EventDispatcher.Dispatch(new InventoryFullEvent());
                    return state;
                }

                newState.Inventory.Items.Add(new ItemInstance
                {
                    ItemId = evt.ItemId,
                    Quantity = evt.Quantity
                });
            }

            EventDispatcher.Dispatch(new ItemAddedEvent(evt.ItemId, evt.Quantity));
            return newState;
        });
    }

    private void OnRemoveItem(RemoveItemEvent evt)
    {
        StateManager.UpdateState(state =>
        {
            var newState = state.Clone();

            var item = newState.Inventory.Items.Find(i => i.ItemId == evt.ItemId);
            if (item == null)
            {
                Debug.LogWarning($"[InventorySystem] Item not in inventory: {evt.ItemId}");
                return state;
            }

            item.Quantity -= evt.Quantity;
            if (item.Quantity <= 0)
                newState.Inventory.Items.Remove(item);

            EventDispatcher.Dispatch(new ItemRemovedEvent(evt.ItemId, evt.Quantity));
            return newState;
        });
    }

    /// <summary>
    /// Public API for query-only (no mutation)
    /// </summary>
    public ItemInstance GetItem(int itemId)
    {
        return StateManager.GetValue(state =>
            state.Inventory.Items.Find(i => i.ItemId == itemId)
        );
    }

    public int GetItemCount(int itemId)
    {
        return StateManager.GetValue(state =>
        {
            var item = state.Inventory.Items.Find(i => i.ItemId == itemId);
            return item?.Quantity ?? 0;
        });
    }
}

/// <summary>
/// UI component that listens to state changes and updates display
/// </summary>
public sealed class InventoryUIUpdater : IStateListener
{
    public void OnStateChanged(GameState oldState, GameState newState)
    {
        // Only react if inventory actually changed (not just reference difference)
        if (oldState?.Inventory?.Items.Count == newState?.Inventory?.Items.Count)
            return;

        Debug.Log($"[InventoryUI] Inventory changed: {newState.Inventory.Items.Count} items, {newState.Inventory.FreeSlots} slots free");
        // TODO: Actually update InventoryUI panel here
    }
}

/// <summary>
/// Events
/// </summary>

public sealed class AddItemEvent : GameEvent
{
    public int ItemId { get; }
    public int Quantity { get; }

    public AddItemEvent(int itemId, int quantity)
    {
        ItemId = itemId;
        Quantity = quantity;
    }
}

public sealed class RemoveItemEvent : GameEvent
{
    public int ItemId { get; }
    public int Quantity { get; }

    public RemoveItemEvent(int itemId, int quantity)
    {
        ItemId = itemId;
        Quantity = quantity;
    }
}

public sealed class ItemAddedEvent : GameEvent
{
    public int ItemId { get; }
    public int Quantity { get; }

    public ItemAddedEvent(int itemId, int quantity)
    {
        ItemId = itemId;
        Quantity = quantity;
    }
}

public sealed class ItemRemovedEvent : GameEvent
{
    public int ItemId { get; }
    public int Quantity { get; }

    public ItemRemovedEvent(int itemId, int quantity)
    {
        ItemId = itemId;
        Quantity = quantity;
    }
}

public sealed class InventoryFullEvent : GameEvent { }
