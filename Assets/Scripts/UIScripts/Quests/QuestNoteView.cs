using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// One quest note on the board.
///
/// Assembled from parts rather than being a painted image per quest: a paper
/// sprite, the quest's ink sketch, the text, a coin, and a wax seal on the
/// higher tiers. Rebalancing a reward changes a number, not a drawing.
///
/// Each note is pinned at a slightly different angle. A board where every
/// sheet is perfectly square reads as a spreadsheet; a few degrees of
/// variation reads as paper somebody actually nailed up.
/// </summary>
public class QuestNoteView : MonoBehaviour
{
    [Header("Parts")]
    [SerializeField] private Image paperImage;
    [SerializeField] private Image sketchImage;
    [SerializeField] private Image coinImage;
    [SerializeField] private Image sealImage;

    [Tooltip("Ornate border drawn over royal notes only.")]
    [SerializeField] private Image frameImage;

    [Header("Text")]
    [SerializeField] private TMP_Text titleLabel;
    [SerializeField] private TMP_Text bodyLabel;
    [SerializeField] private TMP_Text rewardLabel;
    [SerializeField] private TMP_Text rewardHeaderLabel;

    [Header("State")]
    [SerializeField] private GameObject lockedOverlay;
    [SerializeField] private TMP_Text lockedLabel;

    [Header("Pinning")]
    [Tooltip("Maximum tilt in degrees, randomised per note.")]
    [SerializeField] private float maxTilt = 4f;

    [Tooltip("Maximum scale variation, so notes are not identical sheets.")]
    [Range(0f, 0.15f)]
    [SerializeField] private float maxScaleJitter = 0.04f;

    private Button _button;
    private QuestSO _quest;
    private bool _locked;

    public QuestSO Quest => _quest;

    /// <summary>Raised when a note that can be taken is tapped.</summary>
    public System.Action<QuestSO> OnClicked;

    private void Awake()
    {
        _button = GetComponent<Button>();

        if (_button != null)
        {
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() =>
            {
                if (_quest != null) OnClicked?.Invoke(_quest);
            });
        }
    }

    public void Bind(QuestSO quest, QuestDatabaseSO database, PlayerData player)
    {
        _quest = quest;

        if (quest == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        if (paperImage != null)
        {
            var paper = quest.paperOverride != null
                ? quest.paperOverride
                : database != null ? database.GetPaper(quest.tier) : null;

            if (paper != null) paperImage.sprite = paper;
        }

        if (sketchImage != null)
        {
            sketchImage.sprite = quest.sketch;
            sketchImage.enabled = quest.sketch != null;
            sketchImage.preserveAspect = true;
        }

        if (titleLabel != null) titleLabel.text = quest.questName;
        if (bodyLabel != null) bodyLabel.text = quest.description;

        if (rewardHeaderLabel != null) rewardHeaderLabel.text = "Reward:";
        if (rewardLabel != null) rewardLabel.text = quest.RewardLabel();

        if (coinImage != null && database != null)
        {
            var coin = database.GetCoin(quest);
            coinImage.sprite = coin;
            coinImage.enabled = coin != null;
        }

        // A seal marks work that came from somebody with a seal to use, and
        // royal work carries a different one so it stands out on the board.
        if (sealImage != null && database != null)
        {
            var seal = database.GetSeal(quest.tier);
            sealImage.sprite = seal;
            sealImage.enabled = seal != null;
        }

        if (frameImage != null && database != null)
        {
            var frame = database.GetFrame(quest.tier);
            frameImage.sprite = frame;
            frameImage.enabled = frame != null;
        }

        string reason = quest.LockReason(player);
        _locked = !string.IsNullOrEmpty(reason);

        if (lockedOverlay != null) lockedOverlay.SetActive(_locked);
        if (lockedLabel != null) lockedLabel.text = reason ?? "";
        if (_button != null) _button.interactable = !_locked;

        ApplyPinJitter(quest.questId);
    }

    /// <summary>
    /// Tilt and scale derived from the quest id, so a note looks the same
    /// every time the board is opened rather than jumping about.
    /// </summary>
    private void ApplyPinJitter(int seed)
    {
        var random = new System.Random(seed);

        float tilt = (float)(random.NextDouble() * 2 - 1) * maxTilt;
        float scale = 1f + (float)(random.NextDouble() * 2 - 1) * maxScaleJitter;

        var rect = transform as RectTransform;
        if (rect == null) return;

        rect.localRotation = Quaternion.Euler(0f, 0f, tilt);
        rect.localScale = new Vector3(scale, scale, 1f);
    }

    public void Clear()
    {
        _quest = null;
        gameObject.SetActive(false);
    }
}
