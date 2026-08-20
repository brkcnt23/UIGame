using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// One inventory cell.
///
/// A slot is a button, so tapping it opens the item's details. It draws four
/// things: the icon, the stack count, a quality tint on the frame, and a
/// marker when the item is equipped — equipped gear staying visible in the bag
/// is what stops a player selling the armour off their own back.
///
/// Slots are pooled by InventoryGridBinder rather than created and destroyed,
/// because an inventory redraws every time anything is bought, sold or
/// crafted.
/// </summary>
public class InventorySlotButton : MonoBehaviour
{
    [Header("Parts")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Image frameImage;
    [SerializeField] private TMP_Text quantityLabel;
    [SerializeField] private GameObject equippedMarker;

    [Header("Behaviour")]
    [Tooltip("Tint the frame by item quality.")]
    [SerializeField] private bool tintFrameByQuality = true;

    private Button _button;
    private Item _item;

    public Item Item => _item;
    public bool IsEmpty => _item == null;

    /// <summary>Raised with the item when the slot is tapped. Null slots do nothing.</summary>
    public System.Action<Item> OnClicked;

    private void Awake()
    {
        _button = GetComponent<Button>();

        if (_button != null)
        {
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() =>
            {
                if (_item != null) OnClicked?.Invoke(_item);
            });
        }
    }

    public void SetItem(Item item)
    {
        _item = item;

        if (item == null)
        {
            Clear();
            return;
        }

        if (iconImage != null)
        {
            iconImage.enabled = true;
            iconImage.sprite = item.ItemImage;
            iconImage.preserveAspect = true;
        }

        if (quantityLabel != null)
        {
            // A lone item does not need a "1" cluttering the corner.
            bool showCount = item.Quantity > 1;
            quantityLabel.enabled = showCount;
            if (showCount) quantityLabel.text = item.Quantity.ToString();
        }

        if (frameImage != null && tintFrameByQuality)
        {
            var quality = (ItemQuality)Mathf.Clamp(item.Quality, 0, 4);

            // Common is the baseline and gets no tint — colouring everything
            // means colouring nothing.
            frameImage.color = quality == ItemQuality.Common
                ? Color.white
                : ItemRules.QualityColor(quality);
        }

        if (equippedMarker != null)
            equippedMarker.SetActive(item.IsEquipped);

        if (_button != null) _button.interactable = true;
    }

    public void Clear()
    {
        _item = null;

        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }

        if (quantityLabel != null) quantityLabel.enabled = false;
        if (frameImage != null) frameImage.color = Color.white;
        if (equippedMarker != null) equippedMarker.SetActive(false);

        // An empty slot still reads as a slot, it just cannot be tapped.
        if (_button != null) _button.interactable = false;
    }
}
