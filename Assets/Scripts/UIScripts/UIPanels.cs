using System.Collections.Generic;
using UnityEngine;

public class UIPanels : MonoBehaviour
{
    public List<GameObject> Panels = new List<GameObject>();

    public void Back() {
        DisableAllPanels();
    }

    public void EnablePanel(GameObject panel) {
        gameObject.SetActive(true);
        DisableAllPanels();
        panel.SetActive(true);
    }

    public void DisableAllPanels() {
        foreach (GameObject p in Panels) {
            p.SetActive(false);
        }
    }
}
