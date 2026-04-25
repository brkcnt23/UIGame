using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Global panel navigation with stack-based back button.
/// Maintains history of opened panels, auto-hides previous ones.
/// </summary>
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

    /// <summary>
    /// Open a panel. Hides previous panel if any.
    /// </summary>
    public void OpenPanel(GameObject panel)
    {
        if (panel == null)
        {
            Debug.LogError("[NavigationStack] Null panel");
            return;
        }

        // Hide current panel
        if (_panelStack.Count > 0)
        {
            var current = _panelStack.Peek();
            if (current != panel)
                current.SetActive(false);
        }

        _panelStack.Push(panel);
        panel.SetActive(true);

        Debug.Log($"[NavigationStack] Opened {panel.name}, depth: {_panelStack.Count}");
    }

    /// <summary>
    /// Go back to previous panel. (Instance method wrapper for button onClick)
    /// </summary>
    public void Back()
    {
        if (_panelStack.Count == 0)
        {
            Debug.LogWarning("[NavigationStack] No panels to go back to");
            return;
        }

        var current = _panelStack.Pop();
        current.SetActive(false);

        if (_panelStack.Count > 0)
        {
            var previous = _panelStack.Peek();
            previous.SetActive(true);
            Debug.Log($"[NavigationStack] Went back to {previous.name}");
        }
        else
        {
            Debug.Log("[NavigationStack] Stack empty");
        }
    }

    /// <summary>
    /// Static method for back button. Delegates to singleton instance.
    /// </summary>
    public static void GoBack()
    {
        if (Instance != null)
            Instance.Back();
    }

    /// <summary>
    /// Close all panels and return to root.
    /// </summary>
    public void ClearStack()
    {
        while (_panelStack.Count > 0)
            _panelStack.Pop().SetActive(false);

        Debug.Log("[NavigationStack] Stack cleared");
    }

    public int StackDepth => _panelStack.Count;
}
