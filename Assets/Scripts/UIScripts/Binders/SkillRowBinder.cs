using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// One crafting skill row: name, what it covers, level, and progress.
///
/// The five disciplines are fixed by the game's design, so this binds to a
/// chosen discipline rather than generating rows from a list.
/// </summary>
public class SkillRowBinder : MonoBehaviour
{
    [SerializeField] private CraftDiscipline discipline = CraftDiscipline.Smither;

    [Header("Targets")]
    [SerializeField] private TMP_Text nameLabel;
    [SerializeField] private TMP_Text descriptionLabel;
    [SerializeField] private TMP_Text levelLabel;
    [SerializeField] private TMP_Text xpLabel;
    [SerializeField] private Slider progressBar;

    private void OnEnable() => Refresh();

    public void Refresh()
    {
        var pd = PlayerStatHandler.Instance != null ? PlayerStatHandler.Instance.pd : null;
        if (pd == null) return;

        int level = ExperienceSystem.GetCraftLevel(pd, discipline);
        int xp = ExperienceSystem.GetCraftXP(pd, discipline);
        const int needed = 100;

        if (nameLabel != null)        nameLabel.text = DisplayName(discipline);
        if (descriptionLabel != null) descriptionLabel.text = Description(discipline);
        if (levelLabel != null)       levelLabel.text = $"Lv. {level}";
        if (xpLabel != null)          xpLabel.text = $"{xp} / {needed} XP";

        if (progressBar != null)
        {
            progressBar.minValue = 0f;
            progressBar.maxValue = 1f;
            progressBar.value = Mathf.Clamp01(xp / (float)needed);
        }
    }

    public static string DisplayName(CraftDiscipline d)
    {
        switch (d)
        {
            case CraftDiscipline.Smither:   return "Smithing";
            case CraftDiscipline.Tanner:    return "Tanner";
            case CraftDiscipline.Carpenter: return "Carpenter";
            case CraftDiscipline.Mason:     return "Mason";
            case CraftDiscipline.Alchemist: return "Alchemist";
            default:                        return d.ToString();
        }
    }

    public static string Description(CraftDiscipline d)
    {
        switch (d)
        {
            case CraftDiscipline.Smither:   return "Weapon and armour crafting";
            case CraftDiscipline.Tanner:    return "Leatherworking";
            case CraftDiscipline.Carpenter: return "Woodworking";
            case CraftDiscipline.Mason:     return "Stonework";
            case CraftDiscipline.Alchemist: return "Potion making";
            default:                        return "";
        }
    }

    /// <summary>Icon name in ProfilePanel/icons, when one exists.</summary>
    public static string IconName(CraftDiscipline d)
    {
        switch (d)
        {
            case CraftDiscipline.Smither:   return "smithing";
            case CraftDiscipline.Tanner:    return "leatherworking";
            case CraftDiscipline.Carpenter: return "woodworking";
            case CraftDiscipline.Mason:     return "stonework";
            case CraftDiscipline.Alchemist: return "alchemy";
            default:                        return null;
        }
    }

    public void SetDiscipline(CraftDiscipline d)
    {
        discipline = d;
        Refresh();
    }
}
