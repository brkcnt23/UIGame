using UnityEngine;

[CreateAssetMenu(fileName = "Player", menuName = "SO System/Player")]
public class PlayerSO : ScriptableObject
{
    [Header("Information")]
    public int ID;
    public string Name;
    public int Hour;
    public int Minute;
    public int Day;
    [Header("Stats")]
    public int Level;
    [Space(10)]
    public int MaxHealth;
    public int CurrentHealth;
    [Space(10)]
    public int MaxExperience;
    public int CurrentExperience;

    [Space(10)]
    [Header("Exhaustion")]
    public int Maxexhaustion;
    public int Currentexhaustion;
    
    [Space(10)]
    [Header("Attributes")]
    public int Strength;
    public int Dexterity;
    public int Constitution;
    public int Charisma;

    [Space(10)]
    [Header("Equipment")]
    public int Rations;


    public PlayerSO()
    {
        ID = 0;
        Name = "New Player";
        Hour = 6;
        Minute = 0;
        Day = 0;

        Level = 1;
        MaxHealth = 100;
        CurrentHealth = 100;
        MaxExperience = 100;
        CurrentExperience = 0;
        Strength = 1;
        Dexterity = 1;
        Constitution = 1;
        Charisma = 1;
        Rations = 10;
    }
}
