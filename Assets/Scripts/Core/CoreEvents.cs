/// <summary>
/// Events every system may care about. Dispatched through EventDispatcher.
///
/// Time events are the spine of the game: rations, exhaustion, world simulation,
/// quest timers and production all hang off them. Nothing ticks in Update().
/// </summary>

/// <summary>Raised once per in-game hour.</summary>
public sealed class HourTickEvent : GameEvent
{
    public int Day { get; }
    public int Hour { get; }

    public HourTickEvent(int day, int hour)
    {
        Day = day;
        Hour = hour;
    }
}

/// <summary>Raised once per in-game day, after the final hour tick of that day.</summary>
public sealed class DayTickEvent : GameEvent
{
    public int Day { get; }

    public DayTickEvent(int day)
    {
        Day = day;
    }
}

/// <summary>Raised when the player enters a settlement.</summary>
public sealed class SettlementEnteredEvent : GameEvent
{
    public int SettlementId { get; }

    public SettlementEnteredEvent(int settlementId)
    {
        SettlementId = settlementId;
    }
}

/// <summary>Raised when the player leaves a settlement for the open field.</summary>
public sealed class SettlementExitedEvent : GameEvent
{
    public int SettlementId { get; }

    public SettlementExitedEvent(int settlementId)
    {
        SettlementId = settlementId;
    }
}

/// <summary>Raised whenever health, exhaustion, rations or weight change.</summary>
public sealed class VitalsChangedEvent : GameEvent
{
    public int Health { get; }
    public int Exhaustion { get; }
    public int Rations { get; }

    public VitalsChangedEvent(int health, int exhaustion, int rations)
    {
        Health = health;
        Exhaustion = exhaustion;
        Rations = rations;
    }
}

/// <summary>
/// Raised when the player would die: health reached zero or exhaustion hit the cap.
/// During the test phase nothing consumes this beyond a log line.
/// </summary>
public sealed class PlayerDeathEvent : GameEvent
{
    public enum Cause { Health, Exhaustion }

    public Cause DeathCause { get; }

    public PlayerDeathEvent(Cause cause)
    {
        DeathCause = cause;
    }
}

/// <summary>Raised when reputation changes. Track is Standing or Renown.</summary>
public sealed class ReputationChangedEvent : GameEvent
{
    public TitleTrack Track { get; }
    public int NewValue { get; }
    public int Delta { get; }

    public ReputationChangedEvent(TitleTrack track, int newValue, int delta)
    {
        Track = track;
        NewValue = newValue;
        Delta = delta;
    }
}

/// <summary>Raised when the player earns a new title.</summary>
public sealed class TitleEarnedEvent : GameEvent
{
    public string TitleId { get; }

    public TitleEarnedEvent(string titleId)
    {
        TitleId = titleId;
    }
}

/// <summary>
/// A single line for a settlement's report log. WorldSim raises these;
/// the report panel collects them per settlement.
/// </summary>
public sealed class SettlementReportEvent : GameEvent
{
    public int SettlementId { get; }
    public int Day { get; }
    public string Line { get; }

    public SettlementReportEvent(int settlementId, int day, string line)
    {
        SettlementId = settlementId;
        Day = day;
        Line = line;
    }
}
