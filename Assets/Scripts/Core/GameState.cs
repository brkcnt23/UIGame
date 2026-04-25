using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Game's complete state snapshot. Treat as immutable.
/// Never modify properties directly. Always use StateManager.UpdateState() to mutate.
/// </summary>
public sealed class GameState
{
    // Player data
    public PlayerState Player { get; set; }

    // Current settlement (where player is)
    public SettlementState CurrentSettlement { get; set; }

    // All known settlements
    public Dictionary<int, SettlementState> Settlements { get; set; } = new();

    // Time system
    public TimeState Time { get; set; }

    // Inventory
    public InventoryState Inventory { get; set; }

    // Active job/quest
    public JobState CurrentJob { get; set; }
    public QuestState CurrentQuest { get; set; }

    // UI state (which panel is open, etc)
    public UIState UI { get; set; }

    public GameState Clone()
    {
        return new GameState
        {
            Player = Player?.Clone(),
            CurrentSettlement = CurrentSettlement?.Clone(),
            Settlements = new Dictionary<int, SettlementState>(Settlements),
            Time = Time?.Clone(),
            Inventory = Inventory?.Clone(),
            CurrentJob = CurrentJob?.Clone(),
            CurrentQuest = CurrentQuest?.Clone(),
            UI = UI?.Clone(),
        };
    }
}

/// <summary>
/// Player character state
/// </summary>
public sealed class PlayerState
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Level { get; set; }

    // Core stats
    public int Health { get; set; }
    public int MaxHealth { get; set; }
    public int Exhaustion { get; set; }
    public int MaxExhaustion { get; set; }

    // Resources
    public long Gold { get; set; }
    public long Silver { get; set; }
    public int Ration { get; set; }

    // Attributes
    public int Strength { get; set; }
    public int Dexterity { get; set; }
    public int Constitution { get; set; }
    public int Charisma { get; set; }

    // Equipped items
    public ItemInstance EquippedWeapon { get; set; }
    public ItemInstance EquippedArmor { get; set; }

    public PlayerState Clone()
    {
        return new PlayerState
        {
            Id = Id,
            Name = Name,
            Level = Level,
            Health = Health,
            MaxHealth = MaxHealth,
            Exhaustion = Exhaustion,
            MaxExhaustion = MaxExhaustion,
            Gold = Gold,
            Silver = Silver,
            Ration = Ration,
            Strength = Strength,
            Dexterity = Dexterity,
            Constitution = Constitution,
            Charisma = Charisma,
            EquippedWeapon = EquippedWeapon,
            EquippedArmor = EquippedArmor,
        };
    }
}

/// <summary>
/// Settlement state (village, town, city)
/// </summary>
public sealed class SettlementState
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Level { get; set; } // 1=village, 2=town, 3=city

    // Economic
    public long Treasury { get; set; }
    public int Population { get; set; }

    // Buildings
    public bool HasTavern { get; set; }
    public bool HasShop { get; set; }
    public bool HasSmith { get; set; }
    public bool HasCrafter { get; set; }

    public SettlementState Clone()
    {
        return new SettlementState
        {
            Id = Id,
            Name = Name,
            Level = Level,
            Treasury = Treasury,
            Population = Population,
            HasTavern = HasTavern,
            HasShop = HasShop,
            HasSmith = HasSmith,
            HasCrafter = HasCrafter,
        };
    }
}

/// <summary>
/// Time/calendar state
/// </summary>
public sealed class TimeState
{
    public int Day { get; set; }
    public int Hour { get; set; }
    public int Minute { get; set; }

    public TimeState Clone()
    {
        return new TimeState
        {
            Day = Day,
            Hour = Hour,
            Minute = Minute,
        };
    }

    public int GetTotalMinutes() => Day * 1440 + Hour * 60 + Minute;
}

/// <summary>
/// Inventory state (items player carries)
/// </summary>
public sealed class InventoryState
{
    public List<ItemInstance> Items { get; set; } = new();
    public int Capacity { get; set; } = 20;

    public int UsedSlots => Items.Count;
    public int FreeSlots => Capacity - UsedSlots;

    public InventoryState Clone()
    {
        return new InventoryState
        {
            Items = new List<ItemInstance>(Items),
            Capacity = Capacity,
        };
    }
}

/// <summary>
/// Item instance (item in inventory)
/// </summary>
public sealed class ItemInstance
{
    public int ItemId { get; set; }
    public int Quantity { get; set; }

    public ItemInstance Clone()
    {
        return new ItemInstance
        {
            ItemId = ItemId,
            Quantity = Quantity,
        };
    }
}

/// <summary>
/// Active job state
/// </summary>
public sealed class JobState
{
    public int JobId { get; set; }
    public int TimeRequired { get; set; } // in minutes
    public int TimeRemaining { get; set; }
    public long Reward { get; set; }

    public JobState Clone()
    {
        return new JobState
        {
            JobId = JobId,
            TimeRequired = TimeRequired,
            TimeRemaining = TimeRemaining,
            Reward = Reward,
        };
    }
}

/// <summary>
/// Active quest state
/// </summary>
public sealed class QuestState
{
    public int QuestId { get; set; }
    public string Objective { get; set; }
    public bool IsCompleted { get; set; }

    public QuestState Clone()
    {
        return new QuestState
        {
            QuestId = QuestId,
            Objective = Objective,
            IsCompleted = IsCompleted,
        };
    }
}

/// <summary>
/// UI/Panel state (which panels are open, etc)
/// </summary>
public sealed class UIState
{
    public bool IsInventoryOpen { get; set; }
    public bool IsShopOpen { get; set; }
    public bool IsCraftingOpen { get; set; }
    public bool IsJobListOpen { get; set; }

    public UIState Clone()
    {
        return new UIState
        {
            IsInventoryOpen = IsInventoryOpen,
            IsShopOpen = IsShopOpen,
            IsCraftingOpen = IsCraftingOpen,
            IsJobListOpen = IsJobListOpen,
        };
    }
}
