using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance { get; private set; }

    public List<Item> inventory => PlayerStatHandler.Instance.pd.Items; 
    public List<Item> Resources;
    public List<Item> SpecialItems;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeInventory();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeInventory()
    {
        Resources = new List<Item>();
        SpecialItems = new List<Item>();
    }

    public void AddItem(Item item)
    {
        if (item == null)
        {
            Debug.LogWarning("AddItem: Eklenmek istenen item null.");
            return;
        }

        var existingItem = inventory.Find(x => x.ID == item.ID);

        if (existingItem != null)
        {
            // Increase the quantity if the item already exists
            existingItem.Quantity += item.Quantity;
        }
        else
        {
            // Add the new item to the inventory
            inventory.Add(item);
            AddToCategoryLists(item);
        }

        SyncWithPlayerData();
    }

    private void AddToCategoryLists(Item item)
    {
        if (item.Category == ItemCategory.Resource)
        {
            Resources.Add(item);
        }
        else
        {
            SpecialItems.Add(item);
        }
    }

    public void RemoveItem(Item item, int quantity = 1)
    {
        if (item == null)
        {
            Debug.LogWarning("RemoveItem: Silinmek istenen item null.");
            return;
        }

        var existingItem = inventory.Find(x => x.ID == item.ID);

        if (existingItem != null)
        {
            existingItem.Quantity -= quantity;

            if (existingItem.Quantity <= 0)
            {
                inventory.Remove(existingItem);
                RemoveFromCategoryLists(existingItem);
            }
        }

        SyncWithPlayerData();
    }

    private void RemoveFromCategoryLists(Item item)
    {
        if (item.Category == ItemCategory.Resource)
        {
            Resources.Remove(item);
        }
        else
        {
            SpecialItems.Remove(item);
        }
    }

    private void SyncWithPlayerData()
    {
        PlayerStatHandler.Instance.pd.Items = new List<Item>(inventory);
    }

    public List<Item> GetInventory()
    {
        SyncWithPlayerData();
        return new List<Item>(inventory);
    }

    public bool HasItem(int itemId, int quantity)
    {
        var item = inventory.Find(i => i.ID == itemId);
        return item != null && item.Quantity >= quantity;
    }
}
