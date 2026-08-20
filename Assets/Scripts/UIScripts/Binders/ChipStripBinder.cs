using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Flows short labels — trait names or tags — as chips that wrap onto new rows.
///
/// The count is unpredictable: a fresh character has four tags, one who has
/// lived a while has fifteen. Placing these by hand is not possible, so they
/// are generated and a flexible grid handles the wrapping.
/// </summary>
public class ChipStripBinder : MonoBehaviour
{
    public enum Content
    {
        /// <summary>Permanent personality and origin traits.</summary>
        Characteristics,

        /// <summary>Raw tags used by recipes, events and dialogue.</summary>
        Tags
    }

    [SerializeField] private Content content = Content.Characteristics;

    [Header("Appearance")]
    [SerializeField] private int fontSize = 15;
    [SerializeField] private int chipHeight = 30;
    [SerializeField] private int horizontalPadding = 14;
    [SerializeField] private Sprite chipBackground;

    [Tooltip("0 = no limit.")]
    [SerializeField] private int maxChips = 0;

    [SerializeField] private string emptyMessage = "None yet.";

    private readonly List<GameObject> _chips = new();

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

        var entries = content == Content.Characteristics ? GetCharacteristics() : GetTags();

        if (entries.Count == 0)
        {
            if (!string.IsNullOrEmpty(emptyMessage))
                AddChip(emptyMessage, new Color(0.55f, 0.49f, 0.38f));
            return;
        }

        int shown = 0;
        foreach (var (label, color) in entries)
        {
            if (maxChips > 0 && shown >= maxChips) break;
            AddChip(label, color);
            shown++;
        }
    }

    private List<(string, Color)> GetCharacteristics()
    {
        var result = new List<(string, Color)>();
        var system = TraitSystem.Instance;
        if (system == null) return result;

        // Only the lasting parts of the character. Conditions belong in the
        // active-effects list, where their timers make sense.
        foreach (var kind in new[] { TraitKind.Origin, TraitKind.Personality, TraitKind.Familiarity })
        {
            foreach (var trait in system.GetHeldOfKind(kind))
                result.Add((trait.displayName, TraitRules.ToneColor(trait.tone)));
        }

        return result;
    }

    private List<(string, Color)> GetTags()
    {
        var result = new List<(string, Color)>();
        var pd = PlayerStatHandler.Instance != null ? PlayerStatHandler.Instance.pd : null;

        if (pd?.ActiveTraitTags == null) return result;

        var seen = new HashSet<string>();
        var neutral = new Color(0.80f, 0.75f, 0.65f);

        foreach (var tag in pd.ActiveTraitTags)
        {
            if (string.IsNullOrEmpty(tag)) continue;

            // The tag list also carries trait ids; those already appear as
            // characteristics, so skip them here.
            if (tag.StartsWith("trait_") || tag.StartsWith("origin_") || tag.StartsWith("cond_"))
                continue;

            if (!seen.Add(tag)) continue;

            result.Add((Prettify(tag), neutral));
        }

        return result;
    }

    private void AddChip(string label, Color color)
    {
        var chip = new GameObject($"Chip_{label}", typeof(RectTransform));
        chip.transform.SetParent(transform, false);

        var image = chip.AddComponent<Image>();
        if (chipBackground != null)
        {
            image.sprite = chipBackground;
            image.type = Image.Type.Sliced;
            image.color = Color.white;
        }
        else
        {
            image.color = new Color(0.18f, 0.12f, 0.07f, 0.85f);
        }

        var layout = chip.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(horizontalPadding, horizontalPadding, 2, 2);
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;

        var fitter = chip.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

        var element = chip.AddComponent<LayoutElement>();
        element.minHeight = chipHeight;
        element.preferredHeight = chipHeight;

        var textGo = new GameObject("Text", typeof(RectTransform));
        textGo.transform.SetParent(chip.transform, false);

        var text = textGo.AddComponent<TextMeshProUGUI>();
        text.text = label;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.NoWrap;

        _chips.Add(chip);
    }

    private static string Prettify(string tag)
    {
        var parts = tag.Split('_');
        for (int i = 0; i < parts.Length; i++)
        {
            if (parts[i].Length == 0) continue;
            parts[i] = char.ToUpperInvariant(parts[i][0]) + parts[i].Substring(1);
        }
        return string.Join(" ", parts);
    }

    private void Clear()
    {
        foreach (var chip in _chips)
            if (chip != null) Destroy(chip);

        _chips.Clear();
    }
}
