using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The naming screen.
///
/// Typing a name is the least interesting thing a player does, so the screen
/// answers it with the three ranks that name could carry: seat of a village,
/// then of a town, then of a city. The names go straight into the titles as they
/// are typed, which is what turns "enter a name" into "choose what you will be
/// called".
///
/// The ranks are the real ones from TitleLadder, not decoration — Bailiff, Baron
/// and Duke are the milestones the player actually climbs to.
/// </summary>
public class NewGameSetupUI : MonoBehaviour
{
    [Header("Inputs")]
    public TMP_InputField playerNameInput;
    public TMP_InputField villageNameInput;

    [Header("Title Preview")]
    [Tooltip("Up to three labels. They fill with the village, town and city ranks.")]
    public TMP_Text[] titleLines;

    [Tooltip("Optional. Says which settlement each rank above is the seat of.")]
    public TMP_Text[] tierLines;

    [Header("Buttons")]
    public Button startButton;
    public Button backButton;

    [Header("Feedback")]
    public TMP_Text warningText;

    private const string MissingNameWarning = "Enter a name for your character and your village.";

    /// <summary>
    /// Reeve is left out. It is the seat of a hamlet, which is smaller than the
    /// village the player already starts with, so it reads as a demotion.
    /// </summary>
    private static readonly string[] PreviewTitleIds = { "bailiff", "baron", "duke" };

    private void OnEnable()
    {
        if (playerNameInput != null)
            playerNameInput.onValueChanged.AddListener(OnInputChanged);

        if (villageNameInput != null)
            villageNameInput.onValueChanged.AddListener(OnInputChanged);

        if (backButton != null)
        {
            backButton.onClick.RemoveListener(GoBack);
            backButton.onClick.AddListener(GoBack);
        }

        Refresh();
    }

    private void OnDisable()
    {
        if (playerNameInput != null)
            playerNameInput.onValueChanged.RemoveListener(OnInputChanged);

        if (villageNameInput != null)
            villageNameInput.onValueChanged.RemoveListener(OnInputChanged);

        if (backButton != null)
            backButton.onClick.RemoveListener(GoBack);
    }

    private void OnInputChanged(string _) => Refresh();

    public void Refresh()
    {
        string playerName = playerNameInput != null ? playerNameInput.text.Trim() : "";
        string villageName = villageNameInput != null ? villageNameInput.text.Trim() : "";

        bool ready = playerName.Length > 0 && villageName.Length > 0;

        if (startButton != null)
            startButton.interactable = ready;

        if (warningText != null)
            warningText.text = ready ? "" : MissingNameWarning;

        UpdateTitles(playerName, villageName);
    }

    private void UpdateTitles(string playerName, string villageName)
    {
        for (int i = 0; i < PreviewTitleIds.Length; i++)
        {
            var rung = TitleLadder.ById(PreviewTitleIds[i]);

            SetLine(titleLines, i, rung == null ? "" : rung.Styled(playerName, villageName));

            SetLine(tierLines, i, rung == null
                ? ""
                : "seat of a " + rung.Tier.ToString().ToLowerInvariant());
        }
    }

    private void SetLine(TMP_Text[] lines, int index, string value)
    {
        if (lines == null || index >= lines.Length || lines[index] == null)
            return;

        lines[index].text = value;
    }

    public void GoBack()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.ShowStartGamePanel();
    }
}
