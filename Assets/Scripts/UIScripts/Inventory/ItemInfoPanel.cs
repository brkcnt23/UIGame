using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The details panel for one item.
///
/// Four painted backgrounds, one per kind of item, and the panel shows the one
/// that fits:
///
///   Desc      words only        misc, trade goods, quest items, raw materials
///   Wearable  gear              weapons, armour, shields
///   Bonus     effects           trinkets, potions, provisions
///   Detailed  full breakdown    opened from the others, no action buttons
///
/// Each variant carries its own labels because each background puts its boxes
/// in a different place. Trying to reposition one shared set of labels for
/// four different paintings is how UI code turns into a pile of magic numbers.
///
/// The description standard holds throughout: the world speaks first, the
/// numbers speak second, and never on the same line.
/// </summary>
public class ItemInfoPanel : MonoBehaviour
{
    public enum Layout { Desc, Wearable, Bonus, Detailed }

    /// <summary>One painted background and the labels drawn on it.</summary>
    [System.Serializable]
    public class Variant
    {
        public Layout layout;
        public GameObject root;

        [Header("Header")]
        public Image icon;
        public TMP_Text nameLabel;
        public TMP_Text qualityLabel;
        public TMP_Text categoryLabel;

        [Header("Body")]
        public TMP_Text flavorLabel;

        [Tooltip("Optional. When set, stat rows are spawned into it. Leave " +
                 "empty on variants that use the fixed slots below.")]
        public RectTransform statParent;

        [Header("Fixed stat slots")]
        [Tooltip("Boxes painted into the background. Auto-found by child name " +
                 "when left empty: str, dex, const, cha, gizlistat1, " +
                 "gizlistat2, BonusInfo, BonusInfo2.")]
        public TMP_Text strLabel;
        public TMP_Text dexLabel;
        public TMP_Text constLabel;
        public TMP_Text chaLabel;

        [Tooltip("Secondary numbers: damage, armour, and the like.")]
        public TMP_Text extraSlot1;
        public TMP_Text extraSlot2;

        [Tooltip("Percentage effects, written as sentences.")]
        public TMP_Text bonusSlot1;
        public TMP_Text bonusSlot2;

        [Header("Actions")]
        public Button equipButton;
        public Button useButton;
        public Button dropButton;
        public Button detailButton;
        public Button closeButton;
        public TMP_Text equipLabel;
    }

    [SerializeField] private Variant[] variants;
    [SerializeField] private ItemStatRow statRowPrefab;

    private readonly List<ItemStatRow> _rows = new();
    private Item _item;
    private ItemSO _template;
    private Variant _current;

    private void Awake()
    {
        foreach (var v in variants)
        {
            if (v == null) continue;

            AutoBindSlots(v);

            Wire(v.equipButton, OnEquipPressed);
            Wire(v.useButton, OnUsePressed);
            Wire(v.dropButton, OnDropPressed);
            Wire(v.closeButton, Hide);
            Wire(v.detailButton, () => ShowAs(_item, Layout.Detailed));

            if (v.root != null) v.root.SetActive(false);
        }

        gameObject.SetActive(false);
    }

    /// <summary>
    /// Finds the painted stat boxes by the names they were given in the scene.
    ///
    /// The slots are hand-placed to sit inside boxes the artist drew, so they
    /// cannot be generated — but wiring eight references per variant by hand
    /// is four times the work and one typo away from a silent blank. Matching
    /// on name keeps both the artwork and the Inspector clean.
    /// </summary>
    private static void AutoBindSlots(Variant v)
    {
        if (v.root == null) return;

        v.strLabel   ??= FindLabel(v.root.transform, "str");
        v.dexLabel   ??= FindLabel(v.root.transform, "dex");
        v.constLabel ??= FindLabel(v.root.transform, "const");
        v.chaLabel   ??= FindLabel(v.root.transform, "cha");

        v.extraSlot1 ??= FindLabel(v.root.transform, "gizlistat1");
        v.extraSlot2 ??= FindLabel(v.root.transform, "gizlistat2");

        v.bonusSlot1 ??= FindLabel(v.root.transform, "bonusinfo");
        v.bonusSlot2 ??= FindLabel(v.root.transform, "bonusinfo2");
    }

    private static TMP_Text FindLabel(Transform root, string name)
    {
        foreach (var text in root.GetComponentsInChildren<TMP_Text>(true))
        {
            if (string.Equals(text.name, name, System.StringComparison.OrdinalIgnoreCase))
                return text;
        }

        return null;
    }

    private static void Wire(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null) return;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
    }

    // -----------------------------------------------------------------

    public void Show(Item item) => ShowAs(item, LayoutFor(item));

    public void ShowAs(Item item, Layout layout)
    {
        if (item == null) return;

        _item = item;
        _template = LookupTemplate(item.ID);
        _current = Find(layout) ?? Find(Layout.Desc);

        if (_current == null)
        {
            Debug.LogWarning("[ItemInfo] No variant configured.");
            return;
        }

        gameObject.SetActive(true);

        foreach (var v in variants)
            if (v?.root != null) v.root.SetActive(v == _current);

        DrawHeader();
        DrawFlavor();
        DrawStats();
        DrawActions();
    }

    public void Hide()
    {
        _item = null;
        _template = null;
        ClearRows();
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Which background suits this item. Driven by what the player needs to
    /// read: gear is chosen on its numbers, a trinket on its effect, a trade
    /// good on nothing but its description.
    /// </summary>
    public static Layout LayoutFor(Item item)
    {
        if (item == null) return Layout.Desc;

        switch (item.Category)
        {
            case ItemCategory.Weapon:
            case ItemCategory.Armor:
            case ItemCategory.Shield:
            case ItemCategory.Helmet:
            case ItemCategory.Boots:
            case ItemCategory.Leggings:
            case ItemCategory.Gloves:
                return Layout.Wearable;

            case ItemCategory.Trinket:
            case ItemCategory.Potion:
            case ItemCategory.Consumable:
                return Layout.Bonus;

            default:
                return Layout.Desc;
        }
    }

    private Variant Find(Layout layout)
    {
        foreach (var v in variants)
            if (v != null && v.layout == layout && v.root != null)
                return v;

        return null;
    }

    // -----------------------------------------------------------------

    private void DrawHeader()
    {
        var quality = (ItemQuality)Mathf.Clamp(_item.Quality, 0, 4);

        if (_current.icon != null)
        {
            _current.icon.sprite = _item.ItemImage;
            _current.icon.preserveAspect = true;
            _current.icon.enabled = _item.ItemImage != null;
        }

        if (_current.nameLabel != null) _current.nameLabel.text = _item.Name;

        if (_current.qualityLabel != null)
        {
            // Common is unremarkable and says so by staying quiet.
            bool worthSaying = quality != ItemQuality.Common;
            _current.qualityLabel.enabled = worthSaying;

            if (worthSaying)
            {
                _current.qualityLabel.text = ItemRules.Name(quality);
                _current.qualityLabel.color = ItemRules.QualityColor(quality);
            }
        }

        if (_current.categoryLabel != null)
            _current.categoryLabel.text = CategoryLabel(_item.Category);
    }

    private void DrawFlavor()
    {
        if (_current.flavorLabel == null) return;

        string text = _template != null ? _template.flavorText : "";
        _current.flavorLabel.text = text;
    }

    private void DrawStats()
    {
        FillFixedSlots();

        // Variants built around painted boxes have no row container; the
        // slots above are all they need.
        ClearRows();
        if (_current.statParent == null || statRowPrefab == null) return;

        switch (_item.Category)
        {
            case ItemCategory.Weapon: DrawWeaponStats(); break;

            case ItemCategory.Armor:
            case ItemCategory.Helmet:
            case ItemCategory.Shield:
            case ItemCategory.Boots:
            case ItemCategory.Leggings:
            case ItemCategory.Gloves: DrawArmorStats(); break;

            case ItemCategory.Potion:
            case ItemCategory.Consumable: DrawConsumableStats(); break;

            case ItemCategory.Trinket: DrawTrinketStats(); break;

            case ItemCategory.Resource:
            case ItemCategory.CraftingMaterial: DrawMaterialStats(); break;

            case ItemCategory.TradeGood: AddRow("Trade", "Worth more where it is scarce", Muted); break;
            case ItemCategory.QuestItem: AddRow("Bound", "Cannot be sold or dropped", Warn); break;
        }

        DrawModifierRows();

        AddRow("Weight", _item.TotalWeight.ToString("0.#"), Muted);

        var value = _item.GetSingleValue();
        AddRow("Value", value.Gold > 0 ? $"{value.Gold}g {value.Silver}s" : $"{value.Silver}s", Muted);
    }

    // -----------------------------------------------------------------
    // Fixed painted slots
    // -----------------------------------------------------------------

    /// <summary>
    /// Writes into the boxes painted on the background.
    ///
    /// An empty box is blanked rather than filled with a zero. "STR +0" reads
    /// as a stat the item affects and does not; blank reads as a stat it
    /// simply has nothing to say about.
    /// </summary>
    private void FillFixedSlots()
    {
        SetSlot(_current.strLabel,   "STR",   ModifierFor(StatType.Strength));
        SetSlot(_current.dexLabel,   "DEX",   ModifierFor(StatType.Dexterity));
        SetSlot(_current.constLabel, "CONST", ModifierFor(StatType.Constitution));
        SetSlot(_current.chaLabel,   "CHA",   ModifierFor(StatType.Charisma));

        // The two rolled properties come first — they are what makes this
        // particular piece different from every other one like it, and the
        // first thing a player wants to know about a crafted item.
        if (_item.HasHiddenEffects)
        {
            SetHidden(_current.extraSlot1, _item.HiddenEffects.Count > 0 ? _item.HiddenEffects[0] : null);
            SetHidden(_current.extraSlot2, _item.HiddenEffects.Count > 1 ? _item.HiddenEffects[1] : null);
        }
        else
        {
            var extras = GatherExtras();
            SetRaw(_current.extraSlot1, extras.Count > 0 ? extras[0] : null);
            SetRaw(_current.extraSlot2, extras.Count > 1 ? extras[1] : null);
        }

        var bonuses = GatherBonuses();
        SetRaw(_current.bonusSlot1, bonuses.Count > 0 ? bonuses[0] : null);
        SetRaw(_current.bonusSlot2, bonuses.Count > 1 ? bonuses[1] : null);
    }

    private int ModifierFor(StatType type)
    {
        if (_item?.Modifiers == null) return 0;

        int total = 0;
        foreach (var mod in _item.Modifiers)
            if (mod != null && mod.Type == type)
                total += mod.Value;

        return total;
    }

    private static void SetSlot(TMP_Text label, string prefix, int value)
    {
        if (label == null) return;

        if (value == 0)
        {
            label.text = "";
            return;
        }

        label.text = $"{prefix} {(value > 0 ? "+" : "")}{value}";
        label.color = value > 0 ? Good : Bad;
    }

    private static void SetRaw(TMP_Text label, string text)
    {
        if (label == null) return;
        label.text = text ?? "";
    }

    /// <summary>
    /// A rolled property, coloured by whether it helped. Costs read backwards
    /// — "Exhaustion gain −15%" is good news — so the colour comes from the
    /// roller rather than from the sign.
    /// </summary>
    private static void SetHidden(TMP_Text label, GameplayEffect effect)
    {
        if (label == null) return;

        if (effect == null)
        {
            label.text = "";
            return;
        }

        label.text = CraftedStatRoller.Describe(effect);
        label.color = CraftedStatRoller.ColorFor(effect);
    }

    /// <summary>
    /// The secondary numbers: what a weapon hits for, what armour stops.
    /// Two slots, so only the two that matter most for this item are shown.
    /// </summary>
    private List<string> GatherExtras()
    {
        var lines = new List<string>();
        if (_template == null) return lines;

        switch (_item.Category)
        {
            case ItemCategory.Weapon:
            {
                string scaling = _template.scaling switch
                {
                    ScalingStat.Dexterity => "DEX",
                    ScalingStat.Hybrid    => "STR/DEX",
                    _                     => "STR"
                };

                lines.Add($"{_template.DamageNotation} + {scaling}");
                lines.Add(_template.twoHanded ? "Two-handed" : _template.weaponClass.ToString());
                break;
            }

            case ItemCategory.Armor:
            case ItemCategory.Helmet:
            case ItemCategory.Shield:
            case ItemCategory.Boots:
            case ItemCategory.Leggings:
            case ItemCategory.Gloves:
                lines.Add($"Armour {_template.armorValue}");
                lines.Add(_template.armorWeight.ToString());
                break;

            case ItemCategory.Potion:
            case ItemCategory.Consumable:
                if (_item.HealthRecovery > 0) lines.Add($"+{_item.HealthRecovery} health");
                if (_item.ExhaustionReduction > 0) lines.Add($"−{_item.ExhaustionReduction} exhaustion");
                if (_template.rationValue > 0) lines.Add($"{_template.rationValue} rations");
                break;

            case ItemCategory.Resource:
            case ItemCategory.CraftingMaterial:
            {
                var uses = FindRecipesUsing(_item.ID);
                if (uses.Count > 0)
                    lines.Add($"Used in {uses.Count} recipe{(uses.Count == 1 ? "" : "s")}");
                break;
            }
        }

        return lines;
    }

    /// <summary>
    /// Percentage effects, written the way the game writes everything else —
    /// as a capability, with the number attached rather than leading.
    /// </summary>
    private List<string> GatherBonuses()
    {
        var lines = new List<string>();

        if (_template != null && _template.isMagical
            && !string.IsNullOrWhiteSpace(_template.magicalEffect))
        {
            lines.Add(_template.magicalEffect);
        }

        // Quality is itself a bonus worth stating: a Fine blade is 30% better
        // than the common one, and the player should not have to know the
        // multiplier table to find that out.
        var quality = (ItemQuality)Mathf.Clamp(_item.Quality, 0, 4);
        if (quality != ItemQuality.Common)
        {
            int percent = ItemRules.Multiplier(quality) - 100;
            string word = percent > 0 ? "better" : "worse";
            lines.Add($"{ItemRules.Name(quality)} work — {Mathf.Abs(percent)}% {word} than common");
        }

        return lines;
    }

    private void DrawWeaponStats()
    {
        if (_template == null) return;

        string scaling = _template.scaling switch
        {
            ScalingStat.Dexterity => "DEX",
            ScalingStat.Hybrid    => "STR/DEX",
            _                     => "STR"
        };

        AddRow("Damage", $"{_template.DamageNotation} + {scaling}");
        AddRow("Class", _template.weaponClass.ToString(), Muted);

        if (_template.twoHanded) AddRow("Grip", "Two-handed", Muted);
        else if (_template.CanGoInOffHand) AddRow("Grip", "Off-hand able", Muted);
    }

    private void DrawArmorStats()
    {
        if (_template == null) return;

        AddRow("Armour", _template.armorValue.ToString());

        string weight = _template.armorWeight switch
        {
            ArmorWeight.Light  => "Light — full dexterity",
            ArmorWeight.Medium => "Medium — dexterity capped",
            ArmorWeight.Heavy  => "Heavy — no dexterity",
            _                  => "—"
        };

        AddRow("Class", weight, Muted);
    }

    private void DrawConsumableStats()
    {
        if (_item.HealthRecovery > 0) AddRow("Restores", $"{_item.HealthRecovery} health", Good);
        if (_item.ExhaustionReduction > 0) AddRow("Rest", $"−{_item.ExhaustionReduction} exhaustion", Good);
        if (_template != null && _template.rationValue > 0) AddRow("Food", $"{_template.rationValue} rations", Good);
    }

    private void DrawTrinketStats()
    {
        if (_template != null && _template.isMagical)
            AddRow("Rumour", "They say it works.", Muted);
    }

    private void DrawMaterialStats()
    {
        var uses = FindRecipesUsing(_item.ID);

        if (uses.Count == 0)
        {
            AddRow("Used in", "nothing you know yet", Muted);
            return;
        }

        string preview = string.Join(", ", uses.GetRange(0, Mathf.Min(3, uses.Count)));
        if (uses.Count > 3) preview += $" +{uses.Count - 3}";

        AddRow("Used in", preview, Muted);
    }

    private void DrawModifierRows()
    {
        if (_item.Modifiers == null) return;

        foreach (var mod in _item.Modifiers)
        {
            if (mod == null || mod.Value == 0) continue;

            string sign = mod.Value > 0 ? "+" : "";
            AddRow(mod.Type.ToString(), $"{sign}{mod.Value}", mod.Value > 0 ? Good : Bad);
        }
    }

    // -----------------------------------------------------------------

    private void DrawActions()
    {
        bool equippable = _template != null && _template.IsEquippable;
        bool consumable = _item.Category == ItemCategory.Potion
                       || _item.Category == ItemCategory.Consumable;
        bool bound = _item.Category == ItemCategory.QuestItem;

        if (_current.equipButton != null)
        {
            _current.equipButton.gameObject.SetActive(equippable);

            if (_current.equipLabel != null)
                _current.equipLabel.text = _item.IsEquipped ? "Unequip" : "Equip";
        }

        if (_current.useButton != null)
            _current.useButton.gameObject.SetActive(consumable);

        // Quest items and worn gear cannot be dropped by accident.
        if (_current.dropButton != null)
            _current.dropButton.gameObject.SetActive(!bound && !_item.IsEquipped);
    }

    private void OnEquipPressed()
    {
        if (_item == null) return;

        _item.IsEquipped = !_item.IsEquipped;

        EncumbranceSystem.Instance?.Evaluate();
        PlayerStatHandler.Instance?.RefreshPlayerUI();

        ShowAs(_item, _current.layout);
    }

    private void OnUsePressed()
    {
        if (_item == null) return;

        var pd = PlayerStatHandler.Instance?.pd;
        if (pd == null) return;

        if (_item.HealthRecovery > 0)
            pd.Health = Mathf.Min(pd.MaxHealth, pd.Health + _item.HealthRecovery);

        if (_item.ExhaustionReduction > 0)
            pd.CurrentExhaustionLevel = Mathf.Max(0, pd.CurrentExhaustionLevel - _item.ExhaustionReduction);

        _item.Quantity--;
        if (_item.Quantity <= 0) pd.Items.Remove(_item);

        GameBootstrapper.Events?.Dispatch(new ItemRemovedEvent(_item.ID, 1));
        PlayerStatHandler.Instance?.RefreshPlayerUI();

        Hide();
    }

    private void OnDropPressed()
    {
        if (_item == null) return;

        var pd = PlayerStatHandler.Instance?.pd;
        if (pd?.Items == null) return;

        pd.Items.Remove(_item);

        GameBootstrapper.Events?.Dispatch(new ItemRemovedEvent(_item.ID, _item.Quantity));
        EncumbranceSystem.Instance?.Evaluate();

        Hide();
    }

    // -----------------------------------------------------------------

    private static readonly Color Muted = new Color(0.42f, 0.34f, 0.24f);
    private static readonly Color Good  = new Color(0.24f, 0.46f, 0.20f);
    private static readonly Color Bad   = new Color(0.62f, 0.24f, 0.20f);
    private static readonly Color Warn  = new Color(0.60f, 0.44f, 0.12f);

    private void AddRow(string label, string value, Color? valueColor = null)
    {
        var row = Instantiate(statRowPrefab, _current.statParent);
        row.Set(label, value, valueColor ?? new Color(0.22f, 0.17f, 0.11f));
        _rows.Add(row);
    }

    private void ClearRows()
    {
        foreach (var row in _rows)
            if (row != null) Destroy(row.gameObject);

        _rows.Clear();
    }

    private static string CategoryLabel(ItemCategory category)
    {
        switch (category)
        {
            case ItemCategory.Weapon:           return "Weapon";
            case ItemCategory.Armor:            return "Body armour";
            case ItemCategory.Helmet:           return "Helmet";
            case ItemCategory.Shield:           return "Shield";
            case ItemCategory.Boots:            return "Boots";
            case ItemCategory.Leggings:         return "Leggings";
            case ItemCategory.Gloves:           return "Gloves";
            case ItemCategory.Trinket:          return "Trinket";
            case ItemCategory.Potion:           return "Potion";
            case ItemCategory.Consumable:       return "Provisions";
            case ItemCategory.Resource:         return "Raw material";
            case ItemCategory.CraftingMaterial: return "Crafting material";
            case ItemCategory.TradeGood:        return "Trade good";
            case ItemCategory.QuestItem:        return "Quest item";
            default:                            return "Miscellaneous";
        }
    }

    private static ItemSO LookupTemplate(int itemId)
    {
        var db = GameBootstrapper.Resources != null
            ? GameBootstrapper.Resources.GetItemDatabase()
            : null;

        return db != null ? db.GetByID(itemId) : null;
    }

    private static List<string> FindRecipesUsing(int itemId)
    {
        var result = new List<string>();
        string target = null;

        foreach (var def in ItemCatalog.All)
        {
            if (def.Id != itemId) continue;
            target = def.Name;
            break;
        }

        if (target == null) return result;

        foreach (var recipe in RecipeCatalog.All)
        {
            foreach (var (ingredient, _) in recipe.Ingredients)
            {
                if (!string.Equals(ingredient, target, System.StringComparison.OrdinalIgnoreCase))
                    continue;

                result.Add(recipe.Name);
                break;
            }
        }

        return result;
    }
}
