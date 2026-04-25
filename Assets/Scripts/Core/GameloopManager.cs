using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Main gameloop orchestrator.
/// Order: Input → Events → State Update → Render (automatic via listeners)
///
/// Each frame:
/// 1. Capture input
/// 2. Dispatch input events (systems listen & react)
/// 3. Process time & resource ticks
/// 4. State updates trigger UI re-renders automatically
/// </summary>
public sealed class GameloopManager : MonoBehaviour
{
    private StateManager _stateManager;
    private EventDispatcher _eventDispatcher;

    private float _deltaTime;
    private bool _isPaused = false;

    private void Start()
    {
        // Wait for bootstrap to initialize
        int attempts = 0;
        while (!GameBootstrapper.IsInitialized && attempts < 100)
        {
            attempts++;
            System.Threading.Thread.Sleep(1);
        }

        _stateManager = GameBootstrapper.State;
        _eventDispatcher = GameBootstrapper.Events;

        if (_stateManager == null || _eventDispatcher == null)
        {
            Debug.LogError("[Gameloop] State or Event system not initialized. GameBootstrapper must run first.");
            enabled = false;
            return;
        }

        // Subscribe to system events
        _eventDispatcher.Subscribe<TogglePauseEvent>(OnPauseToggled);

        Debug.Log("[Gameloop] Manager initialized and ready");
    }

    private void Update()
    {
        _deltaTime = Time.deltaTime;

        if (_isPaused)
            return;

        // Phase 1: Capture input
        ProcessInput();

        // Phase 2: Process logic
        ProcessLogic();
    }

    private void ProcessInput()
    {
        // Keyboard
        if (Input.GetKeyDown(KeyCode.E))
            _eventDispatcher.Dispatch(new OpenInventoryEvent());

        if (Input.GetKeyDown(KeyCode.Q))
            _eventDispatcher.Dispatch(new OpenQuestLogEvent());

        if (Input.GetKeyDown(KeyCode.Escape))
            _eventDispatcher.Dispatch(new TogglePauseEvent());

        // Arrow keys / WASD for movement (settlement navigation)
        float moveX = 0, moveY = 0;

        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            moveX = -1;
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            moveX = 1;
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            moveY = 1;
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            moveY = -1;

        if (moveX != 0 || moveY != 0)
            _eventDispatcher.Dispatch(new PlayerMoveEvent(moveX, moveY));
    }

    private void ProcessLogic()
    {
        // Process time advancement, resource consumption, etc
        // This happens in a single atomic state update

        if (_stateManager == null)
        {
            Debug.LogError("[Gameloop] StateManager is null! Bootstrap incomplete.");
            return;
        }

        _stateManager.UpdateState(state =>
        {
            if (state == null)
            {
                Debug.LogError("[Gameloop] State snapshot is null during update!");
                return null;
            }

            GameState newState = null;
            try
            {
                newState = state.Clone();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[Gameloop] Failed to clone state: {ex.Message}\n{ex.StackTrace}");
                return state;
            }

            // Time tick
            if (newState.Time != null)
            {
                newState.Time.Minute += (int)(_deltaTime * 100); // Accelerated time for testing
                if (newState.Time.Minute >= 60)
                {
                    newState.Time.Minute = 0;
                    newState.Time.Hour++;
                    _eventDispatcher.Dispatch(new HourPassedEvent(newState.Time.Hour));
                }

                if (newState.Time.Hour >= 24)
                {
                    newState.Time.Hour = 0;
                    newState.Time.Day++;
                    _eventDispatcher.Dispatch(new DayPassedEvent(newState.Time.Day));
                }
            }

            // Resource consumption
            if (newState.Player != null)
            {
                // Passive exhaustion increase
                newState.Player.Exhaustion = Mathf.Min(
                    newState.Player.Exhaustion + (int)(_deltaTime * 2),
                    newState.Player.MaxExhaustion
                );

                // Ration consumption
                if (_deltaTime > 0 && newState.Time.Hour % 4 == 0) // Every 4 hours (rough)
                {
                    newState.Player.Ration = Mathf.Max(0, newState.Player.Ration - 1);
                    if (newState.Player.Ration == 0)
                    {
                        newState.Player.Health = Mathf.Max(0, newState.Player.Health - 2);
                        _eventDispatcher.Dispatch(new PlayerStarvedEvent());
                    }
                }
            }

            return newState;
        });
    }

    private void OnPauseToggled(TogglePauseEvent evt)
    {
        _isPaused = !_isPaused;
        Debug.Log($"[Gameloop] Game paused: {_isPaused}");
    }

    public bool IsPaused => _isPaused;
}

/// <summary>
/// Game event definitions
/// </summary>

public sealed class OpenInventoryEvent : GameEvent { }
public sealed class OpenQuestLogEvent : GameEvent { }
public sealed class TogglePauseEvent : GameEvent { }

public sealed class PlayerMoveEvent : GameEvent
{
    public float MoveX { get; }
    public float MoveY { get; }
    public PlayerMoveEvent(float moveX, float moveY)
    {
        MoveX = moveX;
        MoveY = moveY;
    }
}

public sealed class HourPassedEvent : GameEvent
{
    public int Hour { get; }
    public HourPassedEvent(int hour) => Hour = hour;
}

public sealed class DayPassedEvent : GameEvent
{
    public int Day { get; }
    public DayPassedEvent(int day) => Day = day;
}

public sealed class PlayerStarvedEvent : GameEvent { }
