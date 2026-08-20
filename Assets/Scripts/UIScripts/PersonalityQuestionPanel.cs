using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The character creation questions: eight scenes from the player's past.
/// The panel builds its own choice buttons, so nothing has to be placed by hand;
/// it only needs a container and a button prefab.
///
/// Flow: GameManager.StartNewGame() -> Begin(callback) -> 8 scenarios ->
/// summary of the resulting trait -> callback with the PersonalityResult.
/// </summary>
public class PersonalityQuestionPanel : MonoBehaviour
{
    [Header("Scenario")]
    public TMP_Text titleText;
    public TMP_Text storyText;
    public TMP_Text progressText;

    [Header("Choices")]
    public Transform choiceContainer;
    public Button choiceButtonPrefab;

    [Header("Navigation")]
    public Button backButton;

    [Header("Result")]
    [Tooltip("Shown after the last scenario. Leave empty to finish immediately.")]
    public GameObject resultPanel;
    public TMP_Text resultTraitText;
    public TMP_Text resultDescriptionText;
    public Button resultContinueButton;

    private readonly List<PersonalityChoice> _answers = new List<PersonalityChoice>();
    private readonly List<Button> _spawnedButtons = new List<Button>();

    private Action<PersonalityResult> _onCompleted;
    private Action _onCancelled;
    private int _index;

    /// <summary>
    /// Starts the questions from the beginning.
    /// <paramref name="onCancelled"/> is called when the player backs out of the first scenario.
    /// </summary>
    public void Begin(Action<PersonalityResult> onCompleted, Action onCancelled = null)
    {
        _onCompleted = onCompleted;
        _onCancelled = onCancelled;
        _answers.Clear();
        _index = 0;

        if (resultPanel != null)
            resultPanel.SetActive(false);

        gameObject.SetActive(true);
        ShowCurrentScenario();
    }

    private void ShowCurrentScenario()
    {
        if (choiceContainer == null || choiceButtonPrefab == null)
        {
            Debug.LogError("PersonalityQuestionPanel: choiceContainer or choiceButtonPrefab is not assigned.");
            Complete();
            return;
        }

        if (_index < 0)
            _index = 0;

        if (_index >= PersonalityScenarioCatalog.Count)
        {
            Complete();
            return;
        }

        PersonalityScenario scenario = PersonalityScenarioCatalog.Scenarios[_index];

        if (titleText != null)
            titleText.text = scenario.Title;

        if (storyText != null)
            storyText.text = scenario.Story;

        if (progressText != null)
            progressText.text = $"Your Story: {_index + 1}/{PersonalityScenarioCatalog.Count}";

        BuildChoiceButtons(scenario);

        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(GoBack);
        }
    }

    private void BuildChoiceButtons(PersonalityScenario scenario)
    {
        ClearChoiceButtons();

        for (int i = 0; i < scenario.Choices.Count; i++)
        {
            PersonalityChoice choice = scenario.Choices[i];

            if (choice == null)
                continue;

            Button button = Instantiate(choiceButtonPrefab, choiceContainer);
            button.gameObject.SetActive(true);

            TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);

            if (label != null)
                label.text = choice.Text;

            PersonalityChoice captured = choice;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => Answer(captured));

            _spawnedButtons.Add(button);
        }
    }

    private void ClearChoiceButtons()
    {
        foreach (Button button in _spawnedButtons)
        {
            if (button != null)
                Destroy(button.gameObject);
        }

        _spawnedButtons.Clear();
    }

    private void Answer(PersonalityChoice choice)
    {
        // Answering again after going back overwrites the old answer.
        if (_index < _answers.Count)
            _answers[_index] = choice;
        else
            _answers.Add(choice);

        _index++;
        ShowCurrentScenario();
    }

    public void GoBack()
    {
        if (_index == 0)
        {
            ClearChoiceButtons();
            gameObject.SetActive(false);

            if (_onCancelled != null)
                _onCancelled();

            return;
        }

        _index--;
        ShowCurrentScenario();
    }

    private void Complete()
    {
        ClearChoiceButtons();

        PersonalityResult result = PersonalityResolver.Resolve(_answers);

        if (resultPanel == null)
        {
            Finish(result);
            return;
        }

        resultPanel.SetActive(true);

        if (progressText != null)
            progressText.text = $"Your Story: {PersonalityScenarioCatalog.Count}/{PersonalityScenarioCatalog.Count}";

        PersonalityTraitDefinition trait = result.Trait;

        if (resultTraitText != null)
            resultTraitText.text = result.TraitDisplayName;

        if (resultDescriptionText != null)
            resultDescriptionText.text = trait != null ? trait.Description : "";

        if (resultContinueButton != null)
        {
            resultContinueButton.onClick.RemoveAllListeners();
            resultContinueButton.onClick.AddListener(() => Finish(result));
        }
        else
        {
            Finish(result);
        }
    }

    private void Finish(PersonalityResult result)
    {
        if (resultPanel != null)
            resultPanel.SetActive(false);

        gameObject.SetActive(false);

        Action<PersonalityResult> callback = _onCompleted;
        _onCompleted = null;

        if (callback != null)
            callback(result);
    }
}
