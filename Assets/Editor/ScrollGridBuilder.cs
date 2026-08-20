#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Converts a grid of slots into a scrolling one.
///
/// The problem with a fixed grid is that the artwork behind it is a fixed
/// image: add a row and the slots run off the parchment. This restructures the
/// grid into the shape Unity expects for scrolling —
///
///   ScrollRect (viewport-sized, masked)
///     └ Viewport            RectMask2D, clips the content
///         └ Content         GridLayoutGroup + ContentSizeFitter, grows down
///             └ slots...
///
/// Content grows downward as slots are added, the viewport stays the size of
/// the parchment, and the mask hides anything past the edge.
///
/// The background image stays outside the scroll area so it does not scroll
/// with the slots — that is what keeps the frame still while the contents move.
///
/// Tools > UIGame > Inventory
/// </summary>
public static class ScrollGridBuilder
{
    [MenuItem("Tools/UIGame/Inventory/Make selected grid scrollable")]
    public static void MakeScrollable()
    {
        var grid = Selection.activeGameObject != null
            ? Selection.activeGameObject.GetComponent<RectTransform>()
            : null;

        if (grid == null)
        {
            EditorUtility.DisplayDialog("Scroll grid builder",
                "Select the object that holds the slots — the one with the " +
                "GridLayoutGroup, for example 'InventoryItems'.",
                "Right");
            return;
        }

        var gridLayout = grid.GetComponent<GridLayoutGroup>();
        if (gridLayout == null)
        {
            EditorUtility.DisplayDialog("Scroll grid builder",
                $"'{grid.name}' has no GridLayoutGroup. Select the object that " +
                "actually lays out the slots.",
                "Right");
            return;
        }

        if (grid.GetComponentInParent<ScrollRect>() != null)
        {
            EditorUtility.DisplayDialog("Scroll grid builder",
                $"'{grid.name}' is already inside a ScrollRect.",
                "Right");
            return;
        }

        Undo.SetCurrentGroupName("Make grid scrollable");
        int group = Undo.GetCurrentGroup();

        var parent = grid.parent as RectTransform;
        int siblingIndex = grid.GetSiblingIndex();

        // Viewport keeps the size the grid had — that is the window the player
        // sees, and it matches the artwork behind it.
        Vector2 windowSize = grid.rect.size;
        Vector2 windowPos = grid.anchoredPosition;

        // --- ScrollRect ------------------------------------------------
        var scrollGo = new GameObject($"{grid.name}Scroll", typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(scrollGo, "Create ScrollRect");

        var scrollRect = scrollGo.GetComponent<RectTransform>();
        scrollRect.SetParent(parent, false);
        scrollRect.SetSiblingIndex(siblingIndex);
        scrollRect.anchorMin = grid.anchorMin;
        scrollRect.anchorMax = grid.anchorMax;
        scrollRect.pivot = grid.pivot;
        scrollRect.anchoredPosition = windowPos;
        scrollRect.sizeDelta = windowSize;

        var scroll = scrollGo.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Elastic;
        scroll.elasticity = 0.1f;
        scroll.inertia = true;
        scroll.decelerationRate = 0.135f;
        scroll.scrollSensitivity = 30f;

        // --- Viewport --------------------------------------------------
        var viewportGo = new GameObject("Viewport", typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(viewportGo, "Create Viewport");

        var viewport = viewportGo.GetComponent<RectTransform>();
        viewport.SetParent(scrollRect, false);
        viewport.anchorMin = Vector2.zero;
        viewport.anchorMax = Vector2.one;
        viewport.offsetMin = Vector2.zero;
        viewport.offsetMax = Vector2.zero;
        viewport.pivot = new Vector2(0.5f, 1f);

        // RectMask2D rather than Mask: no extra draw call and no stencil, which
        // matters on mobile.
        viewportGo.AddComponent<RectMask2D>();

        // --- Content ---------------------------------------------------
        Undo.SetTransformParent(grid, viewport, "Move grid into viewport");

        grid.anchorMin = new Vector2(0f, 1f);
        grid.anchorMax = new Vector2(1f, 1f);
        grid.pivot = new Vector2(0.5f, 1f);
        grid.anchoredPosition = Vector2.zero;
        grid.offsetMin = new Vector2(0f, grid.offsetMin.y);
        grid.offsetMax = new Vector2(0f, grid.offsetMax.y);

        var fitter = grid.GetComponent<ContentSizeFitter>() ?? Undo.AddComponent<ContentSizeFitter>(grid.gameObject);
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Fixed column count so the layout is predictable while the row count
        // grows — flexible would reflow columns as the window resizes.
        if (gridLayout.constraint != GridLayoutGroup.Constraint.FixedColumnCount)
        {
            int columns = EstimateColumns(gridLayout, windowSize.x);
            Undo.RecordObject(gridLayout, "Constrain grid columns");
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = Mathf.Max(1, columns);
        }

        scroll.viewport = viewport;
        scroll.content = grid;

        Selection.activeGameObject = scrollGo;
        Undo.CollapseUndoOperations(group);

        Debug.Log($"[ScrollGrid] '{grid.name}' is now scrollable. " +
                  $"Viewport {windowSize.x:0}x{windowSize.y:0}, " +
                  $"{gridLayout.constraintCount} columns. " +
                  "Keep the parchment background OUTSIDE this ScrollRect so it stays still.");
    }

    [MenuItem("Tools/UIGame/Inventory/Add slots to selected grid")]
    public static void AddSlots()
    {
        var grid = Selection.activeGameObject != null
            ? Selection.activeGameObject.GetComponent<RectTransform>()
            : null;

        if (grid == null || grid.GetComponent<GridLayoutGroup>() == null)
        {
            EditorUtility.DisplayDialog("Add slots",
                "Select the grid object first.", "Right");
            return;
        }

        // Clone the last existing slot so the new ones inherit its artwork and
        // components rather than arriving as blank squares.
        if (grid.childCount == 0)
        {
            EditorUtility.DisplayDialog("Add slots",
                "The grid has no slot to copy. Create one slot by hand first, " +
                "then this will duplicate it.",
                "Right");
            return;
        }

        var template = grid.GetChild(grid.childCount - 1).gameObject;

        for (int i = 0; i < 10; i++)
        {
            var copy = Object.Instantiate(template, grid);
            copy.name = $"Slot ({grid.childCount})";
            Undo.RegisterCreatedObjectUndo(copy, "Add inventory slot");
        }

        Debug.Log($"[ScrollGrid] Added 10 slots. '{grid.name}' now has {grid.childCount}.");
    }

    private static int EstimateColumns(GridLayoutGroup layout, float availableWidth)
    {
        float cell = layout.cellSize.x + layout.spacing.x;
        if (cell <= 0f) return 5;

        float usable = availableWidth - layout.padding.left - layout.padding.right;
        return Mathf.Max(1, Mathf.FloorToInt((usable + layout.spacing.x) / cell));
    }
}
#endif
