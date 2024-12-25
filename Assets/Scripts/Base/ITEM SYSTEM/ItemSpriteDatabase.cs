using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ItemSpriteDatabase", menuName = "Shop/ItemSpriteDatabase")]
public class ItemSpriteDatabase : ScriptableObject
{
    [System.Serializable]
    public class ItemSprite
    {
        public ItemCategory Category;
        public int Quality;
        public Sprite Image;
    }

    public List<ItemSprite> itemSprites = new List<ItemSprite>();

    public Sprite GetSprite(ItemCategory category, int quality)
    {
        foreach (var itemSprite in itemSprites)
        {
            if (itemSprite.Category == category && itemSprite.Quality == quality)
            {
                return itemSprite.Image;
            }
        }

        return null; // Default or placeholder sprite
    }
}
