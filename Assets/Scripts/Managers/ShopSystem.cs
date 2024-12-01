using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShopSystem : MonoBehaviour
{
    public static ShopSystem Instance { get; private set; }

    [SerializeField] private GameObject shopPanel;   // The shop UI panel
    [SerializeField] private Transform itemContainer; // Parent object for item entries
    [SerializeField] private GameObject itemPrefab;   // Prefab for shop items
    private List<Item> shopItems; // Items available in the shop

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
            UpdateShopUI();
        }
    }

    public void UpdateShopUI()
    {
        foreach (Transform child in itemContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (Item item in shopItems)
        {
            GameObject newItem = Instantiate(itemPrefab, itemContainer);
            newItem.GetComponentInChildren<TMP_Text>().text = $"{item.Name} - {item.Value} silver";

            newItem.GetComponentInChildren<UnityEngine.UI.Button>().onClick.AddListener(() =>
            {
                BuyItem(item);
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
        }
        else
        {
            Debug.Log("Not enough silver to buy this item.");
        }
    }
}
