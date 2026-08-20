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

    public List<ItemStackData> requiredItemStacks = new List<ItemStackData>();
    public List<ItemStackData> rewardItemStacks = new List<ItemStackData>();
    public List<ItemStackData> questItemStacks = new List<ItemStackData>();

    public int settlementID;
    public bool isTaken;
    public bool isCompleted;
    public string questLocation;
    public float[] questLocationCoordinates = new float[2];
    public QuestType questType;

    public void QuestComplete(PlayerData playerData)
    {
        if (!isCompleted)
        {
            Debug.Log("Quest is not completed.");
            return;
        }

        RemoveRequiredAndQuestItems(playerData);
        Reward(playerData);
    }

    public void Reward(PlayerData playerData)
    {
        if (rewardItemStacks != null && rewardItemStacks.Count > 0)
        {
            ItemRewardHelper.GiveItems(rewardItemStacks);
        }
        else if (rewardItems != null && rewardItems.Count > 0)
        {
            foreach (Item item in rewardItems)
            {
                playerData.Items.Add(item);
            }
        }

        playerData.Quests.Remove(this);
        playerData.AddMoney(0, Silver);

        // Level-scaled and curve-aware; also recomputes the character level.
        ExperienceSystem.GrantExperience(playerData, Experience);

        PlayerStatHandler.Instance.AddStatXP(TargetStat, Random.Range(StatRewardMin, StatRewardMax));
    }

    public void QuestFail(PlayerData playerData)
    {
        RemoveRequiredAndQuestItems(playerData);

        playerData.Quests.Remove(this);
        isTaken = false;

        Debug.Log("Quest failed.");

        if (questType == QuestType.Location)
        {
            if (TravelSystem.Instance != null && TravelSystem.Instance.inTravel)
            {
                TravelSystem.Instance.CancelTravelAndReturn(settlementID);
            }
        }
    }

    public void QuestStart(PlayerData playerData)
    {
        if (questItemStacks != null && questItemStacks.Count > 0)
        {
            ItemRewardHelper.GiveItems(questItemStacks);
        }
        else if (questItems != null && questItems.Count > 0)
        {
            foreach (Item item in questItems)
            {
                playerData.Items.Add(item);
            }
        }

        playerData.Quests.Add(this);

        Debug.Log($"Quest started: {Name} You have {hoursToComplete} hours to complete this quest.");

        isTaken = true;
    }

    public void QuestCancel(PlayerData playerData)
    {
        if (questItemStacks != null && questItemStacks.Count > 0)
        {
            ItemRewardHelper.RemoveItems(questItemStacks);
        }
        else if (questItems != null && questItems.Count > 0)
        {
            foreach (Item item in questItems)
            {
                playerData.Items.Remove(item);
            }
        }

        playerData.Quests.Remove(this);
        isTaken = false;

        Debug.Log("Quest cancelled.");
    }

    public void QuestCheck(PlayerData playerData)
    {
        if (hoursToComplete <= 0)
        {
            Debug.Log($"Quest {Name} has expired.");
            QuestFail(playerData);
            return;
        }

        if (requiredItemStacks != null && requiredItemStacks.Count > 0)
        {
            if (!ItemRewardHelper.HasItems(requiredItemStacks))
            {
                return;
            }
        }
        else
        {
            if (!HasAllRequiredItems(playerData))
            {
                Debug.Log("Player does not have all required items.");
                return;
            }
        }

        if (settlementID != 0 &&
            SettlementHandler.Instance != null &&
            SettlementHandler.Instance.settlement != null &&
            settlementID != SettlementHandler.Instance.settlement.ID)
        {
            Debug.Log("Player is not in the correct settlement.");
            return;
        }

        isCompleted = true;
    }

    public void TryToTake()
    {
        QuestStart(PlayerStatHandler.Instance.pd);
        QuestCheck(PlayerStatHandler.Instance.pd);
    }

    // -----------------------------
    // HELPERS
    // -----------------------------

    private void RemoveRequiredAndQuestItems(PlayerData playerData)
    {
        if (requiredItemStacks != null && requiredItemStacks.Count > 0)
        {
            ItemRewardHelper.RemoveItems(requiredItemStacks);
        }
        else if (requiredItems != null && requiredItems.Count > 0)
        {
            foreach (Item item in requiredItems)
            {
                playerData.Items.Remove(item);
            }
        }

        if (questItemStacks != null && questItemStacks.Count > 0)
        {
            ItemRewardHelper.RemoveItems(questItemStacks);
        }
        else if (questItems != null && questItems.Count > 0)
        {
            foreach (Item item in questItems)
            {
                playerData.Items.Remove(item);
            }
        }
    }

    private bool HasAllRequiredItems(PlayerData playerData)
    {
        if (requiredItems == null || requiredItems.Count == 0)
            return true;

        foreach (Item item in requiredItems)
        {
            if (!playerData.Items.Contains(item))
            {
                return false;
            }
        }

        return true;
    }
}