using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Items/ItemDatabase")]
public class ItemDatabase : ScriptableObject
{
    public List<ItemSO> items = new List<ItemSO>();

    private Dictionary<int, ItemSO> itemsById;
    private Dictionary<string, ItemSO> itemsByName;

    private void OnEnable()
    {
        RebuildIndex();
    }

    public void RebuildIndex()
    {
        itemsById = new Dictionary<int, ItemSO>();
        itemsByName = new Dictionary<string, ItemSO>();

        foreach (var it in items)
        {
            if (it == null)
                continue;

            if (!itemsById.ContainsKey(it.ID))
                itemsById[it.ID] = it;
            else
                Debug.LogWarning($"ItemDatabase: Duplicate ID detected -> {it.ID} ({it.itemName})");

            if (!string.IsNullOrEmpty(it.itemName))
            {
                if (!itemsByName.ContainsKey(it.itemName))
                    itemsByName[it.itemName] = it;
                else
                    Debug.LogWarning($"ItemDatabase: Duplicate Name detected -> {it.itemName}");
            }
        }
    }

    public ItemSO GetByID(int id)
    {
        if (itemsById == null)
            RebuildIndex();

        itemsById.TryGetValue(id, out var so);
        return so;
    }

    public ItemSO GetByName(string name)
    {
        if (itemsByName == null)
            RebuildIndex();

        itemsByName.TryGetValue(name, out var so);
        return so;
    }

    public Item GetItemInstanceByID(int id, int quantity = 1)
    {
        var so = GetByID(id);
        return so != null ? so.ToItem(quantity) : null;
    }

    public bool ContainsID(int id)
    {
        if (itemsById == null)
            RebuildIndex();

        return itemsById.ContainsKey(id);
    }

    public bool ContainsName(string name)
    {
        if (itemsByName == null)
            RebuildIndex();

        return itemsByName.ContainsKey(name);
    }

    public List<ItemSO> GetAllByCategory(ItemCategory category)
    {
        List<ItemSO> result = new List<ItemSO>();

        foreach (var item in items)
        {
            if (item == null) continue;
            if (item.category == category)
                result.Add(item);
        }

        return result;
    }
}