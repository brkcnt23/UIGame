# Eksik Item Görselleri — ChatGPT Promptları

Aşağıdaki promptları ChatGPT'ye yapıştır. Her prompt bir kategoriyi kapsar.

---

## Prompt A — Silah + Başlık

```
Generate 2 separate images. One image per numbered entry — do not combine
them into a sheet or grid.

Medieval fantasy game UI. Painted illustration style, warm earth tones, soft
rim light. Each item: single object, centered, three-quarter view. No text,
no border, no frame. TRANSPARENT BACKGROUND — nothing behind the object.

1. A shortbow, wooden limbs strung with cord, simple elegant grip,
   seen from the side.
2. An iron helmet, raised visor, riveted construction, burnished steel,
   seen from a slight angle.
```

---

## Prompt B — Gövde (Zırh)

```
Generate 2 separate images. One per numbered entry, not a sheet.

Medieval fantasy game UI. Painted style, fabric and leather in warm tones.
Each item: centered, three-quarter view. No text. TRANSPARENT BACKGROUND.

1. A peasant tunic, unbleached linen, simple seams, worn at the collar,
   hung on an invisible form to show drape.
2. A leather cuirass, fitted chest piece, supple brown leather with stitched
   edges, reinforced at the shoulders.
```

---

## Prompt C — Beden (Bacak + Ayakkabı)

```
Generate 2 separate images. One per numbered entry, not a sheet.

Medieval fantasy game UI. Painted style, leather and wool. Each item:
centered, three-quarter view. No text. TRANSPARENT BACKGROUND.

1. Leather leggings, fitted to the thigh, tooled seams, rich brown leather,
   seen as if laid out.
2. Leather boots, calf-height, supple leather, simple laces, sole worn
   from travel.
```

---

## Prompt D — Craft + Food + Special

```
Generate 3 separate images. One per numbered entry, not a sheet.

Medieval fantasy game UI. Painted style. Each item: centered, three-quarter
view. No text. TRANSPARENT BACKGROUND.

1. A bunch of common herbs, tied with twine, sage and mint, fresh green,
   a few leaves already dried.
2. A travel ration wrapped in cloth, a bundled package of bread and dried
   meat, compact and tied.
3. Flint and steel striking together, sparks flying, seen mid-strike against
   a dark background (but background itself is transparent).
```

---

## Talimatlar

1. **ChatGPT'ye yapıştır:** Her prompt'u (A, B, C, D) ayrı ayrı yapıştır, 4 çalışma.
2. **Dosya adlandırması:** ChatGPT'den indirdiğinde şu şekilde adlandır:
   - `Shortbow.png`
   - `IronHelm.png`
   - `PeasantTunic.png`
   - `LeatherCuirass.png`
   - `LeatherLeggings.png`
   - `LeatherBoots.png`
   - `CommonHerbs.png`
   - `TravelRation.png`
   - `FlintAndSteel.png`
3. **Kalite derecesi:** Bu itemlerin kalite derecesi yoktur (silah/zırh değil). Sadece tek dosya her item için.
4. **Klasör:** `Assets/UI Elements/` içine yapıştır.

---

## İpuçları

- **Shortbow:** Longbow ve Hunting Bow'ı referans al (mevcut), aynı stil.
- **Iron Helm:** Bascinet, Nasal Helm, Great Helm var — aynı metal işçiliği.
- **Peasant Tunic:** Gambeson ve diğer kıyafetler referans — basit, köylü tarzı.
- **Leather Cuirass:** Leather Cap ve Leather Boots ile stil uyumlu olsun.
- **Travel Ration:** Bread, Dried Meat gibi yiyeceklere bak, benzer stil.
- **Flint & Steel:** `FlintNSteel.png` dosyası var mı bak — varsa yeni üretmeye gerek yok.

---

## Sonrasında

1. Dosyaları `Assets/UI Elements/` içine koy.
2. ItemCatalog.cs'i açıp `Sling` satırını sil (160 → 159).
3. Unity bağlantısı kontrol et, görseller otomatik yüklenecek.
