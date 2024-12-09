using System.Collections.Generic;
using UnityEngine;
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
        player.AddStats(TargetStat, Random.Range(StatRewardMin, StatRewardMax + 1));
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
        player.AddStats(TargetStat, -Random.Range(StatRewardMin, StatRewardMax + 1));

        AddExperience(player, Experience * 2);
    }

    public void NeutralChoce(PlayerStatHandler player)
    {
        player.pd.Silver += Silver / 2;
        player.AddStats(TargetStat, -Random.Range(StatRewardMin, StatRewardMax + 1) / 2);

        AddExperience(player, Experience);
    }

    public void EvilChoice(PlayerStatHandler player)
    {
        player.pd.Silver += Silver * 2;
        player.AddStats(TargetStat, Random.Range(StatRewardMin, StatRewardMax + 1));
        player.pd.Alignment -= 1;

        AddExperience(player, Experience);
    }

    public void SuccessChoice(PlayerStatHandler player)
    {
        player.pd.Silver += Silver;
        player.AddStats(TargetStat, Random.Range(StatRewardMin, StatRewardMax + 1));

        AddExperience(player, Experience);
    }

    public void EventDeclined(PlayerStatHandler playerData)
    {
        AddExperience(playerData, -Experience);
    }

    public void FollowUpEvent(PlayerStatHandler playerData)
    {
        playerData.pd.Silver += Silver;
        playerData.AddStats(TargetStat, Random.Range(StatRewardMin, StatRewardMax + 1));
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
    public string choiceType;
    public string outcome;
}