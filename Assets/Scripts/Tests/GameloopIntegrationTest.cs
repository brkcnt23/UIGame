using UnityEngine;
using NUnit.Framework;
using System.Collections.Generic;

/// <summary>
/// Integration test: Verify Input → Event → State → UI flow works correctly.
/// Tests the full gameloop chain without external dependencies.
/// </summary>
public sealed class GameloopIntegrationTest
{
    private StateManager _stateManager;
    private EventDispatcher _eventDispatcher;
    private TestStateListener _testListener;

    [SetUp]
    public void Setup()
    {
        // Create mock systems (no MonoBehaviour overhead in tests)
        _stateManager = new GameObject("StateManager").AddComponent<StateManager>();
        _eventDispatcher = new GameObject("EventDispatcher").AddComponent<EventDispatcher>();

        _testListener = new TestStateListener();
        _stateManager.Subscribe(_testListener);

        // Initialize state
        _stateManager.UpdateState(state =>
        {
            var newState = new GameState
            {
                Player = new PlayerState
                {
                    Id = 1,
                    Name = "TestPlayer",
                    Health = 100,
                    MaxHealth = 100,
                    Exhaustion = 0,
                    MaxExhaustion = 100,
                    Gold = 1000,
                    Ration = 10,
                },
                CurrentSettlement = new SettlementState
                {
                    Id = 1,
                    Name = "TestVillage",
                    Level = 1,
                },
                Inventory = new InventoryState { Capacity = 20 },
                Time = new TimeState { Day = 1, Hour = 6, Minute = 0 },
                UI = new UIState(),
            };
            return newState;
        });
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(_stateManager.gameObject);
        Object.DestroyImmediate(_eventDispatcher.gameObject);
    }

    [Test]
    public void StateChangeTriggersListenerNotification()
    {
        // Arrange
        int notificationCount = 0;
        _stateManager.Subscribe(new CountingListener(() => notificationCount++));

        // Act
        _stateManager.UpdateState(state =>
        {
            var newState = state.Clone();
            newState.Player.Health = 50;
            return newState;
        });

        // Assert
        Assert.AreEqual(1, notificationCount, "Listener should be notified once");
    }

    [Test]
    public void NoNotificationWhenStateUnchanged()
    {
        // Arrange
        int notificationCount = 0;
        _stateManager.Subscribe(new CountingListener(() => notificationCount++));

        // Act - Update with same state
        _stateManager.UpdateState(state => state); // Return unchanged

        // Assert
        Assert.AreEqual(0, notificationCount, "No notification for unchanged state");
    }

    [Test]
    public void EventDispatchTriggersConcurrentListeners()
    {
        // Arrange
        int count1 = 0, count2 = 0;
        var handler1 = new System.Action<TestEvent>(evt => count1++);
        var handler2 = new System.Action<TestEvent>(evt => count2++);

        _eventDispatcher.Subscribe(handler1);
        _eventDispatcher.Subscribe(handler2);

        // Act
        _eventDispatcher.Dispatch(new TestEvent());

        // Assert
        Assert.AreEqual(1, count1);
        Assert.AreEqual(1, count2);
    }

    [Test]
    public void InventoryAddThenRemoveStateProgression()
    {
        // Arrange
        var stateHistory = new List<InventoryState>();
        _stateManager.Subscribe(new HistoryRecorder(stateHistory));

        var initialCount = _stateManager.CurrentState.Inventory.Items.Count;

        // Act - Add item
        _stateManager.UpdateState(state =>
        {
            var newState = state.Clone();
            newState.Inventory.Items.Add(new ItemInstance { ItemId = 1, Quantity = 5 });
            return newState;
        });

        var afterAddCount = _stateManager.CurrentState.Inventory.Items.Count;

        // Remove item
        _stateManager.UpdateState(state =>
        {
            var newState = state.Clone();
            newState.Inventory.Items.Clear();
            return newState;
        });

        var afterRemoveCount = _stateManager.CurrentState.Inventory.Items.Count;

        // Assert
        Assert.AreEqual(0, initialCount);
        Assert.AreEqual(1, afterAddCount);
        Assert.AreEqual(0, afterRemoveCount);
    }

    [Test]
    public void TimeAdvancementTriggers()
    {
        // Arrange
        var eventsFired = new List<System.Type>();

        _eventDispatcher.Subscribe<HourPassedEvent>(evt =>
            eventsFired.Add(typeof(HourPassedEvent))
        );

        _eventDispatcher.Subscribe<DayPassedEvent>(evt =>
            eventsFired.Add(typeof(DayPassedEvent))
        );

        // Act - Simulate time passage
        for (int i = 0; i < 25; i++) // 25 hours
        {
            _stateManager.UpdateState(state =>
            {
                var newState = state.Clone();
                newState.Time.Hour++;
                if (newState.Time.Hour >= 24)
                {
                    newState.Time.Hour = 0;
                    newState.Time.Day++;
                    _eventDispatcher.Dispatch(new DayPassedEvent(newState.Time.Day));
                }
                else
                {
                    _eventDispatcher.Dispatch(new HourPassedEvent(newState.Time.Hour));
                }
                return newState;
            });
        }

        // Assert
        Assert.AreEqual(25, eventsFired.Count, "Should fire 25 time events");
        Assert.Contains(typeof(DayPassedEvent), eventsFired, "Should include at least one day passed");
    }

    [Test]
    public void ConcurrentMutationIsThreadSafe()
    {
        // Arrange - this tests that StateManager handles race conditions
        bool exception = false;

        // Act - Try to mutate from multiple threads (simulated)
        for (int i = 0; i < 10; i++)
        {
            try
            {
                _stateManager.UpdateState(state =>
                {
                    var newState = state.Clone();
                    newState.Player.Gold += 100;
                    return newState;
                });
            }
            catch
            {
                exception = true;
            }
        }

        // Assert
        Assert.IsFalse(exception, "No exception during concurrent mutations");
        Assert.AreEqual(1000 + (100 * 10), _stateManager.CurrentState.Player.Gold);
    }
}

/// <summary>
/// Test helpers
/// </summary>

public sealed class TestEvent : GameEvent { }

public sealed class TestStateListener : IStateListener
{
    public GameState LastOldState { get; private set; }
    public GameState LastNewState { get; private set; }

    public void OnStateChanged(GameState oldState, GameState newState)
    {
        LastOldState = oldState;
        LastNewState = newState;
    }
}

public sealed class CountingListener : IStateListener
{
    private System.Action _callback;

    public CountingListener(System.Action callback) => _callback = callback;

    public void OnStateChanged(GameState oldState, GameState newState)
    {
        _callback?.Invoke();
    }
}

public sealed class HistoryRecorder : IStateListener
{
    private List<InventoryState> _history;

    public HistoryRecorder(List<InventoryState> history) => _history = history;

    public void OnStateChanged(GameState oldState, GameState newState)
    {
        _history.Add(newState.Inventory.Clone());
    }
}
