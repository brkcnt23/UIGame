using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Drives a progress bar from a player value.
///
/// Works with either a Slider (fill area + background sprite, handle removed)
/// or a bare Image set to Filled. The Slider route is what the profile panel
/// uses, because the artwork is already cut as background / filler.
/// </summary>
public class BarBinder : MonoBehaviour
{
    public enum Source
    {
        CharacterXp,
        Health,
        Exhaustion,
        Weight,
        Standing,
        Renown,
        SkillSmither,
        SkillTanner,
        SkillCarpenter,
        SkillMason,
        SkillAlchemist
    }

    [SerializeField] private Source source;

    [Tooltip("Optional. Filled with '240 / 600' when assigned.")]
    [SerializeField] private TMP_Text valueLabel;

    [Tooltip("Leave empty to use the default for this source.")]
    [SerializeField] private string labelFormat = "";

    private Slider _slider;
    private Image _fillImage;

    private void Awake()
    {
        _slider = GetComponent<Slider>();

        if (_slider == null)
        {
            var image = GetComponent<Image>();
            if (image != null && image.type == Image.Type.Filled)
                _fillImage = image;
        }
    }

    private void OnEnable() => Refresh();

    public void Refresh()
    {
        var pd = PlayerStatHandler.Instance != null ? PlayerStatHandler.Instance.pd : null;
        if (pd == null) return;

        GetValues(pd, out int current, out int max);

        float ratio = max <= 0 ? 0f : Mathf.Clamp01(current / (float)max);

        if (_slider != null)
        {
            _slider.minValue = 0f;
            _slider.maxValue = 1f;
            _slider.value = ratio;
        }
        else if (_fillImage != null)
        {
            _fillImage.fillAmount = ratio;
        }

        if (valueLabel != null)
        {
            string format = string.IsNullOrEmpty(labelFormat) ? "{0} / {1}" : labelFormat;
            valueLabel.text = string.Format(format, current, max);
        }
    }

    private void GetValues(PlayerData pd, out int current, out int max)
    {
        switch (source)
        {
            case Source.CharacterXp:
                current = ExperienceSystem.GetXpIntoCurrentLevel(pd);
                max = ExperienceSystem.CostForNextLevel(pd.Level);
                return;

            case Source.Health:
                current = pd.Health;
                max = Mathf.Max(1, pd.MaxHealth);
                return;

            case Source.Exhaustion:
                current = pd.CurrentExhaustionLevel;
                max = Mathf.Max(1, pd.MaxExhaustionLevel);
                return;

            case Source.Weight:
                current = Mathf.RoundToInt(pd.GetCurrentInventoryWeight());
                max = Mathf.Max(1, Mathf.RoundToInt(pd.GetCarryCapacity()));
                return;

            // Reputation is not on PlayerData yet — the title system lands next.
            // Reading zero is honest; inventing a number is not.
            case Source.Standing:
            case Source.Renown:
                current = 0;
                max = 100;
                return;

            case Source.SkillSmither:   GetSkill(pd, CraftDiscipline.Smither,   out current, out max); return;
            case Source.SkillTanner:    GetSkill(pd, CraftDiscipline.Tanner,    out current, out max); return;
            case Source.SkillCarpenter: GetSkill(pd, CraftDiscipline.Carpenter, out current, out max); return;
            case Source.SkillMason:     GetSkill(pd, CraftDiscipline.Mason,     out current, out max); return;
            case Source.SkillAlchemist: GetSkill(pd, CraftDiscipline.Alchemist, out current, out max); return;

            default:
                current = 0;
                max = 1;
                return;
        }
    }

    private static void GetSkill(PlayerData pd, CraftDiscipline discipline, out int current, out int max)
    {
        current = ExperienceSystem.GetCraftXP(pd, discipline);
        max = 100;
    }
}
