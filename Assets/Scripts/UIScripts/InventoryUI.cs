using System.Collections.Generic;
using UnityEngine;
using TMPro;

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

    // Update inventory UI
    public void UpdateInventoryUI()
    {
        // Clear existing UI entries
        ClearUI(resourceContainer);
        ClearUI(specialItemContainer);

        // Update resources
        List<Item> resources = InventorySystem.Instance.Resources;
        foreach (Item resource in resources)
        {
            GameObject newResource = Instantiate(resourcePrefab, resourceContainer);
            newResource.GetComponentInChildren<TMP_Text>().text = $"{resource.Name} x{resource.Quantity}";
            
            // Sell button functionality
            newResource.GetComponentInChildren<UnityEngine.UI.Button>().onClick.AddListener(() =>
            {
                InventorySystem.Instance.RemoveItem(resource, 1); // Remove 1 unit
                PlayerStatHandler.Instance.AddSilverToPlayer(resource.Value);
                UpdateInventoryUI();
            });
        }

        // Update special items
        List<Item> specialItems = InventorySystem.Instance.SpecialItems;
        foreach (Item specialItem in specialItems)
        {
            GameObject newSpecialItem = Instantiate(specialItemPrefab, specialItemContainer);
            newSpecialItem.GetComponentInChildren<TMP_Text>().text = $"{specialItem.Name}";
            
            // Equip/Unequip functionality (example, can customize further)
            newSpecialItem.GetComponentInChildren<UnityEngine.UI.Button>().onClick.AddListener(() =>
            {
                Debug.Log($"Selected: {specialItem.Name}");
            });
        }

        // Update currency display
        PlayerData pd = PlayerStatHandler.Instance.pd;
        totalSilverText.text = $"Silver: {pd.Silver}";
        totalGoldText.text = $"Gold: {pd.Gold}";
    }

    // Helper to clear existing UI entries
    private void ClearUI(Transform container)
    {
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
    }
}
