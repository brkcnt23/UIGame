# UIGame — Karar Dokümanı

Son güncelleme: 2026-07-25
Bu dosya alınan tasarım ve mimari kararların tek referansıdır. CLAUDE.md vizyonu anlatır; bu dosya **ne yapacağımızı** kilitler.

---

## 0. Durum

Unity **6000.2.7f2** · URP 17.2 · uGUI + TMP · DOTween Pro · mobil portrait
89 C# dosyası · ~13.634 LOC · tek sahne (`Main.unity`)

Oyun kısmen çalışıyor: settlement → job → time → reward döngüsü kapanıyor, travel ve event tetikleniyor, harita ve avatar animasyonu çalışıyor. Crafting ve inventory oturmamış.

Console temiz (0 hata, 3 zararsız uyarı). 6.2.6f2'deki UnityConnect token spam'i sürüm yükseltmesiyle geçti.

---

## 1. Mimari

### 1.1 Kazanan mimari

Kod tabanında üç mimari aynı anda yaşıyordu. Karar: **Core/ katmanı kazanır.**

- `ManagerHolder` + `IInitializable` → **silinecek**
- `LegacyCompatibility.cs` → geçiş bitince **silinecek** (bu dosya durdukça migration bitmemiş sayılır)
- 23 adet `static Instance` → sistem sistem tasfiye edilecek

`GameBootstrapper` çalışıyor ama boş: `IGameSystem`'i implement eden sıfır sınıf var. İskelet hazır, içi doldurulacak. Bu bilinçli bırakılmış bir boşluk, hata değil.

> Mevcut hard-coded kodlar bilinçli hız tercihiydi, hata değil. Hepsi refactor edilecek.

### 1.2 Sahne hiyerarşisi

```
[BOOTSTRAP]              GameBootstrapper (DontDestroyOnLoad)
  ├─ [CORE]              StateManager, EventDispatcher, GameloopManager, ResourceProvider
  ├─ [SYSTEMS]           Time, Food, Economy, Job, Crafting, Shop, Inventory,
  │                      Experience, Travel, Settlement, WorldSim, Battle,
  │                      Companion, Title, Building
  └─ [UI]                UIRouter
       └─ ProfileUI, ActivitiesUI, TownHallUI, TavernUI, CraftingUI, ShopUI,
          InventoryUI, MapUI, EventUI, ArmyUI, HomeUI, ReportUI, CompanionUI
```

Şu an: `Managers` adlı tek GameObject'te 22 script, UI ile sistem karışık. Sorun script sayısı değil, katman ayrımının olmaması.

### 1.3 Execution order

Inspector'da sürükleyerek değil, kodda `Priority` ile. Sahne dosyasında saklanan sıra code review'da görünmez ve merge'de kırılır.

```csharp
public abstract class GameSystemBase : MonoBehaviour, IGameSystem
{
    public abstract int Priority { get; }        // düşük = önce

    protected EventDispatcher Events { get; private set; }
    protected StateManager State { get; private set; }

    public void Initialize(EventDispatcher events, StateManager state)
    {
        Events = events; State = state;
        OnInitialize();
    }

    protected abstract void OnInitialize();
    public virtual void OnHourTick(int hour) { }
    public virtual void OnDayTick(int day) { }
}

public abstract class UIPanelBase : MonoBehaviour
{
    public abstract string PanelId { get; }
    public virtual void OnOpen() { }
    public virtual void OnClose() { }
    public abstract void Refresh();      // state değişiminde çağrılır
}
```

Priority bandları: Core 0-99 · Repository 100-199 · Simülasyon 200-299 · Oyuncu sistemleri 300-399 · UI 900+

### 1.4 asmdef katmanları

`UIGame.Core` → `UIGame.Domain` → `UIGame.Systems` → `UIGame.UI` → `UIGame.Tests`
Bağımlılık tek yönlü. Mimarinin tekrar karışmasını derleyici seviyesinde engeller.

### 1.5 Tekilleştirilecek çift kaynaklar

| Sorun | Karar |
|---|---|
| `PlayerData` ↔ `PlayerState` | `PlayerData` sadece serialization DTO. Runtime otorite `GameState`. Arada açık mapper. |
| `Assets/Data/*.json` ↔ `Assets/SourceData/*.json` | Beş dosya da farklı, senkron bozulmuş. `SourceData` tek kaynak, `Data` kaldırılır. |
| `NavigationStack` × 2 | Biri silinir, `UIRouter`'a devredilir. |
| `BattleManager` / `BattleManagerOLD` | OLD silinir. |
| `SettlementType` ↔ `SettlementTier` | Tek enum'a indirilecek (`SettlementTier`). |

---

## 2. Statlar ve Türetme

**Temel stat 4 tane:** Strength, Dexterity, Constitution, Charisma. Saklanır, XP ile büyür.
**Attack / Defense / Accuracy / Critical türetilmiş değerlerdir** — hesaplanır, saklanmaz.

Intelligence, Wisdom, Stamina **yok**. Büyü savaşta olmadığı için INT gereksiz; algı/sezgi türü etkiler **trait** sisteminde durur (`Observant` zaten mockup'ta var). Sadece birkaç event kontrolü açan şey stat değil trait'tir.

### 2.1 Formüller

```
Mod(stat) = floor((stat - 10) / 2)          // D&D standardı

AttackBonus  = Mod(scalingStat) + Proficiency(silah skill seviyesi)
Damage       = XdY + Mod(scalingStat)
Accuracy     = 10 + Mod(DEX)
Defense      = armorValue + DexKatkısı(zırh ağırlığı)
                 Light  → +Mod(DEX) tam
                 Medium → +min(Mod(DEX), 2)
                 Heavy  → +0
MaxHealth    = taban + (Mod(CON) + 4) × Level
Initiative   = Mod(DEX)

İsabet: d20 + AttackBonus ≥ hedefin Defense'i
Kritik: doğal 20 → çift hasar    ·    Fiyasko: doğal 1
```

`DICE.cs` bu sistem için zaten yazılmış: `RollSuccess(target, roll, modifier, difficulty)`, `RollCriticalSuccess`, d20/d12/d10/d8/d6/d4/d100. Yeni bir zar altyapısı gerekmiyor.

### 2.2 Silah ölçekleme

```csharp
public enum ScalingStat { Strength, Dexterity, Hybrid }
```

| Silah | Scaling | Zar |
|---|---|---|
| Kılıç, balta, gürz | STR | d8 |
| İki elli kılıç, teber | STR | d12 |
| Hançer, rapier | DEX *(finesse)* | d4–d6 |
| Yay, arbalet | DEX | d8–d10 |
| Cirit, mızrak, fırlatma baltası | **Hybrid** | d6 |

Hybrid = `Mod((STR + DEX) / 2)`.

### 2.3 Statların savaş dışı işi

| Stat | Savaş | Savaş dışı |
|---|---|---|
| **STR** | Ağır silah isabet + hasar | Taşıma kapasitesi *(kodda var)*, ağır işçilik job'ları, eventte zorlama seçenekleri |
| **DEX** | Hafif/menzilli isabet, accuracy, defense, initiative | Craft kalite şansı, pusudan kaçınma, gizlilik eventleri |
| **CON** | Max HP | **Exhaustion direnci** — max seviye ve artış hızı; hastalık, zehir, açlık kontrolleri |
| **CHA** | — | Shop fiyatları, quest ödülü pazarlığı, event diplomasisi, companion ve rivalry ikna |

CON'un exhaustion'a bağlanması kritik: oyunun çekirdeği survival, CON o baskıyı yönetir. Şu an CON hiçbir işe yaramıyor.

---

## 3. Savaş

**Ortak iskelet — 1v1 ve ordu aynı yapıyı kullanır.** İki farklı oyun hissi olmaması için.

| Aşama | 1v1 | Ordu |
|---|---|---|
| Savaş öncesi | Duruş seçimi, item, kaçış | Formasyon, birim yerleşimi, hangi kanada yüklenilecek |
| Çözüm | Otomatik, tur tur log | Otomatik, aşama aşama log |
| Ara karar | Her 3 turda veya HP %30 altı | Her aşamada veya kanat kırılınca |
| Ortak katman | Terrain + weather modifikatörü · `DICE` · aynı log paneli | |

**Duruşlar:** Agresif `+2 attack / −2 defense` · Dengeli `0/0` · Temkinli `−2 attack / +2 defense`

**Ara karar örnekleri:**
- 1v1: *"Kolun yaralandı"* → Zorla / Geri çekil *(Mod(DEX) kaçış kontrolü)* / İksir iç
- Ordu: *"Sol kanadın kırılıyor"* → Yedekleri sür / Geri çekil / Tut

### 3.1 Ordu birimleri

**5 birim sabit.** Kodda `UnitType.Knight` var ama UI'da "Cavalry" yazıyor — **`Cavalry` olarak yeniden adlandırılacak**, çünkü `Knight` ünvan merdiveninde kullanılıyor.

`Cavalry · Soldier · Shielder · Archer · Pikeman`

Counter zinciri, terrain avantajları ve weather dezavantajları `BattleSimulator`'da **zaten yazılı** — korunacak.

---

## 4. Ölüm

**Tetikleyici:** HP = 0 **veya** exhaustion seviyesi = limit.
**Exhaustion limiti kademeli:** oyun başında 3, ilerledikçe 6'ya çıkar.

**Test aşamasında ölüm etkisiz.** Sadece `Debug.Log` yazılır, oyun devam eder. Ölüm sonucu (game over / bedel ödeyip devam / permadeath) sonraya bırakıldı.

Mevcut kod bozuk, düzeltilecek:

```csharp
// PlayerStatHandler.CheckExhaustionMaxed()
if (level == max)      Debug.LogError(...);   // hiçbir şey olmuyor
else if (level > max)  GameManager.Instance.Death();

// GameManager.Death()
public void Death() { Debug.Log("You are Dead..."); }   // tek satır
```

91 HP'yle "You are Dead" görülmesinin sebebi bu.

---

## 5. Yerleşim ve Ünvan

### 5.1 Yerleşim kademeleri

```
Hamlet → Village → Town → City
```

- **Castle ayrı bir tier değil.** Town'un görseli: kale etrafına kurulmuş yerleşim.
- **City** = duvarlarıyla tüm yerleşimi içine alan, içinde ayrıca kale bulunan yer.

### 5.2 İtibar

| Dal | Stat | Kaynak |
|---|---|---|
| İdari | **Standing** | Job, crafting, settlement geliştirme, ticaret |
| Askerî | **Renown** | Savaş, haydut temizleme, kervan koruma, ordu |

*Standing* dikey bir kavram (yapı içindeki basamak), *Renown* yatay (adının yayılması). Anlamca çakışmazlar.

### 5.3 Ünvan merdiveni — 26 kademe

**Zorunlu çapraz ilerleme.** Oyuncu tek daldan yürüyemez; her kilometre taşı iki dalı da görmeyi gerektirir.

| # | Dal | Ünvan | Yerleşim | Slot |
|---|---|---|---|---|
| 1 | İdari | Freeman | | |
| 2 | İdari | Tithingman | | |
| 1 | Askerî | Footman | | |
| 2 | Askerî | Man-at-Arms | | |
| ★ | **Ortak** | **REEVE** | Hamlet | 1 |
| 3 | İdari | Hayward | | |
| 4 | İdari | Beadle | | |
| 5 | İdari | Constable | | |
| 3 | Askerî | Veteran | | |
| 4 | Askerî | Sergeant | | |
| 5 | Askerî | Squire | | |
| ★ | **Ortak** | **BAILIFF** | Village | 2 |
| 6 | İdari | Warden | | |
| 7 | İdari | Provost | | |
| 8 | İdari | Chamberlain | | |
| 6 | Askerî | Bannerman | | |
| 7 | Askerî | Household Knight | | |
| 8 | Askerî | Knight | | |
| ★ | **Ortak** | **BARON** | Town | 3 |
| 9 | İdari | Steward | | |
| 10 | İdari | Seneschal | | |
| 11 | İdari | Justiciar | | |
| 9 | Askerî | Knight Banneret | | |
| 10 | Askerî | Castellan | | |
| 11 | Askerî | Marshal | | |
| ★ | **Ortak** | **DUKE** | City | 4 |

**Geçiş kuralı:** her kilometre taşı için toplam 6 ünvan, her daldan en az 2. İlk eşik: toplam 4, her daldan en az 1.

**Kazanma koşulu:**
- Ara kademeler: sadece itibar eşiği, otomatik gelir.
- Kilometre taşları (4 adet): itibar eşiği **+** metrik kapısı (pop/wealth/quality) **+** tören quest'i.

Metrik kapısı "atlama zıplama" olmasın diye var: itibarın yeter ama köyün Village olacak durumda değilse Bailiff olamazsın.

### 5.4 Kilometre taşlarının kilitlediği şeyler

Dördü de geçerli:

1. Yerleşim tier'ı + companion slot
2. Bina türleri ve upgrade'ler (kale, sur, kışla, pazar)
3. Ordu büyüklüğü tavanı
4. Quest ve event katmanı — yüksek ödüllü içerik

### 5.5 Kral

**Oyuncu kral olamaz.** Sadece doğduğu köyü büyütür, fetih yok.

Kral hikâyede var: **Bailiff olunca kraliyetten görevler gelmeye başlar**, oyunun sonunda kral oyuncuyu takdir eder. Bu, oyuncuyu evrenin merkezi yapmadan önemli kılar.

Taçlı görseller **Duke** için kullanılacak. Kral figürü oyuncuya değil, hikâyedeki krala saklanacak.

### 5.6 Uygulanan dosyalar

- `Assets/Scripts/Base/TITLE System/TitleDatabaseSO.cs` — 26 ünvan, tek asset, `Populate Default Titles` context menu'sü, sprite slotları (`mapAvatarIcon`, `titleBadge`)
- `Assets/Scripts/Base/SETTLEMENT System/SettlementIconSetSO.cs` — tier başına kilitli/açık ikon, home ve quest varyantı, nameplate

Oyuncunun harita avatarı ünvana göre değişir (`TitleDefinition.mapAvatarIcon`).

---

## 6. Companion

### 6.1 Ne olduğu

**Hem savaşçı hem bonus sağlayıcı.** Mevcut `Companion` sınıfındaki stat bloğu bilinçli olarak korunuyor — companion yola çıkar, savaşır, XP kazanır, level alır.

Eksik olan bonus katmanı. Şu an tek etki: `companionBonus = Companions.Count * 5f` (taşıma kapasitesi). `companions.json` boş.

### 6.2 Bonus modeli

```csharp
public enum BonusType {
    CraftingResourceCost, CraftingTime, BuildTime, TravelTime,
    ShopBuyPrice, ShopSellPrice, RationConsumption, ExhaustionGain,
    EventSuccessChance, CarryCapacity, JobReward, QuestReward
}
```

- Aynı tip içinde **toplamalı**, çarpımsal değil
- Tip başına **sert tavan** (örn. −%40)
- **Bonus level'la ölçeklenmez, sabit kalır.** Level ile stat ve savaş gücü büyür, yüzde sabit durur.

### 6.3 Tier

| Tier | Bonus | Koşul | Rivalry |
|---|---|---|---|
| Hireling | %2-4 | Sadece para, tavern'de hep var | Yok |
| Notable | %5-8 | Para + tag **veya** quest | Zayıf (faction) |
| Unique | %12-20 | Ünvan + quest + para | Sert kilit |

### 6.4 Rivalry ve ikna

Örnek: **Mark** (−%15 crafting resource) ⟷ **Jacob** (−%20 build time). Biri alınırsa diğeri katılmaz.

Seçilmeyen dünyadan silinmez — rakip bir lord'un yanına katılır, eventlerde karşına çıkar, o settlement onun bonusunu alır.

**İkna iki kapıdan geçer:**

1. **Dünya simülasyonu kapıyı açar.** Jacob'ın settlement'ı kıtlık / veba / haydut akını yerse veya wealth'i eşiğin altına düşerse Jacob "available" olur. Bu, WorldSim'i dekoratif olmaktan çıkarıp **fırsat üreten** sisteme dönüştürür.

2. **Personality tag ikna yolunu belirler** (`NpcData.PersonalityTags` zaten var):

| Tag | İkna yolu |
|---|---|
| `greedy` | Yüksek para |
| `proud` | Ünvanın onun lord'undan yüksek olmalı |
| `honest` | O settlement'ta itibar / tamamlanmış quest |
| `respects_strength` | Lord'unu savaşta yenmek |

### 6.5 Slot ve upkeep

Slot ünvana bağlı: Reeve 1 · Bailiff 2 · Baron 3 · Duke 4.
Rasyon tüketimi zaten çalışıyor (`Companions.Count + 1`). Üstüne ücret ve morale eklenecek.

---

## 7. Eşya Sistemi

### 7.1 Kuşanılabilir — slot bazlı

| Slot | Kategori | Not |
|---|---|---|
| Main Hand | `Weapon` | |
| Off Hand | `Shield` / `Weapon` | Dual wield **sadece hançer ve shortsword**. Two-handed bu slotu kilitler. |
| Head | `Helmet` | |
| Chest | `BodyArmor` | |
| Legs | `Leggings` | |
| Feet | `Boots` | |
| Hands | `Gloves` | |
| Trinket ×2 | `Trinket` | |

### 7.2 Kuşanılamaz

| Kategori | İçerik |
|---|---|
| `Consumable` | İksir, yiyecek, sargı |
| `Resource` | **Ham**: cevher, post, kütük, ot |
| `CraftingMaterial` | **İşlenmiş**: demir külçe, tabaklanmış deri, kalas |
| `TradeGood` | Kullanımı yok, pahalı satılır |
| `QuestItem` | Satılamaz, düşürülemez |
| `Misc` | Geri kalan |

`Resource` → `CraftingMaterial` → `Item` üç aşamalı zinciri ana köy üretim planıyla örtüşür: oduncu kütük çıkarır, marangoz kalasa çevirir, zanaatkâr eşya yapar.

Mevcut kod: `Weapon, Armor, Boots, Leggings, Potion, CraftingMaterial, Resource, Misc`. Shield, Helmet, Gloves, Trinket, TradeGood, QuestItem eklenecek. `IsEquippable` içindeki `Misc` çıkarılacak.

### 7.3 Trinket

Tür serbest — amulet, ring, kapüşonlu pelerin, fener, heykelcik, mendil. **Craft edilemez.** Dünyada sayıları ve yerleri sabittir, görev ödülü olarak gelir.

Trinket stat vermez, **olasılık büker.** Nadir büyünün doğal evi burasıdır.

### 7.4 Kalite kademeleri

```
Crude → Common → Fine → Masterwork → Legendary
```

- Masterwork çok nadir
- **Legendary: her eşyanın dünyada tek bir tanesi vardır.** Nadirlik buradan gelir, ayrıca bir şansa gerek yok.
- Craft skill seviyesi yüksek kalite çıkarma şansını artırır — Skills mockup'ındaki *"Quality Chance +7%"* satırı buna bağlanır

`ItemSO`'da `quality` 0/1/2 (Common/Rare/Epic) olarak var, 5 kademeye genişletilecek.

### 7.5 Silah alanları (ItemSO'ya eklenecek)

```csharp
public WeaponClass weaponClass;
public ScalingStat scaling;
public int damageDiceCount = 1;
public int damageDie;              // 4, 6, 8, 10, 12
public bool twoHanded;
public int armorValue;
public ArmorWeight armorWeight;    // Light / Medium / Heavy
```

### 7.6 Açıklama standardı — ÖNEMLİ

**Anlatı satırı ne yaptığını söyler, mekanik satır ne kadar olduğunu.** İkisi karışmaz.

```
Ashwood Longbow                              Fine

Yıllanmış dişbudaktan, sabırla eğilmiş bir yay.
Kolun uzun menzilde daha az titriyor.
──────────────────
Attack +2      Accuracy +5%
```

**Efekt isimlendirme kuralı: yetenek gibi yaz, stat gibi değil.**

| Kötü | İyi |
|---|---|
| +5% Weather Penalty Bonus | *Göğü okumayı öğrendin — kötü hava seni daha az yavaşlatır* → `Weather penalty −5%` |
| +1 Attack | *Silahı kavrayışın oturdu* → `Attack +1` |
| −15% Crafting Resource Cost | *Malzemeyi israf etmiyorsun* → `Material use −15%` |
| +2 Event Success | *İnsanların yüzünü okuyorsun* → `Persuasion +2` |

Örnekler:
- **Kalkan** — *"Omzuna oturuyor. Darbeyi kolunla değil gövdenle karşılıyorsun."* → `Defense +3`
- **Eldiven** — *"Parmakların soğukta uyuşmuyor, iğne işi bile yapabilirsin."* → `Craft quality +4%`
- **Muska** — *"Kuzeyli bir kadının ördüğü saç teli. Hastalık uğramaz derler."* → `Illness resistance +2`
- **Yüzük** — *"Pazarlıkta parmağında döndürürsün, karşı taraf tereddüt eder."* → `Haggling +2`

### 7.7 Skill'e bağlı değerlendirme metni

Aynı eşya, oyuncunun ilgili skill seviyesine göre farklı okunur:

| Smithing | Metin |
|---|---|
| 1 | *"Ağır bir kılıç. Keskin görünüyor."* |
| 4 | *"Kabza biraz ağır ama denge doğru. İyi çelik, acele dövülmemiş."* |
| 8 | *"Katmanlı çelik. Su verme izleri düzgün — bunu yapan adam ne yaptığını biliyormuş."* |

Maliyet: kalite kademesi (5) × skill bandı (3) = **15 metin**, eşya başına değil toplamda. Sonradan silah sınıfına özel varyant eklenebilir.

Amaç: envanteri tablo olmaktan çıkarıp karakterin bakış açısı yapmak.

---

## 8. Büyü

**Büyü dünyada var ama nadir.** Cadı benzeri, halk inancı seviyesinde. Sorcerer/wizard/warlock büyüsü yok.

- **Sadece eventlerde görünür. Savaşta büyü yok.**
- Büyücüler evrende çok az
- `ItemSO.isMagical` korunur — büyülü trinket'ler oyunun en nadir bulgusudur
- Trinket açıklamalarında belirsiz dil kullanılır (*"derler"*, *"uğramaz"*) — dünya büyüye emin değilmiş gibi konuşur

İkinci oyuna büyü sistemi düşünülebilir.

---

## 9. Dünya Simülasyonu (WorldSim)

Sıfırdan yazılacak. Şu an var olan şey simülasyon değil: `Settlement.AddPopulation/AddWealth` event fırlatıyor, `SettlementHandler` sadece `Print` ediyor. Raporda aynı satırın 15 kez "−4 / +250" tekrarlanmasının sebebi bu.

**Performans:** `Update()` içinde değil, **gün tick'inde**. 15 settlement × günde 1 tick, ölçülemeyecek maliyet. Gerçekçilik bedavaya geliyor.

### 9.1 Olay tablosu

| Olay | Pop | Wealth | Quality | Ek etki |
|---|---|---|---|---|
| Kıtlık | −− | − | | Rasyon fiyatı fırlar |
| Veba | −−− | − | − | Birkaç gün sürer |
| Haydut akını | − | −− | | Shop stoğu düşer |
| Yangın | | − | −− | Bir bina hasar görür |
| Ağır vergi | − | − | | Göç, huzursuzluk |
| Bereketli hasat | + | + | | Rasyon ucuzlar |
| Panayır | | ++ | + | Shop stoğu ve çeşidi artar |
| Göç dalgası | ++ | | | |
| Yeni ticaret yolu | | + | | Kalıcı küçük gelir bonusu |
| Usta gelişi | | | + | **Bir crafter'ın seviyesi +1** |

### 9.2 Shop / crafter seviye artışı

**%0.5 şans, 10 günde bir kontrol.** Hesap: 15 settlement × ~4 birim = 60 birim, yılda ~2190 atış → **yılda ~11 artış** (tüm dünyada ayda ~1). Okunur ve hissedilir.

*(İlk önerilen %0.005 oranı yılda 0.1 artış demekti — pratikte hiç olmazdı.)*

Kör zar yerine "Usta gelişi" olayına bağlanır ki oyuncu raporda **neden** olduğunu görsün.

### 9.3 Rapor

Oyuncu o settlement'ın Town Hall'ında bir iş tamamlayınca oranın raporu açılır ve **satır satır birikir.** Rapor bir ödül olur, job sistemine değer katar.

---

## 10. Ana Köy ve Üretim

Ana köy diğer settlement'lardan farklıdır: içinde carpenter, mason gibi **hammadde çıkaran** üreticiler vardır. Diğer şehirlerde de bunlar var ama sadece kaynak satın almak için.

**Açılış zinciri:** Oyuncu önce seviye 1 bir oduncu satın alıp kurar ve işçi atar. Ancak ondan sonra diğer bina menüleri açılır.

Bu, inşa mekaniğini tek ucuz örnekle öğretir, sonra gerisini açar. Veri olarak `prerequisiteBuildingId` alanıyla çözülür, koda gömülmez.

### 10.1 Home panel kısayolları

**Ortak** (her yerleşimde): Town Hall · Tavern · Shops · Craftsmen
**Sahiplik** (sadece kendi köyünde): Buildings · Barracks *(ünvan ister)* · Companions · Storage

Treasury ayrı buton değil, Town Hall'un içinde.

---

## 11. Harita

### 11.1 Doğru olanlar

- `SettlementButtonPointer` / `MapHandler` ayrımı doğru — pin view, handler controller
- Quest settlement koordinatını üretip `ref` ile geri yazması, save'de kalıcı olması
- Random pozisyonda minimum mesafe kontrolü
- **`AddQuestSettlement()` doğru mimari** — prefab'dan runtime pin üretiyor

### 11.2 Düzeltilecekler

| # | Sorun |
|---|---|
| 1 | `for (int i = 0; i < 14; i++)` — sabit sayı. Sahnede 17 child var, son 3 settlement görülmüyor. |
| 2 | `children[settlements.IndexOf(settlement)]` — JSON sırası ile sahne sırası birebir aynı olmak zorunda. Kimlik eşleştirmesi yok. |
| 3 | `Decider` objesi de `children` listesine giriyor, `PopulateMap`'te null check yok |
| 4 | Koordinat verisi JSON'da yok, sadece sahne transform'unda |
| 5 | `SettlementType` eski karara ait (`Castle` var, `Hamlet`/`City` yok) |
| 6 | Unlock sadece level'a bakıyor, ünvan sistemine geçecek |

### 11.3 Hedef

Modüler versiyonu zaten yazılmış: `AddQuestSettlement()`. Statik settlement'lara da aynı desen uygulanacak:

- JSON'a `x`, `y` alanı eklenir
- Sahnedeki 16 obje silinir, hepsi prefab'dan doğar
- Eşleştirme `IndexOf` ile değil **`ID` ile** olur

Bu **daha az kod**, daha fazla değil. Settlement eklemek JSON'a bir satır yazmaya iner.

---

## 12. Bilinen Buglar

| # | Bug | Not |
|---|---|---|
| 1 | Crafting'de seçim yapıp geri dönünce liste boş | Panel state korunmuyor |
| 2 | Inventory oturmamış | |
| 3 | Profil paneli layout kırık | Panel yeniden tasarlandı, mockup'lar hazır |
| 4 | Tek travel'da Level 27 | `ExperienceSystem` |
| 5 | "You are Dead" 91 HP'de | Bölüm 4 |
| 6 | TMP glyph eksik: ★ `★`, ⚡ `⚡` | LiberationSans SDF'te yok |
| 7 | Craftmans panelinde başıboş "Button" | Debug artığı |
| 8 | `ItemSpriteDatabase` bulunamıyor | Console warning, opsiyonel |
| 9 | Log dili karışık (TR + EN) | Tek dile indirilecek |
| 10 | `JobLogger.spacing` atanmış ama kullanılmıyor | CS0414 uyarısı |

---

## 13. Yol Haritası

1. **Ölü kod temizliği** — `BattleManagerOLD`, boş `Systems/`, `Yeni klasör`, `NavigationStack` ikizi, `TestButton`/`TestScript`, kullanılmayan dosya taraması
2. **Mimari iskelet** — `GameSystemBase`, `UIPanelBase`, `IGameSystem` kaydı, Priority tabanlı exec order
3. **Sistem migrasyonu** — singleton'lar sistem sistem taşınır. `LegacyCompatibility.cs` silinince biter.
4. **asmdef katmanları**
5. **State tekilleştirme** — `PlayerData` → DTO, `GameState` → otorite
6. **Content pipeline tekilleştirme** — `SourceData` tek kaynak
7. **Ünvan + itibar sistemi** (Standing / Renown) — SO'lar yazıldı, sistem bekliyor
8. **Eşya sistemi genişletme** — kategoriler, slotlar, kalite, silah alanları, açıklama standardı
9. **Companion sistemi** — bonus katmanı + rivalry
10. **WorldSim + rapor**
11. **Savaş birleştirme** — 1v1 + ordu tek kod, ara karar noktaları
12. **Harita veri odaklı hâle getirme**
13. **UI router + panel binding** — 144 dağınık `SetActive` toplanır
14. **Bug listesi**
15. **Asset / görsel / ses**

---

## 14. Açık Sorular

1. Ölüm sonrası davranış (test aşamasından sonra)
2. İtibar eşik değerleri — `TitleDatabaseSO`'da taslak (`rank × 100`), dengelenecek
3. Ordu büyüklüğü tavanları — taslak (`segment × 25`)
4. 15 adet skill-bağlı değerlendirme metni yazılacak
5. Ordu paneli UI tasarımı
6. 26 ünvan ikonu ve settlement ikonları üretilecek
7. Companion bonus tavanları sayısal olarak belirlenecek
8. Item level sistemi kalite kademelerinin ötesinde bir şey içerecek mi
