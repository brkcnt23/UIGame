/// <summary>
/// Starting values of a brand new character and their home village.
/// GameManager builds the save from these, and the new game screen previews them,
/// so both always show the same numbers.
/// </summary>
public static class NewGameDefaults
{
    // Character
    public const int Level = 1;
    public const int Health = 100;
    public const int MaxExperience = 149;
    public const int StatValue = 1;
    public const int StatXP = 149;
    public const int SkillLevel = 1;
    public const int SkillXP = 149;
    public const int Rations = 10;
    public const int MaxExhaustionLevel = 10;

    // Purse
    public const int Gold = 5;
    public const int Silver = 0;

    // Clock
    public const int Day = 1;
    public const int Hour = 6;
    public const int Minute = 0;

    // Home village
    public const int VillageQuality = 1;
    public const int VillagePopulation = 10;
    public const int VillageWealthGold = 100;
    public const int VillageWealthSilver = 0;
}
