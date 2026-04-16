using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ShopUI : MonoBehaviour
{
    public Transform shopItemContainer;
    public GameObject shopItemPrefab;

    public GameObject itemDetailPanel;
    public Text itemNameText;
    public Text itemModifiersText;
    public Text itemValueText;
    public Text itemWeightText;
    public Text itemShopCashText;

    public Button buyButton;
    public Button sellButton;

    private List<Item> currentShopItems = new List<Item>();
    private Item selectedItem;
    private Shops currentShop;

    public void InitializeShopUI(List<Item> shopItems, Shops shop = null)
    {
        if (shopItemContainer == null || shopItemPrefab == null)
        {
            Debug.LogError("ShopUI: shopItemContainer or shopItemPrefab is null!");
            return;
        }

        if (shopItems == null)
        {
            Debug.LogWarning("ShopUI: InitializeShopUI called with null shopItems.");
            return;
        }

        foreach (Transform child in shopItemContainer)
        {
            Destroy(child.gameObject);
        }

        currentShopItems = shopItems;
        currentShop = shop;

        foreach (var item in shopItems)
        {
            GameObject itemButton = Instantiate(shopItemPrefab, shopItemContainer);

            Text buttonText = itemButton.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.text = $"{item.Name} x{item.Quantity}";
            }

            Button btn = itemButton.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => ShowItemDetails(item));
            }
        }
    }

    public void ShowItemDetails(Item item)
    {
        if (item == null)
        {
            Debug.LogError("ShopUI: ShowItemDetails called with null item!");
            return;
        }

        selectedItem = item;

        if (itemNameText != null)
            itemNameText.text = item.Name;

        if (itemValueText != null)
        {
            if (currentShop != null)
            {
                Currency buyPrice = currentShop.GetSellPrice(item);
                Currency sellPrice = currentShop.GetBuyPrice(item);

                itemValueText.text = $"Buy: {buyPrice.Gold}g {buyPrice.Silver}s\nSell: {sellPrice.Gold}g {sellPrice.Silver}s";
            }
            else
            {
                itemValueText.text = $"{item.Value.Gold}g {item.Value.Silver}s";
            }
        }

        if (itemWeightText != null)
            itemWeightText.text = $"Weight: {item.Weight:0.0}";

        if (itemShopCashText != null && currentShop != null)
            itemShopCashText.text = $"Shop Cash: {currentShop.Cash.Gold}g {currentShop.Cash.Silver}s";

        if (itemModifiersText != null)
            itemModifiersText.text = GetModifiersText(item);

        if (buyButton != null)
        {
            buyButton.gameObject.SetActive(true);
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(() =>
            {
                if (ShopSystem.Instance != null)
                {
                    ShopSystem.Instance.BuyItem(item);
                }
            });
        }

        if (sellButton != null)
        {
            sellButton.gameObject.SetActive(true);
            sellButton.onClick.RemoveAllListeners();
            sellButton.onClick.AddListener(() =>
            {
                if (ShopSystem.Instance != null)
                {
                    ShopSystem.Instance.SellItem(item);
                }
            });
        }

        if (itemDetailPanel != null)
            itemDetailPanel.SetActive(true);
    }

    private string GetModifiersText(Item item)
    {
        if (item == null || item.Modifiers == null || item.Modifiers.Count == 0)
            return "No modifiers";

        string modifiers = "";
        foreach (var mod in item.Modifiers)
        {
            modifiers += $"{mod.Type}: +{mod.Value}\n";
        }

        return modifiers;
    }
}