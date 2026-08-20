#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Builds the item details panel: four painted backgrounds, one per kind of
/// item, each with its own labels.
///
/// The source images are 1024x1536 with the panel drawn inside transparent
/// padding, and the padding differs per image. The builder sizes each
/// background so its painted area comes out exactly PanelWidth wide, then
/// nudges it so the painted area sits centred — which is why the numbers below
/// are content bounds rather than image sizes.
///
///   ItemDescInfo           x 199..828   y  190..1281    630 x 1092
///   ItemWearableInfo       x 218..805   y   69..1368    588 x 1300
///   ItemBonusInfo          x 203..821   y   82..1411    619 x 1330
///   ItemDetailedInfoPanel  x 253..785   y  224..1374    533 x 1151
///
/// Label positions are set as fractions of the painted area, so nudging one in
/// the Inspector afterwards is easy and re-running does not fight the artwork.
///
/// Tools > UIGame > Inventory > Build item info panel
/// </summary>
public static class ItemInfoPanelBuilder
{
    private const string PrefabFolder = "Assets/Prefabs/Inventory";
    private const string ArtFolder = "Assets/UI Elements";

    private const float SourceW = 1024f;
    private const float SourceH = 1536f;

    /// <summary>Painted width of every variant, in UI units.</summary>
    private const float PanelWidth = 410f;

    private const int NameSize     = 19;
    private const int QualitySize  = 13;
    private const int CategorySize = 11;
    private const int FlavorSize   = 13;
    private const int ButtonSize   = 13;

    private sealed class VariantSpec
    {
        public ItemInfoPanel.Layout Layout;
        public string Sprite;
        public Rect Content;          // painted bounds inside the source image

        // Fractions of the painted area, measured from its top-left.
        public Rect Icon;
        public Rect Name;
        public Rect Quality;
        public Rect Category;
        public Rect Flavor;
        public Rect Stats;
        public Rect Buttons;          // width 0 means no buttons
        public bool HasDetailButton;
    }

    private static readonly VariantSpec[] Specs =
    {
        // Words only: a big description block and a short stat list.
        new VariantSpec
        {
            Layout = ItemInfoPanel.Layout.Desc,
            Sprite = "ItemDescInfo",
            Content = new Rect(199, 190, 630, 1092),
            Icon     = new Rect(0.07f, 0.03f, 0.26f, 0.15f),
            Name     = new Rect(0.36f, 0.04f, 0.58f, 0.06f),
            Quality  = new Rect(0.36f, 0.10f, 0.30f, 0.04f),
            Category = new Rect(0.66f, 0.10f, 0.28f, 0.04f),
            Flavor   = new Rect(0.09f, 0.43f, 0.82f, 0.26f),
            Stats    = new Rect(0.09f, 0.72f, 0.82f, 0.20f),
            Buttons  = new Rect(0.10f, 0.93f, 0.80f, 0.06f),
            HasDetailButton = true
        },

        // Gear: stats matter most, so they get the larger block.
        new VariantSpec
        {
            Layout = ItemInfoPanel.Layout.Wearable,
            Sprite = "ItemWearableInfo",
            Content = new Rect(218, 69, 588, 1300),
            Icon     = new Rect(0.07f, 0.03f, 0.24f, 0.12f),
            Name     = new Rect(0.34f, 0.03f, 0.60f, 0.05f),
            Quality  = new Rect(0.34f, 0.08f, 0.30f, 0.035f),
            Category = new Rect(0.64f, 0.08f, 0.30f, 0.035f),
            Flavor   = new Rect(0.09f, 0.52f, 0.82f, 0.19f),
            Stats    = new Rect(0.09f, 0.74f, 0.82f, 0.21f),
            Buttons  = new Rect(0.10f, 0.955f, 0.80f, 0.04f),
            HasDetailButton = true
        },

        // Effects: one wide block for what the thing does to you.
        new VariantSpec
        {
            Layout = ItemInfoPanel.Layout.Bonus,
            Sprite = "ItemBonusInfo",
            Content = new Rect(203, 82, 619, 1330),
            Icon     = new Rect(0.07f, 0.04f, 0.24f, 0.12f),
            Name     = new Rect(0.34f, 0.04f, 0.60f, 0.05f),
            Quality  = new Rect(0.34f, 0.09f, 0.30f, 0.035f),
            Category = new Rect(0.64f, 0.09f, 0.30f, 0.035f),
            Flavor   = new Rect(0.09f, 0.36f, 0.82f, 0.25f),
            Stats    = new Rect(0.09f, 0.68f, 0.82f, 0.24f),
            Buttons  = new Rect(0.10f, 0.945f, 0.80f, 0.045f),
            HasDetailButton = true
        },

        // The full breakdown, opened from the others. No actions here — this
        // is a reading screen, and a stray Drop button on a reading screen is
        // how inventories get lost.
        new VariantSpec
        {
            Layout = ItemInfoPanel.Layout.Detailed,
            Sprite = "ItemDetailedInfoPanel",
            Content = new Rect(253, 224, 533, 1151),
            Icon     = new Rect(0.08f, 0.03f, 0.24f, 0.12f),
            Name     = new Rect(0.36f, 0.03f, 0.58f, 0.055f),
            Quality  = new Rect(0.36f, 0.09f, 0.30f, 0.04f),
            Category = new Rect(0.64f, 0.09f, 0.30f, 0.04f),
            Flavor   = new Rect(0.09f, 0.20f, 0.82f, 0.16f),
            Stats    = new Rect(0.09f, 0.40f, 0.82f, 0.52f),
            Buttons  = new Rect(0f, 0f, 0f, 0f),
            HasDetailButton = false
        },
    };

    // =================================================================

    [MenuItem("Tools/UIGame/Inventory/Build item info panel", false, 1)]
    public static void Build()
    {
        EnsureFolder(PrefabFolder);

        var statRow = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabFolder}/ItemStatRow.prefab");
        if (statRow == null)
            Debug.LogWarning("[InfoPanel] ItemStatRow.prefab missing — run 'Build inventory prefabs' first.");

        var root = new GameObject("ItemInfoPanel", typeof(RectTransform));
        var rootRect = root.GetComponent<RectTransform>();
        rootRect.sizeDelta = new Vector2(PanelWidth, 900f);

        var panel = root.AddComponent<ItemInfoPanel>();
        var variants = new List<ItemInfoPanel.Variant>();

        foreach (var spec in Specs)
            variants.Add(BuildVariant(rootRect, spec));

        var so = new SerializedObject(panel);

        if (statRow != null)
        {
            var prop = so.FindProperty("statRowPrefab");
            if (prop != null) prop.objectReferenceValue = statRow.GetComponent<ItemStatRow>();
        }

        var array = so.FindProperty("variants");
        array.arraySize = variants.Count;

        for (int i = 0; i < variants.Count; i++)
            WriteVariant(array.GetArrayElementAtIndex(i), variants[i]);

        so.ApplyModifiedPropertiesWithoutUndo();

        string path = $"{PrefabFolder}/ItemInfoPanel.prefab";
        var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[InfoPanel] Built {path} with {variants.Count} variants.\n" +
                  "Drop it into the inventory panel and assign it to " +
                  "InventoryGridBinder's Info Panel field.\n" +
                  "Only one variant is visible at a time; the panel picks by item category.");

        Selection.activeObject = prefab;
        EditorGUIUtility.PingObject(prefab);
    }

    // =================================================================

    private static ItemInfoPanel.Variant BuildVariant(RectTransform parent, VariantSpec spec)
    {
        float scale = PanelWidth / spec.Content.width;
        float paintedH = spec.Content.height * scale;

        var rootRect = NewChild(parent, $"Variant_{spec.Layout}");
        rootRect.anchorMin = new Vector2(0.5f, 1f);
        rootRect.anchorMax = new Vector2(0.5f, 1f);
        rootRect.pivot = new Vector2(0.5f, 1f);
        rootRect.anchoredPosition = Vector2.zero;
        rootRect.sizeDelta = new Vector2(PanelWidth, paintedH);

        // The background is larger than the painted area because of the
        // transparent margin, and offset so the painting lands on the rect.
        var bg = NewChild(rootRect, "Background");
        bg.anchorMin = new Vector2(0.5f, 0.5f);
        bg.anchorMax = new Vector2(0.5f, 0.5f);
        bg.pivot = new Vector2(0.5f, 0.5f);
        bg.sizeDelta = new Vector2(SourceW * scale, SourceH * scale);

        float contentCentreX = spec.Content.x + spec.Content.width * 0.5f;
        float contentCentreY = spec.Content.y + spec.Content.height * 0.5f;
        bg.anchoredPosition = new Vector2(
            (SourceW * 0.5f - contentCentreX) * scale,
            (contentCentreY - SourceH * 0.5f) * scale);

        var bgImage = bg.gameObject.AddComponent<Image>();
        var sprite = FindSprite(spec.Sprite);

        if (sprite != null) bgImage.sprite = sprite;
        else bgImage.color = new Color(0.85f, 0.78f, 0.60f, 0.4f);

        bgImage.raycastTarget = true;

        // ---- Labels, positioned as fractions of the painted area ------
        var iconRect = Place(rootRect, "Icon", spec.Icon, PanelWidth, paintedH);
        var icon = iconRect.gameObject.AddComponent<Image>();
        icon.preserveAspect = true;
        icon.raycastTarget = false;
        icon.enabled = false;

        var nameLabel = PlaceLabel(rootRect, "Name", spec.Name, PanelWidth, paintedH,
                                   "Item Name", NameSize,
                                   new Color(0.24f, 0.18f, 0.11f), TextAlignmentOptions.TopLeft);

        var qualityLabel = PlaceLabel(rootRect, "Quality", spec.Quality, PanelWidth, paintedH,
                                      "Fine", QualitySize,
                                      ItemRules.QualityColor(ItemQuality.Fine), TextAlignmentOptions.TopLeft);

        var categoryLabel = PlaceLabel(rootRect, "Category", spec.Category, PanelWidth, paintedH,
                                       "Weapon", CategorySize,
                                       new Color(0.46f, 0.38f, 0.27f), TextAlignmentOptions.TopRight);

        var flavorLabel = PlaceLabel(rootRect, "Flavor", spec.Flavor, PanelWidth, paintedH,
                                     "Yıllanmış dişbudaktan, sabırla eğilmiş bir yay.", FlavorSize,
                                     new Color(0.30f, 0.24f, 0.16f), TextAlignmentOptions.TopLeft);
        flavorLabel.textWrappingMode = TextWrappingModes.Normal;

        var statParent = Place(rootRect, "StatRows", spec.Stats, PanelWidth, paintedH);
        var statLayout = statParent.gameObject.AddComponent<VerticalLayoutGroup>();
        statLayout.spacing = 2;
        statLayout.childControlWidth = true;
        statLayout.childControlHeight = true;
        statLayout.childForceExpandWidth = true;
        statLayout.childForceExpandHeight = false;

        var variant = new ItemInfoPanel.Variant
        {
            layout = spec.Layout,
            root = rootRect.gameObject,
            icon = icon,
            nameLabel = nameLabel,
            qualityLabel = qualityLabel,
            categoryLabel = categoryLabel,
            flavorLabel = flavorLabel,
            statParent = statParent
        };

        // ---- Buttons, inside the painted area -------------------------
        if (spec.Buttons.width > 0f)
        {
            var actions = Place(rootRect, "Actions", spec.Buttons, PanelWidth, paintedH);

            var layout = actions.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 6;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;

            variant.equipButton = MakeButton(actions, "EquipButton", "Equip", out var equipLabel);
            variant.equipLabel = equipLabel;
            variant.useButton = MakeButton(actions, "UseButton", "Use", out _);
            variant.dropButton = MakeButton(actions, "DropButton", "Drop", out _);

            if (spec.HasDetailButton)
                variant.detailButton = MakeButton(actions, "DetailButton", "More", out _);

            variant.closeButton = MakeButton(actions, "CloseButton", "Close", out _);
        }
        else
        {
            // The detailed view still needs a way out, so it gets a close
            // button on its own rather than none at all.
            var closeRect = NewChild(rootRect, "CloseButton");
            closeRect.anchorMin = new Vector2(1f, 1f);
            closeRect.anchorMax = new Vector2(1f, 1f);
            closeRect.pivot = new Vector2(1f, 1f);
            closeRect.anchoredPosition = new Vector2(-14f, -14f);
            closeRect.sizeDelta = new Vector2(64f, 30f);

            variant.closeButton = Decorate(closeRect, "Close", out _);
        }

        rootRect.gameObject.SetActive(false);
        return variant;
    }

    private static void WriteVariant(SerializedProperty prop, ItemInfoPanel.Variant v)
    {
        prop.FindPropertyRelative("layout").enumValueIndex = (int)v.layout;
        Set(prop, "root", v.root);
        Set(prop, "icon", v.icon);
        Set(prop, "nameLabel", v.nameLabel);
        Set(prop, "qualityLabel", v.qualityLabel);
        Set(prop, "categoryLabel", v.categoryLabel);
        Set(prop, "flavorLabel", v.flavorLabel);
        Set(prop, "statParent", v.statParent);
        Set(prop, "equipButton", v.equipButton);
        Set(prop, "useButton", v.useButton);
        Set(prop, "dropButton", v.dropButton);
        Set(prop, "detailButton", v.detailButton);
        Set(prop, "closeButton", v.closeButton);
        Set(prop, "equipLabel", v.equipLabel);
    }

    private static void Set(SerializedProperty parent, string name, Object value)
    {
        var prop = parent.FindPropertyRelative(name);
        if (prop != null) prop.objectReferenceValue = value;
    }

    // =================================================================

    /// <summary>Places a child using fractions of the painted area.</summary>
    private static RectTransform Place(RectTransform parent, string name, Rect fraction,
                                       float width, float height)
    {
        var rect = NewChild(parent, name);
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(fraction.x * width, -fraction.y * height);
        rect.sizeDelta = new Vector2(fraction.width * width, fraction.height * height);
        return rect;
    }

    private static TMP_Text PlaceLabel(RectTransform parent, string name, Rect fraction,
                                       float width, float height, string text, int size,
                                       Color color, TextAlignmentOptions alignment)
    {
        var rect = Place(parent, name, fraction, width, height);
        var label = rect.gameObject.AddComponent<TextMeshProUGUI>();

        label.text = text;
        label.fontSize = size;
        label.color = color;
        label.alignment = alignment;
        label.raycastTarget = false;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.overflowMode = TextOverflowModes.Ellipsis;

        return label;
    }

    private static Button MakeButton(RectTransform parent, string name, string label,
                                     out TMP_Text labelText)
    {
        var rect = NewChild(parent, name);
        return Decorate(rect, label, out labelText);
    }

    private static Button Decorate(RectTransform rect, string label, out TMP_Text labelText)
    {
        var image = rect.gameObject.AddComponent<Image>();
        var sprite = FindSprite("EmptyButton", "empty_long_button", "AmptyLongButton");

        if (sprite != null)
        {
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
        }
        else
        {
            image.color = new Color(0.32f, 0.22f, 0.13f);
        }

        var button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;

        labelText = NewLabel(rect, "Label", label, ButtonSize,
                             new Color(0.94f, 0.88f, 0.72f), TextAlignmentOptions.Center);
        labelText.rectTransform.anchorMin = Vector2.zero;
        labelText.rectTransform.anchorMax = Vector2.one;
        labelText.rectTransform.offsetMin = Vector2.zero;
        labelText.rectTransform.offsetMax = Vector2.zero;
        labelText.raycastTarget = false;

        return button;
    }

    private static Sprite FindSprite(params string[] candidates)
    {
        if (!AssetDatabase.IsValidFolder(ArtFolder)) return null;

        foreach (var guid in AssetDatabase.FindAssets("t:Sprite", new[] { ArtFolder }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string file = Normalize(Path.GetFileNameWithoutExtension(path));

            foreach (var candidate in candidates)
            {
                if (file != Normalize(candidate)) continue;

                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite != null) return sprite;
            }
        }

        return null;
    }

    private static string Normalize(string s)
        => new string(s.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());

    private static RectTransform NewChild(RectTransform parent, string name)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        return rect;
    }

    private static TMP_Text NewLabel(RectTransform parent, string name, string text,
                                     int size, Color color, TextAlignmentOptions alignment)
    {
        var rect = NewChild(parent, name);
        var label = rect.gameObject.AddComponent<TextMeshProUGUI>();

        label.text = text;
        label.fontSize = size;
        label.color = color;
        label.alignment = alignment;
        label.raycastTarget = false;

        return label;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;

        string parent = Path.GetDirectoryName(path).Replace('\\', '/');
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
    }
}
#endif
