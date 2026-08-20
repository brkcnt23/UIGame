# Shop Ekonomisi

7 dükkân profili, 86 farklı stok kalemi. Hepsi `ItemCatalog` ile eşleşiyor, doğruladım.

---

## 1. Temel kural: hiçbir dükkân her şeyi satmaz

Ve daha önemlisi: **her dükkân kendi zanaatının girdisini satar, çıktısını değil.**

Demirci külçe satar, senden cevher alır. Ama **bitmiş bir greatsword satmaz** — satsaydı craft etmenin anlamı kalmazdı.

Döngü böyle kuruluyor:

```
Ham sat  →  İşlenmiş al  →  Craft et  →  Bitmişi başka yerde sat
   ↓            ↓              ↓                ↓
Demirci     Demirci        Kendi atölyen    Genel/Tüccar
```

Her adım farklı bir dükkândan geçiyor. **Bir yerleşimde neden birden fazla dükkân olduğunun cevabı bu.**

## 2. Dükkânlar

| Dükkân | Satar | Alır | Marj (al/sat) | Max kalite |
|---|---|---|---|---|
| **Blacksmith** | Külçe, perçin, toka, kömür | Cevher, eski silah/zırh | 0.50 / 1.20 | Common |
| **Tanner** | Deri, iplik, kumaş, ip | Post, pelt, yün | 0.50 / 1.15 | Fine |
| **Carpenter** | Kalas, kiriş, **kömür** | Kütük | 0.50 / 1.15 | Fine |
| **Stonemason** | Kesme taş, tuğla, **cam şişe** | Ham taş, kil | 0.50 / 1.15 | Fine |
| **Apothecary** | İksir, bitki, özüt | Bitki, iksir | 0.55 / 1.25 | Fine |
| **General Store** | Yiyecek, ip, ucuz ekipman | **Her şey** | 0.45 / 1.30 | Crude |
| **Merchant House** | Sadece ticaret malı | Sadece ticaret malı | **0.70 / 1.10** | Masterwork |
| **The Curio** | Trinket, göktaşı demiri | Trinket | 0.60 / **1.60** | Legendary |

İki dükkân bilinçli olarak aykırı:

**General Store her şeyi alır ama en az öder.** Acele eden oyuncunun sığınağı, ve bedelini öder.

**Merchant House'un marjı dar (0.70/1.10).** Ticaret malı taşımak için yapılmış — kâr fiyat farkından değil, **mesafeden** gelir.

## 3. Yerleşim kademesine göre dükkân sayısı

| Tier | Dükkânlar |
|---|---|
| Hamlet | General Store |
| Village | + Blacksmith, Carpenter |
| Town | + Tanner, Mason, Apothecary, Merchant House |
| City | + The Curio |

Hamlet'te tek dükkân var — oyuncu craft malzemesi için Village'a gitmek zorunda. **Seyahatin ilk somut sebebi bu.**

## 4. Fiyatı dört şey belirliyor

```
Fiyat = TabanDeğer × DükkânMarjı × ZenginlikFaktörü × ArzFaktörü × PazarlıkFaktörü
```

**Dükkân marjı** — her zaman ucuza al, pahalıya sat.

**Zenginlik** (0.85–1.20) — zengin şehir hem çok öder hem çok ister. Aralık kasten dar; yoksa tek bir şehir dışında ticaret anlamsızlaşır.

**Arz** — asıl mekanik bu. Settlement tag'ine göre:

| Tag | Etki |
|---|---|
| `mining` | Metal %30 ucuz |
| `forestry` | Odun %30 ucuz |
| `pastoral` | Deri/post %30 ucuz |
| `quarry` | Taş %30 ucuz |
| `farming` | Yiyecek %20 ucuz |
| `trade_hub` | Ticaret malı %20 ucuz |
| `remote` | Her şey %35 pahalı |
| `famine` | Yiyecek **2 katı** |

Satarken arz **tersine** çalışır: cevhere boğulmuş bir kasaba senin cevherine iyi para vermez.

**Pazarlık** — Charisma modifier'ı başına %2, artı trait ve companion katkısı. `Trade Sense` trait'i −%7 alış / +%7 satış veriyor. **Charisma'ya nihayet iş bulundu.**

## 5. Anında yeniden satma kilidi

Kritik güvenlik: `MinimumSpread = 0.25`. Satış fiyatı asla alış fiyatının %75'ini geçemez.

Bu olmadan yüksek Charisma'lı oyuncu aynı dükkânda alıp satarak sonsuz para basardı. Pazarlık marjı daraltır ama **asla kapatamaz.**

## 6. Fiyat açıklaması

`PricingSystem.ExplainPrice()` tooltip'e tek satır döndürüyor:

> *"Common here."* · *"Scarce this far out."* · *"A wealthy market."* · *"Little coin in this place."*

Yüksek fiyat böylece **oyunun haksızlığı değil, dünyanın bir gerçeği** gibi okunur. Oyuncu ticaret rotasını bu satırlardan öğrenir.

---

## 7. Ekran akışı önerisi

Şu an `ShopMainPanel` ve `ShopListPanel` ayrı duruyor, doğru bölünme:

```
Settlement → SHOPS butonu
    ↓
ShopListPanel        o yerleşimdeki dükkânlar (isim + tür ikonu + tek satır tanıtım)
    ↓  seç
ShopMainPanel        stok listesi + BUY / SELL sekmesi
```

**BUY / SELL'i iki ayrı ekran değil, tek ekranda sekme yap.** Oyuncu satarken de dükkânın ne sattığını görmeli — "şunu satıp bunu alayım" kararı tek ekranda verilebilmeli.

Satır düzeni: `[ikon] [isim + kalite] [adet] [fiyat] [al/sat butonu]`

Fiyatın altına küçük gri `ExplainPrice()` satırı.

## 8. Dengelenmemiş olanlar

- **Zenginlik referansı 500 altın** = "sıradan kasaba" varsayıldı. `settlements.json`'daki gerçek değerlere bakıp ayarlanacak.
- **Stok yenilenme** henüz yok. WorldSim gün tick'ine bağlanabilir — örneğin 3 günde bir stok yeniden üretilsin.
- **Dükkân kasası** (`Shops.Cash`) kullanılmıyor. Kullanılırsa oyuncu fakir bir köye 10 tane full plate satamaz — gerçekçi ama fazladan UI ister.
- **Trinket'ler The Curio'da satılıyor** ama `isUnique` işaretli. Stok üreticisi bunları atlıyor; ya işareti kaldıracağız ya Curio'ya özel istisna yazacağız.
