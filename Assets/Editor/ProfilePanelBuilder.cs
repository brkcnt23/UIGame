#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
public static class ProfilePanelBuilder
{
    private const string ArtFolder  = "Assets/UI Elements/ProfilePanel";
    private const string IconFolder = "Assets/UI Elements/ProfilePanel/icons";

    private static readonly Color Header = new Color(0.42f, 0.68f, 0.90f);
    private static readonly Color Label  = new Color(0.91f, 0.86f, 0.78f);
    private static readonly Color Value  = Color.white;
    private static readonly Color Muted  = new Color(0.66f, 0.58f, 0.44f);

    private static Dictionary<string, Sprite> _art;
    private static Dictionary<string, Sprite> _icons;
    private static readonly List<string> _report = new();


    [MenuItem("Tools/UIGame/Profile Panel/Wire panel and fill tabs", false, 0)]
    public static void WireAndFill()
    {
        var root = RequireRoot();
        if (root == null) return;

        LoadArt();
        _report.Clear();

        Undo.RegisterFullObjectHierarchyUndo(root.gameObject, "Wire profile panel");

        WireAvatarHeader(root);
        WireTabs(root);
        FillTabs(root);

        EditorUtility.SetDirty(root.gameObject);

        Debug.Log(_report.Count == 0
            ? "[ProfileBuilder] Everything wired."
            : "[ProfileBuilder] Done, with notes:\n  " + string.Join("\n  ", _report));
    }

    /// <summary>
    /// Attaches the binders and leaves the tab contents alone.
    ///
    /// The other two menu items own the three MidContent containers and rebuild
    /// them from scratch, which is right when nobody has laid them out and
    /// destructive once somebody has. A panel arranged by hand needs its labels
    /// bound, not its layout replaced.
    /// </summary>
    [MenuItem("Tools/UIGame/Profile Panel/Wire panel, keep my layout", false, 2)]
    public static void WireOnly()
    {
        var root = RequireRoot();
        if (root == null) return;

        LoadArt();
        _report.Clear();

        Undo.RegisterFullObjectHierarchyUndo(root.gameObject, "Wire profile panel");

        WireAvatarHeader(root);
        WireTabs(root);

        EditorUtility.SetDirty(root.gameObject);

        Debug.Log(_report.Count == 0
            ? "[ProfileBuilder] Header and tabs wired. Tab contents untouched."
            : "[ProfileBuilder] Header and tabs wired, contents untouched. Notes:\n  "
              + string.Join("\n  ", _report));
    }

    [MenuItem("Tools/UIGame/Profile Panel/Fill tab contents only", false, 1)]
    public static void FillOnly()
    {
        var root = RequireRoot();
        if (root == null) return;

        LoadArt();
        _report.Clear();

        Undo.RegisterFullObjectHierarchyUndo(root.gameObject, "Fill profile tabs");
        FillTabs(root);

        EditorUtility.SetDirty(root.gameObject);

        Debug.Log(_report.Count == 0
            ? "[ProfileBuilder] Tab contents rebuilt."
            : "[ProfileBuilder] Done, with notes:\n  " + string.Join("\n  ", _report));
    }

    /// <summary>
    /// Fills the two tabs nobody has laid out and leaves Overview alone.
    ///
    /// Overview is arranged by hand here; Skills and Traits are empty. Filling
    /// all three would trade a finished tab for a generated one.
    /// </summary>
    [MenuItem("Tools/UIGame/Profile Panel/Fill Skills and Traits only", false, 3)]
    public static void FillSkillsAndTraits()
    {
        var root = RequireRoot();
        if (root == null) return;

        LoadArt();
        _report.Clear();

        Undo.RegisterFullObjectHierarchyUndo(root.gameObject, "Fill skills and traits");

        var skills = Find(root, "MidContentSkills");
        var traits = Find(root, "MidContentTraits");

        if (skills != null) BuildSkills(skills);
        else _report.Add("MidContentSkills not found.");

        if (traits != null) BuildTraits(traits);
        else _report.Add("MidContentTraits not found.");

        EditorUtility.SetDirty(root.gameObject);

        Debug.Log(_report.Count == 0
            ? "[ProfileBuilder] Skills and Traits built. Overview untouched."
            : "[ProfileBuilder] Done, with notes: " + string.Join(" | ", _report));
    }

    [MenuItem("Tools/UIGame/Profile Panel/Check what the tool can find", false, 20)]
    public static void Inspect()
    {
        var root = RequireRoot();
        if (root == null) return;

        string[] wanted =
        {
            "AvatarImage", "AvatarFrame", "PlayerName", "LevelTxt",
            "LevelExpBar", "ExpTxt",
            "OverviewTab", "SkillsTab", "TraitsTab",
            "MidContentOverview", "MidContentSkills", "MidContentTraits"
        };

        var lines = wanted
            .Select(n => $"{(Find(root, n) != null ? "found  " : "MISSING")}  {n}")
            .ToList();

        Debug.Log($"[ProfileBuilder] Under '{root.name}':\n  " + string.Join("\n  ", lines));
    }


    private static void WireAvatarHeader(RectTransform root)
    {
        BindLabel(root, "PlayerName", StatBinder.Field.PlayerName);
        BindLabel(root, "LevelTxt", StatBinder.Field.Level, "Level {0}");
        BindLabel(root, "TitleTxt", StatBinder.Field.TitleName, silentIfMissing: true);

        // Portrait: only assign if the slot is empty, so a chosen avatar is
        // never overwritten.
        var avatar = Find(root, "AvatarImage");
        if (avatar != null)
        {
            var image = avatar.GetComponent<Image>();
            if (image != null && image.sprite == null)
            {
                var sprite = Art("avatar1adventurerbg") ?? Art("avatar1adventurer");
                if (sprite != null)
                {
                    Undo.RecordObject(image, "Assign avatar");
                    image.sprite = sprite;
                }
            }
        }
        else
        {
            _report.Add("AvatarImage not found — portrait left alone.");
        }

        WireSlider(root, "LevelExpBar", BarBinder.Source.CharacterXp, "ExpTxt");
    }

    private static void WireSlider(RectTransform root, string sliderName,
                                   BarBinder.Source source, string labelName)
    {
        var sliderRect = Find(root, sliderName);
        if (sliderRect == null)
        {
            _report.Add($"{sliderName} not found — bar not wired.");
            return;
        }

        var slider = sliderRect.GetComponent<Slider>();
        if (slider == null)
        {
            _report.Add($"{sliderName} has no Slider component.");
            return;
        }

        Undo.RecordObject(slider, "Configure slider");
        slider.transition = Selectable.Transition.None;
        slider.interactable = false;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.direction = Slider.Direction.LeftToRight;

        ApplySliderArt(slider);

        var binder = sliderRect.GetComponent<BarBinder>() ?? Undo.AddComponent<BarBinder>(sliderRect.gameObject);

        var so = new SerializedObject(binder);
        so.FindProperty("source").enumValueIndex = (int)source;

        if (!string.IsNullOrEmpty(labelName))
        {
            var label = Find(root, labelName);
            var text = label != null ? label.GetComponent<TMP_Text>() : null;

            if (text != null) so.FindProperty("valueLabel").objectReferenceValue = text;
            else _report.Add($"{labelName} not found — bar has no readout.");
        }

        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void ApplySliderArt(Slider slider)
    {
        var background = slider.transform.Find("Background")?.GetComponent<Image>();
        if (background != null && Art("bar2") != null)
        {
            Undo.RecordObject(background, "Slider background");
            background.sprite = Art("bar2");
            background.type = Image.Type.Sliced;
        }

        if (slider.fillRect != null)
        {
            var fill = slider.fillRect.GetComponent<Image>();
            if (fill != null && Art("barfiller") != null)
            {
                Undo.RecordObject(fill, "Slider fill");
                fill.sprite = Art("barfiller");
                fill.type = Image.Type.Sliced;
            }
        }

        if (slider.handleRect != null)
        {
            var handle = slider.handleRect.GetComponent<Image>();
            if (handle != null) Undo.DestroyObjectImmediate(handle);
        }
    }

    private static void WireTabs(RectTransform root)
    {
        var overviewTab = Find(root, "OverviewTab");
        var skillsTab   = Find(root, "SkillsTab");
        var traitsTab   = Find(root, "TraitsTab");

        var overview = Find(root, "MidContentOverview");
        var skills   = Find(root, "MidContentSkills");
        var traits   = Find(root, "MidContentTraits");

        if (overviewTab == null || skillsTab == null || traitsTab == null)
        {
            _report.Add("One or more tab buttons not found — tabs not wired.");
            return;
        }

        if (overview == null || skills == null || traits == null)
        {
            _report.Add("One or more MidContent containers not found — tabs not wired.");
            return;
        }

        var host = overviewTab.parent != null ? overviewTab.parent.gameObject : root.gameObject;

        var controller = host.GetComponent<ProfileTabController>()
                      ?? Undo.AddComponent<ProfileTabController>(host);

        var so = new SerializedObject(controller);

        AssignArray(so, "tabButtons", new Object[]
        {
            EnsureButton(overviewTab), EnsureButton(skillsTab), EnsureButton(traitsTab)
        });
        AssignArray(so, "tabPages", new Object[]
        {
            overview.gameObject, skills.gameObject, traits.gameObject
        });
        AssignArray(so, "normalSprites", new Object[]
        {
            Art("overviewtab"), Art("skillstab"), Art("traitstab")
        });
        AssignArray(so, "selectedSprites", new Object[]
        {
            Art("overviewtabselected"), Art("skillstabselected"), Art("traitstabselected")
        });

        so.ApplyModifiedPropertiesWithoutUndo();

        skills.gameObject.SetActive(false);
        traits.gameObject.SetActive(false);
        overview.gameObject.SetActive(true);
    }

    private static Button EnsureButton(RectTransform tab)
    {
        var button = tab.GetComponent<Button>();
        if (button == null)
        {
            button = Undo.AddComponent<Button>(tab.gameObject);
            button.targetGraphic = tab.GetComponent<Image>();
            button.transition = Selectable.Transition.None;
        }
        return button;
    }

    // =================================================================
    // Tab contents — the only part this tool owns
    // =================================================================

    private static void FillTabs(RectTransform root)
    {
        var overview = Find(root, "MidContentOverview");
        var skills   = Find(root, "MidContentSkills");
        var traits   = Find(root, "MidContentTraits");

        if (overview != null) BuildOverview(overview);
        else _report.Add("MidContentOverview not found.");

        if (skills != null) BuildSkills(skills);
        else _report.Add("MidContentSkills not found.");

        if (traits != null) BuildTraits(traits);
        else _report.Add("MidContentTraits not found.");
    }

    private static void BuildOverview(RectTransform parent)
    {
        ClearChildren(parent);

        // Two columns that split whatever width the container already has,
        // rather than a fixed cell size that would overflow the frame.
        var grid = Ensure<GridLayoutGroup>(parent.gameObject);
        float width = parent.rect.width > 1f ? parent.rect.width : 960f;
        float cellWidth = (width - 12f) / 2f;

        Undo.RecordObject(grid, "Configure grid");
        grid.cellSize = new Vector2(cellWidth, 300f);
        grid.spacing = new Vector2(12f, 12f);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 2;
        grid.childAlignment = TextAnchor.UpperLeft;

        var fitter = Ensure<ContentSizeFitter>(parent.gameObject);
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var core = MakeFrame(parent, "CoreStatsFrame", "CORE STATS");
        AddRow(core, "strength",     "Strength",     StatBinder.Field.Strength);
        AddRow(core, "dexterity",    "Dexterity",    StatBinder.Field.Dexterity);
        AddRow(core, "constitution", "Constitution", StatBinder.Field.Constitution);
        AddRow(core, "charisma",     "Charisma",     StatBinder.Field.Charisma);
        AddSeparator(core);
        AddRow(core, "attack",     "Attack",     StatBinder.Field.Attack,     muted: true);
        AddRow(core, "armor",      "Defense",    StatBinder.Field.Defense,    muted: true);
        AddRow(core, "accuracy",   "Accuracy",   StatBinder.Field.Accuracy,   muted: true);
        AddRow(core, "movement",   "Initiative", StatBinder.Field.Initiative, muted: true);

        var condition = MakeFrame(parent, "ConditionFrame", "CONDITION");
        AddRow(condition, "health", "Health",     StatBinder.Field.HealthPair);
        AddRow(condition, null,     "Exhaustion", StatBinder.Field.ExhaustionPair);
        AddRow(condition, null,     "Rations",    StatBinder.Field.Rations);
        AddRow(condition, "weight", "Weight",     StatBinder.Field.WeightPair);
        AddSeparator(condition);
        AddSubHeader(condition, "ACTIVE");
        AddTraitStrip(condition, showEffects: false, maxRows: 3);

        var standing = MakeFrame(parent, "StandingFrame", "STANDING");
        AddRow(standing, null, "Title", StatBinder.Field.TitleName);
        AddSeparator(standing);
        AddBarRow(standing, "Standing", BarBinder.Source.Standing, new Color(0.30f, 0.60f, 0.92f));
        AddBarRow(standing, "Renown",   BarBinder.Source.Renown,   new Color(0.42f, 0.78f, 0.35f));
        AddRow(standing, "companions", "Companions", StatBinder.Field.CompanionPair, muted: true);

        var chronicle = MakeFrame(parent, "ChronicleFrame", "CHRONICLE");
        AddRow(chronicle, null,          "Home",      StatBinder.Field.HomeSettlement);
        AddRow(chronicle, null,          "Days",      StatBinder.Field.DaysSurvived);
        AddRow(chronicle, "battleswon",  "Won",       StatBinder.Field.BattlesWon);
        AddRow(chronicle, "battleslost", "Lost",      StatBinder.Field.BattlesLost);
        AddRow(chronicle, null,          "Alignment", StatBinder.Field.Alignment);
        AddRow(chronicle, "wealth",      "Money",     StatBinder.Field.MoneyPair);
    }

    private static void BuildSkills(RectTransform parent)
    {
        ClearChildren(parent);
        MakeVerticalStack(parent);

        var frame = MakeFrame(parent, "CraftingSkillsFrame", "CRAFTING SKILLS", stretch: true);

        foreach (var discipline in new[]
        {
            CraftDiscipline.Smither, CraftDiscipline.Tanner, CraftDiscipline.Carpenter,
            CraftDiscipline.Mason, CraftDiscipline.Alchemist
        })
        {
            AddSkillRow(frame, discipline);
        }

        var bonuses = MakeFrame(parent, "SkillBonusesFrame", "BONUSES", stretch: true);
        AddPassiveList(bonuses);
    }

    private static void BuildTraits(RectTransform parent)
    {
        ClearChildren(parent);
        MakeVerticalStack(parent);

        var characteristics = MakeFrame(parent, "CharacteristicsFrame", "CHARACTERISTICS", stretch: true);
        AddChipStrip(characteristics, ChipStripBinder.Content.Characteristics);

        var tags = MakeFrame(parent, "TagsFrame", "TAGS", stretch: true);
        AddChipStrip(tags, ChipStripBinder.Content.Tags);

        var active = MakeFrame(parent, "ActiveEffectsFrame", "ACTIVE EFFECTS", stretch: true);
        AddTraitStrip(active, showEffects: true, maxRows: 0);

        var passives = MakeFrame(parent, "PassiveBonusesFrame", "PASSIVE BONUSES", stretch: true);
        AddPassiveList(passives);
    }

    // =================================================================
    // Pieces
    // =================================================================

    private static void MakeVerticalStack(RectTransform parent)
    {
        var layout = Ensure<VerticalLayoutGroup>(parent.gameObject);
        Undo.RecordObject(layout, "Configure stack");
        layout.spacing = 12;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        var fitter = Ensure<ContentSizeFitter>(parent.gameObject);
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    private static RectTransform MakeFrame(RectTransform parent, string name, string header,
                                           bool stretch = false)
    {
        var frame = NewChild(parent, name);

        // No background image: the hand-made panel already has its framing,
        // and painting another one over it just muddies the artwork.
        var layout = frame.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(18, 18, 12, 14);
        layout.spacing = 3;
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        if (stretch)
        {
            var fitter = frame.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        var title = NewLabel(frame, "Header", header, 22, Header, TextAlignmentOptions.Left);
        SetHeight(title.rectTransform, 28);

        return frame;
    }

    private static void AddRow(RectTransform parent, string iconName, string label,
                               StatBinder.Field field, bool muted = false)
    {
        var row = NewChild(parent, $"Row_{field}");

        var layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 8;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;

        SetHeight(row, 30);

        var sprite = Icon(iconName);

        // The icon slot is only created when there is art for it; an invisible
        // placeholder in every row wastes width on a narrow phone panel.
        if (sprite != null)
        {
            var iconGo = NewChild(row, "Icon");
            var image = iconGo.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.raycastTarget = false;

            var iconElement = iconGo.gameObject.AddComponent<LayoutElement>();
            iconElement.preferredWidth = 24;
            iconElement.preferredHeight = 24;
            iconElement.flexibleWidth = 0;
        }

        var nameLabel = NewLabel(row, "Label", label, 19,
                                 muted ? Muted : Label, TextAlignmentOptions.Left);
        nameLabel.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;

        var valueLabel = NewLabel(row, "Value", "—", 19,
                                  muted ? Muted : Value, TextAlignmentOptions.Right);
        var valueElement = valueLabel.gameObject.AddComponent<LayoutElement>();
        valueElement.preferredWidth = 96;
        valueElement.flexibleWidth = 0;

        AddBinder(valueLabel.gameObject, field);
    }

    private static void AddBarRow(RectTransform parent, string label,
                                  BarBinder.Source source, Color fillColor)
    {
        var block = NewChild(parent, $"Bar_{label}");

        var layout = block.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 2;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        var top = NewChild(block, "Top");
        SetHeight(top, 24);

        var topLayout = top.gameObject.AddComponent<HorizontalLayoutGroup>();
        topLayout.childControlWidth = true;
        topLayout.childControlHeight = true;
        topLayout.childForceExpandWidth = false;

        var nameLabel = NewLabel(top, "Label", label, 19, Label, TextAlignmentOptions.Left);
        nameLabel.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;

        var valueLabel = NewLabel(top, "Value", "0", 19, Value, TextAlignmentOptions.Right);
        var valueElement = valueLabel.gameObject.AddComponent<LayoutElement>();
        valueElement.preferredWidth = 96;
        valueElement.flexibleWidth = 0;

        var bar = BuildSlider(block, "Bar", source, fillColor);
        SetHeight(bar, 12);

        var binder = bar.GetComponent<BarBinder>();
        if (binder != null)
        {
            var so = new SerializedObject(binder);
            so.FindProperty("valueLabel").objectReferenceValue = valueLabel;
            so.FindProperty("labelFormat").stringValue = "{0}";
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static RectTransform BuildSlider(RectTransform parent, string name,
                                             BarBinder.Source source, Color? fillTint = null)
    {
        var root = NewChild(parent, name);
        SetHeight(root, 14);

        var slider = root.gameObject.AddComponent<Slider>();
        slider.transition = Selectable.Transition.None;
        slider.interactable = false;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0.4f;

        var background = NewChild(root, "Background");
        Stretch(background, 0);
        var backgroundImage = background.gameObject.AddComponent<Image>();
        if (Art("bar2") != null)
        {
            backgroundImage.sprite = Art("bar2");
            backgroundImage.type = Image.Type.Sliced;
        }
        else backgroundImage.color = new Color(0.12f, 0.08f, 0.05f);

        var fillArea = NewChild(root, "Fill Area");
        Stretch(fillArea, 1);

        var fill = NewChild(fillArea, "Fill");
        Stretch(fill, 0);
        var fillImage = fill.gameObject.AddComponent<Image>();
        if (Art("barfiller") != null)
        {
            fillImage.sprite = Art("barfiller");
            fillImage.type = Image.Type.Sliced;
        }
        if (fillTint.HasValue) fillImage.color = fillTint.Value;

        slider.fillRect = fill;
        slider.targetGraphic = fillImage;

        var binder = root.gameObject.AddComponent<BarBinder>();
        var so = new SerializedObject(binder);
        so.FindProperty("source").enumValueIndex = (int)source;
        so.ApplyModifiedPropertiesWithoutUndo();

        return root;
    }

    private static void AddSkillRow(RectTransform parent, CraftDiscipline discipline)
    {
        var row = NewChild(parent, $"Skill_{discipline}");
        SetHeight(row, 70);

        var layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(4, 4, 6, 6);
        layout.spacing = 12;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;

        var icon = Icon(SkillRowBinder.IconName(discipline));
        if (icon != null)
        {
            var iconGo = NewChild(row, "Icon");
            var image = iconGo.gameObject.AddComponent<Image>();
            image.sprite = icon;
            image.preserveAspect = true;

            var iconElement = iconGo.gameObject.AddComponent<LayoutElement>();
            iconElement.preferredWidth = 48;
            iconElement.preferredHeight = 48;
            iconElement.flexibleWidth = 0;
        }

        var body = NewChild(row, "Body");
        var bodyLayout = body.gameObject.AddComponent<VerticalLayoutGroup>();
        bodyLayout.spacing = 2;
        bodyLayout.childControlWidth = true;
        bodyLayout.childControlHeight = true;
        bodyLayout.childForceExpandHeight = false;
        body.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;

        var nameLabel = NewLabel(body, "Name", SkillRowBinder.DisplayName(discipline),
                                 21, Label, TextAlignmentOptions.Left);
        SetHeight(nameLabel.rectTransform, 24);

        var descLabel = NewLabel(body, "Description", SkillRowBinder.Description(discipline),
                                 14, Muted, TextAlignmentOptions.Left);
        SetHeight(descLabel.rectTransform, 17);

        var bar = BuildSlider(body, "Progress", SkillSource(discipline));
        SetHeight(bar, 12);

        var right = NewChild(row, "Right");
        var rightLayout = right.gameObject.AddComponent<VerticalLayoutGroup>();
        rightLayout.spacing = 2;
        rightLayout.childAlignment = TextAnchor.UpperRight;
        rightLayout.childControlWidth = true;
        rightLayout.childControlHeight = true;
        rightLayout.childForceExpandHeight = false;

        var rightElement = right.gameObject.AddComponent<LayoutElement>();
        rightElement.preferredWidth = 120;
        rightElement.flexibleWidth = 0;

        var levelLabel = NewLabel(right, "Level", "Lv. 1", 19, Value, TextAlignmentOptions.Right);
        var xpLabel = NewLabel(right, "Xp", "0 / 100 XP", 13, Muted, TextAlignmentOptions.Right);

        var binder = row.gameObject.AddComponent<SkillRowBinder>();
        var so = new SerializedObject(binder);
        so.FindProperty("discipline").enumValueIndex = DisciplineIndex(discipline);
        so.FindProperty("nameLabel").objectReferenceValue = nameLabel;
        so.FindProperty("descriptionLabel").objectReferenceValue = descLabel;
        so.FindProperty("levelLabel").objectReferenceValue = levelLabel;
        so.FindProperty("xpLabel").objectReferenceValue = xpLabel;
        so.FindProperty("progressBar").objectReferenceValue = bar.GetComponent<Slider>();
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void AddTraitStrip(RectTransform parent, bool showEffects, int maxRows)
    {
        var strip = NewChild(parent, "TraitStrip");

        var layout = strip.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 2;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandHeight = false;

        var fitter = strip.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var binder = strip.gameObject.AddComponent<TraitStripBinder>();
        var so = new SerializedObject(binder);

        var filter = so.FindProperty("kindFilter");
        filter.arraySize = 1;
        filter.GetArrayElementAtIndex(0).enumValueIndex = (int)TraitKind.Condition;

        so.FindProperty("showEffectLine").boolValue = showEffects;
        so.FindProperty("maxRows").intValue = maxRows;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void AddChipStrip(RectTransform parent, ChipStripBinder.Content content)
    {
        var strip = NewChild(parent, "ChipStrip");

        var layout = strip.gameObject.AddComponent<GridLayoutGroup>();
        layout.cellSize = new Vector2(150, 32);
        layout.spacing = new Vector2(8, 8);
        layout.constraint = GridLayoutGroup.Constraint.Flexible;
        layout.childAlignment = TextAnchor.UpperLeft;

        var fitter = strip.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var binder = strip.gameObject.AddComponent<ChipStripBinder>();
        var so = new SerializedObject(binder);
        so.FindProperty("content").enumValueIndex = (int)content;

        var chip = Art("thickbar");
        if (chip != null) so.FindProperty("chipBackground").objectReferenceValue = chip;

        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void AddPassiveList(RectTransform parent)
    {
        var list = NewChild(parent, "PassiveList");

        var layout = list.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 4;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandHeight = false;

        var fitter = list.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        list.gameObject.AddComponent<PassiveBonusBinder>();
    }

    private static void AddSubHeader(RectTransform parent, string text)
    {
        var label = NewLabel(parent, "SubHeader", text, 14, Muted, TextAlignmentOptions.Left);
        SetHeight(label.rectTransform, 20);
    }

    private static void AddSeparator(RectTransform parent)
    {
        var line = NewChild(parent, "Separator");

        var image = line.gameObject.AddComponent<Image>();
        if (Art("line") != null)
        {
            image.sprite = Art("line");
            image.type = Image.Type.Sliced;
        }
        else image.color = new Color(0.36f, 0.25f, 0.16f);

        image.raycastTarget = false;

        var element = line.gameObject.AddComponent<LayoutElement>();
        element.minHeight = 3;
        element.preferredHeight = 3;
        element.flexibleHeight = 0;
    }

    // =================================================================
    // Helpers
    // =================================================================

    private static RectTransform RequireRoot()
    {
        var go = Selection.activeGameObject;

        if (go == null)
        {
            EditorUtility.DisplayDialog("Profile panel",
                "Select the panel root — the object that contains AvatarPanel " +
                "and MiddleSec, for example 'Test'.",
                "Right");
            return null;
        }

        var rect = go.GetComponent<RectTransform>();

        if (rect == null)
        {
            // Selecting a plain GameObject used to return null here and every
            // caller quietly gave up, so the tool looked like it had not run at
            // all - no log, no dialog, nothing.
            EditorUtility.DisplayDialog("Profile panel",
                $"'{go.name}' is not a UI object - it has a Transform, not a " +
                "RectTransform. Select the panel root under the Canvas instead.",
                "Right");
        }

        return rect;
    }

    /// <summary>Depth-first search by exact name, anywhere below the root.</summary>
    private static RectTransform Find(RectTransform root, string name)
    {
        if (root.name == name) return root;

        for (int i = 0; i < root.childCount; i++)
        {
            var child = root.GetChild(i) as RectTransform;
            if (child == null) continue;

            var found = Find(child, name);
            if (found != null) return found;
        }

        return null;
    }

    private static void BindLabel(RectTransform root, string objectName,
                                  StatBinder.Field field, string format = "",
                                  bool silentIfMissing = false)
    {
        var rect = Find(root, objectName);
        if (rect == null)
        {
            if (!silentIfMissing) _report.Add($"{objectName} not found — not bound.");
            return;
        }

        if (rect.GetComponent<TMP_Text>() == null)
        {
            _report.Add($"{objectName} has no TMP text component.");
            return;
        }

        var binder = rect.GetComponent<StatBinder>() ?? Undo.AddComponent<StatBinder>(rect.gameObject);

        var so = new SerializedObject(binder);
        so.FindProperty("field").enumValueIndex = (int)field;
        if (!string.IsNullOrEmpty(format)) so.FindProperty("format").stringValue = format;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void LoadArt()
    {
        _art = LoadFolder(ArtFolder);
        _icons = LoadFolder(IconFolder);
    }

    private static Dictionary<string, Sprite> LoadFolder(string folder)
    {
        var map = new Dictionary<string, Sprite>();
        if (!AssetDatabase.IsValidFolder(folder)) return map;

        foreach (var guid in AssetDatabase.FindAssets("t:Sprite", new[] { folder }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (System.IO.Path.GetDirectoryName(path).Replace('\\', '/') != folder) continue;

            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null) continue;

            string key = Normalize(System.IO.Path.GetFileNameWithoutExtension(path));
            if (!map.ContainsKey(key)) map[key] = sprite;
        }

        return map;
    }

    private static Sprite Art(string name) => Lookup(_art, name);
    private static Sprite Icon(string name) => Lookup(_icons, name);

    private static Sprite Lookup(Dictionary<string, Sprite> map, string name)
    {
        if (map == null || string.IsNullOrEmpty(name)) return null;
        return map.TryGetValue(Normalize(name), out var sprite) ? sprite : null;
    }

    private static string Normalize(string s)
        => new string(s.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());

    private static RectTransform NewChild(RectTransform parent, string name)
    {
        var go = new GameObject(name, typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(go, "Create UI element");

        var rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        return rect;
    }

    private static TMP_Text NewLabel(RectTransform parent, string name, string text,
                                     int size, Color color, TextAlignmentOptions alignment)
    {
        var rect = NewChild(parent, name);
        var label = rect.gameObject.AddComponent<TextMeshProUGUI>();

        label.text = text;
        label.fontSize = size;
        label.color = color;
        label.alignment = alignment;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.overflowMode = TextOverflowModes.Ellipsis;

        return label;
    }

    private static void AddBinder(GameObject go, StatBinder.Field field, string format = "")
    {
        var binder = go.AddComponent<StatBinder>();
        var so = new SerializedObject(binder);
        so.FindProperty("field").enumValueIndex = (int)field;
        if (!string.IsNullOrEmpty(format)) so.FindProperty("format").stringValue = format;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetHeight(RectTransform rect, float height)
    {
        var element = rect.GetComponent<LayoutElement>() ?? rect.gameObject.AddComponent<LayoutElement>();
        element.minHeight = height;
        element.preferredHeight = height;
        element.flexibleHeight = 0;
    }

    private static void Stretch(RectTransform rect, float padding)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(padding, padding);
        rect.offsetMax = new Vector2(-padding, -padding);
    }

    private static T Ensure<T>(GameObject go) where T : Component
        => go.GetComponent<T>() ?? Undo.AddComponent<T>(go);

    private static void AssignArray(SerializedObject so, string propertyName, Object[] values)
    {
        var prop = so.FindProperty(propertyName);
        if (prop == null) return;

        prop.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++)
            prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
    }

    private static int DisciplineIndex(CraftDiscipline d)
    {
        var names = System.Enum.GetNames(typeof(CraftDiscipline));
        for (int i = 0; i < names.Length; i++)
            if (names[i] == d.ToString()) return i;
        return 0;
    }

    private static BarBinder.Source SkillSource(CraftDiscipline d)
    {
        switch (d)
        {
            case CraftDiscipline.Tanner:    return BarBinder.Source.SkillTanner;
            case CraftDiscipline.Carpenter: return BarBinder.Source.SkillCarpenter;
            case CraftDiscipline.Mason:     return BarBinder.Source.SkillMason;
            case CraftDiscipline.Alchemist: return BarBinder.Source.SkillAlchemist;
            default:                        return BarBinder.Source.SkillSmither;
        }
    }

    private static void ClearChildren(RectTransform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
            Undo.DestroyObjectImmediate(parent.GetChild(i).gameObject);
    }
}
#endif
