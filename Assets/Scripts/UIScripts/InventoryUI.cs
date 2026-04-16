using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance { get; private set; }

    [Header("UI Panels")]
    [SerializeField] private GameObject inventoryPanel;

    [Header("Resources UI")]
    [SerializeField] private Transform resourceContainer;
    [SerializeField] private GameObject resourcePrefab;

    [Header("Special Items UI")]
    [SerializeField] private Transform specialItemContainer;
    [SerializeField] private GameObject specialItemPrefab;

    [Header("Currency UI")]
    public TMP_Text totalSilverText;
    public TMP_Text totalGoldText;
    public TMP_Text totalWeightText;

    [Header("Equipped Items UI")]
    [SerializeField] private Transform equippedItemsContainer;

    [Header("Inventory Grid UI")]
    [SerializeField] private GameObject inventoryGridGO;
    [SerializeField] private GridLayoutGroup inventoryGrid;
    [SerializeField] private GameObject inventoryItemPrefab;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);

        if (inventoryGridGO != null)
        {
            inventoryGrid = inventoryGridGO.GetComponent<GridLayoutGroup>();
            if (inventoryGrid != null)
            {
                inventoryGrid.cellSize = new Vector2(150, 150);
                inventoryGrid.spacing = new Vector2(10, 10);
            }
        }
    }

    public void ToggleInventory()
    {
        if (inventoryPanel == null)
        {
            Debug.LogError("InventoryUI: inventoryPanel is null!");
            return;
        }

        inventoryPanel.SetActive(!inventoryPanel.activeSelf);

        if (inventoryPanel.activeSelf)
        {
            UpdateInventoryUI();
        }
    }

    public void UpdateInventoryUI()
    {
        if (InventorySystem.Instance == null || PlayerStatHandler.Instance == null || PlayerStatHandler.Instance.pd == null)
        {
            Debug.LogError("InventoryUI: required systems are null, cannot update inventory UI.");
            return;
        }

        if (resourceContainer != null)
            ClearUI(resourceContainer);

        if (specialItemContainer != null)
            ClearUI(specialItemContainer);

        foreach (var resource in InventorySystem.Instance.ResourceItems)
        {
            AddResourceUI(resource);
        }

        foreach (var specialItem in InventorySystem.Instance.SpecialItems)
        {
            AddSpecialItemUI(specialItem);
        }

        UpdateCurrencyTexts();
        UpdateWeightText();
        PopulateInventoryGrid();
        UpdateEquippedItemsUI();
    }

    private void UpdateCurrencyTexts()
    {
        if (PlayerStatHandler.Instance == null || PlayerStatHandler.Instance.pd == null)
            return;

        Currency money = PlayerStatHandler.Instance.pd.GetMoney();

        if (totalSilverText != null)
            totalSilverText.text = $"Silver: {money.Silver}";

        if (totalGoldText != null)
            totalGoldText.text = $"Gold: {money.Gold}";
    }

    private void UpdateWeightText()
    {
        if (totalWeightText == null || PlayerStatHandler.Instance == null)
            return;

        float currentWeight = PlayerStatHandler.Instance.GetCurrentWeight();
        float carryCapacity = PlayerStatHandler.Instance.GetCarryCapacity();

        totalWeightText.text = $"Load: {currentWeight:0.0} / {carryCapacity:0.0}";
    }

    private void PopulateInventoryGrid()
    {
        if (inventoryGridGO == null)
        {
            Debug.LogError("InventoryUI: inventoryGridGO is null!");
            return;
        }

        ClearUI(inventoryGridGO.transform);

        if (InventorySystem.Instance == null)
        {
            Debug.LogError("InventoryUI: InventorySystem.Instance is null!");
            return;
        }

        List<Item> items = InventorySystem.Instance.GetInventory();

        foreach (Item item in items)
        {
            GameObject newItem = Instantiate(inventoryItemPrefab, inventoryGridGO.transform);

            TMP_Text itemText = newItem.GetComponentInChildren<TMP_Text>();
            if (itemText != null)
            {
                itemText.text = $"{item.Name}\n x{item.Quantity}";
            }

            Button itemButton = newItem.GetComponentInChildren<Button>();
            if (itemButton != null)
            {
                itemButton.onClick.RemoveAllListeners();
                itemButton.onClick.AddListener(() =>
                {
                    Debug.Log($"Selected item: {item.Name}");
                });
            }

            Image itemImage = newItem.GetComponentInChildren<Image>();
            if (itemImage != null && item.ItemImage != null)
            {
                itemImage.sprite = item.ItemImage;
            }
        }
    }

    private void AddResourceUI(Item resource)
    {
        if (resourceContainer == null || resourcePrefab == null || resource == null)
            return;

        GameObject newResource = Instantiate(resourcePrefab, resourceContainer);

        TMP_Text txt = newResource.GetComponentInChildren<TMP_Text>();
        if (txt != null)
        {
            txt.text = $"{resource.Name} x{resource.Quantity}";
        }

        Button btn = newResource.GetComponentInChildren<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                InventorySystem.Instance.RemoveItem(resource, 1);
                PlayerStatHandler.Instance.AddSilverToPlayer(resource.Value.Silver);
                UpdateInventoryUI();
            });
        }
    }

    private void AddSpecialItemUI(Item specialItem)
    {
        if (specialItemContainer == null || specialItemPrefab == null || specialItem == null)
            return;

        GameObject newSpecialItem = Instantiate(specialItemPrefab, specialItemContainer);

        TMP_Text txt = newSpecialItem.GetComponentInChildren<TMP_Text>();
        if (txt != null)
        {
            txt.text = $"{specialItem.Name}";
        }

        Button btn = newSpecialItem.GetComponentInChildren<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                Debug.Log($"Selected: {specialItem.Name}");
            });
        }
    }

    private void UpdateEquippedItemsUI()
    {
        if (equippedItemsContainer == null || specialItemPrefab == null || PlayerStatHandler.Instance == null)
            return;

        ClearUI(equippedItemsContainer);

        var equippedItems = new Dictionary<string, Item>
        {
            { "Weapon", PlayerStatHandler.Instance.EquippedSword },
            { "Armor", PlayerStatHandler.Instance.EquippedArmor },
            { "Leggings", PlayerStatHandler.Instance.EquippedLeggings },
            { "Boots", PlayerStatHandler.Instance.EquippedBoots },
            { "Potion", PlayerStatHandler.Instance.EquippedPotion },
            { "Misc", PlayerStatHandler.Instance.EquippedMisc }
        };

        foreach (var slot in equippedItems)
        {
            GameObject newItemSlot = Instantiate(specialItemPrefab, equippedItemsContainer);

            TMP_Text nameText = newItemSlot.GetComponentInChildren<TMP_Text>();
            if (nameText != null)
            {
                nameText.text = slot.Value != null ? $"{slot.Key}: {slot.Value.Name}" : $"{slot.Key}: Empty";
            }
        }
    }

    private void ClearUI(Transform container)
    {
        if (container == null) return;

        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
    }
}