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

        // Each answer moves one axis by two and a second by one. Eight questions
        // reach every axis to the same distance — plus or minus five — which is
        // what lets the three strongest be compared at the end. An axis touched
        // by fewer questions would simply never win.
        //
        // Which option carries which score is shuffled on purpose. A player who
        // notices that the top answer is always the flattering one stops
        // answering and starts optimising, and the measure is over.

        // ===========================================================
        // 1. CONSCIENTIOUSNESS — with composure underneath
        // ===========================================================
        list.Add(new CreationQuestion
        {
            Id = "q_journey",
            Prompt = "You leave on a long road at first light.",
            Answers = new List<CreationAnswer>
            {
                A("basics", "Pack the basics, think on the road", "It usually works out.",
                  0, 0, 0, 0, new[] { S(Axis.Conscientiousness, 0) }),

                A("prepared", "Rations, route, spare blade, and where you sleep each night",
                  "Nothing on that road will be a surprise.",
                  0, 0, 1, 0, new[] { S(Axis.Conscientiousness, 2), S(Axis.Neuroticism, -1) }),

                A("improvise", "Morning will show you what you need",
                  "Carrying half of it back is worse than lacking it.",
                  0, 1, 0, 0, new[] { S(Axis.Conscientiousness, -2), S(Axis.Openness, 1) }),
            }
        });

        // ===========================================================
        // 2. OPENNESS — at the forge
        // ===========================================================
        list.Add(new CreationQuestion
        {
            Id = "q_forge",
            Prompt = "A master smith shows you a way of working iron that nobody has tried.",
            Answers = new List<CreationAnswer>
            {
                A("old_way", "The old way has held for centuries",
                  "It has buried better men than him.",
                  0, 0, 0, 0, new[] { S(Axis.Openness, -2), S(Axis.Conscientiousness, 1) }),

                A("if_it_works", "If it works, tradition can move", "Nobody eats a custom.",
                  0, 0, 0, 0, new[] { S(Axis.Openness, 2) }),

                A("small_piece", "Try it on a small piece first", "Then argue about it.",
                  0, 1, 0, 0, new[] { S(Axis.Openness, 0), S(Axis.Conscientiousness, 1) }),
            }
        });

        // ===========================================================
        // 3. EXTRAVERSION — and what you extend to strangers
        // ===========================================================
        list.Add(new CreationQuestion
        {
            Id = "q_traders",
            Prompt = "Traders from a far country take the long table. You barely have their language.",
            Answers = new List<CreationAnswer>
            {
                A("sit_down", "Sit with them and find a way to talk",
                  "Hands and bread will carry most of it.",
                  0, 0, 0, 1, new[] { S(Axis.Extraversion, 2), S(Axis.Agreeableness, 1) }),

                A("let_them_come", "You would rather they came to you", "They usually do.",
                  0, 0, 0, 0, new[] { S(Axis.Extraversion, -2) }),

                A("if_chance", "If the chance comes, you will speak", "No need to make one.",
                  0, 0, 0, 0, new[] { S(Axis.Extraversion, 0) }),
            }
        });

        // ===========================================================
        // 4. NEUROTICISM — when the plan is gone
        // ===========================================================
        list.Add(new CreationQuestion
        {
            Id = "q_plan_broke",
            Prompt = "The enemy comes from a direction nobody watched. The plan is gone.",
            Answers = new List<CreationAnswer>
            {
                A("few_seconds", "Take a few seconds to understand what happened", "Then move.",
                  0, 0, 0, 0, new[] { S(Axis.Neuroticism, 0) }),

                A("lose_focus", "When it slips out of hand, holding focus is the hard part",
                  "You have seen it happen to steadier men.",
                  0, 0, 0, 0, new[] { S(Axis.Neuroticism, 2) }),

                A("new_plan", "Find a new plan. Panic comes afterwards, if there is time",
                  "There usually is not.",
                  0, 0, 1, 0, new[] { S(Axis.Neuroticism, -2), S(Axis.Conscientiousness, 1) }),
            }
        });

        // ===========================================================
        // 5. AGREEABLENESS — toward someone who has earned nothing
        // ===========================================================
        list.Add(new CreationQuestion
        {
            Id = "q_wounded",
            Prompt = "A wounded man lies by the road. They say he robbed a caravan this morning.",
            Answers = new List<CreationAnswer>
            {
                A("his_choice", "He can live with what he chose", "Or not.",
                  0, 0, 0, 0, new[] { S(Axis.Agreeableness, -2) }),

                A("bind_first", "Bind the wound first, the reckoning comes after",
                  "A dead man answers no questions either.",
                  0, 0, 0, 1, new[] { S(Axis.Agreeableness, 2), S(Axis.Extraversion, 1) }),

                A("to_the_guard", "Hand him to the guard", "Let the judgement be somebody else's.",
                  0, 0, 0, 0, new[] { S(Axis.Agreeableness, 0) }),
            }
        });

        // ===========================================================
        // 6. AGREEABLENESS — when you are the one owed
        // ===========================================================
        list.Add(new CreationQuestion
        {
            Id = "q_debtor",
            Prompt = "A farmer who owes you says the harvest failed and he cannot pay.",
            Answers = new List<CreationAnswer>
            {
                A("part_now", "Part now, the rest after the next harvest", "Written down, this time.",
                  0, 0, 0, 0, new[] { S(Axis.Agreeableness, 0), S(Axis.Conscientiousness, 1) }),

                A("debt_is_debt", "A debt is a debt. A word given is kept",
                  "Mercy this year is a queue at your door the next.",
                  0, 0, 0, 0, new[] { S(Axis.Agreeableness, -2), S(Axis.Conscientiousness, 1) }),

                A("give_time", "Give him time", "He will remember which of you did.",
                  0, 0, 0, 0, new[] { S(Axis.Agreeableness, 2) }),
            }
        });

        // ===========================================================
        // 7. OPENNESS — in front of something shut
        // ===========================================================
        list.Add(new CreationQuestion
        {
            Id = "q_old_door",
            Prompt = "Deep in the wood stands an old door, marked in a hand you do not know.",
            Answers = new List<CreationAnswer>
            {
                A("must_know", "You do not leave without knowing what is behind it",
                  "You would think about it for years.",
                  0, 1, 0, 0, new[] { S(Axis.Openness, 2) }),

                A("shut_for_reason", "Some doors are shut for a reason", "That is reason enough.",
                  0, 0, 0, 0, new[] { S(Axis.Openness, -2) }),

                A("read_ground", "Read the ground around it first",
                  "Whoever marked it may still come back.",
                  0, 0, 0, 0, new[] { S(Axis.Openness, 0), S(Axis.Extraversion, -1) }),
            }
        });

        // ===========================================================
        // 8. NEUROTICISM — and what an insult costs you
        // ===========================================================
        list.Add(new CreationQuestion
        {
            Id = "q_slighted",
            Prompt = "A noble makes you small in front of a full hall.",
            Answers = new List<CreationAnswer>
            {
                A("carry_it", "You carry it a long while", "It comes back at odd hours.",
                  0, 0, 0, 0, new[] { S(Axis.Neuroticism, 2), S(Axis.Extraversion, -1) }),

                A("words_only", "Words do not move you much",
                  "He will need more than a room to work with.",
                  0, 0, 1, 0, new[] { S(Axis.Neuroticism, -2) }),

                A("keep_hold", "You will not forget it, but you keep hold of yourself",
                  "There is a better hour for it.",
                  0, 0, 0, 1, new[] { S(Axis.Neuroticism, 0), S(Axis.Extraversion, 1) }),
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
    /// How far an axis could possibly be pushed, given the questions as written.
    ///
    /// Measured rather than written down, because the ranking below compares
    /// axes against each other: an axis two questions touch would beat one that
    /// only one question touches, every single time, and the loser would never
    /// grant a trait no matter how the player answered. Reading the reach off
    /// the questions means editing a question cannot quietly break that.
    /// </summary>
    public static int Reach(Axis axis)
    {
        int total = 0;

        foreach (var question in Questions)
        {
            int strongest = 0;

            foreach (var answer in question.Answers)
                foreach (var shift in answer.Shifts)
                    if (shift.Axis == axis)
                        strongest = Mathf.Max(strongest, Mathf.Abs(shift.Value));

            total += strongest;
        }

        return Mathf.Max(1, total);
    }

    /// <summary>
    /// The three axes the answers spoke loudest on, as traits.
    ///
    /// Ranked rather than thresholded. A threshold hands one player five traits
    /// and another none, which makes two characters hard to compare and makes a
    /// cautious set of answers feel like a punishment. Everyone leaves creation
    /// with the same three slots filled; what differs is by whom.
    ///
    /// Scores are divided by their own reach first, so an axis is judged on how
    /// far it was pushed of what was available to it, not on its raw total.
    ///
    /// An axis sitting exactly on zero says nothing and is skipped, so a player
    /// who takes the middle option eight times can come away with fewer than
    /// three. That is the honest reading of those answers.
    /// </summary>
    public static List<string> DeriveTraits(Dictionary<Axis, int> profile)
    {
        const int Wanted = 3;

        int Get(Axis a) => profile != null && profile.TryGetValue(a, out int v) ? v : 0;

        var scored = new List<(Axis Axis, int Score, float Strength)>();

        foreach (Axis axis in System.Enum.GetValues(typeof(Axis)))
        {
            int score = Get(axis);

            if (score == 0)
                continue;

            scored.Add((axis, score, Mathf.Abs(score) / (float)Reach(axis)));
        }

        // Enum order breaks ties, so the same answers always give the same three.
        scored.Sort((x, y) =>
        {
            int byStrength = y.Strength.CompareTo(x.Strength);
            return byStrength != 0 ? byStrength : ((int)x.Axis).CompareTo((int)y.Axis);
        });

        var traits = new List<string>();

        foreach (var entry in scored)
        {
            if (traits.Count >= Wanted)
                break;

            string trait = TraitFor(entry.Axis, entry.Score, entry.Strength);

            if (!string.IsNullOrEmpty(trait) && !traits.Contains(trait))
                traits.Add(trait);
        }

        return traits;
    }

    /// <summary>
    /// Which trait an axis grants, by direction and by how hard it was pushed.
    ///
    /// Both ends grant something. A low score is not a failed high one — a
    /// character who keeps to himself is Scrapwise, not a worse version of
    /// Common Folk Ease.
    /// </summary>
    private static string TraitFor(Axis axis, int score, float strength)
    {
        bool strong = strength >= 0.6f;

        switch (axis)
        {
            case Axis.Conscientiousness:
                return score > 0
                    ? (strong ? "trait_iron_routine" : "trait_patient_worker")
                    : "trait_risk_seeker";

            case Axis.Agreeableness:
                return score > 0
                    ? (strong ? "trait_kind_unyielding" : "trait_hidden_mercy")
                    : (strong ? "trait_cold_pragmatist" : "trait_proud");

            case Axis.Neuroticism:
                return score > 0 ? "trait_brooding" : "trait_calm_mind";

            case Axis.Openness:
                return score > 0 ? "trait_quick_study" : "trait_camp_discipline";

            case Axis.Extraversion:
                return score > 0 ? "trait_common_folk_ease" : "trait_scrapwise";

            default:
                return null;
        }
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
