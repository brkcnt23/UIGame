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

        TargetStat = StatType.Constitution;
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
        Choice choice = new Choice
        {
            choiceText = choiceText,
            choiceType = choiceType,
            outcome = outcome
        };

        choices.Add(choice);
    }

    public void AddChoiceForTheFollowUpEvent(Event_SO_Constructor followUpEvent)
    {
        if (followUpEvent == null || choiceForTheFollowUpEvent == null)
            return;

        foreach (Choice c in followUpEvent.choices)
        {
            if (c.choiceText == choiceForTheFollowUpEvent.choiceText)
            {
                return;
            }
        }

        string choiceText = $"{choiceForTheFollowUpEvent.choiceText} (Success from {Name} Event)";
        string choiceType = choiceForTheFollowUpEvent.choiceType;
        string outcome = choiceForTheFollowUpEvent.outcome;

        followUpEvent.AddChoices(choiceText, choiceType, outcome);
    }

    public void GoodChoice(PlayerStatHandler player)
    {
        player.pd.AddMoney(0, Silver / 2);
        player.AddStatXP(TargetStat, UnityRandom.Range(StatRewardMin, StatRewardMax));
        player.pd.Alignment += 1;

        AddExperience(player, Experience);

        HandleFollowUpChoiceInjection();
    }

    public void FailChoice(PlayerStatHandler player)
    {
        player.pd.TrySpendMoney(0, Silver);
        player.AddStatXP(TargetStat, -UnityRandom.Range(StatRewardMin, StatRewardMax));

        AddExperience(player, Experience * 2);
    }

    public void NeutralChoce(PlayerStatHandler player)
    {
        player.pd.AddMoney(0, Silver / 2);
        player.AddStatXP(TargetStat, -UnityRandom.Range(StatRewardMin, StatRewardMax) / 2);

        AddExperience(player, Experience);
    }

    public void EvilChoice(PlayerStatHandler player)
    {
        player.pd.AddMoney(0, Silver * 2);
        player.AddStatXP(TargetStat, UnityRandom.Range(StatRewardMin, StatRewardMax));
        player.pd.Alignment -= 1;

        AddExperience(player, Experience);
    }

    public void SuccessChoice(PlayerStatHandler player)
    {
        player.pd.AddMoney(0, Silver);
        player.AddStatXP(TargetStat, UnityRandom.Range(StatRewardMin, StatRewardMax));

        AddExperience(player, Experience);
    }

    public void EventDeclined(PlayerStatHandler playerData)
    {
        AddExperience(playerData, -Experience);
    }

    public void FollowUpEvent(PlayerStatHandler playerData)
    {
        playerData.pd.AddMoney(0, Silver);
        playerData.AddStatXP(TargetStat, UnityRandom.Range(StatRewardMin, StatRewardMax));
        AddExperience(playerData, Experience);
    }

    private void AddExperience(PlayerStatHandler playerData, int experience)
    {
        // Routed through the gateway: rewards shrink as the player outlevels
        // the content, penalties pass through unscaled, level is recomputed
        // on the rising curve. This is what stops one event chain from
        // catapulting a fresh character twenty levels.
        ExperienceSystem.GrantExperience(playerData.pd, experience);
    }

    private void HandleFollowUpChoiceInjection()
    {
        if (followUpEventID == 0 || EventHandler.Instance == null)
            return;

        foreach (Event_SO_Constructor e in EventHandler.Instance.events)
        {
            if (e.ID == followUpEventID)
            {
                AddChoiceForTheFollowUpEvent(e);
                break;
            }
        }
    }

    public void HandleEvent(PlayerStatHandler playerData, Choice choice)
    {
        if (choice == null || playerData == null)
            return;

        encounterCooldown = 2;

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

        if (choice.RewardItemStacks != null && choice.RewardItemStacks.Count > 0)
        {
            ItemRewardHelper.GiveItems(choice.RewardItemStacks);
        }

        if (PlayerUISystem.Instance != null)
        {
            PlayerUISystem.Instance.UpdateUIObjects();
        }
    }
}

[System.Serializable]
public class Choice
{
    public string choiceText;
    public string choiceType; // "Good", "Evil", "Neutral", "Success", "Fail", "Decline", "FollowUp"
    public string outcome;

    // Follow-up Event
    public int followUpEventID;

    // Requirements
    public string RequireStat;
    public int RequireStatValue;
    public int RequireMoney;        // silver bazlı
    public bool isFight;
    public string RequireItemName;
    public int RequireItemId;
    public int RequireItemQuantity = 1;
    public int RequireHealth;
    public int RequireRation;
    public int RequireArmySize;

    // Rewards / penalties
    public int SilverReward;
    public int StatRewardMin;
    public int StatRewardMax;
    public StatType TargetStat;
    public int ExperienceReward;
    public int AlignmentChange;
    public List<ItemStackData> RewardItemStacks = new List<ItemStackData>();

    public bool CheckRequirements(PlayerStatHandler player)
    {
        if (player == null || player.pd == null)
            return false;

        Currency playerMoney = player.pd.GetMoney();
        int totalPlayerSilver = playerMoney.Gold * 100 + playerMoney.Silver;

        if (RequireMoney > 0 && totalPlayerSilver < RequireMoney) return false;
        if (RequireHealth > 0 && player.pd.Health < RequireHealth) return false;
        if (RequireRation > 0 && player.pd.Rations < RequireRation) return false;
        if (RequireArmySize > 0 && (player.pd.PlayerArmy == null || player.pd.PlayerArmy.GetTotalUnits() < RequireArmySize)) return false;

        if (!string.IsNullOrEmpty(RequireStat) && RequireStatValue > 0)
        {
            int playerStatValue = GetPlayerStatValue(player, RequireStat);
            if (playerStatValue < RequireStatValue) return false;
        }

        if (RequireItemId > 0)
        {
            var stateManager = GameBootstrapper.State;
            if (stateManager == null) return false;

            var hasItem = stateManager.GetValue(state =>
            {
                var item = state.Inventory.Items.Find(i => i.ItemId == RequireItemId);
                return item != null && item.Quantity >= RequireItemQuantity;
            });

            if (!hasItem) return false;
        }
        else if (!string.IsNullOrEmpty(RequireItemName))
        {
            bool hasItem = player.pd.Items.Exists(item => item.Name == RequireItemName);
            if (!hasItem) return false;
        }

        return true;
    }

    public void ExecuteChoice(PlayerStatHandler player)
    {
        if (player == null || player.pd == null)
            return;

        int statChange = 0;
        if (StatRewardMin != 0 || StatRewardMax != 0)
        {
            statChange = UnityRandom.Range(StatRewardMin, StatRewardMax);
        }

        switch (choiceType)
        {
            case "Good":
                player.pd.AddMoney(0, SilverReward);
                player.AddStatXP(TargetStat, statChange);
                player.pd.Alignment += AlignmentChange;
                AddExperience(player, ExperienceReward);
                break;

            case "Evil":
                player.pd.AddMoney(0, SilverReward);
                player.AddStatXP(TargetStat, statChange);
                player.pd.Alignment += AlignmentChange;
                AddExperience(player, ExperienceReward);
                break;

            case "Neutral":
                player.pd.AddMoney(0, SilverReward);
                player.AddStatXP(TargetStat, statChange);
                AddExperience(player, ExperienceReward);
                break;

            case "Success":
                player.pd.AddMoney(0, SilverReward);
                player.AddStatXP(TargetStat, statChange);
                AddExperience(player, ExperienceReward);
                break;

            case "Fail":
                player.pd.TrySpendMoney(0, SilverReward);
                player.AddStatXP(TargetStat, -statChange);
                AddExperience(player, -ExperienceReward);
                break;

            case "Decline":
                AddExperience(player, -ExperienceReward);
                break;

            case "FollowUp":
                player.pd.AddMoney(0, SilverReward);
                player.AddStatXP(TargetStat, statChange);
                AddExperience(player, ExperienceReward);
                break;
        }

        if (RewardItemStacks != null && RewardItemStacks.Count > 0)
        {
            ItemRewardHelper.GiveItems(RewardItemStacks);
        }

        if (PlayerUISystem.Instance != null)
        {
            PlayerUISystem.Instance.UpdateUIObjects();
        }
    }

    private void AddExperience(PlayerStatHandler playerData, int experience)
    {
        // Routed through the gateway: rewards shrink as the player outlevels
        // the content, penalties pass through unscaled, level is recomputed
        // on the rising curve. This is what stops one event chain from
        // catapulting a fresh character twenty levels.
        ExperienceSystem.GrantExperience(playerData.pd, experience);
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