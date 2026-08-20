using UnityEngine;

/// <summary>
/// Counts down event encounter cooldowns, one per day.
///
/// Moved out of TimeSystem.NormalizeTime(). It also had a subtle bug there:
/// the block only ran when Hour crossed 24 inside a single normalize call, so
/// advancing three days at once still decremented cooldowns exactly once.
/// Travel — where several days pass in one step — is precisely where events
/// matter most.
///
/// Now it is one decrement per DayTickEvent, and the dispatcher emits one
/// event per day actually crossed.
/// </summary>
public sealed class EventCooldownSystem : GameSystemBase
{
    public override int Priority => SystemPriority.EventSystem;

    [SerializeField] private bool verbose;

    protected override void OnInitialize()
    {
        Log("Event cooldowns now tick once per day.");
    }

    protected override void OnDayTick(int day)
    {
        var handler = EventHandler.Instance;
        if (handler?.events == null)
            return;

        int stillCooling = 0;

        foreach (var e in handler.events)
        {
            if (e == null || e.encounterCooldown <= 0)
                continue;

            e.encounterCooldown--;
            if (e.encounterCooldown > 0)
                stillCooling++;
        }

        if (verbose && stillCooling > 0)
            Log($"Day {day}: {stillCooling} event(s) still on cooldown.");
    }
}
