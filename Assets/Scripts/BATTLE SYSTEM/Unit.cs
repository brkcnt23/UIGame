/// <summary>
/// Every kind of soldier in the game.
///
/// The first five keep the numbers they were given, because those numbers are
/// written into every save file that already exists; anything new is appended.
/// Renumbering this enum would silently turn somebody's knights into bowmen.
///
/// The three original foot types are the poor tier - that is the art that
/// exists for them, and it is what UnitCatalog maps them to.
/// </summary>
public enum UnitType
{
    Knight = 0,
    Soldier = 1,
    Archer = 2,
    Pikeman = 3,
    Shielder = 4,

    PoorSoldier = 5,
    PoorHorseman = 6,
    Cavalry = 7,
    HeavyArcher = 8,
    HeavyPikeman = 9,

    /// <summary>No unit. Used by UnitCatalog to mean "this one promotes to nothing".</summary>
    None = 99
}

/// <summary>
/// A stack of one kind of soldier, not a single man.
///
/// Battles are resolved between stacks, so experience belongs to the stack as
/// well: the twenty men who have come through ten fights together are the
/// veterans, and it is the stack that promotes. Tracking individuals would mean
/// deciding which particular levy survived, which is a lot of bookkeeping for
/// something the player never sees.
/// </summary>
[System.Serializable]
public class Unit
{
    public UnitType Type;
    public int Count;

    /// <summary>
    /// Battles this stack has won and come out of. Reaching the threshold is
    /// what makes a promotion free — the veterans have earned it rather than
    /// been bought.
    /// </summary>
    public int BattlesWon;

    /// <summary>Fights a stack must win before it can promote without paying.</summary>
    public const int BattlesForFreePromotion = 10;

    public bool IsVeteran => BattlesWon >= BattlesForFreePromotion;
}
