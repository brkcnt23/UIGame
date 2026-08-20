using TMPro;
using UnityEngine;

/// <summary>
/// One "label ......... value" line in the item details panel.
///
/// Kept as its own component so the info panel can spawn as many as an item
/// needs — a dagger has three lines, a full plate has six.
/// </summary>
public class ItemStatRow : MonoBehaviour
{
    [SerializeField] private TMP_Text labelText;
    [SerializeField] private TMP_Text valueText;

    public void Set(string label, string value, Color valueColor)
    {
        if (labelText != null) labelText.text = label;

        if (valueText != null)
        {
            valueText.text = value;
            valueText.color = valueColor;
        }
    }
}
