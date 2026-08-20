using TMPro;
using UnityEngine;

/// <summary>
/// Fills one value field from the player.
///
/// This replaces the twenty hand-dragged TMP_Text references on NavUISystem.
/// Drop it on the value label, pick what it shows, done — no code change when
/// the panel is rearranged, and a designer can move rows around freely.
///
/// The panel calls RefreshAll(); each binder reads what it needs.
/// </summary>
public class StatBinder : MonoBehaviour
{
    public enum Field
    {
        // Attributes
        Strength, Dexterity, Constitution, Charisma,

        // Derived combat
        Attack, Defense, Accuracy, Initiative, CriticalChance,

        // Condition
        Health, HealthMax, HealthPair,
        Exhaustion, ExhaustionPair,
        Rations, Weight, WeightPair,

        // Progress
        Level, Experience, ExperiencePair, ExperienceRatio,

        // Identity
        PlayerName, TitleName, Alignment, HomeSettlement,

        // Money
        Gold, Silver, MoneyPair,

        // Record
        DaysSurvived, BattlesWon, BattlesLost, CompanionPair
    }

    [SerializeField] private Field field;

    [Tooltip("Optional. '{0}' is replaced by the value, e.g. 'Lv {0}'.")]
    [SerializeField] private string format = "";

    private TMP_Text _label;

    private void Awake()
    {
        _label = GetComponent<TMP_Text>();
        if (_label == null)
            Debug.LogWarning($"[StatBinder] No TMP_Text on '{name}'.");
    }

    private void OnEnable() => Refresh();

    public void Refresh()
    {
        if (_label == null) return;

        var pd = PlayerStatHandler.Instance != null ? PlayerStatHandler.Instance.pd : null;
        if (pd == null)
        {
            _label.text = "—";
            return;
        }

        string value = Read(pd);
        _label.text = string.IsNullOrEmpty(format) ? value : string.Format(format, value);
    }

    private string Read(PlayerData pd)
    {
        switch (field)
        {
            case Field.Strength:     return pd.Strength.ToString();
            case Field.Dexterity:    return pd.Dexterity.ToString();
            case Field.Constitution: return pd.Constitution.ToString();
            case Field.Charisma:     return pd.Charisma.ToString();

            case Field.Attack:         return Signed(DerivedStats.Attack(pd));
            case Field.Defense:        return DerivedStats.Defense(pd).ToString();
            case Field.Accuracy:       return DerivedStats.Accuracy(pd).ToString();
            case Field.Initiative:     return Signed(DerivedStats.Initiative(pd));
            case Field.CriticalChance: return DerivedStats.CriticalChance(pd) + "%";

            case Field.Health:         return pd.Health.ToString();
            case Field.HealthMax:      return pd.MaxHealth.ToString();
            case Field.HealthPair:     return $"{pd.Health}/{pd.MaxHealth}";

            case Field.Exhaustion:     return pd.CurrentExhaustionLevel.ToString();
            case Field.ExhaustionPair: return $"{pd.CurrentExhaustionLevel}/{pd.MaxExhaustionLevel}";

            case Field.Rations:        return pd.Rations.ToString();

            case Field.Weight:         return Mathf.RoundToInt(pd.GetCurrentInventoryWeight()).ToString();
            case Field.WeightPair:     return WeightPair(pd);

            case Field.Level:           return pd.Level.ToString();
            case Field.Experience:      return ExperienceSystem.GetXpIntoCurrentLevel(pd).ToString();
            case Field.ExperiencePair:  return $"{ExperienceSystem.GetXpIntoCurrentLevel(pd)}/" +
                                               $"{ExperienceSystem.CostForNextLevel(pd.Level)}";
            case Field.ExperienceRatio: return Ratio().ToString("P0");

            case Field.PlayerName:      return string.IsNullOrEmpty(pd.Name) ? "—" : pd.Name;
            case Field.TitleName:       return CurrentTitle();
            case Field.Alignment:       return AlignmentLabel(pd.Alignment);
            case Field.HomeSettlement:  return HomeName();

            case Field.Gold:      return pd.GetMoney().Gold.ToString();
            case Field.Silver:    return pd.GetMoney().Silver.ToString();
            case Field.MoneyPair:
            {
                var m = pd.GetMoney();
                return $"{m.Gold}g {m.Silver}s";
            }

            case Field.DaysSurvived: return pd.Day.ToString();
            case Field.BattlesWon:   return pd.TotalBattlesWon.ToString();
            case Field.BattlesLost:  return pd.TotalBattlesLost.ToString();
            case Field.CompanionPair:
                return $"{(pd.Companions?.Count ?? 0)}/{CompanionSlots()}";

            default: return "—";
        }
    }

    private float Ratio()
    {
        var pd = PlayerStatHandler.Instance.pd;
        int need = ExperienceSystem.CostForNextLevel(pd.Level);
        return need <= 0 ? 0f : Mathf.Clamp01(ExperienceSystem.GetXpIntoCurrentLevel(pd) / (float)need);
    }

    private static string Signed(int v) => v >= 0 ? $"+{v}" : v.ToString();

    /// <summary>
    /// Weight reads "45/90", and turns amber, orange or red as the load
    /// crosses each encumbrance band — the warning arrives before the penalty
    /// does, which is the only fair way to run a carry limit.
    /// </summary>
    private string WeightPair(PlayerData pd)
    {
        int carried = Mathf.RoundToInt(pd.GetCurrentInventoryWeight());
        int capacity = Mathf.RoundToInt(pd.GetCarryCapacity());

        if (_label != null && EncumbranceSystem.Instance != null)
            _label.color = EncumbranceSystem.BandColor(EncumbranceSystem.Instance.CurrentBand);

        return $"{carried}/{capacity}";
    }

    private static string AlignmentLabel(int alignment)
    {
        if (alignment >= 5) return "Good";
        if (alignment <= -5) return "Evil";
        return "Neutral";
    }

    private static string CurrentTitle()
    {
        // Title system is data-driven but not yet wired to PlayerData.
        // Until then the sheet reads honestly rather than inventing a rank.
        return "Commoner";
    }

    private static string HomeName()
    {
        var home = HomeSettlementHandler.Instance != null
            ? HomeSettlementHandler.Instance.homeSettlement
            : null;

        return home != null && !string.IsNullOrEmpty(home.Name) ? home.Name : "—";
    }

    private static int CompanionSlots()
    {
        // Slots come from the title track; 1 until titles are hooked up.
        return 1;
    }
}
