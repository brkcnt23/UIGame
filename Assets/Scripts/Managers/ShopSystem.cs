using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ShopSystem : MonoBehaviour
{
    public static ShopSystem Instance { get; private set; }

    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject shopListPanel;

    [SerializeField] private Transform itemContainer;
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private GameObject ShopButtonPrefab;

    [SerializeField] private ItemDatabase itemDatabase;

    private List<Item> shopItems;
    public Shops currentShop;

    private readonly Dictionary<int, ShopItemEntry> currentEntryLookup = new Dictionary<int, ShopItemEntry>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            if (itemDatabase == null)
            {
                itemDatabase = Resources.Load<ItemDatabase>("ItemDatabase");
                if (itemDatabase == null)
                {
                    Debug.LogWarning("ShopSystem: ItemDatabase not found in Resources.");
                }
            }
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        if (shopPanel != null)
            shopPanel.SetActive(false);

        if (shopListPanel != null)
            shopListPanel.SetActive(false);
    }

    private void OnEnable()
    {
        if (SettlementHandler.Instance != null && SettlementHandler.Instance.settlement != null)
        {
            SettlementHandler.Instance.settlement.OnShopEntered += HandleShopEntered;
        }
    }

    private void OnDisable()
    {
        if (SettlementHandler.Instance != null && SettlementHandler.Instance.settlement != null)
        {
            SettlementHandler.Instance.settlement.OnShopEntered -= HandleShopEntered;
        }
    }

    private void HandleShopEntered(Shops shop)
    {
        if (shop != null)
        {
            OpenShop(shop);
        }
    }

    private void OpenShop(Shops shop)
    {
        currentShop = shop;
        if (shopPanel != null)
            shopPanel.SetActive(true);
        if (shopListPanel != null)
            shopListPanel.SetActive(false);
        DisplayShopItems(shop);
    }

    // -----------------------------
    // PANEL CONTROL
    // -----------------------------

    public void ToggleShop()
    {
        if (shopPanel == null || shopListPanel == null)
        {
            Debug.LogError("ShopSystem: shopPanel or shopListPanel is null! Cannot toggle shop.");
            return;
        }

        bool willOpen = !shopPanel.activeSelf && !shopListPanel.activeSelf;

        if (willOpen)
        {
            PopulateShopList();
        }
        else
        {
            CloseShop();
        }
    }

    public void CloseShop()
    {
        if (shopPanel != null) shopPanel.SetActive(false);
        if (shopListPanel != null) shopListPanel.SetActive(false);

        if (itemContainer != null)
            ClearUI(itemContainer);

        currentShop = null;
        currentEntryLookup.Clear();
    }

    // -----------------------------
    // SHOP LIST
    // -----------------------------

    public void PopulateShopList()
    {
        if (shopListPanel == null || shopPanel == null)
        {
            Debug.LogError("ShopSystem: shopListPanel or shopPanel is null! Cannot populate shop list.");
            return;
        }

        if (SettlementHandler.Instance == null || SettlementHandler.Instance.settlement == null)
        {
            Debug.LogError("ShopSystem: SettlementHandler.Instance or current settlement is null!");
            return;
        }

        ClearUI(shopListPanel.transform);
        shopPanel.SetActive(false);
        shopListPanel.SetActive(true);

        Settlement currentSettlement = SettlementHandler.Instance.settlement;

        foreach (Shops shop in currentSettlement.Shops)
        {
            GameObject shopButton = Instantiate(ShopButtonPrefab, shopListPanel.transform);

            TMP_Text label = shopButton.GetComponentInChildren<TMP_Text>();
            if (label != null)
            {
                label.text = $"{shop.Name}\nCash: {shop.Cash.Gold}g {shop.Cash.Silver}s";
            }

            Button btn = shopButton.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    currentShop = shop;
                    SettlementHandler.Instance.HandleShopEntered(shop);
                    DisplayShopItems(shop);
                });
            }
        }
    }

    // -----------------------------
    // BUY FROM SHOP
    // -----------------------------

    public void BuyItem(Item item)
    {
        if (item == null)
        {
            Debug.LogError("ShopSystem: Cannot buy null item!");
            return;
        }

        if (currentShop == null)
        {
            Debug.LogWarning("ShopSystem: No current shop selected.");
            return;
        }

        var stateManager = GameBootstrapper.State;
        var eventBus = GameBootstrapper.Events;

        if (stateManager == null || eventBus == null)
        {
            Debug.LogError("ShopSystem: GameBootstrapper systems not initialized");
            return;
        }

        Currency itemCost = currentShop.GetSellPrice(item);

        // Check player money via state
        bool hasEnoughMoney = stateManager.GetValue(state =>
            (state.Player.Gold * 100 + state.Player.Silver) >= (itemCost.Gold * 100 + itemCost.Silver)
        );

        if (!hasEnoughMoney)
        {
            Debug.Log("Not enough money to buy this item.");
            return;
        }

        // Deduct money from player
        stateManager.UpdateState(state =>
        {
            var newState = state.Clone();
            long totalSilver = newState.Player.Gold * 100 + newState.Player.Silver;
            long costSilver = itemCost.Gold * 100 + itemCost.Silver;
            totalSilver -= costSilver;

            newState.Player.Gold = (long)(totalSilver / 100);
            newState.Player.Silver = (long)(totalSilver % 100);
            return newState;
        });

        // Add item to inventory
        eventBus.Dispatch(new AddItemEvent(item.ID, 1));

        // Update shop inventory
        currentShop.AddCash(itemCost.Gold, itemCost.Silver);
        if (currentShop.ItemEntries != null && currentEntryLookup.TryGetValue(item.ID, out var entry))
        {
            entry.Quantity -= 1;
            if (entry.Quantity <= 0)
                currentShop.ItemEntries.Remove(entry);
        }

        Debug.Log($"Purchased {item.Name} for {itemCost}");
        DisplayShopItems(currentShop);
    }

    // -----------------------------
    // SELL TO SHOP
    // -----------------------------

    public void SellItem(Item item)
    {
        if (item == null)
        {
            Debug.LogError("ShopSystem: Cannot sell null item!");
            return;
        }

        if (currentShop == null)
        {
            Debug.LogWarning("ShopSystem: No current shop selected.");
            return;
        }

        if (InventorySystem.Instance == null || PlayerStatHandler.Instance == null)
        {
            Debug.LogError("ShopSystem: Required systems are null.");
            return;
        }

        if (!currentShop.AcceptsItem(item))
        {
            Debug.Log($"{currentShop.Name} does not accept this item category.");
            return;
        }

        if (item.Quality > currentShop.MaxAffordableItemQuality)
        {
            Debug.Log($"{currentShop.Name} cannot afford items of this quality.");
            return;
        }

        Currency buyPrice = currentShop.GetBuyPrice(item);

        if (!currentShop.CanAfford(buyPrice))
        {
            Debug.Log($"{currentShop.Name} does not have enough cash to buy {item.Name}.");
            return;
        }

        bool shopSpendSuccess = currentShop.TrySpendCash(buyPrice.Gold, buyPrice.Silver);
        if (!shopSpendSuccess)
        {
            Debug.Log("Sell failed: shop cash transaction failed.");
            return;
        }

        var stateManager = GameBootstrapper.State;
        var eventBus = GameBootstrapper.Events;

        if (stateManager == null || eventBus == null)
        {
            Debug.LogError("ShopSystem: GameBootstrapper systems not initialized");
            return;
        }

        // Add money to player
        stateManager.UpdateState(state =>
        {
            var newState = state.Clone();
            newState.Player.Gold += buyPrice.Gold;
            newState.Player.Silver += buyPrice.Silver;
            if (newState.Player.Silver >= 100)
            {
                newState.Player.Gold += newState.Player.Silver / 100;
                newState.Player.Silver %= 100;
            }
            return newState;
        });

        // Remove item from inventory
        eventBus.Dispatch(new RemoveItemEvent(item.ID, 1));

        // Add item to shop
        Item shopExistingItem = currentShop.Items.Find(x => x.ID == item.ID);
        if (shopExistingItem != null && shopExistingItem.Stackable)
        {
            shopExistingItem.Quantity += 1;
        }
        else
        {
            currentShop.Items.Add(item.Clone(1));
        }

        Debug.Log($"Sold {item.Name} to {currentShop.Name} for {buyPrice}");
        DisplayShopItems(currentShop);
    }

    // -----------------------------
    // DISPLAY SHOP ITEMS
    // -----------------------------

    public void DisplayShopItems(Shops shop)
    {
        if (shop == null)
        {
            Debug.LogError("ShopSystem: Cannot display items for null shop!");
            return;
        }

        if (itemContainer == null || shopPanel == null || shopListPanel == null)
        {
            Debug.LogError("ShopSystem: itemContainer, shopPanel, or shopListPanel is null!");
            return;
        }

        currentEntryLookup.Clear();

        if (shop.Items == null)
            shop.Items = new List<Item>();

        List<Item> displayItems = shop.Items;

        if ((displayItems == null || displayItems.Count == 0) &&
            shop.ItemEntries != null && shop.ItemEntries.Count > 0)
        {
            if (itemDatabase == null)
            {
                Debug.LogWarning("ShopSystem: ItemDatabase is null. Cannot resolve ItemEntries.");
            }
            else
            {
                shopItems = new List<Item>();

                foreach (var entry in shop.ItemEntries)
                {
                    var so = itemDatabase.GetByID(entry.ItemId);
                    if (so == null)
                    {
                        Debug.LogWarning($"ShopSystem: ItemSO not found for ItemId {entry.ItemId}");
                        continue;
                    }

                    var item = so.ToItem(entry.Quantity);

                    if (entry.GoldOverride >= 0 || entry.SilverOverride >= 0)
                    {
                        int gold = entry.GoldOverride >= 0 ? entry.GoldOverride : item.Value.Gold;
                        int silver = entry.SilverOverride >= 0 ? entry.SilverOverride : item.Value.Silver;
                        item.Value = new Currency(gold, silver);
                    }

                    shopItems.Add(item);
                    currentEntryLookup[item.ID] = entry;
                }

                displayItems = shopItems;
            }
        }

        if (displayItems == null || displayItems.Count == 0)
        {
            ClearUI(itemContainer);
            shopListPanel.SetActive(false);
            shopPanel.SetActive(true);
            return;
        }

        ClearUI(itemContainer);
        shopListPanel.SetActive(false);
        shopPanel.SetActive(true);

        foreach (Item item in displayItems)
        {
            GameObject newItem = Instantiate(itemPrefab, itemContainer);

            Toggle itemToggle = newItem.GetComponent<Toggle>();
            if (itemToggle == null)
            {
                Debug.LogError("Item prefab is missing a Toggle component.");
                continue;
            }

            itemToggle.group = itemContainer.GetComponent<ToggleGroup>();

            Image background = itemToggle.transform.Find("Background")?.GetComponent<Image>();
            if (background != null)
            {
                background.sprite = item.ItemImage;
            }

            GameObject itemPanel = newItem.transform.Find("ItemPanel")?.gameObject;
            if (itemPanel == null)
            {
                Debug.LogError("Item prefab is missing the ItemPanel GameObject.");
                continue;
            }

            itemPanel.SetActive(false);

            TMP_Text nameText = itemPanel.transform.Find("Name")?.GetComponent<TMP_Text>();
            TMP_Text statText = itemPanel.transform.Find("Stats")?.GetComponent<TMP_Text>();
            TMP_Text valueText = itemPanel.transform.Find("Value")?.GetComponent<TMP_Text>();
            TMP_Text extraText = itemPanel.transform.Find("Extra")?.GetComponent<TMP_Text>();

            Button buyButton = itemPanel.transform.Find("BuyButton")?.GetComponent<Button>();
            Button sellButton = itemPanel.transform.Find("SellButton")?.GetComponent<Button>();

            if (nameText != null) nameText.text = item.Name;

            if (statText != null)
            {
                statText.text = item.Modifiers != null && item.Modifiers.Count > 0
                    ? string.Join("\n", item.Modifiers.ConvertAll(mod => $"{mod.Type}: +{mod.Value}"))
                    : "No modifiers";
            }

            Currency shopSellPrice = shop.GetSellPrice(item);
            Currency shopBuyPrice = shop.GetBuyPrice(item);

            if (valueText != null)
            {
                valueText.text =
                    $"Buy: {shopSellPrice.Gold}g {shopSellPrice.Silver}s\n" +
                    $"Sell: {shopBuyPrice.Gold}g {shopBuyPrice.Silver}s";
            }

            if (extraText != null)
            {
                extraText.text =
                    $"Qty: {item.Quantity}\n" +
                    $"Weight: {item.Weight:0.0}\n" +
                    $"Shop Cash: {shop.Cash.Gold}g {shop.Cash.Silver}s";
            }

            if (buyButton != null)
            {
                buyButton.onClick.RemoveAllListeners();
                buyButton.onClick.AddListener(() => BuyItem(item));
            }

            if (sellButton != null)
            {
                sellButton.onClick.RemoveAllListeners();
                sellButton.onClick.AddListener(() => SellItem(item));
            }

            itemToggle.onValueChanged.AddListener(isOn =>
            {
                if (isOn)
                {
                    itemPanel.SetActive(true);
                    BringPanelToFront(itemPanel);

                    RectTransform panelRect = itemPanel.GetComponent<RectTransform>();
                    RectTransform toggleRect = itemToggle.GetComponent<RectTransform>();
                    Vector3 togglePosition = toggleRect.position;

                    if (togglePosition.x > Screen.width / 2)
                    {
                        panelRect.pivot = new Vector2(1, 0.5f);
                        panelRect.anchoredPosition = new Vector2(-100, 0);
                    }
                    else
                    {
                        panelRect.pivot = new Vector2(0, 0.5f);
                        panelRect.anchoredPosition = new Vector2(300, 0);
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

        if (canvas == null)
        {
            canvas = panel.AddComponent<Canvas>();
        }

        canvas.overrideSorting = true;
        canvas.sortingOrder = 9999;
    }

    private void ClearUI(Transform container)
    {
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
    }

    private void RefreshAllUI()
    {
        if (PlayerUISystem.Instance != null)
            PlayerUISystem.Instance.UpdateUIObjects();

        // UI updates handled by StateManager listeners
    }
}