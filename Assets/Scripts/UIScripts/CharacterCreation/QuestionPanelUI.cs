using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Drives the hand-built question panel.
///
/// Holds no opinion about what the questions measure. It asks
/// CharacterCreationSystem what to show and reports back which answer was
/// tapped; the scoring, the trait derivation and the stat changes all live
/// there, so the wording of a question can change without this file moving.
///
/// The four buttons already in the panel are the pool. A question with more
/// answers than that gets more made from the prefab, because the origin
/// question has eight and a panel built for four would silently drop half of
/// them - the player would never know the options existed.
/// </summary>
public class QuestionPanelUI : MonoBehaviour
{
    [Header("Question")]
    [SerializeField] private TMP_Text questionLabel;

    [Header("Answers")]
    [Tooltip("The object holding the answer buttons. Its own children are used first.")]
    [SerializeField] private Transform answerPanel;

    [Tooltip("Cloned when a question has more answers than the panel has buttons.")]
    [SerializeField] private Button answerPrefab;

    [Header("Progress")]
    [Tooltip("Optional. Reads 'Your Story  3 / 9'.")]
    [SerializeField] private TMP_Text progressLabel;

    [Tooltip("Optional. Hidden on the first question.")]
    [SerializeField] private Button backButton;

    [Header("Development")]
    [Tooltip("Optional. Answers everything at random and finishes.")]
    [SerializeField] private Button skipButton;

    private readonly List<Button> _buttons = new();
    private readonly List<TMP_Text> _labels = new();
    private bool _subscribed;

    // -----------------------------------------------------------------

    private void OnEnable()
    {
        CollectButtons();
        WireButtons();

        var system = CharacterCreationSystem.Instance;

        if (system == null)
        {
            Debug.LogError("QuestionPanelUI: CharacterCreationSystem is not in the scene. " +
                           "Tools > UIGame > Systems > Add every missing system.");
            return;
        }

        Subscribe(system);
        system.Begin();
    }

    private void OnDisable()
    {
        var system = CharacterCreationSystem.Instance;

        if (system != null && _subscribed)
        {
            system.OnQuestionChanged -= Show;
            system.OnCompleted -= Finish;
        }

        _subscribed = false;
    }

    private void Subscribe(CharacterCreationSystem system)
    {
        if (_subscribed) return;

        system.OnQuestionChanged += Show;
        system.OnCompleted += Finish;
        _subscribed = true;
    }

    /// <summary>
    /// Buttons already sitting in the panel become the pool, in sibling order,
    /// so the layout that was arranged by hand is the layout that gets used.
    /// </summary>
    private void CollectButtons()
    {
        if (_buttons.Count > 0 || answerPanel == null)
            return;

        foreach (Transform child in answerPanel)
        {
            var button = child.GetComponent<Button>();

            if (button == null)
                continue;

            _buttons.Add(button);
            _labels.Add(button.GetComponentInChildren<TMP_Text>(true));
        }

        if (_buttons.Count == 0)
            Debug.LogWarning("QuestionPanelUI: no answer buttons found under " + answerPanel.name + ".");
    }

    private void WireButtons()
    {
        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(() => CharacterCreationSystem.Instance?.Back());
        }

        if (skipButton != null)
        {
            skipButton.onClick.RemoveAllListeners();
            skipButton.onClick.AddListener(() => CharacterCreationSystem.Instance?.RandomizeAndApply());
        }
    }

    // -----------------------------------------------------------------

    private void Show(CreationQuestion question)
    {
        var system = CharacterCreationSystem.Instance;

        if (question == null || system == null)
            return;

        if (questionLabel != null)
            questionLabel.text = question.Prompt;

        if (progressLabel != null)
            progressLabel.text = $"Your Story   {system.CurrentIndex + 1} / {system.QuestionCount}";

        if (backButton != null)
            backButton.gameObject.SetActive(system.CurrentIndex > 0);

        Grow(question.Answers.Count);

        for (int i = 0; i < _buttons.Count; i++)
        {
            bool used = i < question.Answers.Count;

            _buttons[i].gameObject.SetActive(used);

            if (!used)
                continue;

            var answer = question.Answers[i];

            if (i < _labels.Count && _labels[i] != null)
                _labels[i].text = answer.Text;

            // Rebound every question. The listener closes over this answer, so a
            // stale one from the previous question would submit the wrong id.
            _buttons[i].onClick.RemoveAllListeners();
            _buttons[i].onClick.AddListener(() => CharacterCreationSystem.Instance?.Answer(answer.Id));
        }
    }

    /// <summary>Makes up the shortfall from the prefab, once.</summary>
    private void Grow(int needed)
    {
        if (_buttons.Count >= needed || answerPanel == null)
            return;

        if (answerPrefab == null)
        {
            Debug.LogWarning($"QuestionPanelUI: this question has {needed} answers but the panel " +
                             $"has {_buttons.Count} buttons and no prefab to make more. " +
                             "The extra answers will not be shown.");
            return;
        }

        while (_buttons.Count < needed)
        {
            var button = Instantiate(answerPrefab, answerPanel);
            button.name = "AnswerButton (" + _buttons.Count + ")";

            _buttons.Add(button);
            _labels.Add(button.GetComponentInChildren<TMP_Text>(true));
        }
    }

    private void Finish()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.FinishNewGame();
            return;
        }

        Debug.LogError("QuestionPanelUI: GameManager.Instance is null, cannot start the game.");
    }
}
