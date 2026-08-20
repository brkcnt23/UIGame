using System.Collections.Generic;

[System.Serializable]
public class PlayerData
{
    public int ID;
    public string Name;
    public string VillageName;

    public int Hour;
    public int Minute;
    public int Day;

    public int Level;
    public int Health;
    public int MaxHealth;
    public int Experience;
    public int MaxExperience;

    // Legacy fields - bir süre daha dursun, eski kodları kırmamak için.
    public int Gold;
    public int Silver;

    // Yeni ana para kaynağı
    public Currency Money;

    public int Alignment;

    public int Strength;
    public int StrengthXP;
    public int Dexterity;
    public int DexterityXP;
    public int Constitution;
    public int ConstitutionXP;
    public int Charisma;
    public int CharismaXP;

    public int Rations;

    public Army PlayerArmy { get; set; }

    public int GetMaxUnits()
    {
        return Charisma * 10;
    }

    public int MaxExhaustionLevel;
    public int CurrentExhaustionLevel;

    public int SmitherSkillLevel;
    public int SmitherSkillXP;
    public int TannerSkillLevel;
    public int TannerSkillXP;
    public int CarpenterSkillLevel;
    public int CarpenterSkillXP;
    public int MasonSkillLevel;
    public int MasonSkillXP;
    public int AlchemistSkillLevel;
    public int AlchemistSkillXP;

    public int TotalBattlesFought;
    public int TotalBattlesWon;
    public int TotalBattlesLost;

    public int LastSleepDay;
    public int LastSleepHour;
    public int LastSleepMinute;

    public int LastMealDay;
    public int LastMealHour;
    public int LastMealMinute;
    public List<string> HistoryTags = new List<string>();
    /// <summary>Legacy flat tag list. Kept in sync by TraitSystem; recipes read it.</summary>
    public List<string> ActiveTraitTags = new List<string>();

    /// <summary>Traits with their stacks and expiry. The real record.</summary>
    public List<ActiveTrait> ActiveTraits = new List<ActiveTrait>();
    public List<int> LearnedRecipeIds = new List<int>();
    public List<string> LearnedStations = new List<string>();
    public List<string> LearnedTools = new List<string>();
    public List<Companion> Companions = new List<Companion>();
    public List<Item> Items = new List<Item>();
    public List<ItemStackData> ItemStacks = new List<ItemStackData>();
    public List<Unit> Units = new List<Unit>();
    public List<Quest_SO_Constructor> Quests = new List<Quest_SO_Constructor>();

    public string LastSettlementName;

    public bool HasDied;

    // -----------------------------
    // MONEY HELPERS
    // -----------------------------

    public void InitializeMoneyFromLegacyIfNeeded()
    {
        // Yeni Money boş ama legacy alanlarda veri varsa, legacy'den üret
        if (Money.Gold == 0 && Money.Silver == 0 && (Gold > 0 || Silver > 0))
        {
            Money = new Currency(Gold, Silver);
        }

        SyncLegacyMoneyFromMoney();
    }

    public void SetMoney(int gold, int silver)
    {
        Money = new Currency(gold, silver);
        SyncLegacyMoneyFromMoney();
    }

    public void SyncLegacyMoneyFromMoney()
    {
        Gold = Money.Gold;
        Silver = Money.Silver;
    }

    public void SyncMoneyFromLegacy()
    {
        Money = new Currency(Gold, Silver);
        SyncLegacyMoneyFromMoney();
    }

    public bool HasEnoughMoney(int gold, int silver)
    {
        return Money.HasEnough(gold, silver);
    }

    public bool HasEnoughMoney(Currency amount)
    {
        return Money.HasEnough(amount.Gold, amount.Silver);
    }

    public void AddMoney(int gold, int silver)
    {
        Money.Add(gold, silver);
        SyncLegacyMoneyFromMoney();
    }

    public void AddMoney(Currency amount)
    {
        Money.Add(amount.Gold, amount.Silver);
        SyncLegacyMoneyFromMoney();
    }

    public bool TrySpendMoney(int gold, int silver)
    {
        if (!HasEnoughMoney(gold, silver))
            return false;

        Money.Subtract(gold, silver);
        SyncLegacyMoneyFromMoney();
        return true;
    }

    public bool TrySpendMoney(Currency amount)
    {
        return TrySpendMoney(amount.Gold, amount.Silver);
    }

    public Currency GetMoney()
    {
        return Money;
    }

    // Eski kodlar tamamen temizlenene kadar dursun
    public void CheckIfSilverToGold()
    {
        SyncMoneyFromLegacy();
    }

    // -----------------------------
    // INVENTORY / WEIGHT HELPERS
    // -----------------------------

    public float GetCurrentInventoryWeight()
    {
        if (Items == null || Items.Count == 0)
            return 0f;

        float total = 0f;
        foreach (var item in Items)
        {
            if (item == null) continue;
            total += item.TotalWeight;
        }

        return total;
    }

    public float GetCarryCapacity()
    {
        // Başlangıç için basit formül
        // Sonra companion / mount / cart bonusları eklenebilir
        float baseCapacity = 30f;
        float strengthBonus = Strength * 5f;

        float companionBonus = 0f;
        if (Companions != null && Companions.Count > 0)
        {
            companionBonus = Companions.Count * 5f;
        }

        return baseCapacity + strengthBonus + companionBonus;
    }

    public bool IsOverweight()
    {
        return GetCurrentInventoryWeight() > GetCarryCapacity();
    }

    public float GetWeightRatio()
    {
        float capacity = GetCarryCapacity();
        if (capacity <= 0f) return 0f;

        return GetCurrentInventoryWeight() / capacity;
    }
}

[System.Serializable]
public class ItemStackData
{
    public int ItemId;
    public int Quantity;
}

[System.Serializable]
public class Companion
{
    public string Name;
    public string Description;
    public int Level;
    public int Health;
    public int MaxHealth;
    public int Experience;
    public int MaxExperience;

    public int Strength;
    public int Dexterity;
    public int Constitution;
    public int Charisma;

    public int SmitherSkillLevel;
    public int SmitherSkillXP;
    public int TannerSkillLevel;
    public int TannerSkillXP;
    public int CarpenterSkillLevel;
    public int CarpenterSkillXP;
    public int MasonSkillLevel;
    public int MasonSkillXP;
    public int AlchemistSkillLevel;
    public int AlchemistSkillXP;

    public bool HasDied;
}