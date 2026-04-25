using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Immutable state container. Single source of truth.
/// All state mutations happen here via UpdateState(updater).
/// UI systems subscribe to OnStateChanged.
/// </summary>
public sealed class StateManager : MonoBehaviour
{
    private GameState _currentState;
    private readonly List<IStateListener> _listeners = new();
    private readonly object _lockObj = new();

    public GameState CurrentState
    {
        get
        {
            lock (_lockObj)
                return _currentState;
        }
    }

    /// <summary>
    /// Initialize with default state. Call once during bootstrap.
    /// </summary>
    public void InitializeState()
    {
        lock (_lockObj)
        {
            _currentState = new GameState
            {
                Player = new PlayerState
                {
                    Id = 1,
                    Name = "Player",
                    Level = 1,
                    Health = 100,
                    MaxHealth = 100,
                    Exhaustion = 0,
                    MaxExhaustion = 100,
                    Gold = 100,
                    Silver = 50,
                    Ration = 5,
                    Strength = 10,
                    Dexterity = 10,
                    Constitution = 10,
                    Charisma = 10,
                },
                Time = new TimeState
                {
                    Day = 1,
                    Hour = 6,
                    Minute = 0,
                },
                Inventory = new InventoryState
                {
                    Items = new List<ItemInstance>(),
                    Capacity = 20,
                },
                UI = new UIState(),
                Settlements = new Dictionary<int, SettlementState>(),
            };
        }
    }

    public void Subscribe(IStateListener listener)
    {
        lock (_lockObj)
        {
            if (!_listeners.Contains(listener))
                _listeners.Add(listener);
        }
    }

    public void Unsubscribe(IStateListener listener)
    {
        lock (_lockObj)
            _listeners.Remove(listener);
    }

    /// <summary>
    /// Update state atomically. Updater receives current state, must return new state instance.
    /// If returned state == current state, no notification sent.
    /// </summary>
    public void UpdateState(Func<GameState, GameState> updater)
    {
        lock (_lockObj)
        {
            var newState = updater(_currentState);

            if (newState == _currentState)
                return; // No change

            var oldState = _currentState;
            _currentState = newState;

            // Notify all listeners (UI, systems, etc)
            NotifyListeners(oldState, newState);
        }
    }

    private void NotifyListeners(GameState oldState, GameState newState)
    {
        // Copy list to avoid modification during iteration
        var listenersCopy = new List<IStateListener>(_listeners);

        foreach (var listener in listenersCopy)
        {
            try
            {
                listener.OnStateChanged(oldState, newState);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error in state listener {listener.GetType().Name}: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Direct read without locking. Use only if you're certain no mutations happen in parallel.
    /// For safe reads, use CurrentState property.
    /// </summary>
    public T GetValue<T>(Func<GameState, T> selector)
    {
        lock (_lockObj)
            return selector(_currentState);
    }
}

/// <summary>
/// Listener for state changes. Implement to react to state mutations.
/// </summary>
public interface IStateListener
{
    void OnStateChanged(GameState oldState, GameState newState);
}
