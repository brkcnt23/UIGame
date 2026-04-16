# ITEM SYSTEM

Bu klasör item sisteminin temel yapılarını içerir.

## Yapı Taşları
- `ItemSO`: Tüm item tanımları ScriptableObject olarak tutulur.
- `ItemDatabase`: Tüm `ItemSO` kayıtlarının listesi ve ID/isim ile lookup sağlar.
- `ItemSpriteDatabase`: Item kategori + kalite üzerinden icon döndüren SO.

## Örnek Üretim (Editor)
- Menü: **Tools > Items > Create Example ItemSOs and SpriteDB**
- İşlem:
  - `ItemDatabase` ve `ItemSpriteDatabase` oluşturulur (Assets/Resources)
  - 3 adet örnek Sword itemi (kalite 1/2/3) oluşturulur
  - Iconlar `ItemSpriteDatabase` üzerinden çekilir

## Akış (Önerilen)
1. `ItemSpriteDatabase` içinde kategori/kaliteye karşılık gelen sprite’ları ekle.
2. `ItemSO` oluştur ve `icon` alanını otomatik (sprite DB) veya manuel ata.
3. `ItemDatabase` içine ilgili item’ları ekle.
4. Shop/Crafting/Inventory sistemi `ItemDatabase` üzerinden item üretir.

## InventorySystem Entegrasyonu
- `InventorySystem.AddItem(ItemSO itemSo, int quantity)`
- `InventorySystem.RemoveItemById(int itemId, int quantity)`
- `InventorySystem.HasItem(ItemSO itemSo, int quantity)`
- `InventorySystem` `Resources/ItemDatabase` üzerinden icon/quality otomatik doldurur.

## ShopSystem Entegrasyonu
- `Shops.ItemEntries`: `ItemId`, `Quantity`, `GoldOverride`, `SilverOverride` (ItemSO tabanlı stok)
- `ShopSystem` önce `Items` listesine bakar, boşsa `ItemEntries` ile `ItemDatabase` üzerinden gösterir.

## Save/Load (ItemStacks)
- `PlayerData.ItemStacks`: `ItemId` + `Quantity` şeklinde save edilir.
- `InventorySystem` load sırasında `ItemStacks` varsa `ItemDatabase` üzerinden `Item` oluşturur.
- `InventorySystem` her değişimde `ItemStacks` listesini günceller.

## Quest/Event Item Ödülleri
- `Quest_SO_Constructor`: `requiredItemStacks`, `rewardItemStacks`, `questItemStacks` (ItemId + Quantity)
- `Event_SO_Constructor.Choice`: `RewardItemStacks` ve `RequireItemId/RequireItemQuantity`
- Ödül/şartlar `ItemRewardHelper` üzerinden `InventorySystem` ile uygulanır.

## Notlar
- `ItemSO` içindeki `quality` değeri `ItemSpriteDatabase` ile eşleşir.
- Eğer icon boş ise, sprite DB üzerinden otomatik atanabilir.
