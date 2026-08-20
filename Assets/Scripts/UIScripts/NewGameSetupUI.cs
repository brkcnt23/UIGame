using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Drives the "new game" input screen: it previews what the player is about to
/// start with and only lets them press start once both names are filled in.
/// Every reference is optional, so the screen keeps working while it is being built.
/// </summary>
public class NewGameSetupUI : MonoBehaviour
{
    [Header("Inputs")]
    public TMP_InputField playerNameInput;
    public TMP_InputField villageNameInput;

    [Header("Character Preview (top 3 labels)")]
    public TMP_Text[] characterLines;

    [Header("Village Preview (bottom 3 labels)")]
    public TMP_Text[] villageLines;

    [Header("Buttons")]
    public Button startButton;
    public Button backButton;

    [Header("Feedback")]
    public TMP_Text warningText;

    private const string MissingNameWarning = "Enter a name for your character and your village.";

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

    private void OnInputChanged(string _)
    {
        Refresh();
    }

    public void Refresh()
    {
        string playerName = playerNameInput != null ? playerNameInput.text.Trim() : "";
        string villageName = villageNameInput != null ? villageNameInput.text.Trim() : "";

        bool ready = playerName.Length > 0 && villageName.Length > 0;

        if (startButton != null)
            startButton.interactable = ready;

        if (warningText != null)
            warningText.text = ready ? "" : MissingNameWarning;

        UpdateCharacterPreview(playerName);
        UpdateVillagePreview(villageName);
    }

    private void UpdateCharacterPreview(string playerName)
    {
        string displayName = playerName.Length > 0 ? playerName : "Nameless wanderer";

        SetLine(characterLines, 0, $"{displayName}  ·  Level {NewGameDefaults.Level}");
        SetLine(characterLines, 1,
            $"STR {NewGameDefaults.StatValue}   DEX {NewGameDefaults.StatValue}   " +
            $"CON {NewGameDefaults.StatValue}   CHA {NewGameDefaults.StatValue}");
        SetLine(characterLines, 2,
            $"Health {NewGameDefaults.Health}  ·  Rations {NewGameDefaults.Rations}  ·  " +
            $"Purse {NewGameDefaults.Gold}g {NewGameDefaults.Silver}s");
    }

    private void UpdateVillagePreview(string villageName)
    {
        string displayName = villageName.Length > 0 ? villageName : "Your village";

        SetLine(villageLines, 0, $"{displayName}  ·  Village (Quality {NewGameDefaults.VillageQuality})");
        SetLine(villageLines, 1,
            $"Population {NewGameDefaults.VillagePopulation}  ·  " +
            $"Treasury {NewGameDefaults.VillageWealthGold}g {NewGameDefaults.VillageWealthSilver}s");
        SetLine(villageLines, 2, "Tavern · Town Hall · Walls · Blacksmith · General Store");
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
