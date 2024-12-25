using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ShopUI : MonoBehaviour
{
    public Transform shopItemContainer; // Parent for shop item buttons (Grid)
    public GameObject shopItemPrefab;  // Prefab for shop items
    public GameObject itemDetailPanel; // Panel to show item details
    public Text itemNameText;          // Text for item name
    public Text itemModifiersText;     // Text for item modifiers
    public Text itemValueText;         // Text for item value
    public Button buyButton;           // Button to buy the item

    private List<Item> currentShopItems;
    private Item selectedItem;

    public void InitializeShopUI(List<Item> shopItems)
    {
        // Clear existing shop items
        foreach (Transform child in shopItemContainer)
        {
            Destroy(child.gameObject);
        }

        currentShopItems = shopItems;

        // Populate shop grid with items
        foreach (var item in shopItems)
        {
            GameObject itemButton = Instantiate(shopItemPrefab, shopItemContainer);
            itemButton.GetComponentInChildren<Text>().text = item.Name;
            itemButton.GetComponent<Button>().onClick.AddListener(() => ShowItemDetails(item));
        }
    }

    public void ShowItemDetails(Item item)
    {
        selectedItem = item;

        // Populate item details panel
        itemNameText.text = item.Name;
        itemValueText.text = $"{item.Value.Gold}g {item.Value.Silver}s";
        itemModifiersText.text = GetModifiersText(item);

        // Enable the buy button and bind its click event
        buyButton.gameObject.SetActive(true);
        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() => BuyItem(item));

        // Show the item detail panel
        itemDetailPanel.SetActive(true);
    }

    private string GetModifiersText(Item item)
    {
        if (item.Modifiers == null || item.Modifiers.Count == 0)
            return "No modifiers";

        string modifiers = "";
        foreach (var mod in item.Modifiers)
        {
            modifiers += $"{mod.Type}: +{mod.Value}\n";
        }

        return modifiers;
    }

    public void BuyItem(Item item)
    {
        Debug.Log($"Buying item: {item.Name}");
    }
}
