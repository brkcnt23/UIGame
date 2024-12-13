using System;
using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance { get; private set; }

    public List<Item> inventory;      // Full inventory
    public List<Item> Resources;       // Only resource items
    public List<Item> SpecialItems;    // Only special items

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
        inventory = new List<Item>();
        Resources = new List<Item>();
        SpecialItems = new List<Item>();
    }

    public void AddItem(Item item)
    {
        inventory.Add(item);

        if (item.Category == ItemCategory.Resource)
        {
            var existingResource = Resources.Find(x => x.ID == item.ID);
            if (existingResource != null)
            {
                existingResource.Quantity += item.Quantity;
            }
            else
            {
                Resources.Add(item);
            }
        }
        else
        {
            SpecialItems.Add(item);
        }
    }

    public void RemoveItem(Item item, int quantity = 1)
    {
        if (item.Category == ItemCategory.Resource)
        {
            var resource = Resources.Find(x => x.ID == item.ID);
            if (resource != null)
            {
                resource.Quantity -= quantity;
                if (resource.Quantity <= 0)
                {
                    Resources.Remove(resource);
                    inventory.Remove(resource);
                }
            }
        }
        else
        {
            SpecialItems.Remove(item);
            inventory.Remove(item);
        }
    }

    public List<Item> GetInventory()
    {
        PlayerStatHandler.Instance.pd.Items = new List<Item>(inventory);
        return inventory;
    }
}
