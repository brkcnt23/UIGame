using Unity;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class UIHandler : MonoBehaviour
{
    public static UIHandler Instance { get; private set; }

    [Header("Settlement")]
    public TMP_Text SettlementName;
    public TMP_Text SettlementDescription;


    public void Awake()
    {
        // Check if instance already exists
        if (Instance == null)
        {
            // If not, set instance to this
            Instance = this;
        }
        else
        {
            // If instance already exists, destroy this
            Destroy(gameObject);
        }
    }

    public void UpdateSettlementInfo(Settlement settlement)
    {
        SettlementName.text = settlement.Name;
        SettlementDescription.text = $"Population: {settlement.Population}\nQuality:{settlement.Quality}\nWealth:{settlement.Wealth}";
    }
}