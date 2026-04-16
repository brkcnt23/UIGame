using System.Collections.Generic;
using UnityEngine;

// Attach this to your Managers GameObject
public class ManagerHolder : MonoBehaviour
{
    [Tooltip("Managers to initialize in order. If empty, all IInitializable children will be auto-detected.")]
    public List<MonoBehaviour> orderedManagers = new List<MonoBehaviour>();

    [Tooltip("Persist across scenes.")]
    public bool dontDestroyOnLoad = true;

    private bool isInitialized;

    private void Awake()
    {
        if (dontDestroyOnLoad)
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        InitializeManagers();
    }

    public void InitializeManagers()
    {
        if (isInitialized) return;

        // If user provided explicit order, use it; otherwise scan children
        if (orderedManagers != null && orderedManagers.Count > 0)
        {
            foreach (var mb in orderedManagers)
            {
                TryInitialize(mb);
            }
        }
        else
        {
            var initializables = GetComponentsInChildren<MonoBehaviour>(true);
            foreach (var mb in initializables)
            {
                TryInitialize(mb);
            }
        }

        isInitialized = true;
    }

    private void TryInitialize(MonoBehaviour component)
    {
        if (component == null) return;

        var initializable = component as IInitializable;
        if (initializable != null)
        {
            initializable.Initialize();
        }
    }
}



