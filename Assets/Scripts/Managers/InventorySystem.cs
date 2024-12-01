using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance;

    public List<Item> Resources;     // Stackable items like wood, stone
    public List<Item> SpecialItems;  // Equipment slots (e.g., sword, armor)

    public int MaxSpecialItemSlots = 6;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Resources = new List<Item>();
            SpecialItems = new List<Item>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Add item to inventory
    public void AddItem(Item item)
    {
        if (item.IsStackable)
        {
            Item existingResource = Resources.Find(i => i.ID == item.ID);
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
            if (SpecialItems.Count < MaxSpecialItemSlots)
            {
                SpecialItems.Add(item);
            }
            else
            {
                Debug.LogWarning("No more slots available for special items.");
            }
        }

        UpdateUI();
    }

    // Remove item from inventory
    public void RemoveItem(Item item, int quantity = 1)
    {
        if (item.IsStackable)
        {
            Item resource = Resources.Find(i => i.ID == item.ID);
            if (resource != null)
            {
                resource.Quantity -= quantity;
                if (resource.Quantity <= 0)
                {
                    Resources.Remove(resource);
                }
            }
        }
        else
        {
            SpecialItems.Remove(item);
        }

        UpdateUI();
    }

    // Update UI
    public void UpdateUI()
    {
        InventoryUI.Instance.UpdateInventoryUI();
    }
}
