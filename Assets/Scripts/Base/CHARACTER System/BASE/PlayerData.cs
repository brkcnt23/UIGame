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
    public int Gold;
    public int Silver;

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
    
    public List<Companion> Companions = new List<Companion>();
    public List<Item> Items = new List<Item>();
    public List<Quest_SO_Constructor> Quests = new List<Quest_SO_Constructor>();

    public string LastSettlementName;

    public bool HasDied;
    public void CheckIfSilverToGold()
    {
        if (Silver >= 100)
        {
            Gold += Silver / 100;
            Silver = Silver % 100;
        }

        if(Silver < 0)
        {
            Gold -= 1;
            Silver += 100;
        }
    }
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
