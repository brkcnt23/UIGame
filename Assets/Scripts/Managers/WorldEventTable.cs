using NEXUS.Utilities;

/// <summary>
/// One thing that can happen to a settlement overnight.
///
/// The text matters as much as the numbers. A report line saying
/// "POPULATION: -4, WEALTH: +250" tells the player nothing; "Plague — the sick
/// are carried out at dawn" tells them why, and makes the world feel governed
/// by causes rather than by a random number generator.
/// </summary>
public sealed class WorldEvent
{
    public string Id;
    public string DisplayName;
    public string Summary;

    public int PopulationDelta;
    public int WealthDelta;
    public int QualityDelta;

    /// <summary>Weight in the draw. Higher means more common.</summary>
    public int Weight = 10;

    /// <summary>"A master arrives" — bumps one crafter a level.</summary>
    public bool UpgradesACrafter;
}

/// <summary>
/// The table WorldSimSystem draws from. Weighted so hardship is common enough
/// to hurt and windfalls are rare enough to feel earned.
///
/// This is deliberately a plain C# table rather than ScriptableObjects: it is
/// read-only balance data, and keeping it in one file makes the whole economy
/// legible at a glance. If designers ever need to tune it without recompiling,
/// it becomes a SO then.
/// </summary>
public static class WorldEventTable
{
    private static readonly WorldEvent[] Events =
    {
        new WorldEvent {
            Id = "famine", DisplayName = "Famine",
            Summary = "granaries run low and the price of bread doubles",
            PopulationDelta = -6, WealthDelta = -120, QualityDelta = 0, Weight = 10
        },
        new WorldEvent {
            Id = "plague", DisplayName = "Plague",
            Summary = "the sick are carried out at dawn",
            PopulationDelta = -14, WealthDelta = -80, QualityDelta = -2, Weight = 5
        },
        new WorldEvent {
            Id = "bandit_raid", DisplayName = "Bandit raid",
            Summary = "riders took what the storehouse held",
            PopulationDelta = -3, WealthDelta = -260, QualityDelta = 0, Weight = 12
        },
        new WorldEvent {
            Id = "fire", DisplayName = "Fire",
            Summary = "a workshop burned through the night",
            PopulationDelta = -1, WealthDelta = -140, QualityDelta = -3, Weight = 8
        },
        new WorldEvent {
            Id = "heavy_levy", DisplayName = "Heavy levy",
            Summary = "the crown's collectors came twice this season",
            PopulationDelta = -4, WealthDelta = -180, QualityDelta = -1, Weight = 9
        },

        new WorldEvent {
            Id = "good_harvest", DisplayName = "Bountiful harvest",
            Summary = "the fields gave more than anyone expected",
            PopulationDelta = 4, WealthDelta = 150, QualityDelta = 1, Weight = 12
        },
        new WorldEvent {
            Id = "fair", DisplayName = "Fair",
            Summary = "traders filled the square for three days",
            PopulationDelta = 1, WealthDelta = 320, QualityDelta = 2, Weight = 8
        },
        new WorldEvent {
            Id = "settlers", DisplayName = "Settlers arrive",
            Summary = "families came looking for work and stayed",
            PopulationDelta = 12, WealthDelta = 20, QualityDelta = 0, Weight = 7
        },
        new WorldEvent {
            Id = "trade_route", DisplayName = "New trade route",
            Summary = "a merchant company added this stop to its circuit",
            PopulationDelta = 2, WealthDelta = 200, QualityDelta = 1, Weight = 4
        },
        new WorldEvent {
            Id = "master_arrives", DisplayName = "A master arrives",
            Summary = "a craftsman of some reputation has taken a workshop here",
            PopulationDelta = 1, WealthDelta = 40, QualityDelta = 2, Weight = 3,
            UpgradesACrafter = true
        },
    };

    private static readonly int TotalWeight;

    static WorldEventTable()
    {
        foreach (var e in Events)
            TotalWeight += e.Weight;
    }

    /// <summary>Weighted draw. Never returns null.</summary>
    public static WorldEvent Pick()
    {
        int roll = Dice.Roll(1, TotalWeight + 1);
        int cursor = 0;

        foreach (var e in Events)
        {
            cursor += e.Weight;
            if (roll <= cursor)
                return e;
        }

        return Events[Events.Length - 1];
    }

    public static WorldEvent GetById(string id)
    {
        foreach (var e in Events)
        {
            if (e.Id == id)
                return e;
        }
        return null;
    }
}
