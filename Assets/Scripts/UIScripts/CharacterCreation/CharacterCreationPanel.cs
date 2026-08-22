using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The character creation screen.
///
/// It knows nothing about what the questions measure. It asks
/// CharacterCreationSystem what to show, reports back which answer was tapped,
/// and shows the finished character at the end. The scoring, the trait
/// derivation and the stat changes all live in the system, so this panel can be
/// rebuilt or thrown away without touching the measure itself.
///
/// Sits between naming the character and entering the world: GameManager builds
/// the PlayerData first, the answers modify it, and only then is the save
/// written and the game loaded.
/// </summary>
public class CharacterCreationPanel : MonoBehaviour
{
    [Header("Question")]
    [SerializeField] private GameObject questionRoot;
    [SerializeField] private TMP_Text progressLabel;
    [SerializeField] private TMP_Text promptLabel;
    [SerializeField] private RectTransform answerContainer;
    [SerializeField] private CreationAnswerView answerTemplate;
    [SerializeField] private ScrollRect answerScroll;
    [SerializeField] private Button backButton;

    [Header("Summary")]
    [SerializeField] private GameObject summaryRoot;
    [SerializeField] private TMP_Text summaryNameLabel;
    [SerializeField] private TMP_Text summaryStatsLabel;
    [SerializeField] private TMP_Text summaryProfileLabel;
    [SerializeField] private TMP_Text summaryTraitsLabel;
    [SerializeField] private Button continueButton;

    [Header("Development")]
    [Tooltip("Optional. Answers everything at random and jumps to the summary.")]
    [SerializeField] private Button skipButton;

    private readonly List<CreationAnswerView> _spawned = new();
    private bool _subscribed;

    // -----------------------------------------------------------------
    // Lifecycle
    // -----------------------------------------------------------------

    private void OnEnable()
    {
        ResolveMissingReferences();

        var system = CharacterCreationSystem.Instance;

        if (system == null)
        {
            // Without the system there is nothing to ask. Rather than strand the
            // player on a dead screen, hand straight back to the game and say
            // loudly what is missing.
            Debug.LogError("CharacterCreationPanel: CharacterCreationSystem is not in the scene. " +
                           "Add it to a GameObject so GameBootstrapper can initialise it. Skipping creation.");
            Finish();
            return;
        }

        if (answerTemplate != null)
            answerTemplate.gameObject.SetActive(false);

        WireButtons();
        Subscribe(system);

        if (summaryRoot != null) summaryRoot.SetActive(false);
        if (questionRoot != null) questionRoot.SetActive(true);

        system.Begin();
    }

    private void OnDisable()
    {
        var system = CharacterCreationSystem.Instance;

        if (system != null && _subscribed)
        {
            system.OnQuestionChanged -= ShowQuestion;
            system.OnCompleted -= ShowSummary;
        }

        _subscribed = false;
    }

    private void Subscribe(CharacterCreationSystem system)
    {
        if (_subscribed) return;

        system.OnQuestionChanged += ShowQuestion;
        system.OnCompleted += ShowSummary;
        _subscribed = true;
    }

    private void WireButtons()
    {
        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(GoBack);
        }

        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(Finish);
        }

        if (skipButton != null)
        {
            skipButton.onClick.RemoveAllListeners();
            skipButton.onClick.AddListener(SkipAll);
        }
    }

    // -----------------------------------------------------------------
    // Questions
    // -----------------------------------------------------------------

    private void ShowQuestion(CreationQuestion question)
    {
        var system = CharacterCreationSystem.Instance;

        if (question == null || system == null)
            return;

        if (questionRoot != null) questionRoot.SetActive(true);
        if (summaryRoot != null) summaryRoot.SetActive(false);

        if (progressLabel != null)
            progressLabel.text = $"Your Story   {system.CurrentIndex + 1} / {system.QuestionCount}";

        if (promptLabel != null)
            promptLabel.text = question.Prompt;

        if (backButton != null)
            backButton.gameObject.SetActive(system.CurrentIndex > 0);

        BuildAnswers(question);

        // A new question always starts at the top, otherwise the previous
        // scroll position can hide the first option.
        if (answerScroll != null)
        {
            Canvas.ForceUpdateCanvases();
            answerScroll.verticalNormalizedPosition = 1f;
        }
    }

    private void BuildAnswers(CreationQuestion question)
    {
        if (answerContainer == null || answerTemplate == null)
        {
            Debug.LogWarning("CharacterCreationPanel: answer container or template is not assigned.");
            return;
        }

        // Views are pooled rather than destroyed and rebuilt. Every question has
        // four options except the origin one, so the pool settles immediately.
        for (int i = 0; i < question.Answers.Count; i++)
        {
            CreationAnswerView view;

            if (i < _spawned.Count)
            {
                view = _spawned[i];
            }
            else
            {
                view = Instantiate(answerTemplate, answerContainer);
                view.name = "Answer" + i;
                _spawned.Add(view);
            }

            view.OnClicked = null;
            view.Bind(question.Answers[i]);
            view.OnClicked = Choose;
        }

        for (int i = question.Answers.Count; i < _spawned.Count; i++)
            _spawned[i].gameObject.SetActive(false);
    }

    private void Choose(CreationAnswer answer)
    {
        if (answer == null) return;

        var system = CharacterCreationSystem.Instance;
        if (system != null) system.Answer(answer.Id);
    }

    private void GoBack()
    {
        var system = CharacterCreationSystem.Instance;
        if (system != null) system.Back();
    }

    private void SkipAll()
    {
        var system = CharacterCreationSystem.Instance;
        if (system != null) system.RandomizeAndApply();
    }

    // -----------------------------------------------------------------
    // Summary
    // -----------------------------------------------------------------

    private void ShowSummary()
    {
        var system = CharacterCreationSystem.Instance;
        var pd = PlayerStatHandler.Instance != null ? PlayerStatHandler.Instance.pd : null;

        if (questionRoot != null) questionRoot.SetActive(false);

        if (summaryRoot == null)
        {
            // No summary screen was built. The character is already applied, so
            // there is nothing left to wait for.
            Finish();
            return;
        }

        summaryRoot.SetActive(true);

        if (summaryNameLabel != null && pd != null)
            summaryNameLabel.text = string.IsNullOrEmpty(pd.VillageName)
                ? pd.Name
                : pd.Name + " of " + pd.VillageName;

        if (summaryStatsLabel != null && pd != null)
            summaryStatsLabel.text =
                $"STR {pd.Strength}    DEX {pd.Dexterity}    CON {pd.Constitution}    CHA {pd.Charisma}";

        if (summaryProfileLabel != null && system != null)
        {
            string description = system.GetProfileSummary();

            summaryProfileLabel.text = string.IsNullOrEmpty(description)
                ? "An even hand, so far."
                : char.ToUpper(description[0]) + description.Substring(1);
        }

        if (summaryTraitsLabel != null)
            summaryTraitsLabel.text = BuildTraitList();
    }

    /// <summary>
    /// Reads back what was actually granted rather than recomputing it, so the
    /// screen cannot disagree with the character.
    /// </summary>
    private string BuildTraitList()
    {
        var traitSystem = TraitSystem.Instance;
        var lines = new List<string>();

        if (traitSystem != null)
        {
            foreach (var active in traitSystem.Active)
            {
                if (active == null || string.IsNullOrEmpty(active.traitId))
                    continue;

                var so = traitSystem.Database != null
                    ? traitSystem.Database.Get(active.traitId)
                    : null;

                lines.Add(so != null && !string.IsNullOrEmpty(so.displayName)
                    ? so.displayName
                    : Prettify(active.traitId));
            }
        }

        if (lines.Count == 0)
        {
            // TraitSystem is not in the scene, or the trait assets were never
            // generated. The ids still say who the character became.
            var pd = PlayerStatHandler.Instance != null ? PlayerStatHandler.Instance.pd : null;

            if (pd != null && pd.ActiveTraitTags != null)
                foreach (var tag in pd.ActiveTraitTags)
                    if (tag != null && tag.StartsWith("trait_"))
                        lines.Add(Prettify(tag));
        }

        if (lines.Count == 0)
            return "No traits were granted.";

        var sb = new StringBuilder();

        foreach (var line in lines)
            sb.AppendLine(line);

        return sb.ToString().TrimEnd();
    }

    /// <summary>Turns trait_calm_mind into Calm Mind, for when there is no asset to ask.</summary>
    private static string Prettify(string traitId)
    {
        string body = traitId.StartsWith("trait_") ? traitId.Substring(6) : traitId;
        string[] words = body.Split('_');

        for (int i = 0; i < words.Length; i++)
            if (words[i].Length > 0)
                words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1);

        return string.Join(" ", words);
    }

    // -----------------------------------------------------------------

    /// <summary>Writes the finished character to the save and enters the world.</summary>
    private void Finish()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.FinishNewGame();
            return;
        }

        Debug.LogError("CharacterCreationPanel: GameManager.Instance is null, cannot start the game.");
    }

    // -----------------------------------------------------------------
    // Builder support
    // -----------------------------------------------------------------

    /// <summary>
    /// Fills in whatever was left unassigned by looking the objects up by name.
    ///
    /// This is what lets a generic screen builder produce this panel: the
    /// builder does not have to know which label is the prompt, it only has to
    /// name it "PromptLabel". Anything dragged in by hand wins, so the lookup
    /// never overrides a deliberate choice.
    /// </summary>
    private void ResolveMissingReferences()
    {
        if (questionRoot == null) questionRoot = FindChild("QuestionRoot")?.gameObject;
        if (summaryRoot == null) summaryRoot = FindChild("SummaryRoot")?.gameObject;

        if (progressLabel == null) progressLabel = FindText("ProgressLabel");
        if (promptLabel == null) promptLabel = FindText("PromptLabel");
        if (summaryNameLabel == null) summaryNameLabel = FindText("SummaryName");
        if (summaryStatsLabel == null) summaryStatsLabel = FindText("SummaryStats");
        if (summaryProfileLabel == null) summaryProfileLabel = FindText("SummaryProfile");
        if (summaryTraitsLabel == null) summaryTraitsLabel = FindText("SummaryTraits");

        if (backButton == null) backButton = FindButton("BackButton");
        if (continueButton == null) continueButton = FindButton("ContinueButton");
        if (skipButton == null) skipButton = FindButton("SkipButton");

        if (answerScroll == null)
            answerScroll = GetComponentInChildren<ScrollRect>(true);

        if (answerTemplate == null)
            answerTemplate = GetComponentInChildren<CreationAnswerView>(true);

        if (answerContainer == null && answerScroll != null)
            answerContainer = answerScroll.content;
    }

    private Transform FindChild(string childName)
    {
        foreach (var t in GetComponentsInChildren<Transform>(true))
            if (t != transform && t.name == childName)
                return t;

        return null;
    }

    private TMP_Text FindText(string childName)
    {
        var found = FindChild(childName);
        return found != null ? found.GetComponent<TMP_Text>() : null;
    }

    private Button FindButton(string childName)
    {
        var found = FindChild(childName);
        return found != null ? found.GetComponent<Button>() : null;
    }
}
