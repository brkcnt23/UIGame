using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Event bus for inter-system communication.
/// Type-safe, decoupled message passing.
/// Systems publish events, other systems subscribe.
/// </summary>
public sealed class EventDispatcher : MonoBehaviour
{
    private readonly Dictionary<Type, List<Delegate>> _subscribers = new();

    /// <summary>
    /// Subscribe to an event type. Callback receives the event.
    /// </summary>
    public void Subscribe<T>(Action<T> handler) where T : GameEvent
    {
        var type = typeof(T);

        lock (_subscribers)
        {
            if (!_subscribers.ContainsKey(type))
                _subscribers[type] = new();

            _subscribers[type].Add(handler);
        }
    }

    /// <summary>
    /// Unsubscribe from an event type.
    /// </summary>
    public void Unsubscribe<T>(Action<T> handler) where T : GameEvent
    {
        var type = typeof(T);

        lock (_subscribers)
        {
            if (_subscribers.ContainsKey(type))
                _subscribers[type].Remove(handler);
        }
    }

    /// <summary>
    /// Dispatch an event to all subscribers of that type.
    /// Exceptions in handlers are caught and logged.
    /// </summary>
    public void Dispatch<T>(T evt) where T : GameEvent
    {
        var type = typeof(T);

        List<Delegate> handlers;
        lock (_subscribers)
        {
            if (!_subscribers.TryGetValue(type, out var list))
                return;

            handlers = new List<Delegate>(list);
        }

        foreach (var handler in handlers)
        {
            try
            {
                ((Action<T>)handler)(evt);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error in event handler for {type.Name}: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}

/// <summary>
/// Base class for all game events. Inherit to create specific event types.
/// </summary>
public abstract class GameEvent
{
    public float Timestamp { get; } = Time.time;
}
