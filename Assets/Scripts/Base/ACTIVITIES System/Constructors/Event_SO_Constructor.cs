using System;
using System.Collections.Generic;
using UnityEngine;
using UnityRandom = UnityEngine.Random;
[System.Serializable]
public class Event_SO_Constructor : SO_Base
{
    public Event_SO_Constructor()
    {
        Type = SOTypes.EVENT;

        ID = 0;
        Name = "New Event";
        Description = "This is a new event.";

        DC = 10;

        CompletionDay = 0;
        CompletionHour = 1;
        CompletionMinute = 0;

        Silver = 100;

        TargetStat = "Constitution";
        StatRewardMin = 1;
        StatRewardMax = 3;
    }
    public bool isHaveWar = false;
    public int encounterCooldown = 0;

    public List<Choice> choices = new List<Choice>();

    public int followUpEventID;

    public Choice choiceForTheFollowUpEvent;

    public void AddChoices(string choiceText, string choiceType, string outcome)
    {
        Choice choice = new Choice();
        choice.choiceText = choiceText;
        choice.choiceType = choiceType;
        choice.outcome = outcome;

        choices.Add(choice);
    }

    public void AddChoiceForTheFollowUpEvent(Event_SO_Constructor followUpEvent)
    {

        foreach (Choice c in followUpEvent.choices)
        {
            if (c.choiceText == choiceForTheFollowUpEvent.choiceText)
            {
                return;
            }
        }
        string choiceText = $"{choiceForTheFollowUpEvent.choiceText} (Success from {Name} Evnet)";
        string choiceType = choiceForTheFollowUpEvent.choiceType;
        string outcome = choiceForTheFollowUpEvent.outcome;

        followUpEvent.AddChoices(choiceText, choiceType, outcome);
    }

    public void GoodChoice(PlayerStatHandler player)
    {
        player.pd.Silver += Silver / 2;
        player.AddStats(TargetStat, UnityRandom.Range(StatRewardMin, StatRewardMax + 1));
        player.pd.Alignment += 1;

        AddExperience(player, Experience);

        if (followUpEventID != 0)
        {

            foreach (Event_SO_Constructor e in EventHandler.Instance.events)
            {
                if (e.ID == followUpEventID)
                {
                    AddChoiceForTheFollowUpEvent(e);
                    break;
                }
            }

        }
    }

    public void FailChoice(PlayerStatHandler player)
    {
        player.pd.Silver -= Silver;
        player.AddStats(TargetStat, -UnityRandom.Range(StatRewardMin, StatRewardMax + 1));

        AddExperience(player, Experience * 2);
    }

    public void NeutralChoce(PlayerStatHandler player)
    {
        player.pd.Silver += Silver / 2;
        player.AddStats(TargetStat, -UnityRandom.Range(StatRewardMin, StatRewardMax + 1) / 2);

        AddExperience(player, Experience);
    }

    public void EvilChoice(PlayerStatHandler player)
    {
        player.pd.Silver += Silver * 2;
        player.AddStats(TargetStat, UnityRandom.Range(StatRewardMin, StatRewardMax + 1));
        player.pd.Alignment -= 1;

        AddExperience(player, Experience);
    }

    public void SuccessChoice(PlayerStatHandler player)
    {
        player.pd.Silver += Silver;
        player.AddStats(TargetStat, UnityRandom.Range(StatRewardMin, StatRewardMax + 1));

        AddExperience(player, Experience);
    }

    public void EventDeclined(PlayerStatHandler playerData)
    {
        AddExperience(playerData, -Experience);
    }

    public void FollowUpEvent(PlayerStatHandler playerData)
    {
        playerData.pd.Silver += Silver;
        playerData.AddStats(TargetStat, UnityRandom.Range(StatRewardMin, StatRewardMax + 1));
        AddExperience(playerData, Experience);
    }

    private void AddExperience(PlayerStatHandler playerData, int experience)
    {
        playerData.pd.Experience += experience;
        ExperienceSystem.UpdateCharacterLevel(playerData.pd);
    }

    public void HandleEvent(PlayerStatHandler playerData, Choice choice)
    {
        encounterCooldown = 4;

        //call the appropriate method based on the choice type
        switch (choice.choiceType)
        {
            case "Good":
                GoodChoice(playerData);
                break;
            case "Fail":
                FailChoice(playerData);
                break;
            case "Neutral":
                NeutralChoce(playerData);
                break;
            case "Evil":
                EvilChoice(playerData);
                break;
            case "Success":
                SuccessChoice(playerData);
                break;
            case "Decline":
                EventDeclined(playerData);
                break;
            case "FollowUp":
                FollowUpEvent(playerData);
                break;
        }

        playerData.pd.CheckIfSilverToGold();
    }
}

[System.Serializable]
public class Choice
{
    public string choiceText;
    public string choiceType; // "Good", "Evil", "Neutral", "Success", "Fail", "Decline", "FollowUp"
    public string outcome;

    // Follow-up Event
    // If choiceType is "FollowUp", this ID should link to another event.
    public int followUpEventID;

    // Requirements
    public string RequireStat;      // e.g., "Strength"
    public int RequireStatValue;    // minimum required value of that stat
    public int RequireMoney;        // minimum required Silver
    public bool isFight;            // if this choice involves combat
    public string RequireItemName;  // if a specific item is required
    public int RequireHealth;       // minimum health required
    public int RequireRation;       // minimum rations required
    public int RequireArmySize;     // minimum army size required

    // Rewards (or penalties)
    public int SilverReward;
    public int StatRewardMin;
    public int StatRewardMax;
    public string TargetStat; // e.g., "Constitution", "Strength", "Charisma", "Dexterity"
    public int ExperienceReward;
    public int AlignmentChange; // +1 for Good, -1 for Evil, 0 for Neutral/Success, etc.

    /// <summary>
    /// Check if the player meets the requirements for this choice.
    /// For example, if the choice requires a certain amount of Silver or a certain stat level.
    /// </summary>
    public bool CheckRequirements(PlayerStatHandler player)
    {
        int totalPlayerSilver = player.pd.Gold * 100 + player.pd.Silver; // Convert gold to silver and add silver

        // Check Silver Requirement
        if (RequireMoney > 0 && totalPlayerSilver < RequireMoney) return false;

        // Check Health Requirement
        if (RequireHealth > 0 && player.pd.Health < RequireHealth) return false;

        // Check Ration Requirement
        if (RequireRation > 0 && player.pd.Rations < RequireRation) return false;

        // Check Army Size Requirement
        if (RequireArmySize > 0 && (player.pd.PlayerArmy == null || player.pd.PlayerArmy.GetTotalUnits() < RequireArmySize)) return false;

        // Check stat requirement if specified
        if (!string.IsNullOrEmpty(RequireStat) && RequireStatValue > 0)
        {
            int playerStatValue = GetPlayerStatValue(player, RequireStat);
            if (playerStatValue < RequireStatValue) return false;
        }

        // Check item requirement if specified
        if (!string.IsNullOrEmpty(RequireItemName))
        {
            bool hasItem = player.pd.Items.Exists(item => item.Name == RequireItemName);
            if (!hasItem) return false;
        }

        return true;
    }

    /// <summary>
    /// Execute the choice and apply its effects based on the choiceType.
    /// </summary>
    public void ExecuteChoice(PlayerStatHandler player)
    {
        int statChange = 0;
        if (StatRewardMin != 0 || StatRewardMax != 0)
        {
            // If stat rewards are specified, get a random amount in the range
            statChange = UnityRandom.Range(StatRewardMin, StatRewardMax + 1);
        }

        switch (choiceType)
        {
            case "Good":
                // Good: Generally positive alignment, some rewards
                player.pd.Silver += SilverReward;
                if (!string.IsNullOrEmpty(TargetStat)) player.AddStats(TargetStat, statChange);
                player.pd.Alignment += AlignmentChange;
                AddExperience(player, ExperienceReward);
                break;

            case "Evil":
                // Evil: Gains might be higher in some resources but alignment decreases
                player.pd.Silver += SilverReward;
                if (!string.IsNullOrEmpty(TargetStat)) player.AddStats(TargetStat, statChange);
                player.pd.Alignment += AlignmentChange;
                AddExperience(player, ExperienceReward);
                break;

            case "Neutral":
                // Neutral: Balanced outcome
                player.pd.Silver += SilverReward;
                if (!string.IsNullOrEmpty(TargetStat)) player.AddStats(TargetStat, statChange);
                AddExperience(player, ExperienceReward);
                break;

            case "Success":
                // Success: Usually a positive outcome with good rewards
                player.pd.Silver += SilverReward;
                if (!string.IsNullOrEmpty(TargetStat)) player.AddStats(TargetStat, statChange);
                AddExperience(player, ExperienceReward);
                break;

            case "Fail":
                // Fail: Negative outcome, might lose silver, stats or experience
                player.pd.Silver -= SilverReward; // or apply negative silver if that makes sense
                if (!string.IsNullOrEmpty(TargetStat)) player.AddStats(TargetStat, -statChange);
                AddExperience(player, -ExperienceReward); // Lose experience as penalty
                break;

            case "Decline":
                // Decline: Player refuses event and loses experience
                AddExperience(player, -ExperienceReward);
                break;

            case "FollowUp":
                // FollowUp: Triggers another event. You can also give rewards before loading next event.
                player.pd.Silver += SilverReward;
                if (!string.IsNullOrEmpty(TargetStat)) player.AddStats(TargetStat, statChange);
                AddExperience(player, ExperienceReward);

                // Handling the follow-up event can be done outside this method, once you return followUpEventID.
                // For example:
                // EventHandler.Instance.LoadEvent(followUpEventID);
                break;
        }

        player.pd.CheckIfSilverToGold();
    }

    private void AddExperience(PlayerStatHandler playerData, int experience)
    {
        playerData.pd.Experience += experience;
        ExperienceSystem.UpdateCharacterLevel(playerData.pd);
    }

    private int GetPlayerStatValue(PlayerStatHandler player, string statName)
    {
        switch (statName)
        {
            case "Strength": return player.pd.Strength;
            case "Dexterity": return player.pd.Dexterity;
            case "Constitution": return player.pd.Constitution;
            case "Charisma": return player.pd.Charisma;
            default: return 0;
        }
    }
}