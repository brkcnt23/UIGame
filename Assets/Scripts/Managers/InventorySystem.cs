using UnityEngine;
using System;

/// <summary>
/// Reference implementation for migrating a system onto GameSystemBase.
///
/// OLD: Awake() { Resources.Load("ItemDatabase") }
/// NEW: OnInitialize() { GameBootstrapper.Resources }
///
/// OLD: mutate the inventory directly
/// NEW: State.UpdateState() — every subscriber is notified once
///
/// Note the symmetry: everything subscribed in OnInitialize is released in
/// OnShutdown. Handlers that are never removed are the usual cause of
/// "the event fires twice after a scene reload".
/// </summary>
public sealed partial class InventorySystem : GameSystemBase
{
    public override int Priority => SystemPriority.Inventory;

    private ItemDatabase _itemDatabase;
    private InventoryUIUpdater _uiUpdater;

    protected override void OnInitialize()
    {
        _itemDatabase = Resources != null ? Resources.GetItemDatabase() : null;
        if (_itemDatabase == null)
        {
            LogError("ItemDatabase not found. Inventory will not function.");
            return;
        }

        Events.Subscribe<AddItemEvent>(OnAddItem);
        Events.Subscribe<RemoveItemEvent>(OnRemoveItem);

        _uiUpdater = new InventoryUIUpdater();
        State.Subscribe(_uiUpdater);
    }

    protected override void OnShutdown()
    {
        Events.Unsubscribe<AddItemEvent>(OnAddItem);
        Events.Unsubscribe<RemoveItemEvent>(OnRemoveItem);

        if (_uiUpdater != null)
        {
            State.Unsubscribe(_uiUpdater);
            _uiUpdater = null;
        }
    }

    private void OnAddItem(AddItemEvent evt)
    {
        State.UpdateState(state =>
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
                    Events.Dispatch(new InventoryFullEvent());
                    return state;
                }

                newState.Inventory.Items.Add(new ItemInstance
                {
                    ItemId = evt.ItemId,
                    Quantity = evt.Quantity
                });
            }

            Events.Dispatch(new ItemAddedEvent(evt.ItemId, evt.Quantity));
            return newState;
        });
    }

    private void OnRemoveItem(RemoveItemEvent evt)
    {
        State.UpdateState(state =>
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

            Events.Dispatch(new ItemRemovedEvent(evt.ItemId, evt.Quantity));
            return newState;
        });
    }

    /// <summary>
    /// Public API for query-only (no mutation)
    /// </summary>
    public ItemInstance GetItem(int itemId)
    {
        return State.GetValue(state =>
            state.Inventory.Items.Find(i => i.ItemId == itemId)
        );
    }

    public int GetItemCount(int itemId)
    {
        return State.GetValue(state =>
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
