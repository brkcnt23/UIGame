/// <summary>
/// Contract for every game system registered by GameBootstrapper.
///
/// Execution order is declared in code via Priority — never in the Inspector.
/// Inspector ordering hides itself in the scene file, breaks on merge, and is
/// invisible in code review.
/// </summary>
public interface IGameSystem
{
    /// <summary>Lower runs first. Use the constants in SystemPriority.</summary>
    int Priority { get; }

    /// <summary>
    /// Called once during bootstrap, after ResourceProvider is ready.
    /// Subscribe to events, resolve dependencies, prepare for gameplay.
    /// </summary>
    void Initialize(EventDispatcher eventDispatcher, StateManager stateManager);

    /// <summary>Called once on teardown. Unsubscribe here.</summary>
    void Shutdown();
}
