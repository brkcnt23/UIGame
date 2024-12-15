using System.Collections.Generic;
using UnityEngine;

public enum QuestType
{
    defaultQuest,
    Location
}
[System.Serializable]
public class Quest_SO_Constructor : SO_Base
{
    public Quest_SO_Constructor()
    {
        Type = SOTypes.QUEST;

        ID = 0;
        Name = "New Quest";
        Description = "This is a new quest.";

        DC = 10;

        CompletionDay = 0;
        CompletionHour = 1;
        CompletionMinute = 0;

        Silver = 100;

        TargetStat = StatType.Constitution;
        StatRewardMin = 1;
        StatRewardMax = 3;
    }

    public int hoursToComplete;
    public List<Item> requiredItems = new List<Item>();
    public List<Item> rewardItems = new List<Item>();
    public List<Item> questItems = new List<Item>();
    public int settlementID;
    public bool isTaken;
    public bool isCompleted;
    public string questLocation;
    public float[] questLocationCoordinates = new float[2];
    public QuestType questType;

    public void QuestComplete(PlayerData playerData)
    {
        if(isCompleted == false)
        {
            Debug.Log("Quest is not completed.");
            return;
        }
        foreach (Item item in requiredItems)
        {
            playerData.Items.Remove(item);
        }

        foreach (Item item in questItems)
        {
            playerData.Items.Remove(item);
        }

        Reward(playerData);

    }

    public void Reward(PlayerData playerData)
    {
        if (rewardItems.Count > 0)
        {

            foreach (Item item in rewardItems)
            {
                playerData.Items.Add(item);
            }
        }

        playerData.Quests.Remove(this);
        playerData.Silver += Silver;
        playerData.CheckIfSilverToGold();
        playerData.Experience += Experience;
        
        PlayerStatHandler.Instance.AddStats(TargetStat, Random.Range(StatRewardMin, StatRewardMax + 1));
        
        ExperienceSystem.UpdateCharacterLevel(playerData);
    }

    public void QuestFail(PlayerData playerData)
    {
        // Remove required and quest items from player
        foreach (Item item in requiredItems)
        {
            playerData.Items.Remove(item);
        }

        foreach (Item item in questItems)
        {
            playerData.Items.Remove(item);
        }

        // Remove quest from player's quest list
        playerData.Quests.Remove(this);

        isTaken = false;

        Debug.Log("Quest failed.");

        if(questType == QuestType.Location)
        {
            if(TravelSystem.Instance.inTravel)
            {
                TravelSystem.Instance.CancelTravelAndReturn(settlementID);
            }
        }
    }

    public void QuestStart(PlayerData playerData)
    {
        // Add quest and quest items to player's quest list

        foreach (Item item in questItems)
        {
            playerData.Items.Add(item);
        }

        playerData.Quests.Add(this);

        Debug.Log($"Quest started: {Name} You have {hoursToComplete} hours to complete this quest.");

        isTaken = true;

    }

    public void QuestCancel(PlayerData playerData)
    {
        // Remove quest items from player
        foreach (Item item in questItems)
        {
            playerData.Items.Remove(item);
        }

        // Remove quest from player's quest list
        playerData.Quests.Remove(this);

        isTaken = false;

        Debug.Log("Quest cancelled.");
    }

    public void QuestCheck(PlayerData playerData)
    {
        // Check if player has required items

        if(hoursToComplete <= 0)
        {
            Debug.Log($"Quest {Name} has expired.");
            QuestFail(playerData);
            return;
        }

        List<Item> requiredItems = new List<Item>();

        foreach (Item item in this.requiredItems)
        {
            if (playerData.Items.Contains(item))
            {
                requiredItems.Add(item);
            }
            else
            {
                return;
            }
        }

        // If player has all required items, complete quest
        if (settlementID != SettlementHandler.Instance.settlement.ID && settlementID != 0)
        {
            Debug.Log("Player is not in the correct settlement.");
            return;
        }
        if (requiredItems == this.requiredItems)
        {
            isCompleted = true;
        }
        else
        {
            Debug.Log("Player does not have all required items.");
        }
    }

    public void TryToTake()
    {
        QuestStart(PlayerStatHandler.Instance.pd);
        QuestCheck(PlayerStatHandler.Instance.pd);
    }
}