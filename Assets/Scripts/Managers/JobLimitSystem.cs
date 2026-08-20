using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Weekly cap on repeatable work.
///
/// Exhaustion and hunger stop the player from spamming a job within a day;
/// this stops them from living off the same job for weeks. Both brakes exist
/// so that neither has to be brutal on its own.
///
/// Default: each distinct job 3 times per 7-day week. The week rolls over on
/// day % 7 == 0, counted from day ticks — no real-time clocks.
///
/// Not yet persisted into PlayerData; on load, counters start fresh. That is
/// acceptable while saves are test data — flagged in the session notes.
/// </summary>
public sealed class JobLimitSystem : GameSystemBase
{
    public override int Priority => SystemPriority.JobLimits;

    public static JobLimitSystem Instance { get; private set; }

    [Tooltip("How many times the same job can be worked per week.")]
    [SerializeField] private int usesPerWeek = 3;

    [Tooltip("Days in a week for the rollover.")]
    [SerializeField] private int daysPerWeek = 7;

    [SerializeField] private bool verbose;

    private readonly Dictionary<string, int> _usesThisWeek = new();
    private int _lastRolloverDay;

    protected override void OnInitialize()
    {
        Instance = this;
        Log($"Job cap: {usesPerWeek} per {daysPerWeek} days.");
    }

    protected override void OnShutdown()
    {
        if (Instance == this)
            Instance = null;
    }

    protected override void OnDayTick(int day)
    {
        if (day - _lastRolloverDay < daysPerWeek)
            return;

        _lastRolloverDay = day;
        _usesThisWeek.Clear();

        if (verbose)
            Log($"Day {day}: new week, job counters reset.");
    }

    /// <summary>How many more times this job can be worked this week.</summary>
    public int RemainingUses(string jobId)
    {
        if (string.IsNullOrEmpty(jobId))
            return usesPerWeek;

        _usesThisWeek.TryGetValue(jobId, out int used);
        return Mathf.Max(0, usesPerWeek - used);
    }

    public bool CanWork(string jobId) => RemainingUses(jobId) > 0;

    /// <summary>
    /// Registers one use. Returns false — without consuming — when the cap
    /// is already reached, so callers can gate on the return value alone.
    /// </summary>
    public bool TryConsumeUse(string jobId)
    {
        if (string.IsNullOrEmpty(jobId))
            return true;

        _usesThisWeek.TryGetValue(jobId, out int used);

        if (used >= usesPerWeek)
        {
            if (verbose)
                Log($"'{jobId}' is at its weekly cap ({usesPerWeek}).");
            return false;
        }

        _usesThisWeek[jobId] = used + 1;
        return true;
    }
}
