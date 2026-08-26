#if UNITY_EDITOR
using System;

/// <summary>
/// What a screen is, as data.
///
/// Positions are fractions of the parent, never pixels. The canvas on this
/// project is not the reference resolution and never will be — a 1920x1080
/// reference at match 0.5 produces a 978x2119 canvas on a 1080x2340 phone, and
/// a different one on every other handset. Anything written as a pixel offset
/// is a guess that happens to be right on one device.
///
/// JsonUtility drives this, which rules out dictionaries, polymorphism and
/// nullable fields. Hence the flat element class with a "type" string and the
/// three fixed nesting levels — group, element, child — rather than one
/// self-referencing node type, which JsonUtility handles badly.
/// </summary>
public static class UIScreenSpecTypes { }

[Serializable]
public class SpecRect
{
    /// <summary>Fraction of the parent's width kept clear on the left.</summary>
    public float left;
    public float right;

    /// <summary>Fraction of the parent's height kept clear above.</summary>
    public float top;
    public float bottom;

    /// <summary>Fraction of the parent's height. -1 stretches between top and bottom.</summary>
    public float height = -1f;

    /// <summary>Fraction of the parent's width. -1 stretches between left and right.</summary>
    public float width = -1f;

    /// <summary>Horizontal placement when width is set: left, center, right.</summary>
    public string anchor = "center";

    /// <summary>
    /// Vertical placement when height is set: top, center, bottom.
    ///
    /// Explicit because it cannot be inferred. A spec giving bottom and height
    /// but no top is indistinguishable from one giving top 0, so a rect meant to
    /// sit above the bottom edge silently pinned itself to the ceiling instead.
    /// </summary>
    public string vanchor = "top";
}

[Serializable]
public class SpecPadding
{
    public int left, right, top, bottom;
}

/// <summary>A leaf. Lives inside a button or a scroll template.</summary>
[Serializable]
public class SpecChild
{
    public string type = "label";
    public string name = "";

    public string text = "";
    public string font = "body";
    public float size = 30f;
    public string color = "ink";
    public string align = "left";
    public bool italic;

    public string sprite = "";
    public string fit = "stretch";

    /// <summary>
    /// Marks which label a bound view fills in. The creation screen's answer
    /// template uses "title" and "subtext".
    /// </summary>
    public string role = "";

    /// <summary>
    /// Fraction of the row this child takes, in a horizontal template. Zero
    /// lets it share what is left.
    /// </summary>
    public float width;

    /// <summary>
    /// Width divided by height for an image child, so a portrait keeps its
    /// shape. Soldier art is 0.8; leave it at zero for anything square.
    /// </summary>
    public float aspect;
}

[Serializable]
public class SpecElement
{
    /// <summary>label, image, button, scroll.</summary>
    public string type = "label";
    public string name = "";

    public SpecRect rect = new SpecRect();

    // --- label ---------------------------------------------------------
    public string text = "";
    public string font = "body";
    public float size = 30f;
    public string color = "ink";
    public string align = "center";
    public bool italic;

    // --- image and button ----------------------------------------------
    public string sprite = "";

    /// <summary>cover, fit, stretch, slice. Anything else is treated as stretch.</summary>
    public string fit = "stretch";

    /// <summary>Hex like #2A1B0Ecc, or a palette name. Empty leaves the sprite alone.</summary>
    public string tint = "";

    /// <summary>Text drawn centred on a button. Empty means an icon-only button.</summary>
    public string label = "";

    // --- scroll ---------------------------------------------------------
    public float spacing = 18f;
    public SpecPadding padding = new SpecPadding();

    /// <summary>The repeated item inside a scroll. Cloned at runtime, hidden at build.</summary>
    public SpecChild[] template = new SpecChild[0];

    public string templateSprite = "";

    /// <summary>Fraction of the canvas height, not pixels.</summary>
    public float templateMinHeight = 0.055f;

    public SpecPadding templatePadding = new SpecPadding();

    /// <summary>
    /// Component put on the template item, by type name. Keeps the builder from
    /// knowing about any one screen: the spec names what it wants.
    /// </summary>
    public string templateComponent = "";

    /// <summary>
    /// Lay the template's children left to right instead of stacked. A roster
    /// row is a portrait beside a name, not a portrait above one.
    /// </summary>
    public bool templateHorizontal;

    /// <summary>Marks this object for the screen's component to find. See SpecGroup.role.</summary>
    public string role = "";

    public SpecChild[] children = new SpecChild[0];
}

[Serializable]
public class SpecGroup
{
    public string name = "";

    /// <summary>Groups that start hidden, like a summary page behind the questions.</summary>
    public bool startsHidden;

    public SpecElement[] elements = new SpecElement[0];
}

[Serializable]
public class SpecBackground
{
    public string sprite = "";
    public string fit = "cover";
    public string tint = "";
}

[Serializable]
public class UIScreenSpec
{
    /// <summary>Object name under the canvas. Rebuilding replaces the one with this name.</summary>
    public string screen = "";

    /// <summary>Screens driven by a manager start hidden and are switched on in code.</summary>
    public bool startsHidden = true;

    /// <summary>Component added to the root, by type name. Empty adds nothing.</summary>
    public string component = "";

    public SpecBackground background = new SpecBackground();

    public SpecGroup[] groups = new SpecGroup[0];
}
#endif
