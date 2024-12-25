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
        ClearUI(shopListPanel.transform);
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
        // Ensure the shop's items list is initialized
        if (shop.Items == null)
        {
            Debug.LogWarning("Shop items list is null. Initializing an empty list.");
            shop.Items = new List<Item>();
        }

        if (shop.Items.Count == 0)
        {
            Debug.LogWarning("Shop has no items to display.");
            return; // Exit if no items are available
        }

        ClearUI(itemContainer); // Clear previous items
        shopListPanel.SetActive(false);
        shopPanel.SetActive(true);

        foreach (Item item in shop.Items)
        {
            GameObject newItem = Instantiate(itemPrefab, itemContainer);

            // Ensure components are present
            Toggle itemToggle = newItem.GetComponent<Toggle>();
            if (itemToggle == null)
            {
                Debug.LogError("Item prefab is missing a Toggle component.");
                continue;
            }

            itemToggle.group = itemContainer.GetComponent<ToggleGroup>();

            Image background = itemToggle.transform.Find("Background")?.GetComponent<Image>();
            if (background == null)
            {
                Debug.LogError("Item prefab's background image is missing.");
                continue;
            }

            // Set the item's image from the database
            background.sprite = item.ItemImage;
            if (background.sprite == null)
            {
                Debug.LogWarning($"Item '{item.Name}' has no image assigned in the database.");
            }

            GameObject itemPanel = newItem.transform.Find("ItemPanel")?.gameObject;
            if (itemPanel == null)
            {
                Debug.LogError("Item prefab is missing the ItemPanel GameObject.");
                continue;
            }

            itemPanel.SetActive(false); // Panel is initially hidden

            // Populate the panel with item details
            TMP_Text nameText = itemPanel.transform.Find("Name")?.GetComponent<TMP_Text>();
            TMP_Text statText = itemPanel.transform.Find("Stats")?.GetComponent<TMP_Text>();
            TMP_Text valueText = itemPanel.transform.Find("Value")?.GetComponent<TMP_Text>();
            Button buyButton = itemPanel.transform.Find("BuyButton")?.GetComponent<Button>();

            if (nameText != null) nameText.text = item.Name;
            if (statText != null)
            {
                statText.text = item.Modifiers != null && item.Modifiers.Count > 0
                    ? string.Join("\n", item.Modifiers.ConvertAll(mod => $"{mod.Type}: +{mod.Value}"))
                    : "No modifiers";
            }
            if (valueText != null) valueText.text = $"{item.Value.Gold}g {item.Value.Silver}s";

            if (buyButton != null)
            {
                buyButton.onClick.RemoveAllListeners();
                buyButton.onClick.AddListener(() =>
                {
                    BuyItem(item);
                });
            }

            // Positioning and showing the panel dynamically
            itemToggle.onValueChanged.AddListener(isOn =>
 {
     if (isOn)
     {
         itemPanel.SetActive(true);

         // Bring the panel to the front
         BringPanelToFront(itemPanel);

         RectTransform panelRect = itemPanel.GetComponent<RectTransform>();
         RectTransform toggleRect = itemToggle.GetComponent<RectTransform>();
         Vector3 togglePosition = toggleRect.position;

         if (togglePosition.x > Screen.width / 2)
         {
             panelRect.pivot = new Vector2(1, 0.5f);
             panelRect.anchoredPosition = new Vector2(-100, 0); // Panel to the left of the toggle
         }
         else
         {
             panelRect.pivot = new Vector2(0, 0.5f);
             panelRect.anchoredPosition = new Vector2(300, 0); // Panel to the right of the toggle
         }
     }
     else
     {
         itemPanel.SetActive(false);
     }
 });

        }
    }

    private void BringPanelToFront(GameObject panel)
    {
        Canvas canvas = panel.GetComponent<Canvas>();

        // Ensure the panel has a Canvas component for sorting
        if (canvas == null)
        {
            canvas = panel.AddComponent<Canvas>();
        }

        canvas.overrideSorting = true;
        canvas.sortingOrder = 9999; // Set a very high sorting order to ensure it's on top
    }

    private void ClearUI(Transform container)
    {
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
    }


}
