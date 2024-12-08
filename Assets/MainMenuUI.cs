using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("Buttons")]
    public Button startGameButton;
    public Button continueButton;
    public Button creditsButton;
    public Button settingsButton;

    private void Start()
    {
        startGameButton.onClick.AddListener(() => GameManager.Instance.ShowStartGamePanel());
        continueButton.onClick.AddListener(() => GameManager.Instance.LoadLastSavedGame());
        creditsButton.onClick.AddListener(() => GameManager.Instance.ShowCreditsPanel());
        settingsButton.onClick.AddListener(() => GameManager.Instance.ShowSettingsPanel());
    }
}
