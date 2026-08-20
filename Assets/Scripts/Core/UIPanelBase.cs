using UnityEngine;

/// <summary>
/// Base class for every UI panel.
///
/// Panels never turn each other on and off. They register with UIRouter and the
/// router decides what is visible. That is what replaces the ~144 scattered
/// SetActive calls currently spread across the UI scripts.
///
/// A panel reads state and draws it. It does not own state.
/// </summary>
public abstract class UIPanelBase : MonoBehaviour
{
    [Tooltip("Unique id used by UIRouter.Open(). Keep it stable — saves and deep links use it.")]
    [SerializeField] private string panelId;

    [Tooltip("If true, opening this panel pushes the previous one onto the back stack.")]
    [SerializeField] private bool pushToBackStack = true;

    [Tooltip("Root object toggled on open/close. Defaults to this GameObject.")]
    [SerializeField] private GameObject content;

    public string PanelId => string.IsNullOrEmpty(panelId) ? GetType().Name : panelId;
    public bool PushToBackStack => pushToBackStack;
    public bool IsOpen { get; private set; }

    protected StateManager State { get; private set; }
    protected EventDispatcher Events { get; private set; }

    private GameObject Root => content != null ? content : gameObject;

    /// <summary>Called by UIRouter during registration.</summary>
    public void Bind(EventDispatcher events, StateManager state)
    {
        Events = events;
        State = state;
        OnBind();
    }

    public void Open()
    {
        if (IsOpen)
        {
            Refresh();
            return;
        }

        Root.SetActive(true);
        IsOpen = true;
        OnOpen();
        Refresh();
    }

    public void Close()
    {
        if (!IsOpen)
            return;

        OnClose();
        IsOpen = false;
        Root.SetActive(false);
    }

    /// <summary>
    /// Redraw from current state. Called on open and whenever state changes
    /// while this panel is visible. Must be safe to call repeatedly.
    /// </summary>
    public abstract void Refresh();

    /// <summary>Wire up button listeners and cache references here.</summary>
    protected virtual void OnBind() { }

    protected virtual void OnOpen() { }
    protected virtual void OnClose() { }

    /// <summary>Convenience for back buttons wired in the Inspector.</summary>
    public void Back()
    {
        UIRouter.Instance?.Back();
    }
}
