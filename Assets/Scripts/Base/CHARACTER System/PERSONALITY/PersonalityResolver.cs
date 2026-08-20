using System.Collections.Generic;

/// <summary>
/// What the character creation scenarios produced: the raw Big Five scores and
/// the trait the character starts the game with.
/// </summary>
public class PersonalityResult
{
    public BigFiveScores Scores;
    public BigFiveDimension DominantDimension;
    public BigFiveDimension SecondDimension;
    public string TraitId;

    public PersonalityTraitDefinition Trait
    {
        get { return PersonalityTraits.GetById(TraitId); }
    }

    public string TraitDisplayName
    {
        get
        {
            PersonalityTraitDefinition definition = Trait;
            return definition != null ? definition.DisplayName : TraitId;
        }
    }
}

/// <summary>
/// Turns the answers of the eight scenarios into one starting trait.
/// The strongest dimension decides the trait; when the two strongest are close
/// (within <see cref="BlendThreshold"/>) the pair gets its own mixed trait.
/// </summary>
public static class PersonalityResolver
{
    public const int BlendThreshold = 1;

    private static readonly BigFiveDimension[] DimensionOrder =
    {
        BigFiveDimension.Conscientiousness,
        BigFiveDimension.Extraversion,
        BigFiveDimension.Agreeableness,
        BigFiveDimension.Openness,
        BigFiveDimension.Stability
    };

    private static readonly Dictionary<BigFiveDimension, string> DominantTraits =
        new Dictionary<BigFiveDimension, string>
        {
            { BigFiveDimension.Conscientiousness, PersonalityTraits.Ambitious },
            { BigFiveDimension.Extraversion, PersonalityTraits.Proud },
            { BigFiveDimension.Agreeableness, PersonalityTraits.HonestNature },
            { BigFiveDimension.Openness, PersonalityTraits.RiskSeeker },
            { BigFiveDimension.Stability, PersonalityTraits.CalmMind }
        };

    public static PersonalityResult Resolve(IEnumerable<PersonalityChoice> answers)
    {
        BigFiveScores total = new BigFiveScores();

        if (answers != null)
        {
            foreach (PersonalityChoice choice in answers)
            {
                if (choice != null)
                    total = total + choice.Scores;
            }
        }

        return Resolve(total);
    }

    public static PersonalityResult Resolve(BigFiveScores scores)
    {
        // Strongest dimension first; equal scores keep the declared order.
        List<BigFiveDimension> ordered = new List<BigFiveDimension>(DimensionOrder);
        ordered.Sort((x, y) =>
        {
            int byScore = scores.Get(y).CompareTo(scores.Get(x));

            if (byScore != 0)
                return byScore;

            return System.Array.IndexOf(DimensionOrder, x).CompareTo(System.Array.IndexOf(DimensionOrder, y));
        });

        BigFiveDimension dominant = ordered[0];
        BigFiveDimension second = ordered[1];

        string traitId = DominantTraits[dominant];

        if (scores.Get(dominant) - scores.Get(second) <= BlendThreshold)
        {
            string blended = GetBlendedTrait(dominant, second);

            if (blended != null)
                traitId = blended;
        }

        return new PersonalityResult
        {
            Scores = scores,
            DominantDimension = dominant,
            SecondDimension = second,
            TraitId = traitId
        };
    }

    /// <summary>
    /// Traits for the pairs that describe something the single dimensions do not.
    /// Any other pair keeps the dominant dimension's trait.
    /// </summary>
    private static string GetBlendedTrait(BigFiveDimension a, BigFiveDimension b)
    {
        if (Pair(a, b, BigFiveDimension.Conscientiousness, BigFiveDimension.Agreeableness))
            return PersonalityTraits.KindButUnyielding;

        if (Pair(a, b, BigFiveDimension.Conscientiousness, BigFiveDimension.Stability))
            return PersonalityTraits.ColdPragmatist;

        if (Pair(a, b, BigFiveDimension.Agreeableness, BigFiveDimension.Stability))
            return PersonalityTraits.HiddenMercy;

        return null;
    }

    private static bool Pair(BigFiveDimension a, BigFiveDimension b, BigFiveDimension first, BigFiveDimension second)
    {
        return (a == first && b == second) || (a == second && b == first);
    }
}
