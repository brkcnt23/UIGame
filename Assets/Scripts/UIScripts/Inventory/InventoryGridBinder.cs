using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Fills the inventory grid from the player's bag and keeps the scroll content
/// the right height.
///
/// Slots are pooled. An inventory redraws on every purchase, sale and craft,
/// and destroying two hundred GameObjects each time would stutter on a phone.
///
/// The grid always shows a few empty rows past the last item, so the bag reads
/// as having room rather than as being exactly full — and so a newly bought
/// item has somewhere visible to land.
/// </summary>
public class InventoryGridBinder : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform gridParent;
    [SerializeField] private InventorySlotButton slotPrefab;

    [Tooltip("Optional. Stretched to match the content height so a tall " +
             "parchment grows with the list.")]
    [SerializeField] private RectTransform backdrop;

    [Tooltip("Optional. Rebuilds the parchment from row strips instead of " +
             "stretching a single image — keeps the painted cells undistorted.")]
    [SerializeField] private InventoryBackdropTiler backdropTiler;

    [Tooltip("Optional. Opened when a slot is tapped.")]
    [SerializeField] private ItemInfoPanel infoPanel;

    [Header("Layout")]
    [Tooltip("Empty rows kept below the last item.")]
    [SerializeField] private int trailingEmptyRows = 2;

    [Tooltip("Never fewer slots than this, so a new bag still looks like a bag.")]
    [SerializeField] private int minimumSlots = 24;

    [Header("Filter")]
    [SerializeField] private bool showEquipped = true;
    [SerializeField] private ItemCategory[] categoryFilter;

    private readonly List<InventorySlotButton> _slots = new();
    private GridLayoutGroup _grid;

    private void Awake()
    {
        if (gridParent == null) gridParent = transform as RectTransform;
        _grid = gridParent != null ? gridParent.GetComponent<GridLayoutGroup>() : null;
    }

    private void OnEnable()
    {
        Refresh();

        var events = GameBootstrapper.Events;
        if (events != null)
        {
            events.Subscribe<ItemAddedEvent>(OnInventoryChanged);
            events.Subscribe<ItemRemovedEvent>(OnInventoryChanged);
        }
    }

    private void OnDisable()
    {
        var events = GameBootstrapper.Events;
        if (events != null)
        {
            events.Unsubscribe<ItemAddedEvent>(OnInventoryChanged);
            events.Unsubscribe<ItemRemovedEvent>(OnInventoryChanged);
        }
    }

    private void OnInventoryChanged(ItemAddedEvent _) => Refresh();
    private void OnInventoryChanged(ItemRemovedEvent _) => Refresh();

    // -----------------------------------------------------------------

    public void Refresh()
    {
        if (gridParent == null || slotPrefab == null) return;

        var items = GetItems();
        int needed = CalculateSlotCount(items.Count);

        EnsureSlots(needed);

        for (int i = 0; i < _slots.Count; i++)
        {
            if (i < items.Count) _slots[i].SetItem(items[i]);
            else _slots[i].Clear();

            _slots[i].gameObject.SetActive(i < needed);
        }

        ResizeBackdrop(needed);
    }

    private List<Item> GetItems()
    {
        var result = new List<Item>();
        var pd = PlayerStatHandler.Instance != null ? PlayerStatHandler.Instance.pd : null;

        if (pd?.Items == null) return result;

        foreach (var item in pd.Items)
        {
            if (item == null) continue;
            if (!showEquipped && item.IsEquipped) continue;

            if (categoryFilter != null && categoryFilter.Length > 0)
            {
                bool match = false;
                foreach (var c in categoryFilter)
                    if (item.Category == c) { match = true; break; }

                if (!match) continue;
            }

            result.Add(item);
        }

        return result;
    }

    /// <summary>
    /// Rounds up to whole rows and adds the trailing empties, so the grid
    /// never ends with a half row.
    /// </summary>
    private int CalculateSlotCount(int itemCount)
    {
        int columns = _grid != null && _grid.constraint == GridLayoutGroup.Constraint.FixedColumnCount
            ? Mathf.Max(1, _grid.constraintCount)
            : 5;

        int rowsForItems = Mathf.CeilToInt(itemCount / (float)columns);
        int rows = rowsForItems + Mathf.Max(0, trailingEmptyRows);

        return Mathf.Max(minimumSlots, rows * columns);
    }

    private void EnsureSlots(int count)
    {
        while (_slots.Count < count)
        {
            var slot = Instantiate(slotPrefab, gridParent);
            slot.name = $"Slot ({_slots.Count})";
            slot.OnClicked += HandleSlotClicked;
            _slots.Add(slot);
        }
    }

    /// <summary>
    /// Stretches the parchment to whatever height the grid ended up at.
    ///
    /// Only useful when the backdrop is a 9-sliced or tiled image. A fixed
    /// drawing with cells painted into it will distort, which is why the
    /// artwork wants slicing.
    /// </summary>
    private void ResizeBackdrop(int slotCount)
    {
        if (_grid == null) return;

        int columns = _grid.constraint == GridLayoutGroup.Constraint.FixedColumnCount
            ? Mathf.Max(1, _grid.constraintCount)
            : 5;

        int rows = Mathf.CeilToInt(slotCount / (float)columns);

        // Preferred: rebuild from strips, so the painted cells keep their
        // proportions however long the bag gets.
        if (backdropTiler != null)
        {
            backdropTiler.SetRowCount(rows);
            return;
        }

        // Fallback for a single stretched image.
        if (backdrop == null) return;

        float height = _grid.padding.top + _grid.padding.bottom
                     + rows * _grid.cellSize.y
                     + Mathf.Max(0, rows - 1) * _grid.spacing.y;

        backdrop.sizeDelta = new Vector2(backdrop.sizeDelta.x, height);
    }

    private void HandleSlotClicked(Item item)
    {
        if (item == null) return;

        if (infoPanel != null) infoPanel.Show(item);
        else Debug.Log($"[Inventory] {item.Name} ×{item.Quantity}");
    }

#if UNITY_EDITOR
    // -----------------------------------------------------------------
    // Editor preview
    //
    // Slots are spawned at runtime, so the grid is empty while the panel is
    // being laid out. These fill it with dummies so the alignment against the
    // painted parchment can actually be seen.
    // -----------------------------------------------------------------

    [ContextMenu("Preview slots in editor")]
    private void PreviewSlots()
    {
        if (gridParent == null || slotPrefab == null)
        {
            Debug.LogWarning("[Inventory] Assign gridParent and slotPrefab first.");
            return;
        }

        ClearPreview();

        for (int i = 0; i < minimumSlots; i++)
        {
            var slot = (InventorySlotButton)UnityEditor.PrefabUtility
                .InstantiatePrefab(slotPrefab, gridParent);

            slot.name = $"~Preview Slot ({i})";
            slot.gameObject.hideFlags = HideFlags.DontSave;
        }

        Debug.Log($"[Inventory] {minimumSlots} preview slots placed. " +
                  "They are not saved with the scene — clear them when done.");
    }

    [ContextMenu("Clear preview slots")]
    private void ClearPreview()
    {
        if (gridParent == null) return;

        for (int i = gridParent.childCount - 1; i >= 0; i--)
        {
            var child = gridParent.GetChild(i);
            if (child.name.StartsWith("~Preview"))
                DestroyImmediate(child.gameObject);
        }
    }
#endif
}
