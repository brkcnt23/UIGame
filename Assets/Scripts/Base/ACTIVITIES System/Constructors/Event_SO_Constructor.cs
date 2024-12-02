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

    public string[] choices = new string[5];

    public void EventSuccessful(PlayerStatHandler player)
    {
        player.pd.Silver += Silver;
        player.AddStats(TargetStat, Random.Range(StatRewardMin, StatRewardMax + 1));

        AddExperience(player, Experience);
    }

    public void EventFailed(PlayerStatHandler player)
    {
        player.pd.Silver -= Silver;
        player.AddStats(TargetStat, -Random.Range(StatRewardMin, StatRewardMax + 1));

        AddExperience(player, Experience * 2);
    }

    public void EventNeutral(PlayerStatHandler player)
    {
        player.pd.Silver += Silver / 2;

        AddExperience(player, Experience);
    }

    public void EventCritical(PlayerStatHandler player)
    {
        player.pd.Silver += Silver * 2;
        player.AddStats(TargetStat, Random.Range(StatRewardMin, StatRewardMax + 1) * 2);
    }

    public void EventDeclined(PlayerStatHandler playerData)
    {
        AddExperience(playerData, -Experience);
    }

    private void AddExperience(PlayerStatHandler playerData, int experience)
    {
        ExperienceSystem.AddExperience(playerData.pd, experience);
    }

    public void HandleEvent(PlayerStatHandler playerData, int choice)
    {
        switch (choice)
        {
            case 0:
                EventSuccessful(playerData);
                break;
            case 1:
                EventFailed(playerData);
                break;
            case 2:
                EventNeutral(playerData);
                break;
            case 3:
                EventCritical(playerData);
                break;
            case 4:
                EventDeclined(playerData);
                break;
        }
    }
}