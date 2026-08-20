using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Character creation built on the Five Factor Model (CANOE / OCEAN) — the
/// most validated personality framework in psychology.
///
///   C  Conscientiousness  order, discipline, follow-through
///   A  Agreeableness      cooperation, trust, warmth
///   N  Neuroticism        emotional reactivity (low = steady)
///   O  Openness           curiosity, appetite for the unfamiliar
///   E  Extraversion       social energy, assertiveness
///
/// Answers do not hand out traits directly. Each answer moves the five axes,
/// and the traits fall out of the resulting profile at the end. Two players
/// can reach "Calm Mind" by different routes, and no single question decides
/// who the character is — which is the point of a profile rather than a class
/// picker.
///
/// One question per axis would be worthless psychometrically; the real
/// instruments use dozens. Eight questions with two touches per axis is the
/// honest compromise for a game: enough that the result feels earned, few
/// enough that nobody skips it.
/// </summary>
public enum Axis
{
    Conscientiousness,
    Agreeableness,
    Neuroticism,
    Openness,
    Extraversion
}

public sealed class AxisShift
{
    public Axis Axis;
    public int Value;

    public AxisShift(Axis axis, int value)
    {
        Axis = axis;
        Value = value;
    }
}

public sealed class CreationAnswer
{
    public string Id;
    public string Text;

    /// <summary>Short line under the option, in the world's voice.</summary>
    public string Subtext;

    /// <summary>Set only on the origin question — origins are a choice, not a score.</summary>
    public string GrantsTraitId;

    public int Strength;
    public int Dexterity;
    public int Constitution;
    public int Charisma;

    public List<AxisShift> Shifts = new();
    public List<string> Tags = new();
}

public sealed class CreationQuestion
{
    public string Id;
    public string Prompt;
    public List<CreationAnswer> Answers = new();
}

public static class CharacterCreation
{
    private static List<CreationQuestion> _all;
    public static List<CreationQuestion> Questions => _all ??= Build();

    private static AxisShift S(Axis a, int v) => new AxisShift(a, v);

    private static CreationAnswer A(string id, string text, string subtext,
                                    int str, int dex, int con, int cha,
                                    AxisShift[] shifts, string trait = null, string[] tags = null)
    {
        return new CreationAnswer
        {
            Id = id, Text = text, Subtext = subtext,
            Strength = str, Dexterity = dex, Constitution = con, Charisma = cha,
            Shifts = new List<AxisShift>(shifts),
            GrantsTraitId = trait,
            Tags = new List<string>(tags ?? new string[0])
        };
    }

    private static List<CreationQuestion> Build()
    {
        var list = new List<CreationQuestion>();

        // ===========================================================
        // 0. ORIGIN — not a personality measure. Where you come from
        //    decides your starting craft and your first trait.
        // ===========================================================
        list.Add(new CreationQuestion
        {
            Id = "origin",
            Prompt = "Where did you grow up?",
            Answers = new List<CreationAnswer>
            {
                A("farm", "On a farm", "Fields, animals, and the same work every year.",
                  1, 0, 2, 0, new[] { S(Axis.Conscientiousness, 1) },
                  "origin_farm", new[] { "farm_raised" }),

                A("forge", "Beside a forge", "Heat, hammering, and a father who never explained anything twice.",
                  2, 1, 0, 0, new[] { S(Axis.Conscientiousness, 1) },
                  "origin_forge", new[] { "forge_raised" }),

                A("woodcutter", "At the treeline", "Timber came down and you helped carry it.",
                  2, 0, 1, 0, new[] { S(Axis.Extraversion, -1) },
                  "origin_woodcutter", new[] { "woodcutter_raised" }),

                A("mason", "In a mason's yard", "Stone dust in everything, including the bread.",
                  1, 0, 2, 0, new[] { S(Axis.Conscientiousness, 1) },
                  "origin_mason", new[] { "mason_raised" }),

                A("caravan", "On the road", "Your family moved goods and never stayed anywhere.",
                  0, 2, 0, 1, new[] { S(Axis.Openness, 1) },
                  "origin_caravan", new[] { "caravan_raised" }),

                A("street", "In the alleys of a town", "Nobody fed you unless you arranged it yourself.",
                  0, 3, 0, 0, new[] { S(Axis.Agreeableness, -1) },
                  "origin_street", new[] { "street_raised" }),

                A("chapel", "Under a chapel roof", "Taught letters and mercy by people short of both.",
                  0, 0, 0, 3, new[] { S(Axis.Agreeableness, 1) },
                  "origin_chapel", new[] { "chapel_raised" }),

                A("camp", "Among soldiers", "Camp followers raised you between marches.",
                  1, 1, 1, 0, new[] { S(Axis.Neuroticism, -1) },
                  "origin_camp", new[] { "camp_raised" }),
            }
        });

        // ===========================================================
        // 1. CONSCIENTIOUSNESS — how you treat obligation
        // ===========================================================
        list.Add(new CreationQuestion
        {
            Id = "q_promise",
            Prompt = "You promised to help a neighbour at dawn. You wake up ill.",
            Answers = new List<CreationAnswer>
            {
                A("go_anyway", "Go anyway", "You said you would.",
                  0, 0, 1, 0, new[] { S(Axis.Conscientiousness, 2), S(Axis.Neuroticism, -1) }),

                A("send_word", "Send word, go tomorrow", "The work still gets done.",
                  0, 0, 0, 1, new[] { S(Axis.Conscientiousness, 1), S(Axis.Agreeableness, 1) }),

                A("rest", "Stay in bed", "They will manage. You are no use half-dead.",
                  0, 0, 0, 0, new[] { S(Axis.Conscientiousness, -1), S(Axis.Agreeableness, -1) }),

                A("forgot", "You had already forgotten", "There was a lot on.",
                  0, 1, 0, 0, new[] { S(Axis.Conscientiousness, -2) }),
            }
        });

        // ===========================================================
        // 2. CONSCIENTIOUSNESS — how you approach work
        // ===========================================================
        list.Add(new CreationQuestion
        {
            Id = "q_work",
            Prompt = "You are given a job with no supervision and no deadline.",
            Answers = new List<CreationAnswer>
            {
                A("plan", "Plan it out first", "Measure twice. You would rather do it once.",
                  0, 1, 0, 0, new[] { S(Axis.Conscientiousness, 2), S(Axis.Openness, -1) }),

                A("steady", "Work a set amount every day", "Same hours, no drama.",
                  0, 0, 1, 0, new[] { S(Axis.Conscientiousness, 2) }),

                A("bursts", "Work in bursts when it interests you", "It gets done. Eventually. Well.",
                  0, 0, 0, 0, new[] { S(Axis.Conscientiousness, -1), S(Axis.Openness, 2) }),

                A("last_minute", "Leave it until it is urgent", "You work better with a fire behind you.",
                  0, 1, 0, 0, new[] { S(Axis.Conscientiousness, -2), S(Axis.Neuroticism, 1) }),
            }
        });

        // ===========================================================
        // 3. AGREEABLENESS — what you do with an advantage
        // ===========================================================
        list.Add(new CreationQuestion
        {
            Id = "q_advantage",
            Prompt = "A trader miscounts and hands you more than you paid for.",
            Answers = new List<CreationAnswer>
            {
                A("return_it", "Say something immediately", "It is not yours.",
                  0, 0, 0, 1, new[] { S(Axis.Agreeableness, 2), S(Axis.Conscientiousness, 1) },
                  null, new[] { "honest" }),

                A("keep_quiet", "Say nothing and walk", "They should count better.",
                  0, 1, 0, 0, new[] { S(Axis.Agreeableness, -2) },
                  null, new[] { "pragmatic" }),

                A("come_back", "Keep it, come back later", "You needed it more today than they did.",
                  0, 0, 0, 0, new[] { S(Axis.Agreeableness, -1), S(Axis.Neuroticism, 1) }),

                A("split", "Point it out and ask for a fair price instead", "Everyone leaves satisfied.",
                  0, 0, 0, 2, new[] { S(Axis.Agreeableness, 1), S(Axis.Extraversion, 1) },
                  null, new[] { "merchant_minded" }),
            }
        });

        // ===========================================================
        // 4. AGREEABLENESS — how you handle someone else's failure
        // ===========================================================
        list.Add(new CreationQuestion
        {
            Id = "q_failure",
            Prompt = "Someone working beside you makes a mistake that costs you both.",
            Answers = new List<CreationAnswer>
            {
                A("cover", "Cover for them", "It could have been you.",
                  0, 0, 0, 1, new[] { S(Axis.Agreeableness, 2) },
                  null, new[] { "kind" }),

                A("teach", "Show them how to avoid it", "Anger fixes nothing. Instruction might.",
                  0, 0, 0, 1, new[] { S(Axis.Agreeableness, 1), S(Axis.Conscientiousness, 1) }),

                A("report", "Report it", "Consequences exist for a reason.",
                  0, 0, 0, 0, new[] { S(Axis.Agreeableness, -2), S(Axis.Conscientiousness, 1) }),

                A("leave", "Stop working with them", "You do not carry other people's errors twice.",
                  1, 0, 0, -1, new[] { S(Axis.Agreeableness, -1), S(Axis.Extraversion, -1) }),
            }
        });

        // ===========================================================
        // 5. NEUROTICISM — how you sit with bad news
        // ===========================================================
        list.Add(new CreationQuestion
        {
            Id = "q_bad_news",
            Prompt = "Something has gone badly wrong and there is nothing to do until morning.",
            Answers = new List<CreationAnswer>
            {
                A("sleep", "Sleep", "Tomorrow needs you rested more than tonight needs you worried.",
                  0, 0, 1, 0, new[] { S(Axis.Neuroticism, -2) },
                  null, new[] { "steady" }),

                A("prepare", "Prepare for the morning", "You cannot fix it, but you can be ready.",
                  0, 0, 0, 0, new[] { S(Axis.Neuroticism, -1), S(Axis.Conscientiousness, 2) }),

                A("company", "Find someone to sit with", "It is easier out loud.",
                  0, 0, 0, 1, new[] { S(Axis.Extraversion, 2), S(Axis.Neuroticism, 1) }),

                A("turn_over", "Turn it over all night", "You will have looked at it from every side by dawn.",
                  0, 0, -1, 0, new[] { S(Axis.Neuroticism, 2), S(Axis.Openness, 1) }),
            }
        });

        // ===========================================================
        // 6. OPENNESS — appetite for the unfamiliar
        // ===========================================================
        list.Add(new CreationQuestion
        {
            Id = "q_unknown",
            Prompt = "A road you have never taken runs off to the side. It adds two days.",
            Answers = new List<CreationAnswer>
            {
                A("take_it", "Take it", "You want to know what is down there.",
                  0, 1, 0, 0, new[] { S(Axis.Openness, 2), S(Axis.Conscientiousness, -1) }),

                A("ask", "Ask someone about it first", "Curiosity is fine. Blind curiosity is not.",
                  0, 0, 0, 1, new[] { S(Axis.Openness, 1), S(Axis.Extraversion, 1) }),

                A("note_it", "Note it and carry on", "Another time, with more rations.",
                  0, 0, 1, 0, new[] { S(Axis.Conscientiousness, 1) }),

                A("known_road", "Stay on the road you know", "You have somewhere to be.",
                  0, 0, 0, 0, new[] { S(Axis.Openness, -2), S(Axis.Conscientiousness, 1) }),
            }
        });

        // ===========================================================
        // 7. EXTRAVERSION — where your energy comes from
        // ===========================================================
        list.Add(new CreationQuestion
        {
            Id = "q_tavern",
            Prompt = "A crowded tavern, end of a long day. What do you do?",
            Answers = new List<CreationAnswer>
            {
                A("join", "Join the loudest table", "You will know half of them by midnight.",
                  0, 0, 0, 2, new[] { S(Axis.Extraversion, 2), S(Axis.Agreeableness, 1) }),

                A("listen", "Sit near the talk and listen", "You learn more this way than anyone talking does.",
                  0, 1, 0, 0, new[] { S(Axis.Openness, 1), S(Axis.Extraversion, -1) }),

                A("business", "Find whoever is worth knowing", "You are not here to drink.",
                  0, 0, 0, 1, new[] { S(Axis.Extraversion, 1), S(Axis.Agreeableness, -1) }),

                A("corner", "Take a corner and eat alone", "The day is over. That is enough.",
                  0, 0, 1, -1, new[] { S(Axis.Extraversion, -2) }),
            }
        });

        // ===========================================================
        // 8. AMBITION — sets the tone for the title track
        // ===========================================================
        list.Add(new CreationQuestion
        {
            Id = "q_ambition",
            Prompt = "Why are you leaving your village?",
            Answers = new List<CreationAnswer>
            {
                A("rise", "To become someone", "You want more than the life you were handed.",
                  0, 0, 0, 2, new[] { S(Axis.Extraversion, 1), S(Axis.Agreeableness, -1) },
                  "trait_ambitious", new[] { "ambitious" }),

                A("skill", "To learn a trade properly", "Nobody left here can teach you anything.",
                  0, 1, 0, 1, new[] { S(Axis.Openness, 2) },
                  "trait_quick_study", new[] { "quick_study" }),

                A("coin", "To make money", "You have watched what poverty does to people.",
                  0, 1, 0, 1, new[] { S(Axis.Conscientiousness, 1), S(Axis.Agreeableness, -1) },
                  "trait_trade_sense", new[] { "merchant_minded" }),

                A("return", "To come back and fix this place", "Someone has to, and nobody else is offering.",
                  1, 0, 1, 0, new[] { S(Axis.Agreeableness, 2), S(Axis.Conscientiousness, 1) },
                  "trait_kind_unyielding", new[] { "homebound" }),
            }
        });

        return list;
    }

    public static CreationQuestion GetQuestion(string id)
        => Questions.Find(q => q.Id == id);

    public static CreationAnswer GetAnswer(string questionId, string answerId)
        => GetQuestion(questionId)?.Answers.Find(a => a.Id == answerId);

    // ===============================================================
    // Profile -> traits
    // ===============================================================

    /// <summary>
    /// Turns the five scores into traits. Thresholds are deliberately not
    /// symmetric: a strong score in either direction says something, a middling
    /// one says nothing and grants nothing. Most characters come out with three
    /// or four traits, not eight.
    /// </summary>
    public static List<string> DeriveTraits(Dictionary<Axis, int> profile)
    {
        var traits = new List<string>();

        int Get(Axis a) => profile.TryGetValue(a, out int v) ? v : 0;

        const int High = 3;
        const int Low = -2;

        // Conscientiousness
        if (Get(Axis.Conscientiousness) >= High + 1) traits.Add("trait_iron_routine");
        else if (Get(Axis.Conscientiousness) >= High) traits.Add("trait_patient_worker");
        else if (Get(Axis.Conscientiousness) <= Low) traits.Add("trait_risk_seeker");

        // Agreeableness
        if (Get(Axis.Agreeableness) >= High + 1) traits.Add("trait_kind_unyielding");
        else if (Get(Axis.Agreeableness) >= High) traits.Add("trait_hidden_mercy");
        else if (Get(Axis.Agreeableness) <= Low - 1) traits.Add("trait_cold_pragmatist");
        else if (Get(Axis.Agreeableness) <= Low) traits.Add("trait_proud");

        // Neuroticism — low is the desirable end, so it reads inverted
        if (Get(Axis.Neuroticism) <= Low) traits.Add("trait_calm_mind");
        else if (Get(Axis.Neuroticism) >= High) traits.Add("trait_brooding");

        // Openness
        if (Get(Axis.Openness) >= High) traits.Add("trait_quick_study");
        else if (Get(Axis.Openness) <= Low) traits.Add("trait_camp_discipline");

        // Extraversion
        if (Get(Axis.Extraversion) >= High) traits.Add("trait_common_folk_ease");
        else if (Get(Axis.Extraversion) <= Low) traits.Add("trait_scrapwise");

        return traits;
    }

    /// <summary>A short line describing the profile, for the summary screen.</summary>
    public static string DescribeProfile(Dictionary<Axis, int> profile)
    {
        int Get(Axis a) => profile.TryGetValue(a, out int v) ? v : 0;

        var parts = new List<string>();

        if (Get(Axis.Conscientiousness) >= 3) parts.Add("methodical");
        else if (Get(Axis.Conscientiousness) <= -2) parts.Add("impulsive");

        if (Get(Axis.Agreeableness) >= 3) parts.Add("warm");
        else if (Get(Axis.Agreeableness) <= -2) parts.Add("guarded");

        if (Get(Axis.Neuroticism) <= -2) parts.Add("steady");
        else if (Get(Axis.Neuroticism) >= 3) parts.Add("restless");

        if (Get(Axis.Openness) >= 3) parts.Add("curious");
        else if (Get(Axis.Openness) <= -2) parts.Add("settled");

        if (Get(Axis.Extraversion) >= 3) parts.Add("outgoing");
        else if (Get(Axis.Extraversion) <= -2) parts.Add("solitary");

        if (parts.Count == 0)
            return "Hard to read, and probably comfortable that way.";

        return "A " + string.Join(", ", parts) + " sort.";
    }

    public static string AxisLabel(Axis a)
    {
        switch (a)
        {
            case Axis.Conscientiousness: return "Discipline";
            case Axis.Agreeableness:     return "Warmth";
            case Axis.Neuroticism:       return "Volatility";
            case Axis.Openness:          return "Curiosity";
            case Axis.Extraversion:      return "Sociability";
            default:                     return a.ToString();
        }
    }
}
