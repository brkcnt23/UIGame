public enum UnitType
{
    Knight,
    Soldier,
    Archer,
    Pikeman,
    Shielder
}

[System.Serializable]
public class Unit
{
    public UnitType Type;
    public int Count;
}

public class Knight : Unit
{
}

public class Soldier : Unit
{
}

public class Archer : Unit
{
}

public class Pikeman : Unit
{
}

public class Shielder : Unit
{
}
