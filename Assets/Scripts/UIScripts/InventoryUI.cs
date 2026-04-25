using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// New pattern: Subscribe to StateManager instead of using InventorySystem.Instance
/// </summary>
public partial class InventoryUI : MonoBehaviour, IStateListener
{
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

    private StateManager _stateManager;

    private void Start()
    {
        // Subscribe to state changes (new pattern)
        _stateManager = GameBootstrapper.State;
        if (_stateManager == null)
        {
            Debug.LogError("[InventoryUI] GameBootstrapper not initialized. Make sure GameBootstrapper GameObject is in scene with GameBootstrapper component.");
            return;
        }

        _stateManager.Subscribe(this);

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

    // New pattern: State changes trigger UI updates
    public void OnStateChanged(GameState oldState, GameState newState)
    {
        // Only update if inventory changed
        if (oldState?.Inventory != newState?.Inventory)
        {
            UpdateInventoryUI();
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
        if (_stateManager == null)
        {
            Debug.LogError("InventoryUI: StateManager not initialized");
            return;
        }

        var state = _stateManager.CurrentState;
        if (state?.Inventory == null)
        {
            Debug.LogError("InventoryUI: Inventory state is null");
            return;
        }

        if (resourceContainer != null)
            ClearUI(resourceContainer);

        if (specialItemContainer != null)
            ClearUI(specialItemContainer);

        // Display inventory items
        foreach (var itemInstance in state.Inventory.Items)
        {
            AddItemUI(itemInstance);
        }

        UpdateCurrencyTexts();
        PopulateInventoryGrid();
    }

    private void AddItemUI(ItemInstance itemInstance)
    {
        if (resourceContainer == null) return;
        if (resourcePrefab == null) return;

        GameObject newItem = Instantiate(resourcePrefab, resourceContainer);
        TMP_Text txt = newItem.GetComponentInChildren<TMP_Text>();
        if (txt != null)
        {
            txt.text = $"Item {itemInstance.ItemId} x{itemInstance.Quantity}";
        }
    }

    private void UpdateCurrencyTexts()
    {
        var state = _stateManager?.CurrentState;
        if (state?.Player == null)
            return;

        if (totalSilverText != null)
            totalSilverText.text = $"Silver: {state.Player.Silver}";

        if (totalGoldText != null)
            totalGoldText.text = $"Gold: {state.Player.Gold}";
    }

    private void PopulateInventoryGrid()
    {
        if (inventoryGridGO == null)
            return;

        ClearUI(inventoryGridGO.transform);

        var state = _stateManager?.CurrentState;
        if (state?.Inventory == null)
            return;

        foreach (var itemInstance in state.Inventory.Items)
        {
            if (inventoryItemPrefab == null)
                continue;

            GameObject newItem = Instantiate(inventoryItemPrefab, inventoryGridGO.transform);

            TMP_Text itemText = newItem.GetComponentInChildren<TMP_Text>();
            if (itemText != null)
            {
                itemText.text = $"ID: {itemInstance.ItemId}\n x{itemInstance.Quantity}";
            }

            Button itemButton = newItem.GetComponentInChildren<Button>();
            if (itemButton != null)
            {
                var itemId = itemInstance.ItemId; // Capture for closure
                itemButton.onClick.RemoveAllListeners();
                itemButton.onClick.AddListener(() =>
                {
                    GameBootstrapper.Events?.Dispatch(new RemoveItemEvent(itemId, 1));
                });
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