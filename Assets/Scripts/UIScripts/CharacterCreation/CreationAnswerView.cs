using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// One answer on the creation screen: what you would do, and a line underneath
/// in the character's own voice.
///
/// The subtext is not decoration. Without it every option reads as a mechanical
/// choice and the player picks whichever sounds strongest; with it they pick the
/// one that sounds like them, which is the only way a personality measure comes
/// back with anything honest.
/// </summary>
public class CreationAnswerView : MonoBehaviour
{
    [SerializeField] private TMP_Text titleLabel;
    [SerializeField] private TMP_Text subtextLabel;

    private Button _button;
    private CreationAnswer _answer;
    private bool _wired;

    /// <summary>Raised with the answer this view is showing.</summary>
    public System.Action<CreationAnswer> OnClicked;

    public CreationAnswer Answer => _answer;

    /// <summary>
    /// Wiring happens here rather than in Awake because the template this is
    /// cloned from sits disabled — Awake would not have run by the time the
    /// panel binds the clone.
    /// </summary>
    private void EnsureWired()
    {
        if (_wired) return;
        _wired = true;

        _button = GetComponent<Button>();

        if (_button == null) return;

        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(() =>
        {
            if (_answer != null) OnClicked?.Invoke(_answer);
        });
    }

    public void Bind(CreationAnswer answer)
    {
        EnsureWired();

        _answer = answer;

        if (answer == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        if (titleLabel != null)
            titleLabel.text = answer.Text;

        if (subtextLabel != null)
        {
            bool hasSubtext = !string.IsNullOrEmpty(answer.Subtext);
            subtextLabel.text = hasSubtext ? answer.Subtext : "";
            subtextLabel.gameObject.SetActive(hasSubtext);
        }
    }

    /// <summary>Used by the builder so the labels do not have to be dragged in.</summary>
    public void SetLabels(TMP_Text title, TMP_Text subtext)
    {
        titleLabel = title;
        subtextLabel = subtext;
    }
}
