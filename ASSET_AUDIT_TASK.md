# Görev: Görsel Envanteri ve İsimlendirme Denetimi

Bu dosya tek başına yeterlidir. Önceki sohbete ihtiyaç yoktur.

**Amaç:** Projedeki PNG dosyalarını beklenen isimlerle karşılaştır, eşleşmeyenleri ve eksikleri raporla, gerekirse yeniden adlandır.

---

## Girdi dosyaları

| Ne | Nerede |
|---|---|
| Beklenen isim listesi | `EXPECTED_ASSET_NAMES.txt` (proje kökü) |
| Görseller | `Assets/UI Elements/` ve alt klasörleri |

`EXPECTED_ASSET_NAMES.txt` üç bölümden oluşur, her satır `Ad|BeklenenDosyaAdı` biçiminde:

```
### ITEM (ItemCatalog.cs) — beklenen sprite adi
Club|club
Hand Axe|handaxe
...

### QUEST IKON AILESI (quests.json) — kac gorev kullaniyor
crown|6
cart|5
...

### TRAIT (TraitCatalog.cs) — beklenen ikon adi
Farm Raised|FarmRaised
...
```

---

## Eşleştirme kuralı

Karşılaştırma **normalize edilmiş** yapılır: sadece harf ve rakam bırak, hepsini küçült.

```
"Iron Ore.png"  → ironore
"IRON_ORE.png"  → ironore
"iron ore1.png" → ironore1  →  sondaki rakam kalite kademesi, at → ironore
```

Yani `Iron Ore.png`, `IronOre.png`, `ironore.png` **aynı sayılır.** Boşluk, alt çizgi, büyük/küçük harf önemsizdir.

Silah ve zırhta sondaki rakam kalite kademesidir: `Falchion1..4`. Bunlar tek bir item'a aittir, taban ad `Falchion`'dur.

---

## Yapılacaklar

### 1. Envanter çıkar

`Assets/UI Elements/` altındaki tüm `.png` dosyalarını listele. Şu klasörleri **atla**: `Buttons`, `Backgrounds`, `Texts`, `MaterialTexture tmp`.

`.meta` dosyalarını sayma.

### 2. Item karşılaştırması

`EXPECTED_ASSET_NAMES.txt` içindeki ITEM bölümündeki her satır için:

- Beklenen ad normalize edilip dosyalar arasında aranır
- **Bulundu** → tamam
- **Bulunamadı** → eksik listesine

Sonra ters yönde: hangi PNG hiçbir item'a karşılık gelmiyor → *fazla* listesine.

### 3. Quest ikon karşılaştırması

Quest ikonları `Assets/UI Elements/Quests/Icons/` altındadır ve **aile adıyla eşleşmez** — ressam çizdiği şeyin adını vermiştir. Eşleştirme tablosu:

| Aile | Kabul edilen dosya adları |
|---|---|
| livestock | goat, cow, cattle |
| vermin | rat, crow |
| predator | wolf, wolfpack, boar |
| herbs | herbs |
| parcel | crate, parcel |
| shield | shield |
| letter | letter, scroll |
| pick | pickaxelantern, pickaxe, pick |
| timber | axelog, axe, log |
| bandit | hoodedbandit, bandit, hood |
| missing | bootprint, boot, footprint, shoe |
| ledger | openledgerbook, ledger, book |
| tools | hammertongs, hammer, tongs |
| cart | cartwheel, loadedwagon, cart, wagon, wheel |
| banner | heraldicbanner, banner |
| crown | crownquesticon, crown |
| harvest | sheafofwheat, wheat, sickle |

Her aile için dosya var mı kontrol et. Yoksa, kaç görevi etkilediğini `EXPECTED_ASSET_NAMES.txt`'ten al ve raporda belirt.

### 4. Trait karşılaştırması

Trait ikonları `Assets/UI Elements/ProfilePanel/traits/250x250/` altındadır. TRAIT bölümündeki beklenen adlarla karşılaştır.

Not: bazı trait'ler aynı ikonu paylaşır (üç ağırlık kademesi hepsi `Overburdened` kullanır). Bu normaldir, hata sayma.

### 5. Importer alias tablosunu doğrula — EN ÖNEMLİ ADIM

Yukarıdaki tablo **olması gerekeni** anlatır. Oyunun gerçekte kullandığı tablo
`Assets/Editor/QuestSOImporter.cs` içindeki `IconAliases` sözlüğüdür.

İkisi ayrışabilir: dosya klasörde durur ama importer bulamaz. Bu durumda denetim
"var" der, oyunda ikon boş çıkar.

Eşleşme mantığı `Resolve()` içindedir ve şudur: alias normalize edilir, önce tam
eşleşme aranır, sonra **dosya adı alias ile başlıyor mu** diye bakılır
(`StartsWith`). Yani alias `wheat`, dosya `sheafofwheat` → **eşleşmez**, çünkü
dosya `wheat` ile başlamıyor, içeriyor.

Her aile için: koddaki alias listesinden en az biri, klasördeki bir dosya adının
**başlangıcı** mı? Değilse raporda *"dosya var ama importer bulamıyor"* diye
ayrı bir başlık altında listele. Bu, eksik dosyadan farklı bir hatadır.

### 6. Yeniden adlandırma önerisi

Eşleşmeyen ama **açıkça bir item/aileye ait olduğu anlaşılan** dosyalar için öneri üret:

```
FlintNSteel.png  →  FlintAndSteel.png     (katalog: "Flint & Steel")
```

Sadece **öneri** ver. Kendi başına yeniden adlandırma yapma.

---

## Rapor biçimi

Sonucu `ASSET_AUDIT_REPORT.md` olarak proje köküne yaz:

```markdown
# Görsel Denetim Raporu

## Özet
- Item: X / 160 var, Y eksik
- Quest ikon ailesi: X / 17 var
- Trait ikonu: X / 89 var
- Hiçbir yere bağlanmayan PNG: Z adet

## Eksik itemler
| Item | Beklenen dosya adı | Kategori |
|---|---|---|
...

## Eksik quest ikon aileleri
| Aile | Etkilenen görev sayısı |
|---|---|
...

## Eksik trait ikonları
...

## Dosya var ama importer bulamıyor
| Aile | Klasördeki dosya | Koddaki alias'lar | Neden eşleşmiyor |
|---|---|---|---|
...

## Yeniden adlandırma önerileri
| Mevcut | Önerilen | Sebep |
|---|---|---|
...

## Hiçbir yere bağlanmayan dosyalar
(sadece liste, yorum yok)
```

---

## Kurallar

- **Görselleri açıp içeriğine bakma.** Sadece dosya adlarıyla çalış. Görsel okumak pahalıdır ve bu iş için gerekli değildir.
- **Hiçbir dosyayı silme, taşıma veya yeniden adlandırma.** Sadece rapor yaz.
- **Kod dosyalarını değiştirme.**
- Betik yazmak serbesttir; Python + dosya listeleme yeterlidir.

---

## Bilinen durum (2026-08-06)

Bu tarihte yapılan son kontrolde:

- Item: 160 girdiden **155'i eşleşiyor**. Eksik olanlar: `Tin Ingot`, `Silver Ingot`, `Tanned Leather`, `Flint & Steel` *(dosya `FlintNSteel.png` adıyla var, isim uyuşmuyor)*, `Sling` *(bu item kaldırılacak, eksik sayma)*.
- Quest ikon ailesi: 17 aileden **16'sı eşleşiyor**. Eksik: `crown` — 6 görevi etkiliyor.
- Trait: 89 trait, hepsinin ikonu vardı.
- **Bilinen alias uyuşmazlığı (3 adet):** `ledger` → dosya `OpenLedgerBook`,
  `banner` → dosya `HeraldicBanner`, `harvest` → dosya `SheafOfWheat`. Koddaki
  alias'lar bu dosya adlarının başlangıcıyla eşleşmiyor. Denetimde bunları
  doğrula, hâlâ duruyor mu bak.

Yeni görseller eklendiyse bu sayılar değişmiş olabilir. Denetimi sıfırdan yap, bu bölümü sadece beklentiyi anlamak için kullan.
