# Yerleşimler

13 yerleşim yeniden düzenlendi: 60 dükkân, 40 atölye, 7 farklı ekonomi tag'i.

---

## 1. Harita

| Yerleşim | Tier | Nüfus | Wealth | Unlock | Tag | Dükkân | Atölye |
|---|---|---|---|---|---|---|---|
| **Mege** | Hamlet | 200 | 1.200 | lvl 1 | remote, farming | 1 | 1 |
| **Mudfell** | Hamlet | 400 | 2.400 | lvl 2 | remote, pastoral | 1 | 1 |
| **Rasu** | Village | 1.400 | 9.000 | lvl 3 | farming | 3 | 2 |
| **Cryssborn** | Village | 800 | 5.200 | lvl 5 | remote, quarry | 3 | 2 |
| **Woodholmers** | Village | 1.000 | 6.400 | lvl 6 | forestry | 3 | 2 |
| **Mineholmers** | Village | 1.000 | 6.800 | lvl 7 | mining | 3 | 1 |
| **Perchmon** | Village | 1.600 | 10.500 | lvl 8 | pastoral, farming | 3 | 3 |
| **Flanel** | Town | 2.000 | 18.000 | lvl 10 | farming, trade_hub | 7 | 5 |
| **Incan** | Town | 5.500 | 46.000 | lvl 13 | quarry, forestry | 7 | 5 |
| **Shahum** | Town | 6.500 | 58.000 | lvl 16 | trade_hub, pastoral | 7 | 4 |
| **Yrad'si** | Town | 6.500 | 60.000 | lvl 19 | trade_hub | 7 | 4 |
| **Tarus** | Town | 7.000 | 66.000 | lvl 22 | mining, trade_hub | 7 | 4 |
| **Evoynir** | City | 8.000 | 120.000 | lvl 26 | trade_hub, farming | 8 | 6 |

**İsimler artık işlevi anlatıyor.** Mineholmers madenci, Woodholmers oduncu — zaten öyle isimlendirmişsin, tag'leri ona göre verdim. Fiyatlandırma bunları okuyor.

## 2. Ne değişti

**Önce:** 13 yerleşimin hepsinde aynı 4 dükkân (General/Blacksmith/Tanner/Alchemist). Carpenter ve Mason hiç yoktu. Tag yoktu, üretim yeri yoktu.

**Şimdi:** dükkân sayısı tier'a bağlı.

| Tier | Dükkânlar |
|---|---|
| Hamlet | General Store |
| Village | + Blacksmith, Carpenter |
| Town | + Tanner, Stonemason, Apothecary, Merchant House |
| City | + The Curio |

Mege'de tek dükkân var. **Craft malzemesi için yola çıkmak zorundasın** — seyahatin ilk somut sebebi.

## 3. Atölyeler

Yeni sınıf: `CraftStation`. Yerleşimin `Crafters` listesinde duruyor.

**Atölye seviyesi bir tavan.** Level 2 ocak çelik eritemez, oyuncunun kendi Smithing'i kaç olursa olsun — ateş yeterince ısınmıyor.

```csharp
MaxRecipeLevel  = level × 2        // lvl 3 atölye → 6. seviye tarife kadar
TimeMultiplier  = 1 − (level−1) × 0.08    // iyi atölye hızlı, taban %50
QualityBonus    = (level−1) × 5           // iyi alet, iyi sonuç şansı
UseFeeSilver                              // başkasının atölyesi ücretli
```

Bu, oyuncuya **kendi köyünü geliştirmek için somut bir sebep** veriyor: Evoynir'in Great Forge'unda çalışmak için oraya gitmen ve ücret ödemen gerekiyor, ya da kendi ocağını yükseltirsin.

Uzmanlık tag'i olan yerleşim ilgili atölyeyi bir seviye üstün alıyor — Mineholmers'ın ocağı komşularından iyi.

## 4. Fiyatlandırma düzeltmesi

`Wealth` 1.200 ile 120.000 arasında, yani 100 katlık aralık. Eski doğrusal formül iki uçta da doyuyordu, aradaki her kasaba aynı fiyatı veriyordu.

**Logaritmik eğriye çevirdim** — wealth on katına çıkınca fiyat aynı miktarda artıyor:

```
  1.200g → 0.87    hamlet, ortalıkta para yok
  9.000g → 1.00    sıradan köy
 60.000g → 1.13    varlıklı kasaba
120.000g → 1.20    şehir
```

## 5. Savaş ve ekonomik şok

Yeni sistem: `SettlementConflictSystem`. Gün tick'inde çalışıyor.

**Oyunu bozmaması için üç kural koydum:**

**1. Nadir ve sonlu.** Günlük %0.4 ihtimal, aynı anda tek savaş, 12-40 gün sürüyor. Ortalama 8 ayda bir savaş çıkıyor. Dünya canlı olmalı, kaotik değil.

**2. Hasar oran, sabit sayı değil.** Günde hazinenin %1.2'si. Ve **taban var** — bir yerleşim doğal servetinin %25'inin altına asla inemiyor. Ölmüş ekonomi = kapalı dükkân = oyuncunun boş odada "ne oldu" diye durması.

**3. Her şey toparlanıyor.** Her gün doğal seviyesine doğru farkın %2'si kadar geri geliyor. Kötü bir yıl atlatılabilir; kalıcı yara atlatılamaz, çünkü oyuncu oraya sonradan gelmiş olabilir.

**Yayılma etkisi** — asıl istediğin şey buydu:

```
Savaşan iki şehir günlük kaybeder
        ↓
Kaybın %35'i komşulara dağılır
        ↓
trade_hub olan ×1.6 etkilenir   (ticaretle yaşıyor)
remote olan   ×0.4 etkilenir   (zaten kimseyle alışverişi yok)
```

Böylece harita **bağlı bir şey** gibi okunuyor. Fiyatlar birlikte hareket ediyor, iki kasaba ötedeki savaş evde hissediliyor.

**Oyuncunun kendi köyü savaşa girmiyor.** Hiç dahil olmadığın bir savaşta köyünü kaybetmek hikâye değil, kötü sürpriz.

Savaşlar rapora isimli sebeplerle düşüyor:

```
Day 47 · War with Tarus — a caravan seized and never returned.
Day 71 · The war ends — winter arrived and settled it.
```

## 6. Unity'de

`Managers`'a ekle: **`SettlementConflictSystem`**

`settlements.json` hem `SourceData` hem `Resources/SourceData` altında güncellendi.

## 7. Dengelenmemiş

- **Savaş sıklığı %0.4/gün** — oyun testinde çok mu az çok mu fazla göreceğiz
- **`Walls.level`** tier'a göre otomatik verildi (Hamlet 0, City 3); savaş sisteminde henüz kullanılmıyor — surlu şehir daha az hasar almalı
- **Atölye ücretleri** tier × 4 + 6 gümüş; erken oyunda pahalı mı bakmak lazım
- **`MapX/MapY`** eklendi ama `MapHandler` hâlâ sahnedeki objelerden okuyor; harita refactor'ünde bağlanacak
- **`Quality` 15-88 aralığına çekildi** (eskiden 150-900); `Settlement.AddQuality` kullanan yerler kontrol edilmeli
