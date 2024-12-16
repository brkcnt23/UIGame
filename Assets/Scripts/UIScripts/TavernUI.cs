using UnityEngine;
using UnityEngine.UI;

public class TavernUI : MonoBehaviour
{
    public Button SleepButton;
    public Button EatButton;

    private void Start(){
        SleepButton.onClick.AddListener(SleepInTavern);
        EatButton.onClick.AddListener(EatInTavern);
    }
    public void SleepInTavern(){
        PlayerStatHandler.Instance.ConsumeMoney(2,0);
        Debug.Log("you eat and sleep for 2 golds");
        TimeSystem.Instance.SleepTavern();
        PlayerUISystem.Instance.UpdateGoldandSilverText();
    }

    public void EatInTavern(){
        PlayerStatHandler.Instance.ConsumeMoney(1,0);
        Debug.Log("you eat for 1 golds");
        TimeSystem.Instance.UpdateLastMealTime();
        PlayerUISystem.Instance.UpdateGoldandSilverText();
    }
}