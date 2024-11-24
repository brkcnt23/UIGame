using UnityEngine;
[System.Serializable]
public class SO_Base : ScriptableObject
{
    [Header("Type")]
    public SOTypes Type;

    [Header("Information")]
    public int ID;
    public string Name;
    public string Description;

    [Header("Requirements")]
    public int DC;
    [Range(1, 24)]
    public int CompletionHour;
    public int CompletionMinute;

    [Header("Reward")]
    public int Reward;
}

public enum SOTypes
{
    JOB,
    QUEST,
    EVENT,
    CRAFT
}
