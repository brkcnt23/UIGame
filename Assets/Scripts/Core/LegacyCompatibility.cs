using UnityEngine;

/// <summary>
/// TEMPORARY: Backward compatibility layer for old singleton pattern.
/// This is NOT production code — just for migration period.
///
/// Once all systems are refactored to use GameBootstrapper pattern,
/// delete this file and all .Instance references.
/// </summary>

public static class LegacyCompat
{
    public static InventorySystem InventorySystemInstance { get; set; }
    public static InventoryUI InventoryUIInstance { get; set; }
}

/// <summary>
/// TEMPORARY: Fake Instance property on InventorySystem for backward compat.
/// This will be removed after refactoring.
/// </summary>
public partial class InventorySystem
{
    private static InventorySystem _instance;

    public static InventorySystem Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<InventorySystem>();
                if (_instance == null)
                    Debug.LogError("[LegacyCompat] InventorySystem.Instance is null. Register via GameBootstrapper.");
            }
            return _instance;
        }
    }

    private void OnEnable()
    {
        _instance = this;
    }
}

/// <summary>
/// TEMPORARY: Fake Instance property on InventoryUI for backward compat.
/// This will be removed after refactoring.
/// </summary>
public partial class InventoryUI
{
    private static InventoryUI _instance;

    public static InventoryUI Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<InventoryUI>();
                if (_instance == null)
                    Debug.LogError("[LegacyCompat] InventoryUI.Instance is null.");
            }
            return _instance;
        }
    }

    private void OnEnable_Legacy()
    {
        _instance = this;
    }
}
