using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance { get; private set; }

    [SerializeField] private ItemDatabase itemDatabase;

    public List<Item> inventory
    {
        get
        {
            if (PlayerStatHandler.Instance != null && PlayerStatHandler.Instance.pd != null)
            {
                if (PlayerStatHandler.Instance.pd.Items == null)
                    PlayerStatHandler.Instance.pd.Items = new List<Item>();

                return PlayerStatHandler.Instance.pd.Items;
            }

            Debug.LogWarning("InventorySystem: PlayerStatHandler.Instance or pd is null! Returning empty list.");
            return new List<Item>();
        }
    }

    public List<Item> ResourceItems { get; private set; }
    public List<Item> SpecialItems { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeInventory();

            if (itemDatabase == null)
            {
                itemDatabase = Resources.Load<ItemDatabase>("ItemDatabase");
                if (itemDatabase == null)
                {
                    Debug.LogWarning("InventorySystem: ItemDatabase not found in Resources.");
                }
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        TryInitializeFromPlayerData();
        RebuildCategoryCaches();
        SyncWithPlayerData();
    }

    private void InitializeInventory()
    {
        ResourceItems = new List<Item>();
        SpecialItems = new List<Item>();
    }

    private void TryInitializeFromPlayerData()
    {
        if (PlayerStatHandler.Instance == null || PlayerStatHandler.Instance.pd == null)
            return;

        if (PlayerStatHandler.Instance.pd.Items == null)
            PlayerStatHandler.Instance.pd.Items = new List<Item>();

        if (PlayerStatHandler.Instance.pd.ItemStacks == null)
            PlayerStatHandler.Instance.pd.ItemStacks = new List<ItemStackData>();

        // Save'de ItemStacks var ama Items boşsa rebuild et
        if (PlayerStatHandler.Instance.pd.Items.Count == 0 &&
            PlayerStatHandler.Instance.pd.ItemStacks.Count > 0)
        {
            foreach (var stack in PlayerStatHandler.Instance.pd.ItemStacks)
            {
                if (stack == null || stack.Quantity <= 0) continue;

                Item item = null;

                if (itemDatabase != null)
                {
                    var so = itemDatabase.GetByID(stack.ItemId);
                    if (so != null)
                    {
                        item = so.ToItem(stack.Quantity);
                    }
                }

                if (item == null)
                {
                    item = new Item(stack.ItemId, "Unknown Item", 0, 0, ItemCategory.Misc, stack.Quantity);
                }

                PlayerStatHandler.Instance.pd.Items.Add(item);
            }
        }
    }

    // -----------------------------
    // ADD / REMOVE
    // -----------------------------

    public void AddItem(Item item)
    {
        if (item == null)
        {
            Debug.LogWarning("InventorySystem.AddItem: item is null.");
            return;
        }

        AutoFillFromDatabase(item);

        var existingItem = FindStackableMatch(item);
        if (existingItem != null)
        {
            existingItem.Quantity += item.Quantity;
        }
        else
        {
            inventory.Add(item);
        }

        RebuildCategoryCaches();
        SyncWithPlayerData();
    }

    public void AddItem(ItemSO itemSo, int quantity = 1)
    {
        if (itemSo == null)
        {
            Debug.LogWarning("InventorySystem.AddItem(ItemSO): itemSo is null.");
            return;
        }

        AddItem(itemSo.ToItem(quantity));
    }

    public void RemoveItem(Item item, int quantity = 1)
    {
        if (item == null)
        {
            Debug.LogWarning("InventorySystem.RemoveItem: item is null.");
            return;
        }

        RemoveItemById(item.ID, quantity);
    }

    public void RemoveItemById(int itemId, int quantity = 1)
    {
        if (quantity <= 0) return;

        var existingItem = inventory.Find(x => x.ID == itemId);
        if (existingItem == null) return;

        existingItem.Quantity -= quantity;

        if (existingItem.Quantity <= 0)
        {
            inventory.Remove(existingItem);
        }

        RebuildCategoryCaches();
        SyncWithPlayerData();
    }

    public bool HasItem(int itemId, int quantity)
    {
        var item = inventory.Find(i => i.ID == itemId);
        return item != null && item.Quantity >= quantity;
    }

    public bool HasItem(ItemSO itemSo, int quantity)
    {
        if (itemSo == null) return false;
        return HasItem(itemSo.ID, quantity);
    }

    // -----------------------------
    // QUERIES
    // -----------------------------

    public List<Item> GetInventory()
    {
        return new List<Item>(inventory);
    }

    public List<Item> GetItemsByCategory(ItemCategory category)
    {
        return inventory.Where(i => i != null && i.Category == category).ToList();
    }

    public Item GetItemById(int itemId)
    {
        return inventory.Find(i => i.ID == itemId);
    }

    public int GetItemQuantity(int itemId)
    {
        var item = GetItemById(itemId);
        return item != null ? item.Quantity : 0;
    }

    public float GetCurrentWeight()
    {
        float total = 0f;

        foreach (var item in inventory)
        {
            if (item == null) continue;
            total += item.TotalWeight;
        }

        return total;
    }

    public Currency GetTotalInventoryValue()
    {
        Currency total = new Currency(0, 0);

        foreach (var item in inventory)
        {
            if (item == null) continue;
            total.Add(item.TotalValue.Gold, item.TotalValue.Silver);
        }

        return total;
    }

    // -----------------------------
    // INTERNAL HELPERS
    // -----------------------------

    private void AutoFillFromDatabase(Item item)
    {
        if (item == null || itemDatabase == null) return;

        var so = itemDatabase.GetByID(item.ID);
        if (so == null) return;

        if (string.IsNullOrEmpty(item.Name))
            item.Name = so.itemName;

        if (item.ItemImage == null)
            item.ItemImage = so.icon;

        if (item.Quality <= 0)
            item.Quality = so.quality;

        if (item.Weight <= 0f)
            item.Weight = so.weight;

        if (item.MaxStack <= 0)
            item.MaxStack = so.maxStack;

        item.Stackable = so.stackable;
    }

    private Item FindStackableMatch(Item item)
    {
        if (item == null || !item.Stackable) return null;

        return inventory.Find(x =>
            x != null &&
            x.CanStackWith(item));
    }

    private void RebuildCategoryCaches()
    {
        ResourceItems.Clear();
        SpecialItems.Clear();

        foreach (var item in inventory)
        {
            if (item == null) continue;

            if (item.Category == ItemCategory.Resource || item.Category == ItemCategory.CraftingMaterial)
            {
                ResourceItems.Add(item);
            }
            else
            {
                SpecialItems.Add(item);
            }
        }
    }

    private void SyncWithPlayerData()
    {
        if (PlayerStatHandler.Instance == null || PlayerStatHandler.Instance.pd == null)
        {
            Debug.LogWarning("InventorySystem: Cannot sync, PlayerStatHandler or pd is null.");
            return;
        }

        PlayerStatHandler.Instance.pd.Items = new List<Item>(inventory);

        if (PlayerStatHandler.Instance.pd.ItemStacks == null)
            PlayerStatHandler.Instance.pd.ItemStacks = new List<ItemStackData>();

        PlayerStatHandler.Instance.pd.ItemStacks.Clear();

        foreach (var item in inventory)
        {
            if (item == null) continue;

            PlayerStatHandler.Instance.pd.ItemStacks.Add(new ItemStackData
            {
                ItemId = item.ID,
                Quantity = item.Quantity
            });
        }
    }
}