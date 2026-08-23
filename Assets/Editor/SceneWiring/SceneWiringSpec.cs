#if UNITY_EDITOR
using System;

/// <summary>
/// Inspector wiring, as data.
///
/// Dragging a reference into a slot is the slowest part of building a screen and
/// the easiest to get wrong quietly: a field left empty behaves exactly like a
/// feature that was never written. A recipe says which object goes in which
/// slot, so the wiring can be rebuilt after a panel is regenerated instead of
/// being redone by hand.
///
/// JsonUtility has no dictionaries, so field-to-object pairs are arrays.
/// </summary>
[Serializable]
public class WiringField
{
    /// <summary>Field name on the component, exactly as written in the script.</summary>
    public string field = "";

    /// <summary>Object name in the scene. The component is taken off it.</summary>
    public string obj = "";
}

[Serializable]
public class WiringArray
{
    public string field = "";
    public string[] objects = new string[0];
}

[Serializable]
public class WiringRecipe
{
    /// <summary>Scene object the component lives on.</summary>
    public string target = "";

    /// <summary>Component type name. Added if the object does not have it.</summary>
    public string component = "";

    /// <summary>
    /// Leave a slot alone when it already points at something. Off by default,
    /// so a rebuild restores the intended wiring rather than half of it.
    /// </summary>
    public bool keepExisting;

    public WiringField[] fields = new WiringField[0];
    public WiringArray[] arrays = new WiringArray[0];
}

[Serializable]
public class SceneWiringSpec
{
    public WiringRecipe[] recipes = new WiringRecipe[0];
}
#endif
