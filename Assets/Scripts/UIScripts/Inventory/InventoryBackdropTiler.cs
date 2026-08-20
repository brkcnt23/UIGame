using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Grows the parchment behind the inventory grid by stacking row strips.
///
/// The artwork comes in three pieces — a top with the upper edge, a middle
/// that repeats, and a bottom with the lower edge. Rather than stretching one
/// tall drawing (which distorts the painted cells), this lays down one top,
/// as many middles as there are rows, and one bottom.
///
/// The result is a parchment that is exactly as long as the bag needs and
/// still looks hand-painted at both ends.
///
/// Measured from the source art:
///   top     654 x 97   slot row at y 29..90
///   middle  654 x 75   slot row at y  6..67
///   bottom  654 x 106  slot row at y  6..68
/// so every row after the first advances by exactly 75 pixels.
/// </summary>
[ExecuteAlways]
public class InventoryBackdropTiler : MonoBehaviour
{
    [Header("Strips")]
    [SerializeField] private Sprite topSprite;
    [SerializeField] private Sprite middleSprite;
    [SerializeField] private Sprite bottomSprite;

    [Header("Measurements")]
    [Tooltip("Native width of the strips. The whole backdrop is built at this " +
             "width so the drawn cells line up with the grid; scale the parent " +
             "to resize.")]
    [SerializeField] private float stripWidth = 654f;

    [SerializeField] private float topHeight = 97f;
    [SerializeField] private float middleHeight = 75f;
    [SerializeField] private float bottomHeight = 106f;

    [Tooltip("Rows drawn when nothing has asked for a specific count yet.")]
    [SerializeField] private int defaultRows = 6;

    private readonly List<GameObject> _pieces = new();
    private RectTransform _rect;
    private int _currentRows = -1;

    private void Awake()
    {
        _rect = transform as RectTransform;
    }

    private void OnEnable()
    {
        // Rebuild unconditionally rather than only when the count changed —
        // entering play mode destroys the pieces but leaves _currentRows set,
        // which would otherwise leave the parchment blank.
        _currentRows = -1;
        SetRowCount(defaultRows);
    }

    /// <summary>
    /// Redraws with the current settings. Runs in edit mode so the parchment
    /// is visible while the panel is being laid out — an invisible backdrop
    /// makes positioning guesswork.
    /// </summary>
    [ContextMenu("Rebuild")]
    public void Rebuild()
    {
        int rows = _currentRows < 0 ? defaultRows : _currentRows;
        _currentRows = -1;
        SetRowCount(rows);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!isActiveAndEnabled) return;

        // Deferred: Unity forbids creating or destroying objects during
        // OnValidate itself.
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (this == null) return;
            Rebuild();
        };
    }
#endif

    /// <summary>
    /// Rebuilds the parchment for this many slot rows. The first row lives in
    /// the top piece, so a five-row bag needs four middles.
    /// </summary>
    public void SetRowCount(int rows)
    {
        rows = Mathf.Max(1, rows);
        if (rows == _currentRows) return;

        _currentRows = rows;

        Clear();
        if (_rect == null) _rect = transform as RectTransform;

        float y = 0f;

        y += AddPiece("Top", topSprite, topHeight, y);

        // The first row is already drawn in the top piece.
        for (int i = 0; i < rows - 1; i++)
            y += AddPiece($"Middle ({i})", middleSprite, middleHeight, y);

        y += AddPiece("Bottom", bottomSprite, bottomHeight, y);

        if (_rect != null)
            _rect.sizeDelta = new Vector2(stripWidth, y);
    }

    /// <summary>Total height for a given row count, without rebuilding.</summary>
    public float HeightForRows(int rows)
    {
        rows = Mathf.Max(1, rows);
        return topHeight + (rows - 1) * middleHeight + bottomHeight;
    }

    /// <summary>Where the first slot row starts, measured from the top edge.</summary>
    public float FirstRowOffset => 29f;

    // -----------------------------------------------------------------

    private float AddPiece(string name, Sprite sprite, float height, float yOffset)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rect = go.GetComponent<RectTransform>();

        rect.SetParent(transform, false);
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -yOffset);
        rect.sizeDelta = new Vector2(stripWidth, height);

        var image = go.AddComponent<Image>();
        image.raycastTarget = false;

        if (sprite != null) image.sprite = sprite;
        else image.color = new Color(0.85f, 0.78f, 0.60f, 0.4f);

        _pieces.Add(go);
        return height;
    }

    private void Clear()
    {
        foreach (var piece in _pieces)
        {
            if (piece == null) continue;

            if (Application.isPlaying) Destroy(piece);
            else DestroyImmediate(piece);
        }

        _pieces.Clear();
    }
}
