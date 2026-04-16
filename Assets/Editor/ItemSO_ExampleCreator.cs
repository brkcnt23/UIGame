using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;

public static class ItemSO_ExampleCreator
{
    [MenuItem("Tools/Items/Create Example ItemSOs and SpriteDB")]
    public static void CreateExamples()
    {
        // Ensure folders
        Directory.CreateDirectory("Assets/Items");
        Directory.CreateDirectory("Assets/Resources");

        // Create or load ItemDatabase
        var dbPath = "Assets/Resources/ItemDatabase.asset";
        ItemDatabase db = AssetDatabase.LoadAssetAtPath<ItemDatabase>(dbPath);
        if (db == null)
        {
            db = ScriptableObject.CreateInstance<ItemDatabase>();
            AssetDatabase.CreateAsset(db, dbPath);
            Debug.Log("Created ItemDatabase at " + dbPath);
        }

        // Create or load ItemSpriteDatabase
        var spriteDbPath = "Assets/Resources/ItemSpriteDatabase.asset";
        ItemSpriteDatabase spriteDb = AssetDatabase.LoadAssetAtPath<ItemSpriteDatabase>(spriteDbPath);
        if (spriteDb == null)
        {
            spriteDb = ScriptableObject.CreateInstance<ItemSpriteDatabase>();
            AssetDatabase.CreateAsset(spriteDb, spriteDbPath);
            Debug.Log("Created ItemSpriteDatabase at " + spriteDbPath);
        }

        // Ensure sprite entries exist for Weapon qualities 1-3
        for (int q = 1; q <= 3; q++)
        {
            bool exists = false;
            foreach (var it in spriteDb.itemSprites)
            {
                if (it.Category == ItemCategory.Weapon && it.Quality == q) { exists = true; break; }
            }
            if (!exists)
            {
                var entry = new ItemSpriteDatabase.ItemSprite();
                entry.Category = ItemCategory.Weapon;
                entry.Quality = q;
                entry.Image = null; // assign real sprites in inspector
                spriteDb.itemSprites.Add(entry);
            }
        }

        // Create sample ItemSOs: Iron Sword (poor/normal/excellent)
        CreateItemSO("Iron Sword (Poor)", 1001, ItemCategory.Weapon, 1, 0, 50, db, spriteDb);
        CreateItemSO("Iron Sword", 1002, ItemCategory.Weapon, 2, 0, 100, db, spriteDb);
        CreateItemSO("Iron Sword (Excellent)", 1003, ItemCategory.Weapon, 3, 0, 200, db, spriteDb);

        // Mark dirty and save
        EditorUtility.SetDirty(db);
        EditorUtility.SetDirty(spriteDb);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Example ItemSOs + ItemSpriteDatabase placeholders created.\nAssign real sprites to Assets/Resources/ItemSpriteDatabase.asset in the Inspector.");
    }

    private static void CreateItemSO(string name, int id, ItemCategory category, int quality, int gold, int silver, ItemDatabase db, ItemSpriteDatabase spriteDb)
    {
        string safeName = name.Replace(" ", "_").Replace("(", "").Replace(")", "");
        string path = $"Assets/Items/{safeName}.asset";
        ItemSO so = AssetDatabase.LoadAssetAtPath<ItemSO>(path);
        if (so == null)
        {
            so = ScriptableObject.CreateInstance<ItemSO>();
            so.ID = id;
            so.itemName = name;
            so.description = name + " (example item created by editor script).";
            so.category = category;
            so.quality = quality;
            so.goldValue = gold;
            so.silverValue = silver;
            so.stackable = false;
            if (spriteDb != null)
            {
                so.icon = spriteDb.GetSprite(category, quality);
            }
            AssetDatabase.CreateAsset(so, path);
            Debug.Log("Created ItemSO: " + path);
        }
        else if (so.icon == null && spriteDb != null)
        {
            so.icon = spriteDb.GetSprite(category, quality);
            EditorUtility.SetDirty(so);
        }

        if (db != null && !db.items.Contains(so))
        {
            db.items.Add(so);
            EditorUtility.SetDirty(db);
        }
    }
}
#endif
