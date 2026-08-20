using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Single owner of panel visibility.
///
/// Replaces the scattered SetActive calls and the two competing NavigationStack
/// classes. One place decides what is on screen, one back stack, one refresh path.
///
/// Panels register themselves; the router never hard-codes a panel list.
/// </summary>
public sealed class UIRouter : GameSystemBase, IStateListener
{
    public override int Priority => SystemPriority.UIRouter;

    public static UIRouter Instance { get; private set; }

    [Tooltip("Panel opened when the router starts. Leave empty to open nothing.")]
    [SerializeField] private string initialPanelId;

    [Tooltip("Log every navigation. Useful while migrating away from SetActive.")]
    [SerializeField] private bool verbose;

    private readonly Dictionary<string, UIPanelBase> _panels = new();
    private readonly Stack<string> _backStack = new();

    public UIPanelBase CurrentPanel { get; private set; }
    public string CurrentPanelId => CurrentPanel != null ? CurrentPanel.PanelId : null;

    protected override void OnInitialize()
    {
        Instance = this;

        RegisterPanelsInChildren();
        State?.Subscribe(this);

        if (!string.IsNullOrEmpty(initialPanelId))
            Open(initialPanelId);
    }

    protected override void OnShutdown()
    {
        State?.Unsubscribe(this);
        if (Instance == this)
            Instance = null;
    }

    // -----------------------------------------------------------------
    // Registration
    // -----------------------------------------------------------------

    /// <summary>
    /// Finds every UIPanelBase under this object, including inactive ones,
    /// and registers it. Panels start closed.
    /// </summary>
    private void RegisterPanelsInChildren()
    {
        var found = GetComponentsInChildren<UIPanelBase>(true);

        foreach (var panel in found)
            Register(panel);

        Log($"Registered {_panels.Count} panels.");
    }

    public void Register(UIPanelBase panel)
    {
        if (panel == null)
            return;

        var id = panel.PanelId;

        if (_panels.ContainsKey(id))
        {
            LogWarning($"Duplicate panel id '{id}' on {panel.name}. Ignored.");
            return;
        }

        _panels[id] = panel;
        panel.Bind(Events, State);
        panel.Close();
    }

    // -----------------------------------------------------------------
    // Navigation
    // -----------------------------------------------------------------

    public void Open(string panelId)
    {
        if (!_panels.TryGetValue(panelId, out var next))
        {
            LogWarning($"No panel registered with id '{panelId}'.");
            return;
        }

        if (CurrentPanel == next)
        {
            next.Refresh();
            return;
        }

        if (CurrentPanel != null)
        {
            if (next.PushToBackStack)
                _backStack.Push(CurrentPanel.PanelId);

            CurrentPanel.Close();
        }

        CurrentPanel = next;
        next.Open();

        if (verbose)
            Log($"Open '{panelId}' (back stack: {_backStack.Count})");
    }

    /// <summary>Opens a panel without recording history — use for tab switches.</summary>
    public void Replace(string panelId)
    {
        if (!_panels.TryGetValue(panelId, out var next))
        {
            LogWarning($"No panel registered with id '{panelId}'.");
            return;
        }

        CurrentPanel?.Close();
        CurrentPanel = next;
        next.Open();
    }

    public void Back()
    {
        if (_backStack.Count == 0)
        {
            if (verbose)
                Log("Back stack empty.");
            return;
        }

        var previousId = _backStack.Pop();

        if (!_panels.TryGetValue(previousId, out var previous))
        {
            LogWarning($"Back target '{previousId}' is gone. Skipping.");
            Back();
            return;
        }

        CurrentPanel?.Close();
        CurrentPanel = previous;
        previous.Open();

        if (verbose)
            Log($"Back to '{previousId}' (back stack: {_backStack.Count})");
    }

    public void CloseAll()
    {
        foreach (var panel in _panels.Values)
            panel.Close();

        _backStack.Clear();
        CurrentPanel = null;
    }

    public void ClearHistory() => _backStack.Clear();

    public bool IsRegistered(string panelId) => _panels.ContainsKey(panelId);

    public T GetPanel<T>(string panelId) where T : UIPanelBase
    {
        return _panels.TryGetValue(panelId, out var panel) ? panel as T : null;
    }

    // -----------------------------------------------------------------
    // State reaction
    // -----------------------------------------------------------------

    /// <summary>
    /// Only the visible panel redraws. Hidden panels refresh when they open,
    /// so there is no cost in keeping twenty of them registered.
    /// </summary>
    public void OnStateChanged(GameState oldState, GameState newState)
    {
        if (CurrentPanel != null && CurrentPanel.IsOpen)
            CurrentPanel.Refresh();
    }
}
