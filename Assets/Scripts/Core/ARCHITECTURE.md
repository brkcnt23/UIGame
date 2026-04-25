# Game Architecture Refactor

## Problem Statement
Old architecture: 20+ managers, each loading Resources independently, scattered initialization, no clear data flow.
Result: Resource load errors, tight coupling, unpredictable state mutations, UI not syncing with state.

## Solution: Event-Driven Immutable State

### Architecture Layers

```
┌─────────────────────────────────────────┐
│      GameBootstrapper                   │
│  (Single entry point, orchestrates)     │
└────────────┬────────────────────────────┘
             │
    ┌────────┼────────┐
    │        │        │
    ▼        ▼        ▼
 Res Provider State Mgr Event Bus    (Core infrastructure)
    │        │        │
    └────────┼────────┘
             │
    ┌────────┴──────────────────────────┐
    │  Register All GameSystems         │
    │  (InventorySystem, JobSystem, etc)│
    └────────┬──────────────────────────┘
             │
    ┌────────┴───────┐
    │                │
    ▼                ▼
GameloopManager   Systems Subscribe
(Update loop)     (to events & state)
```

### Data Flow (per frame)

```
Input (keyboard) → GameloopManager.Update()
    ↓
Dispatch event: PlayerMoveEvent (via EventBus)
    ↓
Systems listen & react:
  - InventorySystem.OnAddItem(event)
  - TimeSystem.Tick()
    ↓
StateManager.UpdateState(updater function)
    ↓
State is mutated atomically (new instance)
    ↓
Notify all IStateListener
    ↓
UI components update automatically (Observer pattern)
    ↓
Next frame
```

### Key Design Decisions

#### 1. Immutable State (with cloning)
```csharp
StateManager.UpdateState(state =>
{
    var newState = state.Clone();  // Clone = new instance
    newState.Player.Health = 50;   // Mutate clone
    return newState;               // StateManager replaces old state
});
```

**Why:**
- Prevents accidental mutations from hidden references
- Makes state changes traceable (old → new comparison)
- Enables undo/replay (if needed)
- Thread-safe (with lock)

#### 2. Event Bus (Type-Safe)
```csharp
// Systems publish
EventDispatcher.Dispatch(new ItemAddedEvent(itemId, qty));

// Systems subscribe
EventDispatcher.Subscribe<ItemAddedEvent>(OnItemAdded);
```

**Why:**
- Decouples systems (no direct references)
- Multiple systems can react to same event
- Events are timestamped (Timestamp property)
- Type-safe (no string-based event names)

#### 3. Resource Provider (Centralized Load)
```csharp
// OLD: ShopSystem, InventorySystem, CraftingSystem each load
Resources.Load<ItemDatabase>("ItemDatabase");

// NEW: One place
var itemDb = GameBootstrapper.Resources.GetItemDatabase();
```

**Why:**
- Prevents multiple loads of same resource
- Single point of failure (if resource missing, one place to fix)
- Easy to add caching

#### 4. IGameSystem Interface
```csharp
public interface IGameSystem
{
    void Initialize(EventDispatcher eventDispatcher, StateManager stateManager);
}
```

**Why:**
- All systems initialized in deterministic order
- Access to event & state systems guaranteed
- GameBootstrapper auto-registers anything implementing this

---

## Migration Guide: Old System → New System

### Before (Old)
```csharp
public sealed class ShopSystem : MonoBehaviour
{
    private ItemDatabase _itemDb;

    private void Awake()
    {
        _itemDb = Resources.Load<ItemDatabase>("ItemDatabase"); // May fail silently
    }

    public void BuyItem(int itemId)
    {
        var item = _itemDb.GetItem(itemId);
        // Direct reference to other systems
        inventorySystem.AddItem(itemId); // Tight coupling!
    }
}
```

### After (New)
```csharp
public sealed class ShopSystem : GameSystem
{
    private ItemDatabase _itemDb;

    public override void Initialize(EventDispatcher eventDispatcher, StateManager stateManager)
    {
        base.Initialize(eventDispatcher, stateManager);

        // Load via GameBootstrapper (guaranteed to exist)
        _itemDb = Resources.GetItemDatabase();

        // Subscribe to events (decoupled)
        EventDispatcher.Subscribe<BuyItemEvent>(OnBuyItem);
    }

    private void OnBuyItem(BuyItemEvent evt)
    {
        // Mutation via StateManager
        StateManager.UpdateState(state =>
        {
            var newState = state.Clone();
            newState.Player.Gold -= evt.Price;
            newState.Inventory.Items.Add(new ItemInstance { ... });
            return newState;
        });

        // Notify other systems via event
        EventDispatcher.Dispatch(new ItemPurchasedEvent(evt.ItemId));
    }
}
```

### Step-by-Step Migration

For each old manager (ShopSystem, InventorySystem, etc):

1. **Change inheritance**
   ```csharp
   // OLD
   public sealed class ShopSystem : MonoBehaviour
   
   // NEW
   public sealed class ShopSystem : GameSystem
   ```

2. **Remove Awake()**
   ```csharp
   // OLD
   private void Awake() { _itemDb = Resources.Load(...); }
   
   // NEW (in Initialize)
   public override void Initialize(EventDispatcher ed, StateManager sm)
   {
       base.Initialize(ed, sm);
       _itemDb = Resources.GetItemDatabase(); // Via GameBootstrapper
   }
   ```

3. **Replace direct mutations with StateManager**
   ```csharp
   // OLD
   player.health = 50;
   
   // NEW
   StateManager.UpdateState(state =>
   {
       var newState = state.Clone();
       newState.Player.Health = 50;
       return newState;
   });
   ```

4. **Replace method calls with events**
   ```csharp
   // OLD
   inventorySystem.AddItem(itemId);
   
   // NEW
   EventDispatcher.Dispatch(new AddItemEvent(itemId, qty));
   // InventorySystem listens & handles in OnAddItem()
   ```

5. **Add event subscriptions**
   ```csharp
   public override void Initialize(...)
   {
       EventDispatcher.Subscribe<ItemPurchasedEvent>(OnItemPurchased);
       EventDispatcher.Subscribe<OpenShopEvent>(OnOpenShop);
   }
   ```

---

## Testing Strategy

### Unit Tests (per system)
Test business logic in isolation (no full gameloop).

```csharp
[Test]
public void AddItemIncreasesInventoryCount()
{
    var system = new InventorySystem();
    system.Initialize(eventDispatcher, stateManager);
    
    eventDispatcher.Dispatch(new AddItemEvent(1, 5));
    
    Assert.AreEqual(5, stateManager.CurrentState.Inventory.Items[0].Quantity);
}
```

### Integration Tests (systems together)
Test state flow + event propagation.

```csharp
[Test]
public void BuyItemTriggersInventoryUpdate()
{
    // Simulate: ShopSystem dispatches ItemPurchasedEvent
    // InventorySystem listens and updates state
    // UI listens and updates display
    
    shopSystem.OnBuyItem(...);
    
    Assert.AreEqual(9, stateManager.CurrentState.Player.Gold);
    Assert.Contains(newItem, stateManager.CurrentState.Inventory.Items);
}
```

### Gameloop Tests (frame-by-frame)
Test Update() loop behavior.

```csharp
[Test]
public void TimeAdvancesByDeltaTime()
{
    gameloopManager.Update(deltaTime: 0.016f);
    
    Assert.AreEqual(expectedHour, stateManager.CurrentState.Time.Hour);
}
```

---

## Performance Notes

- **Cloning state each frame**: Acceptable (GameState is simple POD). If state grows, use Copy-on-Write.
- **Event subscribers**: Lock-free dispatch (copy list before iteration).
- **StateManager**: Thread-safe (lock around state mutations). Event dispatch is async-safe.
- **Resource loading**: One-time in Initialize(). No loads during gameplay.

---

## Debugging

Enable `GameBootstrapper._debugLogging` to see initialization order:

```
[BOOTSTRAP] === GAME BOOTSTRAP START ===
[BOOTSTRAP] Phase 1: Loading resources...
[BOOTSTRAP] ResourceProvider initialized
[BOOTSTRAP] Phase 2: Initializing state & event systems...
[BOOTSTRAP] Phase 3: Registering game systems...
[BOOTSTRAP] Registered system: InventorySystem
[BOOTSTRAP] Registered system: ShopSystem
[BOOTSTRAP] === GAME BOOTSTRAP COMPLETE ===
```

Use StateManager event subscriptions to trace state changes:

```csharp
stateManager.Subscribe(new DebugStateListener());

public sealed class DebugStateListener : IStateListener
{
    public void OnStateChanged(GameState oldState, GameState newState)
    {
        Debug.Log($"State changed: Player.Health {oldState.Player.Health} → {newState.Player.Health}");
    }
}
```

---

## Next Steps

1. **Move GameBootstrapper to Canvas/MainScene** (persistent)
2. **Refactor each system** (follow migration guide above)
3. **Remove old Awake() patterns** from all managers
4. **Wire up UI** to StateManager listeners
5. **Run tests** (GameloopIntegrationTest should pass)
6. **Remove OLD scene** after verification
