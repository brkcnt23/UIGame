using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Runs the creation questions and turns the answers into a character.
///
/// Deliberately separate from the UI: the panel asks this system what the next
/// question is and reports back which answer was chosen. Swapping the panel
/// later — or skipping it entirely in a test — changes nothing here.
///
/// Applying happens once, at the end, so a player who backs out halfway has
/// not half-modified their character.
/// </summary>
public sealed class CharacterCreationSystem : GameSystemBase
{
    public override int Priority => SystemPriority.PlayerStats - 5;

    public static CharacterCreationSystem Instance { get; private set; }

    [SerializeField] private bool verbose;

    /// <summary>questionId -> answerId, in the order they were answered.</summary>
    private readonly Dictionary<string, string> _answers = new();

    private int _index;

    public System.Action<CreationQuestion> OnQuestionChanged;
    public System.Action OnCompleted;

    public int QuestionCount => CharacterCreation.Questions.Count;
    public int CurrentIndex => _index;
    public bool IsComplete => _index >= QuestionCount;

    public CreationQuestion CurrentQuestion =>
        IsComplete ? null : CharacterCreation.Questions[_index];

    protected override void OnInitialize()
    {
        Instance = this;
    }

    protected override void OnShutdown()
    {
        if (Instance == this)
            Instance = null;
    }

    // -----------------------------------------------------------------
    // Flow
    // -----------------------------------------------------------------

    public void Begin()
    {
        _answers.Clear();
        _index = 0;
        OnQuestionChanged?.Invoke(CurrentQuestion);
    }

    /// <summary>Records the answer and advances. Applies everything at the end.</summary>
    public void Answer(string answerId)
    {
        var q = CurrentQuestion;
        if (q == null) return;

        _answers[q.Id] = answerId;
        _index++;

        if (verbose) Log($"{q.Id} = {answerId}");

        if (IsComplete)
        {
            ApplyAll();
            OnCompleted?.Invoke();
        }
        else
        {
            OnQuestionChanged?.Invoke(CurrentQuestion);
        }
    }

    public void Back()
    {
        if (_index == 0) return;

        _index--;
        OnQuestionChanged?.Invoke(CurrentQuestion);
    }

    /// <summary>
    /// Picks a random answer for every question. Used by the test button so a
    /// developer can reach the game loop without clicking through creation.
    /// </summary>
    public void RandomizeAndApply()
    {
        _answers.Clear();

        foreach (var q in CharacterCreation.Questions)
        {
            if (q.Answers.Count == 0) continue;
            _answers[q.Id] = q.Answers[Random.Range(0, q.Answers.Count)].Id;
        }

        _index = QuestionCount;
        ApplyAll();
        OnCompleted?.Invoke();
    }

    // -----------------------------------------------------------------
    // Application
    // -----------------------------------------------------------------

    /// <summary>The five-factor scores from the answers so far.</summary>
    public Dictionary<Axis, int> Profile { get; private set; } = new();

    private void ApplyAll()
    {
        var pd = PlayerStatHandler.Instance?.pd;
        if (pd == null)
        {
            LogError("No PlayerData. Character creation could not be applied.");
            return;
        }

        var traitSystem = TraitSystem.Instance;
        var tags = new List<string>();

        Profile = new Dictionary<Axis, int>();

        foreach (var kv in _answers)
        {
            var answer = CharacterCreation.GetAnswer(kv.Key, kv.Value);
            if (answer == null) continue;

            pd.Strength     += answer.Strength;
            pd.Dexterity    += answer.Dexterity;
            pd.Constitution += answer.Constitution;
            pd.Charisma     += answer.Charisma;

            foreach (var shift in answer.Shifts)
            {
                Profile.TryGetValue(shift.Axis, out int current);
                Profile[shift.Axis] = current + shift.Value;
            }

            // Origin and ambition grant their trait outright; everything else
            // is decided by the profile below.
            if (!string.IsNullOrEmpty(answer.GrantsTraitId))
            {
                if (traitSystem != null)
                    traitSystem.Grant(answer.GrantsTraitId);
                else
                    tags.Add(answer.GrantsTraitId);
            }

            tags.AddRange(answer.Tags);
        }

        // Personality traits fall out of the five scores, not out of any one
        // answer. Two players can reach Calm Mind by different routes.
        foreach (var traitId in CharacterCreation.DeriveTraits(Profile))
        {
            if (traitSystem != null)
                traitSystem.Grant(traitId);
            else
                tags.Add(traitId);
        }

        // Nothing should be able to leave an attribute below 4 — that is the
        // floor for a functioning adult, and the combat formulas assume it.
        pd.Strength     = Mathf.Max(4, pd.Strength);
        pd.Dexterity    = Mathf.Max(4, pd.Dexterity);
        pd.Constitution = Mathf.Max(4, pd.Constitution);
        pd.Charisma     = Mathf.Max(4, pd.Charisma);

        pd.ActiveTraitTags ??= new List<string>();
        foreach (var t in tags)
            if (!pd.ActiveTraitTags.Contains(t))
                pd.ActiveTraitTags.Add(t);

        Log($"Character created: STR {pd.Strength}  DEX {pd.Dexterity}  " +
            $"CON {pd.Constitution}  CHA {pd.Charisma}");
        Log($"Profile: {CharacterCreation.DescribeProfile(Profile)}");

        foreach (var kv in Profile)
            Log($"  {CharacterCreation.AxisLabel(kv.Key),-12} {kv.Value:+0;-0;0}");

        PlayerStatHandler.Instance.RefreshPlayerUI();
    }

    /// <summary>One line summarising the finished character, for the last screen.</summary>
    public string GetProfileSummary() => CharacterCreation.DescribeProfile(Profile);

    /// <summary>What the player answered, for a summary screen.</summary>
    public IReadOnlyDictionary<string, string> GetAnswers() => _answers;
}
