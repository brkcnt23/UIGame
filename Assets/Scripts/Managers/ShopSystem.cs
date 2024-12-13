using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

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
            shopItems = ItemDatabase.GetAllItems(); // Load items from database
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
        if (pd.Silver >= item.Value)
        {
            pd.Silver -= item.Value;
            InventorySystem.Instance.AddItem(item);
            Debug.Log($"Purchased {item.Name}");
            pd.Items.Add(item);
            currentShop.Items.Remove(item);
            DisplayShopItems(currentShop);
        }
        else
        {
            Debug.Log("Not enough silver to buy this item.");
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
