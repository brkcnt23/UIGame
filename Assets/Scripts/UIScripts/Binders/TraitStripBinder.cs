using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Draws the player's active traits as a live list.
///
/// Rebuilds only when the trait list actually changes, and ticks the remaining
/// time separately — a countdown does not need the whole strip torn down and
/// rebuilt every second.
///
/// Rows are generated at runtime rather than placed by hand, because the
/// number of active effects changes constantly: a player might hold two, or
/// nine after a bad week on the road.
/// </summary>
public class TraitStripBinder : MonoBehaviour
{
    [Header("Filter")]
    [Tooltip("Only show these kinds. Empty means everything.")]
    [SerializeField] private List<TraitKind> kindFilter = new() { TraitKind.Condition };

    [Tooltip("0 = no limit.")]
    [SerializeField] private int maxRows = 0;

    [Header("Appearance")]
    [SerializeField] private int rowHeight = 30;
    [SerializeField] private int iconSize = 22;
    [SerializeField] private int fontSize = 16;
    [SerializeField] private bool showRemainingTime = true;
    [SerializeField] private bool showEffectLine = true;

    [Tooltip("Shown when nothing matches the filter.")]
    [SerializeField] private string emptyMessage = "Nothing of note.";

    private readonly List<GameObject> _rows = new();
    private readonly List<TMP_Text> _timers = new();
    private readonly List<ActiveTrait> _tracked = new();

    private float _timerAccumulator;

    private void OnEnable()
    {
        if (TraitSystem.Instance != null)
            TraitSystem.Instance.OnTraitsChanged += Rebuild;

        Rebuild();
    }

    private void OnDisable()
    {
        if (TraitSystem.Instance != null)
            TraitSystem.Instance.OnTraitsChanged -= Rebuild;
    }

    private void Update()
    {
        if (!showRemainingTime || _timers.Count == 0)
            return;

        // Once a second is plenty for an hour-resolution clock.
        _timerAccumulator += Time.unscaledDeltaTime;
        if (_timerAccumulator < 1f) return;
        _timerAccumulator = 0f;

        RefreshTimers();
    }

    // -----------------------------------------------------------------

    public void Rebuild()
    {
        ClearRows();

        var system = TraitSystem.Instance;
        if (system == null)
        {
            AddMessage("Traits unavailable.");
            return;
        }

        var db = system.Database;
        int shown = 0;

        foreach (var active in system.Active)
        {
            var def = db != null ? db.Get(active.traitId) : null;
            if (def == null) continue;

            if (kindFilter.Count > 0 && !kindFilter.Contains(def.kind))
                continue;

            if (maxRows > 0 && shown >= maxRows)
                break;

            AddRow(def, active);
            shown++;
        }

        if (shown == 0 && !string.IsNullOrEmpty(emptyMessage))
            AddMessage(emptyMessage);
    }

    private void AddRow(TraitSO def, ActiveTrait active)
    {
        var row = new GameObject($"Trait_{def.traitId}", typeof(RectTransform));
        row.transform.SetParent(transform, false);

        var layout = row.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 8;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;

        var element = row.AddComponent<LayoutElement>();
        element.minHeight = rowHeight;
        element.preferredHeight = rowHeight;

        // Icon
        var iconGo = new GameObject("Icon", typeof(RectTransform));
        iconGo.transform.SetParent(row.transform, false);

        var image = iconGo.AddComponent<Image>();
        image.preserveAspect = true;

        if (def.icon != null) image.sprite = def.icon;
        else image.color = new Color(1, 1, 1, 0);

        var iconElement = iconGo.AddComponent<LayoutElement>();
        iconElement.preferredWidth = iconSize;
        iconElement.preferredHeight = iconSize;
        iconElement.flexibleWidth = 0;

        // Name, coloured by tone
        var nameLabel = MakeLabel(row.transform, "Name", def.displayName,
                                  TraitRules.ToneColor(def.tone), TextAlignmentOptions.Left);

        if (def.stackable && active.stacks > 1)
            nameLabel.text += $" ×{active.stacks}";

        var nameElement = nameLabel.gameObject.AddComponent<LayoutElement>();
        nameElement.flexibleWidth = showEffectLine ? 0 : 1;
        nameElement.preferredWidth = showEffectLine ? 130 : 0;

        // Effect summary
        if (showEffectLine)
        {
            var lines = def.GetEffectLines();
            string text = lines.Count > 0 ? string.Join("  ", lines) : "";

            var effectLabel = MakeLabel(row.transform, "Effect", text,
                                        new Color(0.66f, 0.60f, 0.48f), TextAlignmentOptions.Left);

            var effectElement = effectLabel.gameObject.AddComponent<LayoutElement>();
            effectElement.flexibleWidth = 1;
        }

        // Remaining time
        if (showRemainingTime)
        {
            var timerLabel = MakeLabel(row.transform, "Timer", "",
                                       new Color(0.62f, 0.55f, 0.42f), TextAlignmentOptions.Right);

            var timerElement = timerLabel.gameObject.AddComponent<LayoutElement>();
            timerElement.preferredWidth = 60;
            timerElement.flexibleWidth = 0;

            _timers.Add(timerLabel);
            _tracked.Add(active);
        }

        _rows.Add(row);
    }

    private void AddMessage(string message)
    {
        var label = MakeLabel(transform, "Empty", message,
                              new Color(0.55f, 0.49f, 0.38f), TextAlignmentOptions.Left);

        var element = label.gameObject.AddComponent<LayoutElement>();
        element.minHeight = rowHeight;

        _rows.Add(label.gameObject);
    }

    private TMP_Text MakeLabel(Transform parent, string name, string text,
                               Color color, TextAlignmentOptions alignment)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        var label = go.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = fontSize;
        label.color = color;
        label.alignment = alignment;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.overflowMode = TextOverflowModes.Ellipsis;

        return label;
    }

    private void RefreshTimers()
    {
        var pd = PlayerStatHandler.Instance != null ? PlayerStatHandler.Instance.pd : null;
        if (pd == null) return;

        int absoluteHour = pd.Day * 24 + pd.Hour;

        for (int i = 0; i < _timers.Count && i < _tracked.Count; i++)
        {
            if (_timers[i] == null) continue;
            _timers[i].text = _tracked[i].RemainingLabel(absoluteHour);
        }
    }

    private void ClearRows()
    {
        foreach (var row in _rows)
            if (row != null) Destroy(row);

        _rows.Clear();
        _timers.Clear();
        _tracked.Clear();
    }
}
