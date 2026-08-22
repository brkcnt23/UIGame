#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Builds the character creation screen.
///
/// Unlike the profile panel, this one has no hand-made artwork to preserve, so
/// the tool creates the whole hierarchy rather than wiring an existing one. It
/// is laid out with anchors and layout groups instead of fixed positions, which
/// is what lets one screen hold both a four-answer question and the eight-answer
/// origin question without anything running off the parchment.
///
/// Re-running deletes the previous panel and builds a fresh one, so the layout
/// can be tuned in code and rebuilt rather than nudged by hand.
///
/// Tools > UIGame > Character Creation
/// </summary>
public static class CharacterCreationPanelBuilder
{
    private const string PanelName = "CharacterCreationPanel";

    private const string ParchmentPath = "Assets/UI Elements/Backgrounds/PARCHMENT BACKGROUND.png";
    private const string AnswerSpritePath = "Assets/UI Elements/NEE/AmptyLongButton2.png";
    private const string AnswerSpriteAlt = "Assets/UI Elements/NEE/empty_long_button.png";
    private const string BackSpritePath = "Assets/UI Elements/Buttons/R BUTTON.png";
    private const string OkSpritePath = "Assets/UI Elements/NEE/OkButton.png";

    private const string HeadFontPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/CinzelDecorative-Bold SDF.asset";
    private const string BodyFontPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";

    // Ink on parchment. The profile panel's blues are for a dark panel and read
    // as washed out on this background.
    private static readonly Color Ink = new Color(0.16f, 0.10f, 0.05f);
    private static readonly Color InkSoft = new Color(0.36f, 0.26f, 0.15f);
    private static readonly Color InkGold = new Color(0.48f, 0.33f, 0.10f);

    private static TMP_FontAsset _head;
    private static TMP_FontAsset _body;
    private static readonly List<string> _report = new();

    // =================================================================

    [MenuItem("Tools/UIGame/Character Creation/Build panel", false, 0)]
    public static void Build()
    {
        var canvas = FindCanvas();

        if (canvas == null)
        {
            EditorUtility.DisplayDialog("Character creation builder",
                "Select the Canvas the panel should live under, or have one in the open scene.",
                "Right");
            return;
        }

        _report.Clear();
        LoadFonts();

        Undo.SetCurrentGroupName("Build character creation panel");
        int group = Undo.GetCurrentGroup();

        var existing = canvas.transform.Find(PanelName);
        if (existing != null)
        {
            Undo.DestroyObjectImmediate(existing.gameObject);
            _report.Add("Replaced the previous panel.");
        }

        var panelGo = NewRect(PanelName, canvas.transform as RectTransform);
        Undo.RegisterCreatedObjectUndo(panelGo, "Create creation panel");
        Stretch(panelGo, 0, 0, 0, 0);

        var panel = panelGo.AddComponent<CharacterCreationPanel>();

        BuildBackground(panelGo);

        var questionRoot = BuildQuestionSide(panelGo,
            out var progress, out var prompt, out var content,
            out var template, out var scroll, out var back);

        var summaryRoot = BuildSummarySide(panelGo,
            out var sumName, out var sumStats, out var sumProfile,
            out var sumTraits, out var carryOn);

        var skip = BuildSkipButton(panelGo);

        panel.Wire(questionRoot, progress, prompt, content, template, scroll, back,
                   summaryRoot, sumName, sumStats, sumProfile, sumTraits, carryOn, skip);

        summaryRoot.SetActive(false);
        template.gameObject.SetActive(false);
        panelGo.SetActive(false);

        EditorUtility.SetDirty(panelGo);
        Selection.activeGameObject = panelGo;
        Undo.CollapseUndoOperations(group);

        _report.Add("Panel starts disabled. Assign it to GameManager > Character Creation Panel.");

        Debug.Log("[CreationBuilder] Built " + PanelName + ".\n  " + string.Join("\n  ", _report));
    }

    [MenuItem("Tools/UIGame/Character Creation/Add missing systems to scene", false, 1)]
    public static void AddMissingSystems()
    {
        var holder = GameObject.Find("Manager Holder") ?? GameObject.Find("Managers");

        if (holder == null)
        {
            holder = new GameObject("Manager Holder");
            Undo.RegisterCreatedObjectUndo(holder, "Create manager holder");
        }

        var added = new List<string>();

        AddIfMissing<CharacterCreationSystem>(holder, added);
        AddIfMissing<TraitSystem>(holder, added);

        if (added.Count == 0)
        {
            Debug.Log("[CreationBuilder] Both systems were already in the scene.");
            return;
        }

        EditorUtility.SetDirty(holder);
        Debug.Log("[CreationBuilder] Added to '" + holder.name + "': " + string.Join(", ", added) +
                  ".\n  Save the scene, then check the boot log lists them.");
    }

    [MenuItem("Tools/UIGame/Character Creation/Report which systems are missing", false, 20)]
    public static void ReportSystems()
    {
        var lines = new List<string>();

        foreach (var type in typeof(GameSystemBase).Assembly.GetTypes()
                     .Where(t => t.IsSubclassOf(typeof(GameSystemBase)) && !t.IsAbstract)
                     .OrderBy(t => t.Name))
        {
            bool present = Object.FindObjectsByType(type, FindObjectsInactive.Include, FindObjectsSortMode.None).Length > 0;
            lines.Add((present ? "  in scene   " : "  MISSING    ") + type.Name);
        }

        Debug.Log("[CreationBuilder] Game systems:\n" + string.Join("\n", lines));
    }

    // =================================================================
    // Pieces
    // =================================================================

    private static void BuildBackground(GameObject parent)
    {
        var go = NewRect("Background", parent.transform as RectTransform);
        Stretch(go, 0, 0, 0, 0);

        var image = go.AddComponent<Image>();
        image.sprite = Load<Sprite>(ParchmentPath);
        image.type = Image.Type.Simple;
        image.preserveAspect = false;
        image.raycastTarget = false;

        if (image.sprite == null)
        {
            image.color = new Color(0.85f, 0.78f, 0.62f);
            _report.Add("Parchment sprite not found, used a flat colour instead.");
        }
    }

    private static GameObject BuildQuestionSide(GameObject parent,
        out TMP_Text progress, out TMP_Text prompt, out RectTransform content,
        out CreationAnswerView template, out ScrollRect scroll, out Button back)
    {
        var root = NewRect("QuestionRoot", parent.transform as RectTransform);
        Stretch(root, 0, 0, 0, 0);

        var rootRect = root.GetComponent<RectTransform>();

        progress = NewText("ProgressLabel", rootRect, _head, 34, InkGold, TextAlignmentOptions.Center);
        TopBand(progress.rectTransform, 60, 60, 56, 60);

        prompt = NewText("PromptLabel", rootRect, _head, 46, Ink, TextAlignmentOptions.Top);
        TopBand(prompt.rectTransform, 60, 60, 130, 230);

        // --- scrolling answers -----------------------------------------
        var scrollGo = NewRect("AnswerScroll", rootRect);
        Stretch(scrollGo, 40, 40, 380, 190);

        scroll = scrollGo.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Elastic;
        scroll.scrollSensitivity = 30f;

        var viewport = NewRect("Viewport", scrollGo.GetComponent<RectTransform>());
        Stretch(viewport, 0, 0, 0, 0);
        viewport.AddComponent<RectMask2D>();

        var contentGo = NewRect("Content", viewport.GetComponent<RectTransform>());
        content = contentGo.GetComponent<RectTransform>();
        content.anchorMin = new Vector2(0, 1);
        content.anchorMax = new Vector2(1, 1);
        content.pivot = new Vector2(0.5f, 1f);
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = new Vector2(0, 0);

        var layout = contentGo.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 18;
        layout.padding = new RectOffset(0, 0, 0, 24);
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        var fitter = contentGo.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scroll.viewport = viewport.GetComponent<RectTransform>();
        scroll.content = content;

        template = BuildAnswerTemplate(content);

        // --- back ------------------------------------------------------
        back = BuildIconButton("BackButton", rootRect, BackSpritePath, "Back");
        var backRect = back.GetComponent<RectTransform>();
        backRect.anchorMin = new Vector2(0, 0);
        backRect.anchorMax = new Vector2(0, 0);
        backRect.pivot = new Vector2(0, 0);
        backRect.anchoredPosition = new Vector2(40, 40);
        backRect.sizeDelta = new Vector2(130, 110);

        return root;
    }

    private static CreationAnswerView BuildAnswerTemplate(RectTransform parent)
    {
        var go = NewRect("AnswerTemplate", parent);

        var image = go.AddComponent<Image>();
        image.sprite = Load<Sprite>(AnswerSpritePath) ?? Load<Sprite>(AnswerSpriteAlt);
        image.type = Image.Type.Sliced;

        if (image.sprite == null)
        {
            image.color = new Color(0.76f, 0.66f, 0.48f);
            _report.Add("Answer button sprite not found, used a flat colour instead.");
        }

        var button = go.AddComponent<Button>();
        button.targetGraphic = image;

        var colors = button.colors;
        colors.highlightedColor = new Color(1f, 0.96f, 0.86f);
        colors.pressedColor = new Color(0.82f, 0.76f, 0.64f);
        button.colors = colors;

        var layout = go.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(34, 34, 20, 20);
        layout.spacing = 2;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        // No ContentSizeFitter here on purpose. The Content above controls child
        // heights, and a fitter on a controlled child makes Unity complain and
        // fight the layout. The group reads this object's preferred height from
        // its own layout group instead, which the two labels drive.
        var element = go.AddComponent<LayoutElement>();
        element.minHeight = 116;

        var title = NewText("Title", go.GetComponent<RectTransform>(), _head, 34, Ink, TextAlignmentOptions.Left);
        var subtext = NewText("Subtext", go.GetComponent<RectTransform>(), _body, 26, InkSoft, TextAlignmentOptions.TopLeft);
        subtext.fontStyle = FontStyles.Italic;

        var view = go.AddComponent<CreationAnswerView>();
        view.SetLabels(title, subtext);

        return view;
    }

    private static GameObject BuildSummarySide(GameObject parent,
        out TMP_Text name, out TMP_Text stats, out TMP_Text profile,
        out TMP_Text traits, out Button carryOn)
    {
        var root = NewRect("SummaryRoot", parent.transform as RectTransform);
        Stretch(root, 0, 0, 0, 0);

        var rootRect = root.GetComponent<RectTransform>();

        var heading = NewText("SummaryHeading", rootRect, _head, 34, InkGold, TextAlignmentOptions.Center);
        heading.text = "This is who you are";
        TopBand(heading.rectTransform, 60, 60, 150, 60);

        name = NewText("SummaryName", rootRect, _head, 52, Ink, TextAlignmentOptions.Center);
        TopBand(name.rectTransform, 50, 50, 230, 130);

        stats = NewText("SummaryStats", rootRect, _head, 36, InkSoft, TextAlignmentOptions.Center);
        TopBand(stats.rectTransform, 40, 40, 380, 60);

        profile = NewText("SummaryProfile", rootRect, _body, 30, InkSoft, TextAlignmentOptions.Top);
        profile.fontStyle = FontStyles.Italic;
        TopBand(profile.rectTransform, 70, 70, 460, 90);

        var traitsHeading = NewText("TraitsHeading", rootRect, _head, 30, InkGold, TextAlignmentOptions.Center);
        traitsHeading.text = "Traits";
        TopBand(traitsHeading.rectTransform, 60, 60, 580, 50);

        traits = NewText("SummaryTraits", rootRect, _body, 30, Ink, TextAlignmentOptions.Top);
        TopBand(traits.rectTransform, 60, 60, 640, 420);

        carryOn = BuildIconButton("ContinueButton", rootRect, OkSpritePath, "Begin");
        var rect = carryOn.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0);
        rect.anchorMax = new Vector2(0.5f, 0);
        rect.pivot = new Vector2(0.5f, 0);
        rect.anchoredPosition = new Vector2(0, 90);
        rect.sizeDelta = new Vector2(300, 120);

        return root;
    }

    private static Button BuildSkipButton(GameObject parent)
    {
        var button = BuildIconButton("SkipButton", parent.transform as RectTransform, null, "Skip");

        var rect = button.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(1, 1);
        rect.anchoredPosition = new Vector2(-30, -30);
        rect.sizeDelta = new Vector2(150, 70);

        var image = button.GetComponent<Image>();
        if (image != null) image.color = new Color(0.55f, 0.45f, 0.30f, 0.45f);

        _report.Add("Skip button answers at random — delete it before shipping.");

        return button;
    }

    // =================================================================
    // Helpers
    // =================================================================

    private static GameObject NewRect(string name, RectTransform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private static void Stretch(GameObject go, float left, float right, float top, float bottom)
    {
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(-right, -top);
    }

    /// <summary>Full-width band a fixed distance down from the top edge.</summary>
    private static void TopBand(RectTransform rect, float left, float right, float fromTop, float height)
    {
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.offsetMin = new Vector2(left, 0);
        rect.offsetMax = new Vector2(-right, 0);
        rect.anchoredPosition = new Vector2(0, -fromTop);
        rect.sizeDelta = new Vector2(rect.sizeDelta.x, height);
    }

    private static TMP_Text NewText(string name, RectTransform parent, TMP_FontAsset font,
                                    float size, Color color, TextAlignmentOptions alignment)
    {
        var go = NewRect(name, parent);

        var text = go.AddComponent<TextMeshProUGUI>();
        if (font != null) text.font = font;
        text.fontSize = size;
        text.color = color;
        text.alignment = alignment;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.raycastTarget = false;
        text.text = name;

        return text;
    }

    private static Button BuildIconButton(string name, RectTransform parent, string spritePath, string label)
    {
        var go = NewRect(name, parent);

        var image = go.AddComponent<Image>();
        var sprite = string.IsNullOrEmpty(spritePath) ? null : Load<Sprite>(spritePath);

        if (sprite != null)
        {
            image.sprite = sprite;
            image.type = Image.Type.Simple;
            image.preserveAspect = true;
        }
        else
        {
            image.color = new Color(0.62f, 0.50f, 0.32f);

            var text = NewText("Label", go.GetComponent<RectTransform>(), _head, 30,
                               new Color(0.96f, 0.92f, 0.82f), TextAlignmentOptions.Center);
            Stretch(text.gameObject, 0, 0, 0, 0);
            text.text = label;

            if (!string.IsNullOrEmpty(spritePath))
                _report.Add("Sprite missing for " + name + " (" + spritePath + "), used a labelled block.");
        }

        var button = go.AddComponent<Button>();
        button.targetGraphic = image;

        return button;
    }

    private static void AddIfMissing<T>(GameObject holder, List<string> added) where T : Component
    {
        if (Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length > 0)
            return;

        Undo.AddComponent<T>(holder);
        added.Add(typeof(T).Name);
    }

    private static void LoadFonts()
    {
        _head = Load<TMP_FontAsset>(HeadFontPath);
        _body = Load<TMP_FontAsset>(BodyFontPath);

        if (_head == null) _report.Add("Heading font not found at " + HeadFontPath + ".");
        if (_body == null) _report.Add("Body font not found at " + BodyFontPath + ".");
    }

    private static T Load<T>(string path) where T : Object
        => AssetDatabase.LoadAssetAtPath<T>(path);

    private static Canvas FindCanvas()
    {
        if (Selection.activeGameObject != null)
        {
            var fromSelection = Selection.activeGameObject.GetComponentInParent<Canvas>();
            if (fromSelection != null) return fromSelection.rootCanvas;
        }

        return Object.FindFirstObjectByType<Canvas>();
    }
}
#endif
