using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance { get; private set; }

    [Header("UI Panels")]
    [SerializeField] private GameObject inventoryPanel; // Panel to display inventory

    [Header("Resources UI")]
    [SerializeField] private Transform resourceContainer; // Parent object for resource items
    [SerializeField] private GameObject resourcePrefab;   // Prefab for resource entries

    [Header("Special Items UI")]
    [SerializeField] private Transform specialItemContainer; // Parent object for special items
    [SerializeField] private GameObject specialItemPrefab;   // Prefab for special item entries

    [Header("Currency UI")]
    public TMP_Text totalSilverText; // Text to display total silver
    public TMP_Text totalGoldText;   // Text to display total gold
    [Header("Equipped Items UI")]
    [SerializeField] private Transform equippedItemsContainer;

    [Header("Resource Items UI")]
    [SerializeField] private Transform resourceItemsContainer;

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
        }
    }

    private void Start()
    {
        inventoryPanel.SetActive(false); // Hide inventory panel by default
        inventoryGrid = inventoryGridGO.GetComponent<GridLayoutGroup>();
        inventoryGrid.cellSize = new Vector2(150, 150);
        inventoryGrid.spacing = new Vector2(10, 10); // Optional spacing

    }


    // Open or close the inventory panel
    public void ToggleInventory()
    {
        inventoryPanel.SetActive(!inventoryPanel.activeSelf);
        if (inventoryPanel.activeSelf)
        {
            UpdateInventoryUI();
        }
    }
    private void PopulateInventoryGrid()
    {
        ClearUI(inventoryGridGO.transform);

        List<Item> items = InventorySystem.Instance.GetInventory();

        foreach (Item item in items)
        {
            GameObject newItem = Instantiate(inventoryItemPrefab, inventoryGridGO.transform);
            TMP_Text itemText = newItem.GetComponentInChildren<TMP_Text>();

            if (itemText != null)
            {
                itemText.text = item.Name;
            }
            else
            {
                Debug.LogError("TMP_Text component not found on inventoryItemPrefab.");
            }
        }
    }

    // Update inventory UI
    public void UpdateInventoryUI()
    {
        ClearUI(resourceContainer);
        ClearUI(specialItemContainer);

        foreach (var resource in InventorySystem.Instance.Resources)
        {
            AddResourceUI(resource);
        }

        var equippedItems = new Dictionary<string, Item>
    {
        { "Weapon", PlayerStatHandler.Instance.EquippedSword },
        { "Armor", PlayerStatHandler.Instance.EquippedArmor },
        { "Potion", PlayerStatHandler.Instance.EquippedPotion },
        { "Misc", PlayerStatHandler.Instance.EquippedMisc }
    };

        foreach (var slot in equippedItems)
        {
            var newItemSlot = Instantiate(specialItemPrefab, equippedItemsContainer);
            TMP_Text name = newItemSlot.gameObject.transform.GetChild(0).GetComponentInChildren<TMP_Text>();
            string Name = name.text;

            Name = slot.Value != null ? slot.Value.Name : "Empty";

            if (slot.Value != null)
            {
                PlayerStatHandler.Instance.UnequipItem(slot.Value.Category);
                UpdateInventoryUI();
            }
        }

        List<Item> specialItems = InventorySystem.Instance.SpecialItems;
        foreach (Item specialItem in specialItems)
        {
            GameObject newSpecialItem = Instantiate(specialItemPrefab, specialItemContainer);
            newSpecialItem.GetComponentInChildren<TMP_Text>().text = $"{specialItem.Name}";

            newSpecialItem.GetComponentInChildren<UnityEngine.UI.Button>().onClick.AddListener(() =>
            {
                Debug.Log($"Selected: {specialItem.Name}");
            });
        }

        PlayerData pd = PlayerStatHandler.Instance.pd;
        totalSilverText.text = $"Silver: {pd.Silver}";
        totalGoldText.text = $"Gold: {pd.Gold}";
        PopulateInventoryGrid();
        UpdateEquippedItemsUI();
    }
    private void AddResourceUI(Item resource)
    {
        GameObject newResource = Instantiate(resourcePrefab, resourceContainer);
        newResource.GetComponentInChildren<TMP_Text>().text = $"{resource.Name} x{resource.Quantity}";

        newResource.GetComponentInChildren<UnityEngine.UI.Button>().onClick.AddListener(() =>
        {
            InventorySystem.Instance.RemoveItem(resource, 1);
            PlayerStatHandler.Instance.AddSilverToPlayer(resource.Value.Silver);
            UpdateInventoryUI();
        });
    }
    private void UpdateEquippedItemsUI()
    {
        ClearUI(equippedItemsContainer);

        var equippedItems = new Dictionary<string, Item>
    {
        { "Weapon", PlayerStatHandler.Instance.EquippedSword },
        { "Armor", PlayerStatHandler.Instance.EquippedArmor },
        { "Potion", PlayerStatHandler.Instance.EquippedPotion },
        { "Misc", PlayerStatHandler.Instance.EquippedMisc }
    };

        foreach (var slot in equippedItems)
        {
            var newItemSlot = Instantiate(specialItemPrefab, equippedItemsContainer);
            TMP_Text name = newItemSlot.gameObject.transform.GetChild(0).GetComponentInChildren<TMP_Text>();
            string Name = name.text;

            if (slot.Value != null)
            {
                PlayerStatHandler.Instance.UnequipItem(slot.Value.Category);
                UpdateInventoryUI();
            }

        }
    }
    private void ClearUI(Transform container)
    {
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
    }

}
