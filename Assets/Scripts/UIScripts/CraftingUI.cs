using UnityEngine;
using UnityEngine.UI;

public class CraftingUI : MonoBehaviour
{
    public Button StartSmithingButton;
    public Button StartTanningButton;
    public Button StartAlchemyButton;
    void Start()
    {
        StartSmithingButton.onClick.AddListener(StartSmithing);
        StartTanningButton.onClick.AddListener(StartTanning);
        StartAlchemyButton.onClick.AddListener(StartAlchemy);
    }

    public void StartSmithing()
    {
        CraftingSystem.Instance.WorkAsBlacksmith(1, "weapon");
    }
    public void StartTanning()
    {
        CraftingSystem.Instance.WorkAsTanner(1);
    }
    public void StartAlchemy()
    {
        CraftingSystem.Instance.WorkAsAlchemist(1);
    }
}
