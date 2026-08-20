using UnityEngine;

/// <summary>
/// Turns the clock into an event stream.
///
/// TimeSystem knows how to move the clock; it should not also know who cares.
/// This watches the clock and emits one HourTickEvent per hour crossed and one
/// DayTickEvent per day crossed — including when several days pass at once
/// during travel or a long job.
///
/// Nothing here runs in Update(). Ticks are emitted only when TimeSystem
/// reports that the clock moved.
/// </summary>
public sealed class TimeTickDispatcher : GameSystemBase
{
    public override int Priority => SystemPriority.Time;

    public static TimeTickDispatcher Instance { get; private set; }

    [Tooltip("Safety cap. A single advance never emits more ticks than this, " +
             "so a bad call cannot freeze the game.")]
    [SerializeField] private int maxTicksPerAdvance = 24 * 90;   // ~90 game days

    [SerializeField] private bool verbose;

    private int _lastDay = -1;
    private int _lastHour = -1;
    private bool _primed;

    protected override void OnInitialize()
    {
        Instance = this;
        Log("Ready. Waiting for the first clock sync.");
    }

    protected override void OnShutdown()
    {
        if (Instance == this)
            Instance = null;
    }

    /// <summary>
    /// Call after the clock moves. Emits an event for every hour and day
    /// boundary crossed since the previous call.
    ///
    /// The first call only records the position — loading a save at day 185
    /// must not fire 185 days of ticks.
    /// </summary>
    public void SyncTo(int day, int hour)
    {
        int now = day * 24 + hour;

        if (!_primed)
        {
            _lastDay = day;
            _lastHour = hour;
            _primed = true;

            if (verbose)
                Log($"Primed at day {day}, hour {hour}. No ticks emitted.");
            return;
        }

        int previous = _lastDay * 24 + _lastHour;

        if (now == previous)
            return;

        if (now < previous)
        {
            // Clock moved backwards — a load or a debug jump. Re-prime silently.
            _lastDay = day;
            _lastHour = hour;
            LogWarning($"Clock moved backwards ({previous} -> {now}). Re-primed without ticks.");
            return;
        }

        int steps = now - previous;

        if (steps > maxTicksPerAdvance)
        {
            LogWarning($"Advance of {steps} hours exceeds the cap of {maxTicksPerAdvance}. " +
                       "Emitting the cap and jumping the rest.");
            steps = maxTicksPerAdvance;
        }

        int cursor = previous;

        for (int i = 0; i < steps; i++)
        {
            cursor++;

            int tickDay = cursor / 24;
            int tickHour = cursor % 24;

            Events?.Dispatch(new HourTickEvent(tickDay, tickHour));

            // A new day begins when the hour rolls over to 0.
            if (tickHour == 0)
                Events?.Dispatch(new DayTickEvent(tickDay));
        }

        _lastDay = day;
        _lastHour = hour;

        if (verbose)
            Log($"Emitted {steps} hour tick(s) up to day {day}, hour {hour}.");
    }

    /// <summary>Reset after loading a save so the next SyncTo re-primes.</summary>
    public void Reprime(int day, int hour)
    {
        _lastDay = day;
        _lastHour = hour;
        _primed = true;
    }
}
