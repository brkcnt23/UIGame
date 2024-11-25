
using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public int ID;
    public string Name;


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


    public int Strength;
    public int Dexterity;
    public int Constitution;
    public int Charisma;


    public int Rations;


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


    public int LastSleepDay;
    public int LastSleepHour;
    public int LastSleepMinute;

    public int LastMealDay;
    public int LastMealHour;
    public int LastMealMinute;

    public List<Companion> Companions = new List<Companion>();


    public bool HasDied;
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