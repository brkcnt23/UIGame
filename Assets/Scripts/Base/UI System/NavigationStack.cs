using UnityEngine;
using System.Collections.Generic;

namespace UISystem
{
    public sealed class NavigationStack : MonoBehaviour
{
    private Stack<GameObject> _panelStack = new();
    private static NavigationStack _instance;

    public static NavigationStack Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindFirstObjectByType<NavigationStack>();
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Destroy(gameObject);
    }

    public void OpenPanel(GameObject panel)
    {
        if (panel == null) return;

        if (_panelStack.Count > 0)
            _panelStack.Peek().SetActive(false);

        _panelStack.Push(panel);
        panel.SetActive(true);
        Debug.Log($"[NavStack] Opened {panel.name}");
    }

    public void Back()
    {
        if (_panelStack.Count == 0) return;

        _panelStack.Pop().SetActive(false);

        if (_panelStack.Count > 0)
            _panelStack.Peek().SetActive(true);

        Debug.Log("[NavStack] Back");
    }

    public void ClearStack()
    {
        while (_panelStack.Count > 0)
            _panelStack.Pop().SetActive(false);
    }

    public int StackDepth => _panelStack.Count;
}
}