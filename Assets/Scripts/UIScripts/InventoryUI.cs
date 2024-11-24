using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance { get; private set; }

    [SerializeField] private GameObject inventoryPanel; // Panel to display inventory
    [SerializeField] private Transform itemContainer;   // Parent object for item entries
    [SerializeField] private GameObject itemPrefab;     // Prefab for item entry
    public TMP_Text totalSilverText;                    // Text to display total silver
    public TMP_Text totalGoldText;                      // Text to display total gold

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
        // Clear previous entries
        foreach (Transform child in itemContainer)
        {
            Destroy(child.gameObject);
        }

        // Populate inventory
        List<Item> inventory = InventorySystem.Instance.GetInventory();
        foreach (Item item in inventory)
        {
            GameObject newItem = Instantiate(itemPrefab, itemContainer);
            newItem.GetComponentInChildren<TMP_Text>().text = $"{item.Name} - {item.Value} silver";
            
            // Sell button functionality
            newItem.GetComponentInChildren<UnityEngine.UI.Button>().onClick.AddListener(() =>
            {
                InventorySystem.Instance.SellItem(item);
                UpdateInventoryUI();
            });
        }

        // Update player stats
        PlayerData pd = PlayerStatHandler.Instance.pd;
        totalSilverText.text = $"Silver: {pd.Silver}";
        totalGoldText.text = $"Gold: {pd.Gold}";
    }
}
