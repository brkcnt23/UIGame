# Görseli Eksik Item'lar

158 katalog girdisinden **75'inin görseli var, 83'ünün yok.**

Var olanlar ağırlıklı silah ve zırh — yani savaş tarafı hazır. Eksikler ağırlıklı hammadde, malzeme ve ticaret malı — yani **crafting zinciri ve ekonomi tarafı.**

Importer bunları görselsiz de üretir; sadece Inspector'da sprite slotu boş kalır. Sonradan PNG'yi ekleyip importer'ı tekrar çalıştırınca kendiliğinden bağlanır.

---

## Görsel üretim promptu

Bu kategoriler için kalite kademesi gerekmiyor — **tek görsel yeter.** Sadece silah/zırh 4 kademeli.

```
Medieval fantasy game item icon, single object centered, three-quarter view.
[NESNE]. Painted illustration style, warm earth tones, soft rim light,
no text, no border. Plain white background.
```

Toplu üretim için (10-15'erli):

```
Generate these as a matching set of medieval game item icons, same style,
same lighting, same scale, plain white background, no text:
iron ore chunk, copper ore chunk, lump of coal, rolled raw hide,
folded wool, animal bone, timber log, bundle of herbs
```

## Dosya adlandırma

Importer dosya adından eşleştirir, **boşluk ve büyük/küçük harf önemsiz:**

| Item           | Kabul edilen dosya adı                                |
| -------------- | ------------------------------------------------------ |
| Iron Ore       | `Iron Ore.png` · `IronOre.png` · `ironore.png` |
| Tanned Leather | `Tanned Leather.png` · `TannedLeather.png`        |

Kalite kademesi olacaksa sona rakam: `Buckler1.png` … `Buckler4.png`

---

## Mevcut görsellerden not

- `Greadsword` → `greatsword` olarak düzeltildi
- `Iron_ıngot` Türkçe `ı` içeriyor — importer normalize ettiği için sorun değil
- `PeasentTunic` (Peasant değil) — kataloğa o hâliyle yazıldı
- `ChestArmor1-3` → Leather Cuirass'a, `PantsArmor1-3` → Leather Leggings'e atandı
- `yay1-4` → Shortbow, `huntingbow1-4` → Hunting Bow
- `ChatGPT Image ...` adlı 12 dosya isimlendirilmemiş, hiçbir item'a bağlanmadı
