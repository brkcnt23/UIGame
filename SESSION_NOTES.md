# Oturum Notları — Unity'de Yapılacaklar

Uyurken yazılanlar. Kod tarafı hazır, **Unity'de birkaç tıklama gerekiyor.**

---

## 1. Önce bunları yap (sırayla)

### a) Component'leri ekle

`Managers` GameObject'ine şu script'leri ekle:

```
TimeTickDispatcher        (zaten eklemiştin)
QuestTimerSystem          (zaten eklemiştin)
EventCooldownSystem       (zaten eklemiştin)
WorldSimSystem            (zaten eklemiştin)
JobLimitSystem            ← YENİ
TraitSystem               ← YENİ
CharacterCreationSystem   ← YENİ
```

### b) Asset üret — iki menü tıklaması

```
Tools > UIGame > Traits > Generate trait assets
Tools > UIGame > Items  > Generate ItemSO assets from catalog
```

Birincisi **87 trait asset'i** + `Resources/TraitDatabase.asset` üretir. 87 ikonun hepsi eşleşiyor, kontrol ettim.

İkincisi **158 ItemSO** + `Resources/ItemDatabase.asset` üretir. 75'inin görseli var, 83'ü görselsiz üretilir (sprite slotu boş kalır). Sonradan PNG ekleyip tekrar çalıştırınca kendiliğinden bağlanır.

Eksik görsel listesi: `MISSING_ART.md`

### c) Beklenen console çıktısı

```
[BOOTSTRAP] Execution order (8 systems):
      30  TimeTickDispatcher
     200  WorldSimSystem
     295  CharacterCreationSystem
     310  InventorySystem
     335  TraitSystem
     365  JobLimitSystem
     380  QuestTimerSystem
     410  EventCooldownSystem
[ResourceProvider] TraitDatabase loaded (87 traits)
[ResourceProvider] ItemDatabase loaded
```

---

## 2. Ne değişti

### Mobil kaydetme — kritik
`JSONDataHandler` `Application.dataPath` altına yazıyordu. **Editor'de çalışır, telefonda çalışmaz** (APK read-only). Artık `persistentDataPath`.

Okuma üç kademeli: persistentDataPath → `Assets/` (sadece Editor) → `Resources/`. Mevcut test save'lerin çalışmaya devam eder, ilk kaydetmede yeni yere taşınır.

İçerik verisi `Assets/Resources/SourceData/` altına kopyalandı — build'e girmesi için gerekiyordu.

### XP ekonomisi
Eğri düzdü (her level 100 XP), o yüzden bir travel zinciri seni level 27 yapıyordu. Artık artan:

```
L1→2: 100    L5→6: 300    L10→11: 550    L19→20: 1000
Level 20'ye toplam ≈ 10.450
```

Ödüller senin istediğin gibi ölçekleniyor:

```
factor = clamp(1 - 0.03 × (oyuncuLevel - içerikLevel), 0.40, 1.25)

"Help the Scouts" (50 XP): L1'de 50 · L10'da ~37 · L20'de ~21
```

Cezalar ölçeklenmiyor — güçlendikçe başarısızlığın hafiflememeli.

Tüm XP tek kapıdan geçiyor: `ExperienceSystem.GrantExperience()`.

### İş limitleri
Her job **haftada 3 kez**. Gün tick'inde 7 günde bir sıfırlanıyor. Exhaustion günlük freni, bu haftalık freni.

> **Not:** sayaçlar henüz save'e yazılmıyor, oyunu kapatınca sıfırlanıyor. Bilinçli — save formatı değişince ekleyeceğim.

### Trait sistemi
87 trait, 4 tür:

| Tür | Ne | Örnek |
|---|---|---|
| Origin | Nerede büyüdün, hiç kaybolmaz | ForgeRaised, StreetRaised |
| Personality | Seçimlerle kazanılır/kaybedilir | Calm Mind, Cold Pragmatist |
| Familiarity | Çalışarak kazanılır | Forge Familiar |
| Condition | Süreli, saat tick'inde biter | Bleeding, Well Rested |

Yüzde etkiler tip başına toplanıp **%40'ta kesiliyor** — dört crafting trait'i toplayan oyuncu güçlü olur, bedava item üretmez.

Karşıtlar birbirini siliyor: Nourished gelince Starving gidiyor.

Açıklamalar senin istediğin dilde yazıldı:

> **Forge Raised** — *"Sıcağın ve çekiç sesinin içinde büyüdün. Metal, kafana anlam ifade etmeden önce ellerine ediyor."*
> `Craft quality +8%` · `Strength +1`

### Başlangıç soruları
4 soru, 20 cevap. Her cevap: stat değişimi + trait + tag.

1. Nerede büyüdün? → 8 origin
2. En kötü kışı nasıl atlattın? → dayanıklılık/disiplin/ikna/pragmatizm
3. Bir şey ters gidince ilk ne yaparsın? → mizaç
4. Köyünden neden ayrılıyorsun? → hedef

Statlar 8'den başlıyor, sorular ±3 oynatıyor, taban 4. UI'a bağlanmayı bekliyor — `CharacterCreationSystem.Instance.Begin()` ile başlar, `Answer(answerId)` ile ilerler.

Test için `RandomizeAndApply()` var, soruları tıklamadan geçmek istersen.

### Item sistemi
Kategoriler eklendi: Shield, Helmet, Gloves, Trinket, Consumable, TradeGood, QuestItem. Eskiler yerinde kaldı, save'ler bozulmadı.

Kalite: **Crude → Common → Fine → Masterwork → Legendary** (80/100/130/170/220%).

Aynı silahın farklı stat vermesi çözüldü — `ItemSO.RollInstance()`:

```
Kalite çarpanı + ±%15 varyans = her craft farklı sonuç
Bir shortsword +3, diğeri +1
```

`DerivedStats` yazıldı: Attack/Defense/Accuracy/Initiative/CriticalChance artık **saklanmıyor, hesaplanıyor.** Silah DEX'ten mi STR'den mi ölçekleniyor ona bakıyor, zırh ağırlığı DEX katkısını sınırlıyor.

### UI binder
`StatBinder` — profil panelindeki değer label'ına ekle, açılır menüden ne göstereceğini seç. 30 alan destekliyor (HealthPair `91/100`, ExperiencePair `240/600`, MoneyPair `300g 95s`...).

NavUISystem'deki 20 manuel TMP referansının yerini alacak.

---

## 3. Düzeltilen buglar

| Bug | Neydi |
|---|---|
| **Phase 3 boş** | `BeforeSceneLoad`'da `FindObjectsByType` çalışıyordu — sahne henüz yoktu. Kayıt `Start()`'a alındı. |
| **Quest süresi** | `hoursToComplete -= Hour` — geçen süreyi değil günün saatini çıkarıyordu. 14:00'te 1 saat ilerletince 14 saat gidiyordu. |
| **Event cooldown** | `if (Hour >= 24)` bloğunda tek sefer çalışıyordu; 3 günlük travel'da 1 gün düşüyordu. |
| **Level 27** | Düz XP eğrisi + ölçeksiz ödül. |
| **Dice index** | `Dice.Roll(list.Count)` 1..N döndürüyor, index olarak kullanılmış. Tek elemanlı listede hep patlıyordu. `Dice.Pick()` eklendi. |
| **Event çıkmazı** | Bandit Leader's Hideout'ta üç seçenek de kilitliydi. İçeriğe şartsız seçenek eklendi + `EventPanel` artık hiçbiri açılamıyorsa otomatik "Walk away" ekliyor. |
| **SettlementHandler NRE** | `OnEnable/OnDisable` null `settlement`'a erişiyordu. |
| **Buton karıştırma** | `RandomizeButtonOrder` elemanı kendisiyle takas edebiliyordu, dağılım eşit değildi. Fisher-Yates. |
| **Başlangıç statları** | Hepsi 1'di, eventler 5-15 istiyordu. 8'e çekildi. |

---

## 4. Bilinçli bıraktıklarım

- **JobLimitSystem save'e yazmıyor** — save formatı değişince eklenecek
- **`StatBinder.CurrentTitle()` "Commoner" döndürüyor** — ünvan sistemi PlayerData'ya bağlanmadı, uydurma rütbe göstermek yerine dürüst duruyor
- **`CompanionSlots()` 1 döndürüyor** — aynı sebep
- **`TimeSystem.SleepAndEatWhileTraveling`** hâlâ TimeSystem'de — travel state'e bağlı olduğu için TravelSystem'e taşınacak
- **`LegacyCompatibility.cs` duruyor** — migration bitene kadar gerekli

---

## 5. Sana sorularım

**1. Trait kazanma/kaybetme tetikleyicileri.** Sistem hazır ama *ne zaman* trait verileceği yazılmadı. Örnek: 3 gün üst üste aç kalınca `Hunger Hardened` mı gelsin, yoksa sadece event seçimleriyle mi? Kalıcı trait'i kaybetmek neye bağlı olsun?

**2. Ünvanı PlayerData'ya bağlayalım mı?** `TitleDatabaseSO` hazır, 26 ünvan var. `PlayerData`'ya `CurrentAdminTitleId`, `CurrentMartialTitleId`, `Standing`, `Renown` eklersem ünvan sistemi çalışmaya başlar. Save formatı değişir.

**3. Ekipman slot sistemi.** `Item.IsEquipped` ve `EquippedSlot` eklendi ama slot yönetimi (bir slota bir item, two-handed off-hand'i kilitler) henüz yok. Bunu yazayım mı, yoksa önce inventory UI'ı mı açalım?

**4. İlk görev "kulübeni inşa et"** — odun topla → carpenter → ev → ücretsiz uyku. Bunu yazmamı ister misin? `Residentials` altyapısı hazır, `prerequisiteBuildingId` alanı eklenecek.

**5. `ChatGPT Image ...` adlı 12 dosya** `UI Elements` kökünde duruyor, hiçbir item'a bağlanmadı. Ne olduklarını bilmiyorum — silinecek mi, adlandırılacak mı?
