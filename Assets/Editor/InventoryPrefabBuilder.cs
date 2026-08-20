#if UNITY_EDITOR
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Builds the inventory prefabs so they can be dragged in rather than
/// assembled by hand.
///
/// Three prefabs, each doing one job:
///
///   InventorySlot     one cell: frame, icon, count, equipped marker
///   ItemStatRow       one "label ....... value" line for the info panel
///   InventoryScroll   the scrolling grid, wired to the slot prefab
///
/// The scroll prefab is built the way the artwork wants it: the parchment
/// lives INSIDE the scrolling content, because the cells are painted onto it
/// and have to move with the slots. That is the opposite of the usual advice,
/// and it is right here.
///
/// Tools > UIGame > Inventory
/// </summary>
public static class InventoryPrefabBuilder
{
    private const string PrefabFolder = "Assets/Prefabs/Inventory";
    private const string ArtFolder = "Assets/UI Elements";

    [MenuItem("Tools/UIGame/Inventory/Build inventory prefabs", false, 0)]
    public static void BuildAll()
    {
        EnsureFolder(PrefabFolder);

        var slot = BuildSlotPrefab();
        var statRow = BuildStatRowPrefab();
        var scroll = BuildScrollPrefab(slot);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[InventoryBuilder] Built three prefabs in " + PrefabFolder + ":\n" +
                  "  InventorySlot     — drop into a grid, or let the scroll spawn them\n" +
                  "  ItemStatRow       — assign to ItemInfoPanel.statRowPrefab\n" +
                  "  InventoryScroll   — drag into the inventory panel\n\n" +
                  "The scroll's viewport height is the window the player sees. " +
                  "Set it to whatever height your background allows.");

        Selection.activeObject = scroll;
        EditorGUIUtility.PingObject(scroll);
    }

    // =================================================================

    private static GameObject BuildSlotPrefab()
    {
        var root = new GameObject("InventorySlot", typeof(RectTransform));
        var rect = root.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(CellW, CellH);

        // Frame — this is the tinted part, so quality reads at a glance
        // without covering the item art.
        // The cell outline is already painted on the parchment strip, so the
        // slot itself is a transparent hit area that only tints on hover and
        // on quality. Drawing a second frame over the painted one would double
        // every border.
        var frame = root.AddComponent<Image>();
        frame.color = new Color(1f, 1f, 1f, 0.06f);

        var button = root.AddComponent<Button>();
        button.targetGraphic = frame;
        button.transition = Selectable.Transition.ColorTint;

        var colors = button.colors;
        colors.highlightedColor = new Color(1f, 0.95f, 0.80f);
        colors.pressedColor = new Color(0.80f, 0.74f, 0.60f);
        button.colors = colors;

        // Icon, inset so the frame stays visible around it
        var icon = NewChild(rect, "Icon");
        Stretch(icon, 10);
        var iconImage = icon.gameObject.AddComponent<Image>();
        iconImage.preserveAspect = true;
        iconImage.raycastTarget = false;
        iconImage.enabled = false;

        // Stack count, bottom right
        var count = NewChild(rect, "Quantity");
        count.anchorMin = new Vector2(1f, 0f);
        count.anchorMax = new Vector2(1f, 0f);
        count.pivot = new Vector2(1f, 0f);
        count.anchoredPosition = new Vector2(-6f, 4f);
        count.sizeDelta = new Vector2(46, 26);

        var countText = count.gameObject.AddComponent<TextMeshProUGUI>();
        countText.text = "1";
        countText.fontSize = 18;
        countText.alignment = TextAlignmentOptions.BottomRight;
        countText.color = Color.white;
        countText.raycastTarget = false;
        countText.textWrappingMode = TextWrappingModes.NoWrap;
        countText.enabled = false;

        // Equipped marker, top left — small and always in the same place so
        // the eye can scan a full bag for it.
        var marker = NewChild(rect, "EquippedMarker");
        marker.anchorMin = new Vector2(0f, 1f);
        marker.anchorMax = new Vector2(0f, 1f);
        marker.pivot = new Vector2(0f, 1f);
        marker.anchoredPosition = new Vector2(4f, -4f);
        marker.sizeDelta = new Vector2(18, 18);

        var markerImage = marker.gameObject.AddComponent<Image>();
        markerImage.color = new Color(0.42f, 0.78f, 0.35f);
        markerImage.raycastTarget = false;
        marker.gameObject.SetActive(false);

        var slot = root.AddComponent<InventorySlotButton>();
        var so = new SerializedObject(slot);
        so.FindProperty("iconImage").objectReferenceValue = iconImage;
        so.FindProperty("frameImage").objectReferenceValue = frame;
        so.FindProperty("quantityLabel").objectReferenceValue = countText;
        so.FindProperty("equippedMarker").objectReferenceValue = marker.gameObject;
        so.ApplyModifiedPropertiesWithoutUndo();

        return SavePrefab(root, "InventorySlot");
    }

    private static GameObject BuildStatRowPrefab()
    {
        // Sized for the 410-wide info panel — the stat plate has room for
        // about seven of these.
        var root = new GameObject("ItemStatRow", typeof(RectTransform));
        var rect = root.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(300, 24);

        var layout = root.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.spacing = 6;

        var element = root.AddComponent<LayoutElement>();
        element.minHeight = 24;
        element.preferredHeight = 24;

        // Dark ink on parchment — the stat plate is painted paper, so white
        // text would vanish into it.
        var label = NewLabel(rect, "Label", "Damage", 14,
                             new Color(0.42f, 0.34f, 0.24f), TextAlignmentOptions.Left);
        label.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;

        var value = NewLabel(rect, "Value", "1d8 + STR", 14,
                             new Color(0.22f, 0.17f, 0.11f), TextAlignmentOptions.Right);
        var valueElement = value.gameObject.AddComponent<LayoutElement>();
        valueElement.preferredWidth = 120;
        valueElement.flexibleWidth = 0;

        var row = root.AddComponent<ItemStatRow>();
        var so = new SerializedObject(row);
        so.FindProperty("labelText").objectReferenceValue = label;
        so.FindProperty("valueText").objectReferenceValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();

        return SavePrefab(root, "ItemStatRow");
    }

    // Measured from inventory_top / _middle / __bottom. The grid uses these
    // so the spawned slots sit exactly on the painted cells.
    private const float StripWidth   = 654f;
    private const float TopHeight    = 97f;
    private const float MiddleHeight = 75f;
    private const float BottomHeight = 106f;

    private const int   Columns    = 8;
    private const float CellW      = 60f;
    private const float CellH      = 61f;
    private const float SpacingX   = 11.57f;   // pitch 71.57 across 8 columns
    private const float SpacingY   = 14f;      // pitch 75, matching the middle strip
    private const float PadLeft    = 46f;
    private const float PadRight   = 47f;
    private const float PadTop     = 29f;      // first painted row starts here

    private static GameObject BuildScrollPrefab(GameObject slotPrefab)
    {
        var root = new GameObject("InventoryScroll", typeof(RectTransform));
        var rect = root.GetComponent<RectTransform>();

        // Built at the artwork's native width so the grid lines up exactly.
        // Scale the parent to resize; do not change this number.
        rect.sizeDelta = new Vector2(StripWidth, 640);

        var scroll = root.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Elastic;
        scroll.elasticity = 0.1f;
        scroll.inertia = true;
        scroll.decelerationRate = 0.135f;
        scroll.scrollSensitivity = 40f;

        // Viewport. RectMask2D rather than Mask — no stencil buffer, no extra
        // draw call, which matters on a phone.
        var viewport = NewChild(rect, "Viewport");
        Stretch(viewport, 0);
        viewport.pivot = new Vector2(0.5f, 1f);
        viewport.gameObject.AddComponent<RectMask2D>();

        // Content grows downward.
        var content = NewChild(viewport, "Content");
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = new Vector2(0, 640);

        // Parchment INSIDE the content, behind the slots. Built from the three
        // row strips rather than one stretched drawing, so the painted cells
        // keep their proportions however long the bag gets.
        var backdrop = NewChild(content, "Backdrop");
        backdrop.anchorMin = new Vector2(0.5f, 1f);
        backdrop.anchorMax = new Vector2(0.5f, 1f);
        backdrop.pivot = new Vector2(0.5f, 1f);
        backdrop.anchoredPosition = Vector2.zero;
        backdrop.sizeDelta = new Vector2(StripWidth, 640);

        var tiler = backdrop.gameObject.AddComponent<InventoryBackdropTiler>();
        var tilerSo = new SerializedObject(tiler);
        AssignSprite(tilerSo, "topSprite", FindSprite("inventory_top"));
        AssignSprite(tilerSo, "middleSprite", FindSprite("inventory_middle"));
        AssignSprite(tilerSo, "bottomSprite", FindSprite("inventory__bottom", "inventory_bottom"));
        tilerSo.FindProperty("stripWidth").floatValue = StripWidth;
        tilerSo.FindProperty("topHeight").floatValue = TopHeight;
        tilerSo.FindProperty("middleHeight").floatValue = MiddleHeight;
        tilerSo.FindProperty("bottomHeight").floatValue = BottomHeight;
        tilerSo.ApplyModifiedPropertiesWithoutUndo();

        // Grid of slots on top, using the measured cell geometry.
        var grid = NewChild(content, "Grid");
        grid.anchorMin = new Vector2(0.5f, 1f);
        grid.anchorMax = new Vector2(0.5f, 1f);
        grid.pivot = new Vector2(0.5f, 1f);
        grid.anchoredPosition = Vector2.zero;
        grid.sizeDelta = new Vector2(StripWidth, 640);

        var gridLayout = grid.gameObject.AddComponent<GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(CellW, CellH);
        gridLayout.spacing = new Vector2(SpacingX, SpacingY);
        gridLayout.padding = new RectOffset(
            Mathf.RoundToInt(PadLeft), Mathf.RoundToInt(PadRight),
            Mathf.RoundToInt(PadTop), Mathf.RoundToInt(BottomHeight));
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = Columns;
        gridLayout.childAlignment = TextAnchor.UpperLeft;

        var fitter = grid.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scroll.viewport = viewport;
        scroll.content = content;

        var binder = root.AddComponent<InventoryGridBinder>();
        var so = new SerializedObject(binder);
        so.FindProperty("gridParent").objectReferenceValue = grid;
        so.FindProperty("backdrop").objectReferenceValue = backdrop;
        so.FindProperty("backdropTiler").objectReferenceValue = tiler;

        var slotComponent = slotPrefab != null ? slotPrefab.GetComponent<InventorySlotButton>() : null;
        if (slotComponent != null)
            so.FindProperty("slotPrefab").objectReferenceValue = slotComponent;

        so.ApplyModifiedPropertiesWithoutUndo();

        return SavePrefab(root, "InventoryScroll");
    }

    private static void AssignSprite(SerializedObject so, string property, Sprite sprite)
    {
        if (sprite == null) return;

        var prop = so.FindProperty(property);
        if (prop != null) prop.objectReferenceValue = sprite;
    }

    // =================================================================

    private static GameObject SavePrefab(GameObject root, string name)
    {
        string path = $"{PrefabFolder}/{name}.prefab";
        var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);

        Debug.Log($"[InventoryBuilder] {path}");
        return prefab;
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
                if (!file.StartsWith(Normalize(candidate))) continue;

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
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.overflowMode = TextOverflowModes.Ellipsis;

        return label;
    }

    private static void Stretch(RectTransform rect, float padding)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(padding, padding);
        rect.offsetMax = new Vector2(-padding, -padding);
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
