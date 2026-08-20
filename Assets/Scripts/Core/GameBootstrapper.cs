using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Single entry point for game initialization.
///
/// Loads resources once, brings up state and event systems, then initializes
/// every IGameSystem in Priority order. No other script should use Awake to
/// initialize a critical system.
///
/// Boot order:
///   1. ResourceProvider   — databases and assets
///   2. StateManager       — the single source of truth
///   3. EventDispatcher    — the message bus
///   4. GameloopManager    — frame loop
///   5. All IGameSystem    — sorted by Priority, ascending
/// </summary>
public sealed class GameBootstrapper : MonoBehaviour
{
    [SerializeField] private bool _debugLogging = true;

    [Tooltip("Print the full ordered system list on boot. Useful when ordering bugs appear.")]
    [SerializeField] private bool _logSystemOrder = true;

    private static GameBootstrapper _instance;
    private static bool _initialized;

    private ResourceProvider _resourceProvider;
    private StateManager _stateManager;
    private EventDispatcher _eventDispatcher;
    private GameloopManager _gameloopManager;

    private readonly List<IGameSystem> _systems = new();

    public static GameBootstrapper Instance => _instance;
    public static ResourceProvider Resources => _instance != null ? _instance._resourceProvider : null;
    public static StateManager State => _instance != null ? _instance._stateManager : null;
    public static EventDispatcher Events => _instance != null ? _instance._eventDispatcher : null;
    public static bool IsInitialized => _initialized;

    /// <summary>Read-only view of the registered systems, in execution order.</summary>
    public IReadOnlyList<IGameSystem> Systems => _systems;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void OnRuntimeInitialize()
    {
        if (_initialized)
            return;

        var go = new GameObject("[BOOTSTRAP]");
        go.AddComponent<GameBootstrapper>();
        DontDestroyOnLoad(go);
    }

    private void Awake()
    {
        if (_initialized || (_instance != null && _instance != this))
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        Log("=== GAME BOOTSTRAP START ===");

        Log("Phase 1: Loading resources...");
        _resourceProvider = gameObject.AddComponent<ResourceProvider>();
        if (!_resourceProvider.Initialize())
        {
            LogError("ResourceProvider initialization failed. Game cannot continue.");
            return;
        }

        Log("Phase 2: Initializing state & event systems...");
        _stateManager = gameObject.AddComponent<StateManager>();
        _stateManager.InitializeState();
        _eventDispatcher = gameObject.AddComponent<EventDispatcher>();
        _gameloopManager = gameObject.AddComponent<GameloopManager>();

        Log("Core ready. Waiting for the scene before registering systems.");
    }

    /// <summary>
    /// System registration happens in Start, not Awake.
    ///
    /// This object is created at BeforeSceneLoad, so its Awake runs before any
    /// scene object exists — FindObjectsByType would return nothing. Unity runs
    /// every Awake in the scene before any Start, so by the time we get here the
    /// systems are present and can be discovered.
    /// </summary>
    private void Start()
    {
        if (_initialized || !ReferenceEquals(_instance, this))
            return;

        Log("Phase 3: Registering game systems...");
        InitializeAllSystems();

        _initialized = true;
        Log("=== GAME BOOTSTRAP COMPLETE ===");
    }

    /// <summary>
    /// Re-scans for systems that appeared after boot — a newly loaded scene, or
    /// an object spawned at runtime. Already-initialized systems are skipped.
    /// </summary>
    public void ScanForNewSystems()
    {
        if (!_initialized)
            return;

        var all = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        var added = new List<IGameSystem>();

        foreach (var obj in all)
        {
            if (obj is not IGameSystem system || ReferenceEquals(obj, this))
                continue;

            if (_systems.Contains(system))
                continue;

            added.Add(system);
        }

        if (added.Count == 0)
            return;

        added.Sort((a, b) => a.Priority.CompareTo(b.Priority));

        foreach (var system in added)
        {
            _systems.Add(system);

            try
            {
                system.Initialize(_eventDispatcher, _stateManager);
                Log($"Late-registered {system.GetType().Name} (priority {system.Priority}).");
            }
            catch (System.Exception ex)
            {
                LogError($"Failed to initialize {system.GetType().Name}: {ex.Message}");
            }
        }

        _systems.Sort((a, b) => a.Priority.CompareTo(b.Priority));
    }

    private void InitializeAllSystems()
    {
        _systems.Clear();

        var all = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var obj in all)
        {
            if (obj is IGameSystem system && !ReferenceEquals(obj, this))
                _systems.Add(system);
        }

        // Execution order comes from code, not from the scene.
        _systems.Sort((a, b) => a.Priority.CompareTo(b.Priority));

        if (_systems.Count == 0)
        {
            LogWarning("No IGameSystem found. Systems still use the legacy singleton pattern — migrate them one by one.");
        }
        else if (_logSystemOrder)
        {
            var lines = _systems.Select(s => $"    {s.Priority,4}  {s.GetType().Name}");
            Log($"Execution order ({_systems.Count} systems):\n{string.Join("\n", lines)}");
        }

        WarnOnDuplicatePriorities();

        foreach (var system in _systems)
        {
            try
            {
                system.Initialize(_eventDispatcher, _stateManager);
            }
            catch (System.Exception ex)
            {
                LogError($"Failed to initialize {system.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }

    /// <summary>
    /// Two systems sharing a priority means their relative order depends on
    /// scene discovery — exactly the non-determinism we dropped Inspector
    /// ordering to avoid. Warn loudly.
    /// </summary>
    private void WarnOnDuplicatePriorities()
    {
        var clashes = _systems
            .GroupBy(s => s.Priority)
            .Where(g => g.Count() > 1);

        foreach (var clash in clashes)
        {
            var names = string.Join(", ", clash.Select(s => s.GetType().Name));
            LogWarning($"Priority {clash.Key} shared by: {names}. Order between them is not guaranteed.");
        }
    }

    private void OnDestroy()
    {
        if (!ReferenceEquals(_instance, this))
            return;

        // Tear down in reverse so dependents shut down before their dependencies.
        for (int i = _systems.Count - 1; i >= 0; i--)
        {
            try
            {
                _systems[i].Shutdown();
            }
            catch (System.Exception ex)
            {
                LogError($"Failed to shut down {_systems[i].GetType().Name}: {ex.Message}");
            }
        }

        _systems.Clear();
        _initialized = false;
        _instance = null;
    }

    private void Log(string message)
    {
        if (_debugLogging)
            Debug.Log($"[BOOTSTRAP] {message}");
    }

    private void LogWarning(string message) => Debug.LogWarning($"[BOOTSTRAP] {message}");
    private void LogError(string message) => Debug.LogError($"[BOOTSTRAP] {message}");
}
