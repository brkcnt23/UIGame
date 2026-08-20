# Item Envanteri Planı

**Tarih:** 2026-08-10  
**Durum:** 150/160 görsel mevcut | 9 eksik | 1 kaldırılacak

---

## Özet

| Kategori | Mevcut | Toplam | % | Durum |
|----------|--------|--------|---|-------|
| **Silah** | 22 | 24 | 91% | 1 eksik |
| **Kalkan** | 5 | 5 | 100% | ✓ Tamam |
| **Başlık** | 6 | 7 | 85% | 1 eksik |
| **Gövde** | 7 | 9 | 77% | 2 eksik |
| **Bacak** | 4 | 5 | 80% | 1 eksik |
| **Ayakkabı** | 4 | 5 | 80% | 1 eksik |
| **Aksesuar** | 16 | 16 | 100% | ✓ Tamam |
| **İlaç** | 9 | 9 | 100% | ✓ Tamam |
| **Malzeme** | 38 | 38 | 100% | ✓ Tamam |
| **Craft** | 15 | 16 | 93% | 1 eksik |
| **Yiyecek** | 6 | 7 | 85% | 1 eksik |
| **Quest/Special** | 18 | 19 | 94% | 1 eksik |
| **TOPLAM** | **150** | **160** | **93%** | 9 eksik + 1 siling |

---

## 1. Yapılacaklar (Öncelik Sırası)

### A. Sling kaldırılması
- **Aşama:** ItemCatalog.cs'ten satırı sil
- **Etki:** 160 → 159 item
- **Not:** Dosya (`Sling.png`) var, ama item kaldırılıyor

### B. Eksik 9 görsel üretime
Kategoriler ve sayılar:

```
Silah (1):
  ✗ Shortbow

Başlık (1):
  ✗ Iron Helm

Gövde (2):
  ✗ Peasant Tunic
  ✗ Leather Cuirass

Bacak (1):
  ✗ Leather Leggings

Ayakkabı (1):
  ✗ Leather Boots

Craft (1):
  ✗ Common Herbs

Yiyecek (1):
  ✗ Travel Ration

Quest/Special (1):
  ✗ Flint & Steel
```

**Üretime başlamadan önce:**
- Flint & Steel: `FlintNSteel.png` var, ama katalogda bağlantı sorunu var mı bak
- Diğer 8 görsel: stil konsistansı için mevcut dosyalara bak (kalite dereceli olanlar vs.)

---

## 2. Mevcut Durumu

### Tamam (3 kategori / 100%)
- ✓ **Kalkan** 5/5
- ✓ **Aksesuar** 16/16
- ✓ **İlaç** 9/9
- ✓ **Malzeme** 38/38

### Neredeyse Tamam (>80%)
- **Silah** 22/24 (91%) — Shortbow eksik
- **Quest/Special** 18/19 (94%) — Flint & Steel eksik
- **Craft** 15/16 (93%) — Common Herbs eksik
- **Başlık** 6/7 (85%) — Iron Helm eksik
- **Yiyecek** 6/7 (85%) — Travel Ration eksik

### Dikkat Gereken (70-80%)
- **Gövde** 7/9 (77%) — Peasant Tunic, Leather Cuirass eksik
- **Bacak** 4/5 (80%) — Leather Leggings eksik
- **Ayakkabı** 4/5 (80%) — Leather Boots eksik

---

## 3. Dosya Adlandırması Kuralları

Mevcut pratiğe uygun:
- **Kaliteli itemler** (silah/zırh): `ItemName1.png`, `ItemName2.png`, `ItemName3.png`, `ItemName4.png`
  - Örnek: `ArmingSword1.png`, `ArmingSword2.png`, `ArmingSword3.png`, `ArmingSword4.png`
- **Kalitesi olmayan** (aksesuar, yiyecek, vb.): `ItemName.png`
  - Örnek: `Ale.png`, `Bread.png`

**Katalog bağlama:**
- `ItemName` → **taban ad** ile eşleşir
- Sondaki 1-4 kalite derecesi, normalize sırasında otomatik atlanır

---

## 4. Tespit Edilen Sorunlar

| Problem | Çözüm | Durum |
|---------|-------|-------|
| `Tanned Leather` → `Leather` (hatalı) | Katalog güncellendi: `TannedLeather` | ✓ Yapıldı |
| `Flint & Steel` → `FlintAndSteel` (hatalı) | Katalog güncellendi: `FlintNSteel` | ✓ Yapıldı |
| `Iron Ingot` → `Iron_ıngot` (Türkçe ı) | Katalog güncellendi: `IRON INGOT` | ✓ Yapıldı |

---

## 5. İstatistik

**Başarı Oranı:** 150/160 = **93.75%**

**Eksik Görseller Dağılımı:**
- Zırh/Başlık/Bacak: 4 item (44%)
- Silah: 1 item (11%)
- Craft/Yiyecek/Special: 3 item (33%)
- Ayakkabı: 1 item (12%)

**Sonraki Adım:** 9 görsel üretime başla, Sling kaldırılması yap.
