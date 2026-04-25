# GAME LOOP — CORE FLOW

## Genel Oyun Mantığı

Oyun, tamamen UI üzerinden ilerleyen medieval survival / management RPG yapısına sahiptir.

Oyuncu bir karakter oluşturur, kendi doğduğu köyden başlar ve zamanla:
- hayatta kalmaya,
- para kazanmaya,
- mesleklerde gelişmeye,
- item toplamaya / üretmeye,
- farklı yerleşimlere seyahat etmeye,
- event kararlarıyla karakterini şekillendirmeye,
- ileride companion, ordu ve şehir yönetimi açmaya çalışır.

Ana döngü şudur:

Settlement → Action → Time Passes → Resource Check → Event / Reward → Progression → New Decision

---

## 1. Başlangıç — Initialization

Oyuncu yeni oyuna başladığında temel kimliği oluşturulur.

Başlangıçta belirlenenler:
- Oyuncu adı
- Doğduğu köyün adı
- Temel statlar:
  - Strength
  - Dexterity
  - Constitution
  - Charisma
- Başlangıç parası
- Başlangıç rasyonu
- Başlangıç canı
- Başlangıç yorgunluk limiti
- Başlangıç inventory’si
- Home settlement

Oyuncunun doğduğu köy, oyunun merkezidir.

Bu köy başlangıçta küçük, zayıf ve sınırlı imkanlara sahiptir. Oyuncu ilerledikçe bu köyü geliştirebilir, yeni yapılar açabilir, ekonomisini büyütebilir ve ileride onu şehir seviyesine taşıyabilir.

---

## 2. Settlement Phase — Karar Merkezi

Oyuncu bir yerleşimdeyken ana karar fazına girer.

Bu fazda oyuncu şunları yapabilir:

- Shop’a girip alış/satış yapabilir
- Crafting ile item üretebilir
- TownHall’da job alabilir
- Tavern’da quest alabilir
- Rasyon / item / ekipman hazırlığı yapabilir
- Başka settlement’a travel başlatabilir
- İleride companion yönetebilir
- İleride ordu yönetebilir
- İleride kendi köyünü geliştirebilir

Bu faz oyunun “nefes alma ve karar verme” alanıdır.

Oyuncu burada şu soruları düşünür:

- Param yeterli mi?
- Rasyonum var mı?
- Canım düşük mü?
- Yorgunluğum tehlikeli mi?
- Inventory dolu mu?
- Hangi itemleri satmalıyım?
- Hangi craft bana fayda sağlar?
- Travel riskine girmeli miyim?
- Job yapıp güvenli para mı kazanmalıyım?
- Quest alıp daha yüksek risk / ödül mü kovalamalıyım?

---

## 3. Time System — Zamanın Bedeli

Oyundaki her önemli aksiyon zaman harcar.

Zaman harcayan aksiyonlar:
- Job yapmak
- Crafting yapmak
- Travel etmek
- Quest tamamlamak
- Uyku / dinlenme
- İleride settlement upgrade
- İleride army training
- İleride companion görevlendirme

Zaman ilerledikçe sistemler tetiklenir:

- Rasyon tüketimi
- Exhaustion kontrolü
- Event tetikleme ihtimali
- Quest süre kontrolü
- Travel ilerlemesi
- Üretim / upgrade süreci
- Oyuncunun hayatta kalma baskısı

Bu yüzden zaman sadece sayaç değildir. Oyunun ana maliyetlerinden biridir.

---

## 4. Money Management — Para Yönetimi

Oyunda ekonomi Gold / Silver sistemiyle çalışır.

Temel oran:

- 100 Silver = 1 Gold

Oyuncunun para kaynakları:
- Job ödülleri
- Quest ödülleri
- Event sonuçları
- Item satışı
- Craft edilen ürünlerin satışı
- İleride şehir gelirleri
- İleride ticaret / shop sistemi
- İleride orduyla kazanılan ganimetler

Oyuncunun para harcadığı alanlar:
- Item satın alma
- Rasyon satın alma
- Crafting materyali alma
- Potion / healing item alma
- Travel hazırlığı
- İleride companion ücreti
- İleride asker toplama
- İleride ordu bakımı
- İleride köy / şehir geliştirme
- İleride bina upgrade masrafları

Para oyunda sadece “ödül” değildir. Aynı zamanda hayatta kalma ve büyüme aracıdır.

Oyuncu parasını üç ana şey arasında bölmek zorundadır:

1. Günlük hayatta kalma
2. Kişisel güçlenme
3. Uzun vadeli yatırım

Yanlış para yönetimi oyuncuyu doğrudan çıkmaza sokabilir.

Örnek:
- Tüm parasını ekipmana harcarsa rasyon alamaz.
- Rasyon almazsa exhaustion artar.
- Exhaustion artarsa travel ve event riskleri büyür.
- Can düşerse ölüm riski oluşur.

---

## 5. Health Management — Can Yönetimi

Health oyuncunun fiziksel hayatta kalma değeridir.

Can azalabilecek durumlar:
- Eventlerde başarısız seçimler
- Combat / fight eventleri
- Açlık
- Aşırı exhaustion
- Travel sırasında kötü olaylar
- Quest başarısızlıkları
- İleride hastalık / zehir / yaralanma
- İleride savaş sonuçları

Can geri kazanımı için olası yöntemler:
- Potion kullanmak
- Dinlenmek
- Yemek yemek
- Tavern’da konaklamak
- Home settlement’ta iyileşmek
- İleride healer / doctor NPC
- İleride companion yetenekleri
- İleride şehir binası bonusları

Health sistemi doğrudan exhaustion ve rasyon sistemiyle bağlıdır.

Oyuncu sadece cana bakarak güvende olduğunu düşünmemelidir. Çünkü yüksek exhaustion düşük can kadar tehlikelidir.

---

## 6. Exhaustion Management — Yorgunluk Yönetimi

Exhaustion oyunun en önemli survival baskılarından biridir.

Exhaustion artabilecek durumlar:
- Uzun süre uyumamak
- Rasyonsuz kalmak
- Travel sırasında dinlenmemek
- Ağır işlerde çalışmak
- Zorlu crafting / job yapmak
- Eventlerde fiziksel bedel ödemek
- İleride ağır zırh / fazla yük taşımak

Exhaustion sonuçları:
- Oyuncunun performansı düşebilir
- Event başarı ihtimali azalabilir
- Travel daha riskli hale gelebilir
- Can kaybı başlayabilir
- Maksimum seviyeyi aşarsa ölüm gerçekleşebilir

Exhaustion azaltma yolları:
- Uyku
- Dinlenme
- Potion
- Yemek
- Tavern konaklaması
- Home settlement recovery
- İleride companion desteği
- İleride şehir geliştirmelerinden gelen bonuslar

Exhaustion, oyuncuyu sürekli kısa vadeli ve uzun vadeli karar arasında bırakır.

Örnek:
- Hemen travel edebilirsin ama yorgunsan riskli.
- Dinlenirsen zaman geçer ama güvenli olursun.
- Zaman geçerse rasyon tüketilir.
- Rasyon yoksa tekrar exhaustion artar.

---

## 7. Ration / Food Management — Besin Yönetimi

Rasyon oyuncunun temel hayatta kalma kaynağıdır.

Rasyon tüketenler:
- Oyuncu
- İleride companionlar
- İleride ordu birimleri

Başlangıçta sadece oyuncu rasyon tüketir. Companion ve ordu açıldıkça tüketim artar.

Rasyon eksikliği şunlara yol açar:
- Exhaustion artışı
- Can kaybı
- Ordu varsa asker kaybı
- Companion morale düşüşü
- Travel riskinin artması

Rasyon kaynakları:
- Shop’tan satın alma
- Event ödülleri
- Quest ödülleri
- Hunting / gathering
- İleride köy üretimi
- İleride farm binası
- İleride ticaret yolları

Rasyon sistemi, oyuncuyu “hazırlıksız yola çıkma” konusunda cezalandırır.

---

## 8. Inventory Management — Eşya Yönetimi

Inventory oyuncunun taşıdığı tüm itemleri yönetir.

Inventory’de bulunabilecek item türleri:
- Weapon
- Armor
- Boots
- Leggings
- Potion
- Crafting Material
- Resource
- Misc

Inventory sistemi şu kararları doğurur:

- Hangi item tutulmalı?
- Hangi item satılmalı?
- Hangi item craft için saklanmalı?
- Hangi potion travel öncesi gerekli?
- Hangi ekipman karakter build’ine daha uygun?
- Ağırlık sınırı ileride eklendiğinde ne taşınmalı?

Item sistemi ItemSO tabanlıdır.

Bu şu anlama gelir:
- Item tanımı asset olarak tutulur.
- Runtime’da oyuncuya item instance verilir.
- Save/load için item id + quantity tutulur.

Inventory, shop ve crafting sistemleri aynı item database üzerinden çalışmalıdır.

Ana item loop:

Item kazan → Inventory’ye gir → Kullan / Sat / Craft’ta harca → Güçlen / Para kazan

---

## 9. Crafting Loop — Üretim Döngüsü

Crafting iki farklı yapıya ayrılır:

### 1. Recipe Crafting
Oyuncu belirli recipe ile item üretir.

Gerekenler:
- Malzeme
- Recipe
- Skill seviyesi
- Gerekli station
- Gerekli tool
- Bazı durumlarda tag / trait / settlement şartı

Sonuç:
- Item üretilir
- Skill XP kazanılır
- Zaman geçer
- Malzemeler harcanır

### 2. Craft Work
Oyuncu bir meslek dalında çalışır.

Örnek:
- Blacksmith olarak çalışmak
- Tanner olarak çalışmak
- Carpenter olarak çalışmak
- Mason olarak çalışmak
- Alchemist olarak çalışmak

Sonuç:
- Para kazanılır
- Skill XP kazanılır
- Stat XP kazanılabilir
- Bazen item kazanılabilir
- Zaman geçer
- Exhaustion artabilir

Crafting sistemi oyuncunun ekonomik bağımsızlık kazanmasını sağlar.

---

## 10. Job Loop — Güvenli Kazanç

Job sistemi oyuncuya düşük riskli ama zaman maliyetli gelir sağlar.

Job özellikleri:
- Belirli süre ister
- Para verir
- Stat XP verebilir
- Skill gelişimine katkı sağlayabilir
- Genelde quest ve travel’dan daha güvenlidir

Job loop:

Job seç → Süre geçer → Para/XP kazan → Resource check yapılır → Settlement phase’e dön

Job sistemi özellikle oyunun erken aşamasında oyuncunun hayatta kalmasını sağlar.

---

## 11. Quest Loop — Riskli Ödül

Quest sistemi job sisteminden daha riskli ve daha ödüllüdür.

Quest türleri:
- Item teslim questleri
- Location questleri
- Event zincirli questler
- İleride companion questleri
- İleride faction / settlement questleri

Quest gereksinimleri:
- Belirli item
- Belirli settlement
- Belirli süre
- Belirli stat
- Belirli alignment
- İleride trait / companion / army şartı

Quest loop:

Quest al → Hazırlık yap → Travel / action → Şartları tamamla → Ödül al veya başarısız ol

Questler oyuncunun hikaye ve dünya ile etkileşim kurduğu ana sistemlerden biridir.

---

## 12. Travel Loop — Dünya ile Etkileşim

Travel, settlementlar arası geçiş sistemidir.

Travel sırasında:
- Zaman geçer
- Event oluşabilir
- Rasyon tüketilir
- Exhaustion artabilir
- Oyuncu risk alır

Travel öncesi oyuncu şunları düşünmelidir:

- Yeterli rasyon var mı?
- Canım yeterli mi?
- Yorgunluğum düşük mü?
- Gideceğim settlement açık mı?
- Quest için gitmeye değer mi?
- Dönüş yolunu kaldırabilir miyim?

Travel loop:

Hedef seç → Travel başlat → Zaman ilerler → Event tetiklenir → Seçim yapılır → Hedefe ulaşılır

Travel sistemi, oyunun risk ve keşif katmanıdır.

---

## 13. Event Loop — Karar ve Sonuç

Eventler oyunun hikaye, risk ve karakter şekillendirme sistemidir.

Eventlerde oyuncu seçim yapar.

Seçim tipleri:
- Good
- Evil
- Neutral
- Success
- Fail
- Decline
- FollowUp

Event sonuçları:
- Para kazanma / kaybetme
- Can kaybı
- Stat XP
- Alignment değişimi
- Item ödülü
- Quest ilerlemesi
- Follow-up event açılması
- İleride trait değişimi
- İleride companion ilişkisi değişimi

Event sistemi oyuncunun karakterini davranışlarıyla şekillendirir.

---

## 14. Progression Loop — Gelişim

Oyuncu farklı kaynaklardan gelişir.

Gelişim alanları:
- Character Level
- Stat Level
- Skill Level
- Item kalitesi
- Settlement unlock
- Craft recipe unlock
- Companion unlock
- Army unlock
- Home settlement upgrade

Progression kaynakları:
- Job
- Crafting
- Quest
- Event
- Travel
- Shop economy
- İleride battle
- İleride settlement yönetimi

Oyuncunun gelişimi sadece XP barından ibaret değildir. Sistem çok katmanlıdır.

---

## 15. Companion Management — İleride Açılacak Sistem

Companionlar başlangıçta açık değildir.

Oyuncu ilerledikçe companion sistemi unlock olur.

Companionlar:
- Oyuncuya pasif bonus verebilir
- Event seçenekleri açabilir
- Questlerde yardımcı olabilir
- Crafting bonusu sağlayabilir
- Travel riskini azaltabilir
- Ordu yönetiminde görev alabilir
- Şehir yönetiminde role sahip olabilir

Companionların maliyetleri olabilir:
- Rasyon tüketimi
- Para beklentisi
- Morale sistemi
- Loyalty sistemi
- Alignment uyumu

Companion loop:

Companion bul → İlişki kur → Yanına al → Görev ver → Bonus kazan → Risk/maliyet yönet

---

## 16. Army Management — İleride Açılacak Sistem

Oyuncu başlangıçta ordu kuramaz.

Army sistemi ilerleme ile unlock olur.

Ordu yönetimi şunları içerir:
- Asker toplama
- Birim türleri
- Rasyon tüketimi
- Morale
- Eğitim
- Bakım maliyeti
- Savaş gücü
- Travel güvenliği
- Eventlerde güç seçeneği

Army sistemi güçlü ama maliyetli olmalıdır.

Orduya sahip olmak oyuncuya güç verir, fakat hayatta kalma baskısını da büyütür.

Ordu loop:

Asker topla → Besle → Eğit → Travel / battle / eventlerde kullan → Kayıpları yönet → Gücü artır

---

## 17. Home Settlement Management — Doğduğu Köyü Geliştirme

Oyuncunun doğduğu köy oyunun uzun vadeli ana hedeflerinden biridir.

Başlangıçta küçük bir köydür.

Oyuncu yatırım yaptıkça:
- Köy gelişir
- Yeni binalar açılır
- Shop sayısı artar
- Tavern gelişir
- TownHall gelişir
- Üretim artar
- Rasyon üretimi açılır
- Companion / army sistemleri güçlenir
- Gelir oluşur
- Köy zamanla kasaba / şehir olabilir

Geliştirilebilecek alanlar:
- Tavern
- TownHall
- Shops
- Walls
- Farm / food production
- Crafting stations
- Barracks
- Market
- Storage
- Housing

Settlement upgrade maliyetleri:
- Para
- Resource
- Zaman
- Crafting materyali
- Belirli level
- Belirli quest tamamlanması

Home settlement loop:

Para kazan → Resource topla → Köye yatırım yap → Yeni sistem aç → Daha fazla gelir/avantaj kazan → Daha büyük hedeflere ilerle

Bu sistem oyunun uzun vadeli meta progression omurgasıdır.

---

## 18. City / Settlement Economy — İleride Genişleyecek Sistem

İleride oyuncunun köyü büyüdükçe settlement ekonomisi oluşur.

Ekonomi kaynakları:
- Shop gelirleri
- Üretim binaları
- Rasyon üretimi
- Crafting üretimi
- Trade
- Vergi
- Quest / reputation etkileri

Settlement geliştikçe oyuncu sadece bireysel karakter olmaktan çıkar, bir yerleşimin lideri haline gelir.

Bu noktadan sonra oyun kişisel survival’dan yönetim oyununa doğru genişler.

---

## 19. Ana Feedback Loop

Oyunun temel akışı:

```text
Settlement Phase
    ↓
Player chooses action
    ↓
Time passes
    ↓
Money / Health / Ration / Exhaustion check
    ↓
Reward or penalty
    ↓
Inventory update
    ↓
Progression update
    ↓
New systems unlock
    ↓
Return to Settlement Phase