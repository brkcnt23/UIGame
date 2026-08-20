using System;
using System.Collections.Generic;

/// <summary>
/// One choice inside a character creation scenario.
/// No choice is "correct": each one just leans towards different Big Five dimensions.
/// </summary>
[Serializable]
public class PersonalityChoice
{
    public string Text;
    public BigFiveScores Scores;

    public PersonalityChoice(string text, BigFiveScores scores)
    {
        Text = text;
        Scores = scores;
    }
}

/// <summary>
/// A scene from the player's past. The player answers 8 of these before the game starts.
/// </summary>
[Serializable]
public class PersonalityScenario
{
    public string Title;
    public string Story;
    public List<PersonalityChoice> Choices = new List<PersonalityChoice>();

    public PersonalityScenario(string title, string story, params PersonalityChoice[] choices)
    {
        Title = title;
        Story = story;
        Choices = new List<PersonalityChoice>(choices);
    }
}
