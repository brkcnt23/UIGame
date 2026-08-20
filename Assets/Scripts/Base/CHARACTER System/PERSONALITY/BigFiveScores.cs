using System;

/// <summary>
/// Big Five (OCEAN) scores collected during character creation.
/// Neuroticism is stored reverse scored as <see cref="Stability"/>, so every
/// dimension reads "higher is more of this trait".
/// </summary>
[Serializable]
public struct BigFiveScores
{
    public int Conscientiousness;   // discipline, order, planning
    public int Extraversion;        // boldness, directness, social ease
    public int Agreeableness;       // honesty, kindness, mercy
    public int Openness;            // curiosity, adaptability, unorthodoxy
    public int Stability;           // calm, control (reverse scored Neuroticism)

    public BigFiveScores(int conscientiousness, int extraversion, int agreeableness, int openness, int stability)
    {
        Conscientiousness = conscientiousness;
        Extraversion = extraversion;
        Agreeableness = agreeableness;
        Openness = openness;
        Stability = stability;
    }

    public static BigFiveScores operator +(BigFiveScores a, BigFiveScores b)
    {
        return new BigFiveScores(
            a.Conscientiousness + b.Conscientiousness,
            a.Extraversion + b.Extraversion,
            a.Agreeableness + b.Agreeableness,
            a.Openness + b.Openness,
            a.Stability + b.Stability);
    }

    public int Get(BigFiveDimension dimension)
    {
        switch (dimension)
        {
            case BigFiveDimension.Conscientiousness: return Conscientiousness;
            case BigFiveDimension.Extraversion: return Extraversion;
            case BigFiveDimension.Agreeableness: return Agreeableness;
            case BigFiveDimension.Openness: return Openness;
            case BigFiveDimension.Stability: return Stability;
            default: return 0;
        }
    }

    public override string ToString()
    {
        return $"C{Conscientiousness} E{Extraversion} A{Agreeableness} O{Openness} S{Stability}";
    }
}

public enum BigFiveDimension
{
    Conscientiousness = 0,
    Extraversion = 1,
    Agreeableness = 2,
    Openness = 3,
    Stability = 4
}
