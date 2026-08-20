using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shows the totals a player is actually running with.
///
/// Every other list on the sheet shows sources — this one shows results. A
/// player holding four traits that each touch exhaustion wants one line
/// telling them what their exhaustion gain really is, not four lines of
/// arithmetic homework.
///
/// Only non-zero effects appear, and the caps in TraitRules are already
/// applied, so what is displayed is what the game uses.
/// </summary>
public class PassiveBonusBinder : MonoBehaviour
{
    [System.Serializable]
    public class Entry
    {
        public EffectType type;
        public string displayName;
        [TextArea(1, 3)] public string description;
        public Sprite icon;
    }

    [Tooltip("Which totals to show, in order. Zero values are skipped.")]
    [SerializeField]
    private List<Entry> tracked = new()
    {
        new Entry { type = EffectType.ExhaustionGain,    displayName = "Travel Endurance", description = "Reduces exhaustion gained while travelling" },
        new Entry { type = EffectType.SkillXpGain,       displayName = "Craft Affinity",   description = "Extra skill experience from crafting" },
        new Entry { type = EffectType.Persuasion,        displayName = "Social Poise",     description = "Improves tavern and negotiation outcomes" },
        new Entry { type = EffectType.CraftQuality,      displayName = "Steady Hands",     description = "Better chance of a higher quality result" },
        new Entry { type = EffectType.CraftResourceCost, displayName = "Thrift",           description = "Uses fewer materials per craft" },
        new Entry { type = EffectType.RationConsumption, displayName = "Lean Appetite",    description = "Eats less on the road" },
        new Entry { type = EffectType.ShopBuyPrice,      displayName = "Hard Bargainer",   description = "Pays less in shops" },
        new Entry { type = EffectType.IllnessResistance, displayName = "Hardy",            description = "Resists sickness and poison" },
    };

    [Header("Appearance")]
    [SerializeField] private int rowHeight = 44;
    [SerializeField] private int iconSize = 30;
    [SerializeField] private int titleFontSize = 17;
    [SerializeField] private int descriptionFontSize = 13;
    [SerializeField] private string emptyMessage = "No lasting advantages yet.";

    private readonly List<GameObject> _rows = new();

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

    public void Rebuild()
    {
        Clear();

        var system = TraitSystem.Instance;
        if (system == null)
        {
            AddMessage("Traits unavailable.");
            return;
        }

        int shown = 0;

        foreach (var entry in tracked)
        {
            int percent = system.GetPercentBonus(entry.type);
            int flat = system.GetFlatBonus(entry.type);

            if (percent == 0 && flat == 0) continue;

            string value = percent != 0
                ? $"{(percent > 0 ? "+" : "")}{percent}%"
                : $"{(flat > 0 ? "+" : "")}{flat}";

            // For costs, a negative number is the good outcome — colour by
            // benefit rather than by sign.
            bool isCost = entry.type == EffectType.ExhaustionGain
                       || entry.type == EffectType.RationConsumption
                       || entry.type == EffectType.CraftResourceCost
                       || entry.type == EffectType.ShopBuyPrice
                       || entry.type == EffectType.TravelTime
                       || entry.type == EffectType.BuildTime
                       || entry.type == EffectType.DamageTaken;

            bool good = isCost ? (percent + flat) < 0 : (percent + flat) > 0;

            AddRow(entry, value, good);
            shown++;
        }

        if (shown == 0 && !string.IsNullOrEmpty(emptyMessage))
            AddMessage(emptyMessage);
    }

    private void AddRow(Entry entry, string value, bool good)
    {
        var row = new GameObject($"Passive_{entry.type}", typeof(RectTransform));
        row.transform.SetParent(transform, false);

        var layout = row.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 10;
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

        if (entry.icon != null) image.sprite = entry.icon;
        else image.color = new Color(1, 1, 1, 0);

        var iconElement = iconGo.AddComponent<LayoutElement>();
        iconElement.preferredWidth = iconSize;
        iconElement.preferredHeight = iconSize;
        iconElement.flexibleWidth = 0;

        // Title over description
        var textColumn = new GameObject("Text", typeof(RectTransform));
        textColumn.transform.SetParent(row.transform, false);

        var columnLayout = textColumn.AddComponent<VerticalLayoutGroup>();
        columnLayout.spacing = 0;
        columnLayout.childControlWidth = true;
        columnLayout.childControlHeight = true;
        columnLayout.childForceExpandHeight = false;

        var columnElement = textColumn.AddComponent<LayoutElement>();
        columnElement.flexibleWidth = 1;

        MakeLabel(textColumn.transform, "Title", entry.displayName,
                  titleFontSize, new Color(0.91f, 0.86f, 0.78f), TextAlignmentOptions.Left);

        MakeLabel(textColumn.transform, "Description", entry.description,
                  descriptionFontSize, new Color(0.62f, 0.56f, 0.45f), TextAlignmentOptions.Left);

        // Value
        var valueLabel = MakeLabel(row.transform, "Value", value, titleFontSize,
                                   good ? new Color(0.56f, 0.77f, 0.42f) : new Color(0.83f, 0.48f, 0.42f),
                                   TextAlignmentOptions.Right);

        var valueElement = valueLabel.gameObject.AddComponent<LayoutElement>();
        valueElement.preferredWidth = 70;
        valueElement.flexibleWidth = 0;

        _rows.Add(row);
    }

    private void AddMessage(string message)
    {
        var label = MakeLabel(transform, "Empty", message, descriptionFontSize + 2,
                              new Color(0.55f, 0.49f, 0.38f), TextAlignmentOptions.Left);

        var element = label.gameObject.AddComponent<LayoutElement>();
        element.minHeight = rowHeight;

        _rows.Add(label.gameObject);
    }

    private TMP_Text MakeLabel(Transform parent, string name, string text, int size,
                               Color color, TextAlignmentOptions alignment)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        var label = go.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = size;
        label.color = color;
        label.alignment = alignment;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.overflowMode = TextOverflowModes.Ellipsis;

        return label;
    }

    private void Clear()
    {
        foreach (var row in _rows)
            if (row != null) Destroy(row);

        _rows.Clear();
    }
}
