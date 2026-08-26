#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Builds a screen from a JSON spec.
///
/// One tool for every screen, rather than one hand-written builder per screen.
/// The builders that came before it each rediscovered the fonts, the palette
/// and the art paths, and each got the layout wrong in its own way.
///
/// Everything is placed with anchors, which are already fractions of the
/// parent, so a screen is correct on any canvas without the tool knowing the
/// canvas size. The first creation panel put its vertical positions in pixels —
/// top 380, bottom 190 — which read fine against a 1200-tall mental model and
/// bunched everything into the top third of the real 2119-tall canvas. That
/// class of mistake is not possible here: there is nowhere to write a pixel.
///
/// Rebuilding replaces the screen of the same name, so layout is tuned by
/// editing numbers in the spec and running the tool again.
///
/// Tools > UIGame > UI Screens
/// </summary>
public static class UIScreenBuilder
{
    private const string SpecFolder = "Assets/Data/ui";
    private const string ArtRoot = "Assets/UI Elements/";
    private const string HeadFont = "Assets/TextMesh Pro/Resources/Fonts & Materials/CinzelDecorative-Bold SDF.asset";
    private const string BodyFont = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";

    private static TMP_FontAsset _head;
    private static TMP_FontAsset _body;
    private static readonly List<string> _report = new();
    private static Vector2 _canvasSize;

    // =================================================================

    [MenuItem("Tools/UIGame/UI Screens/Build all screens", false, 0)]
    public static void BuildAll()
    {
        if (!Directory.Exists(SpecFolder))
        {
            EditorUtility.DisplayDialog("UI screen builder",
                "No spec folder at " + SpecFolder + ".", "Right");
            return;
        }

        string[] files = Directory.GetFiles(SpecFolder, "*.json");

        if (files.Length == 0)
        {
            EditorUtility.DisplayDialog("UI screen builder",
                "No .json specs in " + SpecFolder + ".", "Right");
            return;
        }

        foreach (string file in files)
            BuildFile(file);

        AssetDatabase.SaveAssets();
    }

    [MenuItem("Tools/UIGame/UI Screens/Build one screen...", false, 1)]
    public static void BuildOne()
    {
        string start = Directory.Exists(SpecFolder) ? SpecFolder : "Assets";
        string path = EditorUtility.OpenFilePanel("Pick a screen spec", start, "json");

        if (string.IsNullOrEmpty(path))
            return;

        BuildFile(ToProjectPath(path));
    }

    // =================================================================

    private static void BuildFile(string specPath)
    {
        string json;

        try
        {
            json = File.ReadAllText(specPath);
        }
        catch (IOException e)
        {
            Debug.LogError("[ScreenBuilder] Could not read " + specPath + ": " + e.Message);
            return;
        }

        UIScreenSpec spec;

        try
        {
            spec = JsonUtility.FromJson<UIScreenSpec>(json);
        }
        catch (System.Exception e)
        {
            Debug.LogError("[ScreenBuilder] " + Path.GetFileName(specPath) + " is not valid JSON: " + e.Message);
            return;
        }

        if (spec == null || string.IsNullOrEmpty(spec.screen))
        {
            Debug.LogError("[ScreenBuilder] " + Path.GetFileName(specPath) + " has no \"screen\" name.");
            return;
        }

        var canvas = FindCanvas();

        if (canvas == null)
        {
            EditorUtility.DisplayDialog("UI screen builder",
                "No Canvas in the open scene.", "Right");
            return;
        }

        _report.Clear();
        LoadFonts();

        var canvasRect = canvas.transform as RectTransform;
        _canvasSize = canvasRect.rect.size;

        Undo.SetCurrentGroupName("Build " + spec.screen);
        int group = Undo.GetCurrentGroup();

        var existing = canvasRect.Find(spec.screen);
        if (existing != null)
        {
            Undo.DestroyObjectImmediate(existing.gameObject);
            _report.Add("Replaced the previous " + spec.screen + ".");
        }

        var root = NewRect(spec.screen, canvasRect);
        Undo.RegisterCreatedObjectUndo(root, "Build screen");
        FullStretch(root);

        BuildBackground(root, spec.background);

        foreach (var g in spec.groups)
            BuildGroup(root, g);

        // Hidden first, then the component. A screen driven by a manager is
        // meant to be off until it is opened, and adding its behaviour to a
        // live object is how a build-time run ends up firing runtime code.
        root.SetActive(!spec.startsHidden);

        if (!string.IsNullOrEmpty(spec.component))
            AddComponentByName(root, spec.component);

        EditorUtility.SetDirty(root);
        Selection.activeGameObject = root;
        Undo.CollapseUndoOperations(group);

        string notes = _report.Count == 0 ? "" : "\n  " + string.Join("\n  ", _report);
        Debug.Log($"[ScreenBuilder] Built {spec.screen} on a {_canvasSize.x:0} x {_canvasSize.y:0} canvas.{notes}");
    }

    // =================================================================
    // Structure
    // =================================================================

    private static void BuildBackground(GameObject parent, SpecBackground bg)
    {
        if (bg == null || string.IsNullOrEmpty(bg.sprite))
            return;

        // The frame crops; the image inside it is what gets scaled. Without the
        // mask a "cover" image spills over the whole canvas.
        var frame = NewRect("Background", parent.transform as RectTransform);
        FullStretch(frame);
        frame.AddComponent<RectMask2D>();

        var go = NewRect("BackgroundImage", frame.transform as RectTransform);
        var image = go.AddComponent<Image>();
        image.raycastTarget = false;

        var sprite = LoadSprite(bg.sprite);

        if (sprite == null)
        {
            image.color = new Color(0.18f, 0.14f, 0.10f);
            FullStretch(go);
            _report.Add("Background sprite missing: " + bg.sprite);
            return;
        }

        image.sprite = sprite;
        ApplyFit(go, image, sprite, bg.fit);

        if (!string.IsNullOrEmpty(bg.tint))
            image.color = ParseColor(bg.tint, Color.white);
    }

    private static void BuildGroup(GameObject parent, SpecGroup group)
    {
        var go = NewRect(string.IsNullOrEmpty(group.name) ? "Group" : group.name,
                         parent.transform as RectTransform);
        FullStretch(go);

        foreach (var element in group.elements)
            BuildElement(go, element);

        if (group.startsHidden)
            go.SetActive(false);
    }

    private static void BuildElement(GameObject parent, SpecElement e)
    {
        switch (e.type)
        {
            case "label": BuildLabel(parent, e); break;
            case "image": BuildImage(parent, e); break;
            case "button": BuildButton(parent, e); break;
            case "scroll": BuildScroll(parent, e); break;
            default:
                _report.Add("Unknown element type '" + e.type + "' on " + e.name + ", skipped.");
                break;
        }
    }

    private static void BuildLabel(GameObject parent, SpecElement e)
    {
        var go = NewRect(Named(e.name, "Label"), parent.transform as RectTransform);
        Place(go, e.rect);
        MakeText(go, e.text, e.font, e.size, e.color, e.align, e.italic);
    }

    private static void BuildImage(GameObject parent, SpecElement e)
    {
        var go = NewRect(Named(e.name, "Image"), parent.transform as RectTransform);
        Place(go, e.rect);

        var image = go.AddComponent<Image>();
        image.raycastTarget = false;

        var sprite = LoadSprite(e.sprite);

        if (sprite != null)
        {
            image.sprite = sprite;

            // A plain image is placed by its own rect, so "cover"/"fit" would
            // fight the spec. Only slicing and tinting apply here.
            if (e.fit == "slice") image.type = Image.Type.Sliced;
            else if (e.fit == "fit") image.preserveAspect = true;
        }
        else if (!string.IsNullOrEmpty(e.sprite))
        {
            _report.Add("Sprite missing: " + e.sprite);
        }

        image.color = string.IsNullOrEmpty(e.tint)
            ? (sprite != null ? Color.white : new Color(0, 0, 0, 0.45f))
            : ParseColor(e.tint, Color.white);
    }

    private static void BuildButton(GameObject parent, SpecElement e)
    {
        var go = NewRect(Named(e.name, "Button"), parent.transform as RectTransform);
        Place(go, e.rect);

        var image = go.AddComponent<Image>();
        var sprite = LoadSprite(e.sprite);

        if (sprite != null)
        {
            image.sprite = sprite;
            if (e.fit == "slice") image.type = Image.Type.Sliced;
            else image.preserveAspect = e.fit == "fit";
        }
        else
        {
            image.color = new Color(0.62f, 0.50f, 0.32f);

            if (!string.IsNullOrEmpty(e.sprite))
                _report.Add("Button sprite missing: " + e.sprite);
        }

        if (!string.IsNullOrEmpty(e.tint))
            image.color = ParseColor(e.tint, image.color);

        var button = go.AddComponent<Button>();
        button.targetGraphic = image;

        var colors = button.colors;
        colors.highlightedColor = new Color(1f, 0.96f, 0.86f);
        colors.pressedColor = new Color(0.82f, 0.76f, 0.64f);
        button.colors = colors;

        foreach (var child in e.children)
            BuildChild(go, child);

        if (!string.IsNullOrEmpty(e.label))
        {
            var labelGo = NewRect("Label", go.transform as RectTransform);
            FullStretch(labelGo);
            MakeText(labelGo, e.label, e.font, e.size, e.color, "center", false);
        }
    }

    private static void BuildScroll(GameObject parent, SpecElement e)
    {
        var go = NewRect(Named(e.name, "Scroll"), parent.transform as RectTransform);
        Place(go, e.rect);

        var scroll = go.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Elastic;
        scroll.scrollSensitivity = 30f;

        var viewport = NewRect("Viewport", go.transform as RectTransform);
        FullStretch(viewport);
        viewport.AddComponent<RectMask2D>();

        var content = NewRect("Content", viewport.transform as RectTransform);
        var contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = Vector2.zero;

        var layout = content.AddComponent<VerticalLayoutGroup>();
        layout.spacing = e.spacing;
        layout.padding = ToRectOffset(e.padding);
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        var fitter = content.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scroll.viewport = viewport.GetComponent<RectTransform>();
        scroll.content = contentRect;

        if (e.template != null && e.template.Length > 0)
            BuildTemplate(content, e);
    }

    private static void BuildTemplate(GameObject content, SpecElement e)
    {
        var go = NewRect("Template", content.transform as RectTransform);

        var image = go.AddComponent<Image>();
        var sprite = LoadSprite(e.templateSprite);

        if (sprite != null)
        {
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
        }
        else
        {
            image.color = new Color(0.76f, 0.66f, 0.48f);

            if (!string.IsNullOrEmpty(e.templateSprite))
                _report.Add("Template sprite missing: " + e.templateSprite);
        }

        go.AddComponent<Button>().targetGraphic = image;

        // A roster row is a portrait beside a name, not above one, so the
        // template can lay its children either way.
        HorizontalOrVerticalLayoutGroup layout = e.templateHorizontal
            ? go.AddComponent<HorizontalLayoutGroup>()
            : (HorizontalOrVerticalLayoutGroup)go.AddComponent<VerticalLayoutGroup>();

        layout.padding = ToRectOffset(e.templatePadding);
        layout.spacing = e.templateHorizontal ? 16 : 2;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = !e.templateHorizontal;
        layout.childForceExpandHeight = e.templateHorizontal;
        layout.childAlignment = TextAnchor.MiddleLeft;

        // No ContentSizeFitter: the Content above controls child heights, and a
        // fitter on a controlled child makes Unity fight its own layout.
        var element = go.AddComponent<LayoutElement>();
        element.minHeight = e.templateMinHeight * _canvasSize.y;

        TMP_Text title = null, subtext = null;

        foreach (var child in e.template)
        {
            var made = BuildChild(go, child);

            if (made == null) continue;

            if (child.role == "title") title = made;
            else if (child.role == "subtext") subtext = made;
        }

        AttachTemplateComponent(go, e.templateComponent, title, subtext);

        go.SetActive(false);
    }

    /// <summary>
    /// The spec names the component; the builder does not know any screen by
    /// name. CreationAnswerView additionally wants its two labels handed to it,
    /// which is the one thing worth a special case rather than an interface
    /// nothing else implements yet.
    /// </summary>
    private static void AttachTemplateComponent(GameObject go, string typeName,
                                                TMP_Text title, TMP_Text subtext)
    {
        if (string.IsNullOrEmpty(typeName))
            return;

        var type = System.Type.GetType(typeName + ", Assembly-CSharp");

        if (type == null)
        {
            _report.Add("Template component type not found: " + typeName);
            return;
        }

        var component = go.AddComponent(type);

        if (component is CreationAnswerView answerView)
            answerView.SetLabels(title, subtext);
    }

    private static TMP_Text BuildChild(GameObject parent, SpecChild c)
    {
        if (c.type == "image")
        {
            var imageGo = NewRect(Named(c.name, "Image"), parent.transform as RectTransform);
            var image = imageGo.AddComponent<Image>();
            image.raycastTarget = false;

            var sprite = LoadSprite(c.sprite);
            if (sprite != null) image.sprite = sprite;
            else if (!string.IsNullOrEmpty(c.sprite)) _report.Add("Sprite missing: " + c.sprite);

            // Artwork keeps its shape. A portrait squeezed into a square slot is
            // the one mistake that makes hand-drawn art look cheap, and the
            // soldier portraits in this project are 0.8, not 1.
            if (c.aspect > 0f)
            {
                var fitter = imageGo.AddComponent<AspectRatioFitter>();
                fitter.aspectRatio = c.aspect;
                fitter.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
            }
            else if (sprite != null)
            {
                image.preserveAspect = true;
            }

            SetWidth(imageGo, c.width);
            return null;
        }

        var go = NewRect(Named(c.name, "Label"), parent.transform as RectTransform);
        var text = MakeText(go, c.text, c.font, c.size, c.color, c.align, c.italic);
        SetWidth(go, c.width);
        return text;
    }

    /// <summary>
    /// A fixed share of the row, in canvas units. Zero leaves the child to take
    /// whatever the layout group has left, which is what a name label wants.
    /// </summary>
    private static void SetWidth(GameObject go, float fraction)
    {
        if (fraction <= 0f) return;

        var element = go.AddComponent<LayoutElement>();
        element.preferredWidth = fraction * _canvasSize.x;
        element.flexibleWidth = 0f;
    }

    // =================================================================
    // Placement
    // =================================================================

    /// <summary>
    /// Anchors are already fractions of the parent, so a spec written in
    /// fractions needs no pixel arithmetic and no knowledge of the canvas.
    /// </summary>
    private static void Place(GameObject go, SpecRect r)
    {
        var rect = go.GetComponent<RectTransform>();

        float xMin, xMax;

        if (r.width >= 0f)
        {
            switch (r.anchor)
            {
                case "left":  xMin = r.left;             xMax = r.left + r.width;  break;
                case "right": xMin = 1f - r.right - r.width; xMax = 1f - r.right;  break;
                default:      xMin = 0.5f - r.width * 0.5f; xMax = 0.5f + r.width * 0.5f; break;
            }
        }
        else
        {
            xMin = r.left;
            xMax = 1f - r.right;
        }

        float yMin, yMax;

        if (r.height >= 0f)
        {
            switch (r.vanchor)
            {
                case "bottom":
                    yMin = r.bottom;
                    yMax = yMin + r.height;
                    break;

                case "center":
                    yMin = 0.5f - r.height * 0.5f;
                    yMax = 0.5f + r.height * 0.5f;
                    break;

                default:
                    // Measured down from the top, which is how a screen is read.
                    yMax = 1f - r.top;
                    yMin = yMax - r.height;
                    break;
            }
        }
        else
        {
            yMin = r.bottom;
            yMax = 1f - r.top;
        }

        rect.anchorMin = new Vector2(xMin, yMin);
        rect.anchorMax = new Vector2(xMax, yMax);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void FullStretch(GameObject go)
    {
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    /// <summary>
    /// cover fills the parent and lets the overflow be cropped; fit keeps the
    /// whole image inside it. Both go through AspectRatioFitter so they stay
    /// right on a handset the tool never saw.
    /// </summary>
    private static void ApplyFit(GameObject go, Image image, Sprite sprite, string fit)
    {
        if (fit == "stretch")
        {
            FullStretch(go);
            return;
        }

        if (fit == "slice")
        {
            FullStretch(go);
            image.type = Image.Type.Sliced;
            return;
        }

        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;

        var fitter = go.AddComponent<AspectRatioFitter>();
        fitter.aspectRatio = sprite.rect.width / sprite.rect.height;
        fitter.aspectMode = fit == "fit"
            ? AspectRatioFitter.AspectMode.FitInParent
            : AspectRatioFitter.AspectMode.EnvelopeParent;
    }

    // =================================================================
    // Bits
    // =================================================================

    private static TMP_Text MakeText(GameObject go, string content, string font, float size,
                                     string color, string align, bool italic)
    {
        var text = go.AddComponent<TextMeshProUGUI>();

        var asset = font == "head" ? _head : _body;
        if (asset != null) text.font = asset;

        text.fontSize = size;
        text.color = Palette(color);
        text.alignment = Align(align);
        text.textWrappingMode = TextWrappingModes.Normal;
        text.raycastTarget = false;

        // Left empty on purpose when the spec says nothing. The previous builder
        // defaulted to the object's name and shipped a screen reading
        // "PROMPTLABEL".
        text.text = content ?? "";

        if (italic) text.fontStyle = FontStyles.Italic;

        return text;
    }

    private static TextAlignmentOptions Align(string align)
    {
        switch (align)
        {
            case "left": return TextAlignmentOptions.Left;
            case "right": return TextAlignmentOptions.Right;
            case "topleft": return TextAlignmentOptions.TopLeft;
            case "top": return TextAlignmentOptions.Top;
            case "bottom": return TextAlignmentOptions.Bottom;
            default: return TextAlignmentOptions.Center;
        }
    }

    private static Color Palette(string name)
    {
        switch (name)
        {
            case "ink": return new Color(0.16f, 0.10f, 0.05f);
            case "ink-soft": return new Color(0.36f, 0.26f, 0.15f);
            case "gold": return new Color(0.85f, 0.70f, 0.36f);
            case "cream": return new Color(0.96f, 0.92f, 0.82f);
            case "white": return Color.white;
            default: return ParseColor(name, new Color(0.96f, 0.92f, 0.82f));
        }
    }

    private static Color ParseColor(string value, Color fallback)
    {
        if (!string.IsNullOrEmpty(value) && ColorUtility.TryParseHtmlString(value, out var parsed))
            return parsed;

        return fallback;
    }

    private static RectOffset ToRectOffset(SpecPadding p)
        => p == null ? new RectOffset() : new RectOffset(p.left, p.right, p.top, p.bottom);

    private static string Named(string name, string fallback)
        => string.IsNullOrEmpty(name) ? fallback : name;

    private static GameObject NewRect(string name, RectTransform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private static Sprite LoadSprite(string path)
    {
        if (string.IsNullOrEmpty(path))
            return null;

        // Specs name art relative to UI Elements, which is where all of it lives.
        string full = path.StartsWith("Assets/") ? path : ArtRoot + path;
        return AssetDatabase.LoadAssetAtPath<Sprite>(full);
    }

    private static void AddComponentByName(GameObject go, string typeName)
    {
        var type = System.Type.GetType(typeName + ", Assembly-CSharp");

        if (type == null)
        {
            _report.Add("Component type not found: " + typeName);
            return;
        }

        go.AddComponent(type);
    }

    private static void LoadFonts()
    {
        _head = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(HeadFont);
        _body = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(BodyFont);

        if (_head == null) _report.Add("Heading font not found.");
        if (_body == null) _report.Add("Body font not found.");
    }

    private static string ToProjectPath(string absolute)
    {
        string root = Application.dataPath.Replace('\\', '/');
        string norm = absolute.Replace('\\', '/');

        return norm.StartsWith(root) ? "Assets" + norm.Substring(root.Length) : norm;
    }

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
