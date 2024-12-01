using UnityEngine;
[System.Serializable]
public class SO_Base
{
    [Header("Type")]
    public SOTypes Type;

    [Header("Information")]
    public int ID;
    public string Name;
    public string Description;

    [Header("Difficulty&Requirements")]
    public int DC;
    public int HealthRequirement;
    public int ArmyPowerRequirement;
    public int SilverRequirement;
    public int StatRequirement;
    

    [Header("Completion Time")]
    public int CompletionDay;
    public int CompletionHour;
    public int CompletionMinute;

    [Header("Reward")]
    public int Silver;
    public string TargetStat;
    public int StatRewardMin;
    public int StatRewardMax;
}

public enum SOTypes
{
    JOB,
    QUEST,
    EVENT,
    CRAFT
}
