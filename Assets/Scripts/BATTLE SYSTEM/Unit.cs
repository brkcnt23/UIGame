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

public class Knight : Unit
{
    public Knight(int count) : base(UnitType.Knight, count)
    {
    }
}

public class Soldier : Unit
{
    public Soldier(int count) : base(UnitType.Soldier, count)
    {
    }
}

public class Archer : Unit
{
    public Archer(int count) : base(UnitType.Archer, count)
    {
    }
}

public class Pikeman : Unit
{
    public Pikeman(int count) : base(UnitType.Pikeman, count)
    {
    }
}

public class Shielder : Unit
{
    public Shielder(int count) : base(UnitType.Shielder, count)
    {
    }
}
