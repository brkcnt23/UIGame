using System.Collections.Generic;

/// <summary>
/// The eight scenes from the player's past that open a new game.
/// They are written in first person, they never judge a choice, and every choice
/// is a valid way to live: the answers only describe who the character already is.
///
/// Scoring: C = Conscientiousness, E = Extraversion, A = Agreeableness,
/// O = Openness, S = Stability (reverse scored Neuroticism).
/// </summary>
public static class PersonalityScenarioCatalog
{
    private static List<PersonalityScenario> _scenarios;

    public static IReadOnlyList<PersonalityScenario> Scenarios
    {
        get
        {
            if (_scenarios == null)
                _scenarios = Build();

            return _scenarios;
        }
    }

    public static int Count
    {
        get { return Scenarios.Count; }
    }

    private static BigFiveScores S(int c, int e, int a, int o, int s)
    {
        return new BigFiveScores(c, e, a, o, s);
    }

    private static List<PersonalityScenario> Build()
    {
        return new List<PersonalityScenario>
        {
            new PersonalityScenario(
                "The Morning Order",
                "Before first light my father names the day's work: the wood, the water, the field. " +
                "He does not ask whether I agree. This morning he names all three at once.",
                new PersonalityChoice("I say nothing and start with the wood.",                    S(2, 0, 1, 0, 0)),
                new PersonalityChoice("I ask him which of the three matters most today.",          S(1, 0, 0, 2, 0)),
                new PersonalityChoice("I tell him it cannot all be done, and choose my own order.", S(1, 2, -1, 0, 0)),
                new PersonalityChoice("I slip out the back door before he finishes speaking.",     S(-1, 0, 0, 0, 2))),

            new PersonalityScenario(
                "The Grey Mare",
                "A buyer wants the grey mare. She is sound to look at, but she goes lame after half a day's walk. " +
                "He has already counted the silver into his hand.",
                new PersonalityChoice("I take the silver and say nothing.",                        S(0, 0, -2, 0, 2)),
                new PersonalityChoice("I mention that she tires early, and let him decide.",       S(2, 0, 1, 0, 0)),
                new PersonalityChoice("I refuse the sale and tell him exactly what is wrong.",     S(0, 1, 2, 0, 0)),
                new PersonalityChoice("I sell her cheaper, for the work she can still do.",        S(0, 0, 1, 2, 0))),

            new PersonalityScenario(
                "The Bigger Boy",
                "A boy a head taller than me takes the bread out of my hands in the square. " +
                "Three others stand close enough to watch what I do next.",
                new PersonalityChoice("I leave, and find something else to eat.",                  S(0, 0, 1, 0, 2)),
                new PersonalityChoice("I keep talking to him until he hands it back.",             S(0, 1, 2, 0, 0)),
                new PersonalityChoice("I hit him, bread or no bread.",                             S(0, 2, -1, 0, -1)),
                new PersonalityChoice("I stand exactly where I am and refuse to move.",            S(2, 0, 0, 0, 1))),

            new PersonalityScenario(
                "The Ledger",
                "The landlord's ledger says we owe two months. We owe one — I remember the day we paid. " +
                "He is not a patient man, and the page is in his hand.",
                new PersonalityChoice("I pay the second month and keep the peace.",                S(-1, 0, 2, 0, 0)),
                new PersonalityChoice("I ask him, carefully, to read the page again.",             S(0, 0, 1, 0, 2)),
                new PersonalityChoice("I tell him plainly that his ledger is wrong.",              S(1, 2, 0, 0, 0)),
                new PersonalityChoice("I come back with the miller, who watched us pay.",          S(2, 0, 0, 1, 0))),

            new PersonalityScenario(
                "The Man at the Roadside",
                "A stranger lies by the road with a broken leg and a full purse at his belt. " +
                "The last village is an hour behind me, and the light is going.",
                new PersonalityChoice("I walk on. The road is no place to be after dark.",         S(0, 0, -2, 0, 2)),
                new PersonalityChoice("I go back to the village and send someone with a cart.",    S(2, 0, 1, 0, 0)),
                new PersonalityChoice("I splint the leg myself and stay with him until morning.",  S(0, 0, 2, 1, 0)),
                new PersonalityChoice("I help him, and take a fair coin from the purse for it.",   S(0, 1, -1, 2, 0))),

            new PersonalityScenario(
                "The Low Road",
                "My master turns the cart onto the low road. I have walked it in this season: it floods, " +
                "and the wheels will sink to the axle. He has not asked what I think.",
                new PersonalityChoice("I follow. He has driven this cart longer than I have lived.", S(0, 0, 2, 0, 1)),
                new PersonalityChoice("I ask whether the low road still holds this time of year.",  S(0, 0, 1, 2, 0)),
                new PersonalityChoice("I say the road will not hold, and I stop walking.",          S(1, 2, 0, 0, 0)),
                new PersonalityChoice("I show him the water line last spring left on the trees.",   S(2, 0, 0, 1, 0))),

            new PersonalityScenario(
                "The Sound Outside",
                "Something moves outside the barn — heavy, slow, and close. " +
                "The lamp is in here with me, and the door is not barred.",
                new PersonalityChoice("I stay still and listen until it passes.",                   S(1, 0, 0, 0, 2)),
                new PersonalityChoice("I call out and ask who is there.",                           S(0, 2, 1, 0, 0)),
                new PersonalityChoice("I run for the house without looking back.",                  S(0, 0, 0, 1, -2)),
                new PersonalityChoice("I take the pitchfork, then bar the door.",                   S(2, 0, 0, 0, 1))),

            new PersonalityScenario(
                "The Same Trick",
                "Last winter I sold river stones as hearth stones, and no one ever knew. " +
                "Winter is here again, and the same buyers stand in the same square.",
                new PersonalityChoice("I do it again. It worked once.",                             S(1, 0, -2, 0, 1)),
                new PersonalityChoice("I walk away and look for other work.",                       S(0, 0, 1, 0, 2)),
                new PersonalityChoice("I find those buyers and tell them what I sold them.",        S(0, 1, 2, 0, 0)),
                new PersonalityChoice("I learn where real hearth stone is cut, and sell that.",     S(1, 0, 0, 2, 0)))
        };
    }
}
