using UnityEngine;

/// <summary>
/// Interface for all game systems. Ensures consistent initialization order and access patterns.
/// All managers/systems must implement this to be registered by GameBootstrapper.
/// </summary>
public interface IGameSystem
{
    /// <summary>
    /// Called once during bootstrap, after ResourceProvider is initialized.
    /// Subscribe to events, load dependencies, prepare for gameplay.
    /// </summary>
    void Initialize(EventDispatcher eventDispatcher, StateManager stateManager);
}

/// <summary>
/// Base class for game systems. Provides common initialization pattern.
/// </summary>
public abstract class GameSystem : MonoBehaviour, IGameSystem
{
    protected EventDispatcher EventDispatcher { get; private set; }
    protected StateManager StateManager { get; private set; }
    protected ResourceProvider Resources => GameBootstrapper.Resources;

    public virtual void Initialize(EventDispatcher eventDispatcher, StateManager stateManager)
    {
        EventDispatcher = eventDispatcher;
        StateManager = stateManager;

        Debug.Log($"[{GetType().Name}] Initialized");
    }
}
