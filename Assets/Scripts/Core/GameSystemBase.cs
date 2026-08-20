using UnityEngine;

/// <summary>
/// Base class for every game system.
///
/// Handles the boilerplate so subclasses only write logic:
///   - stores EventDispatcher / StateManager
///   - auto-subscribes to hour and day ticks
///   - unsubscribes on Shutdown and OnDestroy (no leaked handlers)
///
/// A subclass must declare Priority and implement OnInitialize().
/// </summary>
public abstract class GameSystemBase : MonoBehaviour, IGameSystem
{
    public abstract int Priority { get; }

    protected EventDispatcher Events { get; private set; }
    protected StateManager State { get; private set; }
    protected ResourceProvider Resources => GameBootstrapper.Resources;

    protected bool IsInitialized { get; private set; }

    private bool _tickSubscribed;

    public void Initialize(EventDispatcher eventDispatcher, StateManager stateManager)
    {
        if (IsInitialized)
            return;

        Events = eventDispatcher;
        State = stateManager;

        if (Events != null)
        {
            Events.Subscribe<HourTickEvent>(HandleHourTick);
            Events.Subscribe<DayTickEvent>(HandleDayTick);
            _tickSubscribed = true;
        }

        OnInitialize();
        IsInitialized = true;
    }

    public void Shutdown()
    {
        if (!IsInitialized)
            return;

        UnsubscribeTicks();
        OnShutdown();
        IsInitialized = false;
    }

    /// <summary>Set up the system here. Called once, after dependencies are assigned.</summary>
    protected abstract void OnInitialize();

    /// <summary>Release anything OnInitialize acquired. Tick handlers are already removed.</summary>
    protected virtual void OnShutdown() { }

    /// <summary>Called once per in-game hour. Override only if the system needs it.</summary>
    protected virtual void OnHourTick(int day, int hour) { }

    /// <summary>Called once per in-game day. Rations, world simulation, production.</summary>
    protected virtual void OnDayTick(int day) { }

    private void HandleHourTick(HourTickEvent e) => OnHourTick(e.Day, e.Hour);
    private void HandleDayTick(DayTickEvent e) => OnDayTick(e.Day);

    private void UnsubscribeTicks()
    {
        if (!_tickSubscribed || Events == null)
            return;

        Events.Unsubscribe<HourTickEvent>(HandleHourTick);
        Events.Unsubscribe<DayTickEvent>(HandleDayTick);
        _tickSubscribed = false;
    }

    protected virtual void OnDestroy()
    {
        UnsubscribeTicks();
    }

    protected void Log(string message) => Debug.Log($"[{GetType().Name}] {message}");
    protected void LogWarning(string message) => Debug.LogWarning($"[{GetType().Name}] {message}");
    protected void LogError(string message) => Debug.LogError($"[{GetType().Name}] {message}");
}
