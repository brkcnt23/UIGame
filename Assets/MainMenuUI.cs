using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("Buttons")]
    public Button startGameButton;
    public Button continueButton;
    public Button loadGameButton;
    public Button creditsButton;
    public Button settingsButton;
    public Button exitButton;

    private void Start()
    {
        bool hasSavedGame = GameManager.Instance.HasSavedGame();
        continueButton.gameObject.SetActive(hasSavedGame);
        loadGameButton.interactable = hasSavedGame;

        startGameButton.onClick.AddListener(() => GameManager.Instance.ShowStartGamePanel());
        continueButton.onClick.AddListener(() => GameManager.Instance.LoadLastSavedGame());
        loadGameButton.onClick.AddListener(() => GameManager.Instance.ShowLoadGamePanel());
        creditsButton.onClick.AddListener(() => GameManager.Instance.ShowCreditsPanel());
        settingsButton.onClick.AddListener(() => GameManager.Instance.ShowSettingsPanel());
        exitButton.onClick.AddListener(() => GameManager.Instance.ExitGame());
    }
}
