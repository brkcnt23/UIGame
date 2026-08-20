# UIGame — Mimari Analiz (2026-07-25)

## 0. Özet

Proje teknik olarak sağlıklı bir temele sahip: Unity 6.2, URP, mobil portrait UI, DOTween, ItemSO tabanlı veri modeli, çalışan bir game loop. Sorun kod kalitesi değil — **üç ayrı mimarinin aynı anda yaşıyor olması**. Kod "karışık" hissi buradan geliyor.

Ölçüm: 89 C# dosyası, ~13.634 LOC, 23 singleton, 144 `SetActive` çağrısı, 0 asmdef, 3 ayrı içerik veri kaynağı.

---

## 1. Teknoloji Yığını

| Alan | Durum |
|---|---|
| Unity | 6000.2.6f2 (Unity 6.2) |
| Render | URP 17.2 + 2D Feature Set |
| UI | uGUI + TextMeshPro (UI Toolkit **kullanılmıyor**) |
| Tween | DOTween Pro (Demigiant) |
| Canvas | Screen Space – Overlay, ref 1920x1080, Match 0.5 |
| Persist | JSON (`JSONDataHandler`, 3 save slot) |
| Test | com.unity.test-framework var, asmdef olmadığı için pratikte çalışmaz |
| Sahneler | `Main.unity` (her şey burada), `MapSystemScene.unity`, `Test.unity` |

Ekran görüntündeki 348 console hatası: görünen satırlar `UnityConnectWebRequestException: Token Exchange failed` — bu **lisans/network gürültüsü**, derleme hatası değil. Kodla ilgisi yok.

---

## 2. Ana Problem: Üç Rakip Mimari

Kod tabanında birbirinin yerine geçmesi gereken ama hepsi aynı anda aktif olan üç katman var:

### Katman A — Legacy Singleton (en eski, en yaygın)
`GameManager.Instance`, `TimeSystem.Instance`, `ShopSystem.Instance`, `TravelSystem.Instance`, `PlayerStatHandler.Instance`… toplam **23 adet** `public static X Instance`.

En yoğun bağımlılar:
```
TravelSystem.cs        66 adet .Instance erişimi
JobSystem.cs           49
PlayerStatHandler.cs   35
TimeSystem.cs          33
SettlementHandler.cs   25
```

### Katman B — ManagerHolder + IInitializable (ikinci deneme)
`ManagerHolder.Start()` → child'larda `IInitializable.Initialize()` çağırıyor. Sahne hiyerarşisinde `Manager Holder` objesi olarak duruyor.

### Katman C — Core/ (üçüncü deneme, yarım)
`GameBootstrapper` → `ResourceProvider` → `StateManager` + `EventDispatcher` + `GameloopManager`, `IGameSystem` üzerinden sistem kaydı. Immutable `GameState` snapshot + `Clone()` deseni. Sahnede `GameBootstrapper` objesi var.

### Ve bir de köprü
`Core/LegacyCompatibility.cs` — kendi yorumunda "TEMPORARY, NOT production code" yazıyor. `InventorySystem` ve `InventoryUI`'a `partial class` ile sahte `.Instance` ekliyor.

**Sonuç:** Bir sistemin nereden initialize olduğu, state'in nerede tutulduğu, event'in hangi kanaldan aktığı dosyadan dosyaya değişiyor. Karışıklığın %80'i bu.

---

## 3. Çift Kaynak (Duplicate Source of Truth)

### 3.1 State ikizi
| Legacy | Yeni |
|---|---|
| `PlayerData.cs` (251 satır, JSON'a serialize) | `PlayerState` (`Core/GameState.cs` içinde) |

İkisi de `Gold`, `Silver`, `Health`, `Exhaustion`, `Strength/Dexterity/Constitution/Charisma` tutuyor. Hangisi otorite belli değil. Save/load `PlayerData` üzerinden, yeni sistemler `PlayerState` üzerinden çalışıyor.

### 3.2 İçerik verisi üç yerde
```
Assets/Data/*.json        (runtime kopya)
Assets/SourceData/*.json  (kaynak kopya)
Assets/Resources/Items/   (ItemSO ScriptableObject'ler) + ItemDatabase.asset
```
`Data/` ile `SourceData/` altındaki **beş dosyanın hepsi birbirinden farklı** (companions, events, jobs, quests, settlements) — yani senkronizasyon çoktan bozulmuş.

CLAUDE.md "ItemSO tabanlı, tek item database" diyor ama pratikte JSON + SO ikili yapısı var.

### 3.3 Sınıf ikizleri
- `NavigationStack` iki kez tanımlı: `Core/NavigationStack.cs` (global namespace) ve `Base/UI System/NavigationStack.cs` (`UISystem` namespace). Namespace farkı derlemeyi kurtarıyor ama iki ayrı singleton, iki ayrı panel stack'i var.
- `BattleManager.cs` (361 satır) ve `BattleManagerOLD.cs` yan yana duruyor.

---

## 4. UI Katmanı

Bu senin asıl hedefin olduğu için ayrı başlık.

**Mevcut durum:**
- Panel yönetimi **144 dağınık `SetActive()` çağrısı** ile yapılıyor. Merkezi bir router yok (iki NavigationStack var ama tam benimsenmemiş).
- `UIHandler.cs` içinde ~20 adet manuel `public GameObject` panel referansı.
- `NavUISystem.cs` içinde ~20 adet manuel `public TMP_Text` referansı (levelText, strengthText, smitherSkillLevelText…). Her yeni stat = Inspector'da yeni bir sürükle-bırak.
- `UIPanels.cs` 22 satırlık naif bir show/hide helper.
- UI, sistemleri doğrudan singleton üzerinden okuyor (`CraftingUI` 19, `EventPanel` 15, `PlayerUISystem` 16 `.Instance` erişimi) → data binding yok, "state değişti, UI kendini yenilesin" akışı yok.

**Görsel taraf iyi durumda.** Ekran görüntüsündeki HUD (health / day / clock / rations / exhaustion / gold), profil paneli, Overview–Skills–Traits sekmeleri, alt nav bar — art direction tutarlı ve mobil için doğru ölçeklenmiş. Sorun görselde değil, o görseli besleyen kodda.

---

## 5. Yapısal Hijyen

- **asmdef yok.** Tüm kod tek `Assembly-CSharp` içinde → her değişiklikte full recompile, test izolasyonu imkânsız, katman sınırlarını derleyici zorlamıyor. En düşük maliyetli, en yüksek getirili düzeltme bu.
- **Namespace neredeyse yok.** 89 dosyadan sadece 3'ü namespace kullanıyor (`NEXUS.Utilities`, `UISystem`, `ntw.CurvedTextMeshPro`). Geri kalan her şey global namespace'te.
- **Klasör isimlendirme tutarsız:** `BATTLE SYSTEM`, `Base/ACTIVITIES System`, `Managers`, `Systems` (boş), `Assets/Yeni klasör` (boş), `Assets/PrettyHierarchy-0.1.2`.
- **Katman karışması:** `Managers/` altında `TextProOnACircle.cs` / `TextProOnACurve.cs` gibi saf görsel yardımcılar duruyor. `TestButton.cs` ve `TestScript.cs` `Scripts/` kökünde, `TestButton` içinde 15 `.Instance` erişimi var.
- **Kritik dosyalar şişmiş:** `PlayerStatHandler` 678, `TravelSystem` 645, `CraftWorkSystem` 542, `ShopSystem` 540, `GameManager` 477 satır. `GameManager` hem save slot UI'ı hem panel yönetimi hem oyuncu verisi yüklüyor — üç sorumluluk tek sınıfta.

---

## 6. İyi Olan Şeyler

Sıfırdan başlamayı önermememin sebebi:

- Domain modeli düşünülmüş: `Settlement` → `Village`/`Town`/`Castle` hiyerarşisi, `Residentials` → `Shops`/`Taverns`/`TownHalls`/`Walls`.
- SO Constructor deseni (`Event_SO_Constructor`, `Job_SO_Constructor`, `Quest_SO_Constructor`, `Shop_SO_Constructor`) content pipeline için doğru yaklaşım.
- `RecipeService` + `RecipeContext` + `RecipeDatabaseSO` ayrımı temiz.
- `ShopStockBuilder` + `ShopStockProfileSO` — prosedürel stok üretimi, iyi tasarım.
- `Core/` katmanının **niyeti** doğru: tek entry point, immutable state snapshot, event dispatcher. Sorun tamamlanmamış olması.
- `DICE.cs` merkezi randomizasyon, `GameloopIntegrationTest.cs` var.
- CLAUDE.md kapsamlı ve tutarlı bir tasarım dokümanı — çoğu hobi projesinde bu yok.

---

## 7. Önerilen Yol (sıra önemli)

Sırayı bilinçli olarak "en az risk / en çok netlik" diye dizdim. Her adım tek başına merge edilebilir.

**1. Ölü kod temizliği** (yarım gün, sıfır risk)
`BattleManagerOLD.cs`, `Assets/Yeni klasör`, boş `Systems/`, `TestButton.cs`/`TestScript.cs` → `Tests/`. `NavigationStack` ikizinden birini sil.

**2. Tek mimariye karar ver** (en kritik karar)
Önerim: **Core/ katmanını kazanan ilan et**, Katman B'yi (ManagerHolder) sil. Katman A'yı tek seferde değil, sistem sistem taşı. `LegacyCompatibility.cs` silinene kadar geçiş bitmemiş sayılır.

**3. Tek state otoritesi**
`PlayerData` → salt *serialization DTO*'ya indir. Runtime otorite `PlayerState`/`GameState` olsun. `PlayerData` ↔ `PlayerState` arasına açık bir mapper koy.

**4. Tek content pipeline**
`SourceData/` = editör-zamanı kaynak, ondan SO üret; `Data/` runtime kopyasını kaldır. Ya tamamen SO ya tamamen JSON — ikisi birden değil.

**5. asmdef'leri ekle**
`UIGame.Core` → `UIGame.Domain` → `UIGame.Systems` → `UIGame.UI` → `UIGame.Tests`. Bağımlılık yönü tek yönlü. Bu, mimarinin tekrar karışmasını derleyici seviyesinde engeller.

**6. UI'ı state'e bağla**
Merkezi `UIRouter` (tek NavigationStack) + `StateManager` değişiminde otomatik re-render. `SetActive` çağrılarını router'a topla. Manuel TMP_Text referansları yerine küçük binder component'leri (`StatTextBinder` gibi).

**7. Sistemleri böl**
`GameManager`'ı üçe ayır: `SaveSlotService`, `MainMenuUI`, `PlayerDataRepository`. 500+ satırlık diğer dosyaları da sorumluluk bazında böl.

---

## 8. Karar Gereken Noktalar

Devam etmeden önce senden netleştirmem gereken üç şey var:

1. **Katman C (Core/) gerçekten senin son kararın mı**, yoksa denenip bırakılmış bir deney mi? Cevap tüm refactor planını değiştirir.
2. **Oyun şu an çalışır durumda mı?** (Play'e basınca settlement → job → time → reward döngüsü tamamlanıyor mu?) Çalışan bir baseline varsa refactor'ü test altında yapabiliriz.
3. **UI Toolkit'e geçiş düşünüyor musun?** Management-heavy, liste/tablo ağırlıklı UI için uGUI'dan objektif olarak daha uygun — ama mevcut uGUI + DOTween görsellerini taşımak ciddi iş. Şu aşamada **hayır** derim; önce mimariyi düzelt.
