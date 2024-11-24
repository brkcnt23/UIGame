using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShopSystem : MonoBehaviour
{
    public static ShopSystem Instance { get; private set; }

    [SerializeField] private GameObject shopPanel;   // The shop UI panel
    [SerializeField] private Transform itemContainer; // Parent object for item entries
    [SerializeField] private GameObject itemPrefab;   // Prefab for shop items
    private List<Item> shopItems; // The list of items available in the shop

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            shopItems = new List<Item>
            {
                new Item(1, "Iron Sword", 10,ItemCategory.Weapon),
                new Item(2, "Health Potion", 50,ItemCategory.Potion),
                new Item(3, "Leather Armor", 200,ItemCategory.Armor)
            };
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        shopPanel.SetActive(false); // Hide the shop panel by default
    }

    public void ToggleShop()
    {
        shopPanel.SetActive(!shopPanel.activeSelf);
        if (shopPanel.activeSelf)
        {
            UpdateShopUI();
        }
    }

    public void UpdateShopUI()
    {
        // Clear previous entries
        foreach (Transform child in itemContainer)
        {
            Destroy(child.gameObject);
        }

        // Populate shop items
        foreach (Item item in shopItems)
        {
            GameObject newItem = Instantiate(itemPrefab, itemContainer);
            newItem.GetComponentInChildren<TMP_Text>().text = $"{item.Name} - {item.Value} silver";

            // Buy button functionality
            newItem.GetComponentInChildren<UnityEngine.UI.Button>().onClick.AddListener(() =>
            {
                BuyItem(item);
                UpdateShopUI();
            });
        }
    }

    public void BuyItem(Item item)
    {
        PlayerData pd = PlayerStatHandler.Instance.pd;
        if (pd.Silver >= item.Value)
        {
            pd.Silver -= item.Value;
            InventorySystem.Instance.AddItem(item);
            PlayerUISystem.Instance.UpdateRationText();
            PlayerUISystem.Instance.UpdateClockText();
        }
        else
        {
            Debug.Log("Not enough silver to buy this item.");
        }
    }
}
