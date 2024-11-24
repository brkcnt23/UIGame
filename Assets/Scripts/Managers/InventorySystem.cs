using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance { get; private set; }
    private List<Item> inventory; // List to store items
    public PlayerStatHandler playerStatHandler; // Reference to the player stats
    private EconomySystem economySystem;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            inventory = new List<Item>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Add an item to the inventory
    public void AddItem(Item item)
    {
        inventory.Add(item);
        Debug.Log($"Added {item.Name} to inventory.");
        InventoryUI.Instance.UpdateInventoryUI();
    }

    // Remove an item from the inventory
    public void RemoveItem(Item item)
    {
        if (inventory.Contains(item))
        {
            inventory.Remove(item);
            Debug.Log($"Removed {item.Name} from inventory.");
            InventoryUI.Instance.UpdateInventoryUI();
        }
    }

    // Sell an item for gold/silver
    public void SellItem(Item item)
    {
        if (inventory.Contains(item))
        {
            inventory.Remove(item);
            int silver = item.Value;
            playerStatHandler.pd.Silver += silver;

            // Convert silver to gold if necessary
            economySystem.ConvertSilverToGold();
            Debug.Log($"Sold {item.Name} for {silver} silver.");
            InventoryUI.Instance.UpdateInventoryUI();
        }
    }

    // Get the current inventory
    public List<Item> GetInventory()
    {
        return inventory;
    }

    // Update inventory UI (dummy function)
}
