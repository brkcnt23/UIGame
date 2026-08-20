using UnityEngine;

/// <summary>
/// Owns quest countdowns.
///
/// This logic used to live inside TimeSystem.NormalizeTime(), where it was
/// also wrong:
///
///     quest.hoursToComplete -= Hour;
///
/// That subtracts the absolute hour of day, not the hours that actually
/// elapsed. Advancing one hour at 14:00 removed fourteen hours from every
/// active quest. Advancing at 02:00 removed two.
///
/// Here it is one hour per hour tick, which is what the field means.
/// </summary>
public sealed class QuestTimerSystem : GameSystemBase
{
    public override int Priority => SystemPriority.Quest;

    [Tooltip("Log each expiry. Useful while verifying the timing fix.")]
    [SerializeField] private bool verbose;

    protected override void OnInitialize()
    {
        Log("Quest timers now tick one hour per hour.");
    }

    protected override void OnHourTick(int day, int hour)
    {
        var pd = GetPlayerData();
        if (pd?.Quests == null)
            return;

        for (int i = 0; i < pd.Quests.Count; i++)
        {
            var quest = pd.Quests[i];

            if (quest == null || quest.isCompleted)
                continue;

            if (quest.hoursToComplete <= 0)
                continue;

            quest.hoursToComplete--;

            if (quest.hoursToComplete == 0)
                OnQuestExpired(quest, pd);
        }
    }

    private void OnQuestExpired(Quest_SO_Constructor quest, PlayerData pd)
    {
        if (verbose)
            Log($"Quest expired: {quest.Name}");

        // Failure handling stays with the quest object for now. When the full
        // QuestSystem lands this becomes a QuestFailedEvent instead.
        quest.QuestFail(pd);
    }

    private PlayerData GetPlayerData()
    {
        return PlayerStatHandler.Instance != null ? PlayerStatHandler.Instance.pd : null;
    }
}
