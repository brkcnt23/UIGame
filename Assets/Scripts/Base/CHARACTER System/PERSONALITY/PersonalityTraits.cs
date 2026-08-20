using System.Collections.Generic;

/// <summary>
/// A starting personality trait and the passive bonuses it grants.
/// The numbers are percentages (or a flat roll bonus) and are meant to be tuned;
/// systems read them through <see cref="PersonalityTraits.GetById"/>.
/// </summary>
public class PersonalityTraitDefinition
{
    public string Id;
    public string DisplayName;
    public string Description;

    /// <summary>Extra gold from jobs and craft work, in percent.</summary>
    public int JobRewardPercent;

    /// <summary>Better crafting outcomes, in percent.</summary>
    public int CraftQualityPercent;

    /// <summary>Better buy/sell prices, in percent.</summary>
    public int TradePricePercent;

    /// <summary>Flat bonus added to event and quest rolls.</summary>
    public int EventRollBonus;

    /// <summary>Extra exhaustion recovered from rest and sleep, in percent.</summary>
    public int RestRecoveryPercent;
}

/// <summary>
/// The eight traits a character can start with, one per outcome of the
/// character creation scenarios.
/// </summary>
public static class PersonalityTraits
{
    public const string Ambitious = "Ambitious";
    public const string Proud = "Proud";
    public const string HonestNature = "HonestNature";
    public const string RiskSeeker = "RiskSeeker";
    public const string CalmMind = "CalmMind";
    public const string KindButUnyielding = "KindButUnyielding";
    public const string ColdPragmatist = "ColdPragmatist";
    public const string HiddenMercy = "HiddenMercy";

    private static readonly Dictionary<string, PersonalityTraitDefinition> Definitions =
        new Dictionary<string, PersonalityTraitDefinition>
        {
            {
                Ambitious, new PersonalityTraitDefinition
                {
                    Id = Ambitious,
                    DisplayName = "Ambitious",
                    Description = "You finish what you start, and you count what it earned you.",
                    JobRewardPercent = 8
                }
            },
            {
                Proud, new PersonalityTraitDefinition
                {
                    Id = Proud,
                    DisplayName = "Proud",
                    Description = "You speak first, and you stand where others step back.",
                    TradePricePercent = 5,
                    EventRollBonus = 1
                }
            },
            {
                HonestNature, new PersonalityTraitDefinition
                {
                    Id = HonestNature,
                    DisplayName = "Honest Nature",
                    Description = "You would sooner lose a coin than your word.",
                    TradePricePercent = 8
                }
            },
            {
                RiskSeeker, new PersonalityTraitDefinition
                {
                    Id = RiskSeeker,
                    DisplayName = "Risk Seeker",
                    Description = "An unknown road pulls at you harder than a safe one.",
                    CraftQualityPercent = 5,
                    EventRollBonus = 1
                }
            },
            {
                CalmMind, new PersonalityTraitDefinition
                {
                    Id = CalmMind,
                    DisplayName = "Calm Mind",
                    Description = "Fear reaches you late, and it does not stay long.",
                    EventRollBonus = 1,
                    RestRecoveryPercent = 10
                }
            },
            {
                KindButUnyielding, new PersonalityTraitDefinition
                {
                    Id = KindButUnyielding,
                    DisplayName = "Kind But Unyielding",
                    Description = "You give people your help, never your ground.",
                    JobRewardPercent = 5,
                    TradePricePercent = 5
                }
            },
            {
                ColdPragmatist, new PersonalityTraitDefinition
                {
                    Id = ColdPragmatist,
                    DisplayName = "Cold Pragmatist",
                    Description = "You weigh a thing, you price it, and you move on.",
                    TradePricePercent = 10,
                    RestRecoveryPercent = 5
                }
            },
            {
                HiddenMercy, new PersonalityTraitDefinition
                {
                    Id = HiddenMercy,
                    DisplayName = "Hidden Mercy",
                    Description = "You help quietly, and you would rather no one mentioned it.",
                    EventRollBonus = 1,
                    RestRecoveryPercent = 10
                }
            }
        };

    public static PersonalityTraitDefinition GetById(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        PersonalityTraitDefinition definition;
        return Definitions.TryGetValue(id, out definition) ? definition : null;
    }

    public static IEnumerable<PersonalityTraitDefinition> All
    {
        get { return Definitions.Values; }
    }
}
