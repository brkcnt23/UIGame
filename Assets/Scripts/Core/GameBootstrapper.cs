using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Single entry point for game initialization.
/// Loads resources once, initializes state/event systems, registers all managers.
/// No other script should use Awake to initialize critical systems.
/// </summary>
public sealed class GameBootstrapper : MonoBehaviour
{
    [SerializeField] private bool _debugLogging = true;

    private static GameBootstrapper _instance;
    private static bool _initialized = false;

    private ResourceProvider _resourceProvider;
    private StateManager _stateManager;
    private EventDispatcher _eventDispatcher;
    private GameloopManager _gameloopManager;

    private List<IGameSystem> _systems = new();

    public static GameBootstrapper Instance => _instance;
    public static ResourceProvider Resources => _instance != null ? _instance._resourceProvider : null;
    public static StateManager State => _instance != null ? _instance._stateManager : null;
    public static EventDispatcher Events => _instance != null ? _instance._eventDispatcher : null;
    public static bool IsInitialized => _initialized;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void OnRuntimeInitialize()
    {
        if (_initialized)
            return;

        var bootstrapperGO = new GameObject("[BOOTSTRAP]");
        var bootstrapper = bootstrapperGO.AddComponent<GameBootstrapper>();
        DontDestroyOnLoad(bootstrapperGO);
    }

    private void Awake()
    {
        if (_initialized)
        {
            Destroy(gameObject);
            return;
        }

        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        Log("=== GAME BOOTSTRAP START ===");

        // Phase 1: Load resources (one-time, shared across all systems)
        Log("Phase 1: Loading resources...");
        _resourceProvider = gameObject.AddComponent<ResourceProvider>();
        if (!_resourceProvider.Initialize())
        {
            LogError("ResourceProvider initialization failed. Game cannot continue.");
            return;
        }

        // Phase 2: Initialize core state systems
        Log("Phase 2: Initializing state & event systems...");
        _stateManager = gameObject.AddComponent<StateManager>();
        _stateManager.InitializeState();
        _eventDispatcher = gameObject.AddComponent<EventDispatcher>();
        _gameloopManager = gameObject.AddComponent<GameloopManager>();

        // Phase 3: Find & initialize all systems
        Log("Phase 3: Registering game systems...");
        InitializeAllSystems();

        _initialized = true;
        Log("=== GAME BOOTSTRAP COMPLETE ===");
    }

    private void InitializeAllSystems()
    {
        // Find all IGameSystem components (excluding bootstrapper)
        var allSystems = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

        foreach (var obj in allSystems)
        {
            if (obj is IGameSystem system && obj != this)
            {
                _systems.Add(system);
                Log($"Registered system: {system.GetType().Name}");
            }
        }

        // Initialize in deterministic order (order matters!)
        foreach (var system in _systems)
        {
            try
            {
                system.Initialize(_eventDispatcher, _stateManager);
            }
            catch (System.Exception ex)
            {
                LogError($"Failed to initialize {system.GetType().Name}: {ex.Message}");
            }
        }
    }

    private void Log(string message)
    {
        if (_debugLogging)
            Debug.Log($"[BOOTSTRAP] {message}");
    }

    private void LogError(string message)
    {
        Debug.LogError($"[BOOTSTRAP] {message}");
    }
}
