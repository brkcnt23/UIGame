using UnityEngine;

public class ActivitiesUI : MonoBehaviour
{
    [Header("Selection")]
    public GameObject selectionPanel;

    [Header("Root Panels")]
    public GameObject tavernPanel;
    public GameObject townHallMainPanel;
    public GameObject craftingMainPanel;
    public GameObject jobsLogPanel;
    private GameObject currentPanel;

    private void OnEnable()
    {
        ShowSelection();
    }

    public void ShowSelection()
    {
        if (selectionPanel != null) selectionPanel.SetActive(true);
        if (tavernPanel != null) tavernPanel.SetActive(false);
        if (townHallMainPanel != null) townHallMainPanel.SetActive(false);
        if (craftingMainPanel != null) craftingMainPanel.SetActive(false);

        currentPanel = selectionPanel;
    }

    public void ShowTavern()
    {
        if (selectionPanel != null) selectionPanel.SetActive(false);
        if (tavernPanel != null) tavernPanel.SetActive(true);
        if (townHallMainPanel != null) townHallMainPanel.SetActive(false);
        if (craftingMainPanel != null) craftingMainPanel.SetActive(false);

        currentPanel = tavernPanel;
    }

    public void ShowTownHall()
    {
        if (selectionPanel != null) selectionPanel.SetActive(false);
        if (tavernPanel != null) tavernPanel.SetActive(false);
        if (townHallMainPanel != null) townHallMainPanel.SetActive(true);
        if (craftingMainPanel != null) craftingMainPanel.SetActive(false);
        if (jobsLogPanel != null) jobsLogPanel.SetActive(true);
        currentPanel = townHallMainPanel;
    }

    public void ShowCrafting()
    {
        if (selectionPanel != null) selectionPanel.SetActive(false);
        if (tavernPanel != null) tavernPanel.SetActive(false);
        if (townHallMainPanel != null) townHallMainPanel.SetActive(false);
        if (craftingMainPanel != null) craftingMainPanel.SetActive(true);

        currentPanel = craftingMainPanel;

        CraftingUI craftingUI = craftingPanelOrChild();
        if (craftingUI != null)
        {
            craftingUI.UpdateSettlementShopLevels();
            craftingUI.RefreshUI();
        }
    }

    public void BackToSelection()
    {
        ShowSelection();
    }

    public GameObject GetCurrentPanel()
    {
        return currentPanel;
    }

    private CraftingUI craftingPanelOrChild()
    {
        if (craftingMainPanel == null) return null;
        return craftingMainPanel.GetComponentInChildren<CraftingUI>(true);
    }
}