public enum UnitType
{
    Knight,
    Soldier,
    Archer,
    Pikeman,
    Shielder
}

public class Unit
{
    public UnitType Type { get; set; }
    public int Count { get; set; }

    public Unit(UnitType type, int count)
    {
        Type = type;
        Count = count;
    }
}
