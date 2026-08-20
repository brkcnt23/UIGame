# Üretim Ekonomisi

99 tarif, 5 meslek. Zincir doğrulandı: **99/99 hammaddeye kadar iniyor**, döngü yok, çıkmaz yok.

---

## 1. Metal merdiveni — neden mithril yok

Büyü bu dünyada nadir ve cadı seviyesinde, savaşta hiç yok. Mithril zırh o tonu kırardı. Gerçek metalurji zaten daha iyi bir merdiven veriyor:

| Kademe | Metal | Tarif | Gate |
|---|---|---|---|
| 1 | **Copper Ingot** | 2 Copper Ore + 1 Coal | Smither 1 |
| 2 | **Bronze Ingot** | 2 Copper + 1 Tin + 1 Coal | Smither 2 |
| 3 | **Iron Ingot** | 2 Iron Ore + 2 Coal | Smither 3 |
| 4 | **Steel Ingot** | 2 Iron Ingot + 2 **Charcoal** | Smither 5 |
| 5 | **Meteoric Iron** | — üretilemez, sadece bulunur | Legendary |

**Kalayın işi buldu:** tek başına hiçbir şeye yaramaz, tunç için şart. Gerçek tunç ~%88 bakır; 2:1 oranı hem gerçekçi hem Valheim'la aynı.

**Göktaşı demiri senin mithril'in ve gerçek.** Tutankamon'un hançeri göktaşı demirindendir, tarih boyunca altından değerli sayılmıştır. Üretilemez → Legendary kuralınla (dünyada tek tane) birebir örtüşüyor.

Bir de kritik bir bağ: **çeliğin karbonu marangozun kömüründen geliyor.** Demirci tek başına çelik yapamıyor.

---

## 2. Tasarım ilkesi: kimse kendi kendine yetmesin

Greatsword doğrulanmış zinciri:

```
Greatsword                    Smither 8 · 240 dk
├─ Steel Ingot ×4             Smither 5 · 120 dk
│  ├─ Iron Ingot ×2           Smither 3 ·  75 dk
│  │  ├─ Iron Ore  (ham)
│  │  └─ Coal      (ham)
│  └─ Charcoal ×2             Carpenter 2 · 90 dk   ← marangoz
│     └─ Timber Log (ham)
├─ Leather Strap ×2           Tanner 1 · 15 dk      ← tabakçı
│  └─ Tanned Leather
│     ├─ Raw Hide (ham)
│     └─ Salt     (ham)
├─ Rivets ×3                  Smither 2 · 20 dk
└─ Whetstone ×1               Mason 1 · 20 dk       ← taşçı
```

Tek kılıç **dört mesleğe** dokunuyor. Uzmanlaşmak mümkün, kendine yetmek değil. Bu, ticareti ve companion'ları zorunlu kılıyor — sistem kendi kendine sosyalleşiyor.

---

## 3. İksirler

| İksir | Tarif | Lvl |
|---|---|---|
| Herbal Extract | 3 Common Herbs | 1 |
| Minor Healing Draught | 1 Extract + 1 Glass Vial | 1 |
| Healing Draught | 2 Extract + 1 Bitterroot + 1 Vial | 3 |
| Strong Healing Draught | 3 Extract + 2 Marshbloom + 1 Spirit + 1 Vial | 5 |
| Antidote | 2 Bitterroot + 1 Extract + 1 Vial | 4 |
| Fever Tonic | 1 Marshbloom + 1 Spirit + 1 Vial | 3 |
| Distilled Spirit | 4 Common Herbs + 1 Fireleaf | 3 |

**Glass Vial masondan geliyor** (2 Rough Stone + 2 Coal → 4 şişe). Simyacı camını kendi yapamıyor.

Dört bitki dört rol oynuyor: Common Herbs taban, Bitterroot panzehir, Marshbloom ateş, Fireleaf damıtma.

---

## 4. En derin zincirler

| Adım | Lvl | Süre | Eşya |
|---|---|---|---|
| 4 | 10 | 600 dk | **Full Plate** — 10 çelik, 4 kayış, 4 toka, 12 perçin |
| 4 | 8 | 380 dk | Half Plate |
| 4 | 8 | 220 dk | Plate Greaves |
| 4 | 7 | 180 dk | Bascinet |
| 4 | 6 | 180 dk | Mail Chausses |
| 4 | 6 | 150 dk | Rapier |

Full Plate 10 saatlik oyun içi iş. Yapabilen demirci reklam vermek zorunda değil.

---

## 5. Meslek dağılımı

| Meslek | Ne üretir | Kimden bağımlı |
|---|---|---|
| **Smither** | Külçe, perçin, toka, silah, zincir/plaka zırh | Carpenter (kömür), Tanner (kayış) |
| **Carpenter** | Kalas, kiriş, **kömür**, yay, kalkan gövdesi | Tanner (kiriş teli) |
| **Tanner** | Deri, kumaş, ip, kiriş teli, hafif zırh | Mason (tuz yok — satın alınır) |
| **Mason** | Kesme taş, tuğla, harç, **cam şişe**, bileği taşı | — en bağımsız |
| **Alchemist** | Özüt, iksir, damıtık | Mason (şişe), Tanner (bez) |

Mason en bağımsız meslek ama en az doğrudan ürünü var — inşaat sisteminde asıl değerini bulacak.

---

## 6. Unity'de çalıştırma sırası

```
1. Tools > UIGame > Items   > Generate ItemSO assets from catalog
2. Tools > UIGame > Recipes > Generate recipe assets
3. Tools > UIGame > Recipes > Validate production chain
```

**Sıra önemli** — tarifler item ID'lerine referans veriyor.

3. adım zincirdeki her şeyi doğrular ve tarifi olmayan craftable item'ları listeler.

---

## 7. Eksik görseller

Bu tarifler yeni item'lar getirdi:

- **Tin Ingot** — kalay külçesi
- **Silver Ingot** — gümüş külçesi
- **Meteoric Iron** — koyu, ağır, gökten düşmüş görünmeli

Diğer eksikler `MISSING_ART.md`'de.

---

## 8. Dengelenmemiş olanlar

Sayılar ilk geçiş, oyun testinde ayarlanacak:

- **Süreler** — Full Plate 600 dk çok mu? Oyuncu bunu bir seferde yapamaz, uyku ve yemek araya girer. Belki de doğru olan bu.
- **Malzeme miktarları** — 10 çelik = 20 demir külçesi = 40 demir cevheri. Madencilik hızına göre ayarlanacak.
- **Seviye eşikleri** — Smither 10 tavanı Full Plate için; oraya ulaşmak ne kadar sürüyor henüz bilinmiyor.
- **Başarı şansı** — hepsi şu an %100. Kalite zarı `ItemSO.RollInstance()`'da; başarısızlık şansı eklenirse burası kullanılacak.
