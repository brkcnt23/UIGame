using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ShopSystem : MonoBehaviour
{
    public static ShopSystem Instance { get; private set; }

    [SerializeField] private GameObject shopPanel;   // The shop UI panel
    [SerializeField] private GameObject shopListPanel;   // The shop UI panel

    [SerializeField] private Transform itemContainer; // Parent object for item entries
    [SerializeField] private GameObject itemPrefab;   // Prefab for shop items
    [SerializeField] private GameObject ShopButtonPrefab;

    private List<Item> shopItems; // Items available in the current shop
    public Shops currentShop;    // Reference to the current shop

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
        shopPanel.SetActive(false); // Hide the shop panel by default
    }

    public void ToggleShop()
    {
        shopPanel.SetActive(!shopPanel.activeSelf);
        if (shopPanel.activeSelf)
        {
            PopulateShopList();

        }
        else
        {
            ClearUI(itemContainer);
        }
    }
    public void PopulateShopList()
    {
        shopPanel.SetActive(false);
        shopListPanel.SetActive(true);

        Settlement currentSettlement = SettlementHandler.Instance.settlement;

        foreach (Shops shop in currentSettlement.Shops)
        {
            GameObject shopButton = Instantiate(ShopButtonPrefab);
            shopButton.transform.SetParent(shopListPanel.transform);
            shopButton.GetComponent<Button>().onClick.RemoveAllListeners();
            shopButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                DisplayShopItems(shop);
                SettlementHandler.Instance.HandleShopEntered(shop);
                currentShop = shop;
            });
        }

    }

    public void BuyItem(Item item)
    {
        PlayerData pd = PlayerStatHandler.Instance.pd;

        // Convert the item's value (int) to a Currency instance
        Currency itemCost = new Currency(0, item.Value.Silver);

        // Check if the player has enough currency
        if (pd.Currency.HasEnough(itemCost.Gold, itemCost.Silver))
        {
            // Deduct the item cost from the player's currency
            pd.Currency.Subtract(itemCost.Gold, itemCost.Silver);

            // Add the item to the player's inventory
            InventorySystem.Instance.AddItem(item);
            Debug.Log($"Purchased {item.Name} for {itemCost}");

            // Remove the item from the shop's inventory and refresh the display
            currentShop.Items.Remove(item);
            DisplayShopItems(currentShop);
        }
        else
        {
            Debug.Log("Not enough currency to buy this item.");
        }
    }





    public void DisplayShopItems(Shops shop)
    {
        Debug.Log("DISPLAYED");
        ClearUI(itemContainer);
        shopListPanel.SetActive(false);
        shopPanel.SetActive(true);

        foreach (Item item in shop.Items)
        {
            Item currentItem = item; // Create a local copy to fix closure issue
            GameObject newItem = Instantiate(itemPrefab, itemContainer);
            newItem.GetComponentInChildren<TMP_Text>().text = $"{currentItem.Name} - {currentItem.Value} silver";

            newItem.GetComponentInChildren<UnityEngine.UI.Button>().onClick.AddListener(() =>
            {
                BuyItem(currentItem);
            });
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
