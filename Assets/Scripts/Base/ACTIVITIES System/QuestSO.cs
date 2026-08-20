using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Which kingdom a quest belongs to. Any means it can appear anywhere and
/// keeps the board from ever running dry.
/// </summary>
public enum QuestRealm
{
    Any,
    Karnhold,
    Averlyn,
    Sahenmar
}

/// <summary>
/// The rank of the work, which decides the reward, the paper it is written
/// on, and the voice it is written in.
///
///   Errand      a villager's own hand, plain and a little apologetic
///   Contract    a guild clerk, businesslike
///   Commission  a steward or officer, formal but human
///   Charter     the court, ceremonial and distant
///   Royal       the crown itself, and only in a capital
/// </summary>
public enum QuestTier
{
    Errand,
    Contract,
    Commission,
    Charter,
    Royal
}

/// <summary>
/// One quest, as data.
///
/// The note the player sees is assembled at runtime from a paper sprite, this
/// asset's sketch, and text — rather than being a single painted image per
/// quest. Two reasons: the reward can be rebalanced without redrawing
/// anything, and a hundred quests need a hundred lines of text but still only
/// one sheet of paper.
/// </summary>
[CreateAssetMenu(fileName = "Quest", menuName = "UIGame/Quest")]
public class QuestSO : ScriptableObject
{
    [Header("Identity")]
    public int questId;
    public string questName;

    [Tooltip("Written in the voice of whoever pinned it up.")]
    [TextArea(3, 6)] public string description;

    [Header("Placement")]
    public QuestTier tier = QuestTier.Errand;
    public QuestRealm realm = QuestRealm.Any;

    [Tooltip("Minimum character level before this can be accepted.")]
    [Min(1)] public int minPlayerLevel = 1;

    [Tooltip("Title id required to accept, if any. Royal work is sealed.")]
    public string requiredTitleId = "";

    [Header("Art")]
    [Tooltip("The ink drawing. Transparent background, drawn over the paper.")]
    public Sprite sketch;

    [Tooltip("Leave empty to use a random paper for this tier.")]
    public Sprite paperOverride;

    [Header("Reward")]
    [Min(0)] public int rewardGold;
    [Min(0)] public int rewardSilver;
    [Min(0)] public int rewardExperience;

    public StatType targetStat = StatType.Strength;
    [Min(0)] public int statRewardMin = 1;
    [Min(0)] public int statRewardMax = 2;

    [Header("Cost")]
    [Tooltip("In-game hours the work itself takes.")]
    [Min(0)] public int hoursToComplete = 8;

    [Tooltip("In-game hours before the offer expires once accepted.")]
    [Min(0)] public int hoursBeforeExpiry = 72;

    [Header("Difficulty")]
    [Tooltip("Target number on a d20 roll.")]
    [Min(0)] public int difficultyClass = 10;

    [Header("Requirements")]
    public List<ItemStackData> requiredItems = new();
    public List<ItemStackData> rewardItems = new();

    // ---------------------------------------------------------------

    public int TotalSilver => rewardGold * 100 + rewardSilver;

    /// <summary>"5g 50s" or "90s" — how the note prints it.</summary>
    public string RewardLabel()
    {
        if (rewardGold > 0 && rewardSilver > 0) return $"{rewardGold} gold {rewardSilver} silver";
        if (rewardGold > 0) return $"{rewardGold} gold";
        return $"{rewardSilver} silver";
    }

    /// <summary>Gold is shown for anything worth a gold piece or more.</summary>
    public bool ShowsGoldCoin => rewardGold > 0;

    public bool IsAvailableTo(PlayerData player)
    {
        if (player == null) return false;
        return player.Level >= minPlayerLevel;
    }

    /// <summary>
    /// Royal work is visible before it is reachable — the player should see
    /// what a title would open, not wonder whether anything exists up there.
    /// </summary>
    public string LockReason(PlayerData player)
    {
        if (player == null) return null;

        if (player.Level < minPlayerLevel)
            return $"Asks for someone of level {minPlayerLevel}.";

        if (!string.IsNullOrEmpty(requiredTitleId))
            return "The seal is not for you to break.";

        return null;
    }
}
