using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Army
{
    public List<Unit> Units { get; private set; }
    
    private PlayerStatHandler PlayerStatHandler => PlayerStatHandler.Instance;
    
    public List<Unit> PlayerUnits
    {
        get
        {
            if (PlayerStatHandler?.pd == null)
            {
                Debug.LogWarning("PlayerStatHandler or player data is null!");
                return new List<Unit>();
            }
            return PlayerStatHandler.pd.Units;
        }
    }
    public Army()
    {
        Units = new List<Unit>();
    }

    public void SetUnits(List<Unit> units)
    {
        Units = units;
    }

    /// <summary>
    /// Adds a new unit or increases the count of existing units of the same type.
    /// </summary>
    /// <param name="unit">The unit to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when unit is null.</exception>
    public void AddUnit(Unit unit)
    {
        if (unit == null) throw new ArgumentNullException(nameof(unit));
        if (unit.Count <= 0) throw new ArgumentException("Unit count must be positive", nameof(unit));

        var existingUnit = Units.Find(u => u.Type == unit.Type);
        if (existingUnit != null)
        {
            existingUnit.Count += unit.Count;
        }
        else
        {
            Units.Add(unit);
        }
    }

    /// <summary>
    /// Returns the total count of all units.
    /// </summary>
    public int GetTotalUnits() => Units.Sum(unit => unit.Count);

    /// <summary>
    /// Removes a specific number of units of given type.
    /// </summary>
    /// <param name="type">Unit type to remove.</param>
    /// <param name="count">Number of units to remove.</param>
    /// <returns>True if units were removed, false if unit type wasn't found.</returns>
    public bool RemoveUnit(UnitType type, int count)
    {
        if (count <= 0) return false;

        var unit = Units.Find(u => u.Type == type);
        if (unit == null) return false;

        unit.Count = Mathf.Max(unit.Count - count, 0);
        if (unit.Count == 0)
        {
            Units.Remove(unit);
        }
        return true;
    }

    /// <summary>
    /// Displays all units in the army.
    /// </summary>
    /// <param name="armyName">Name of the army for display purposes.</param>
    public void DisplayUnits(string armyName)
    {
        if (string.IsNullOrEmpty(armyName))
            armyName = "Unnamed Army";

        Debug.Log($"{armyName} Units:");
        if (!Units.Any())
        {
            Debug.Log("No units in army.");
            return;
        }

        foreach (var unit in Units.OrderBy(u => u.Type))
        {
            Debug.Log($"{unit.Type}: {unit.Count}");
        }
    }

    /// <summary>
    /// Adds units to the player's army.
    /// </summary>
    /// <param name="type">Type of unit to add.</param>
    /// <param name="count">Number of units to add.</param>
    /// <returns>True if units were added successfully.</returns>
    public bool AddUnitToPlayerArmy(UnitType type, int count)
    {
        if (count <= 0) return false;
        if (PlayerUnits == null) return false;

        var existingUnit = PlayerUnits.Find(unit => unit.Type == type);
        if (existingUnit != null)
        {
            existingUnit.Count += count;
        }
        else
        {
            PlayerUnits.Add(new Unit() { Type = type, Count = count });
        }
        return true;
    }

    /// <summary>
    /// Removes units from the player's army.
    /// </summary>
    /// <param name="type">Type of unit to remove.</param>
    /// <param name="count">Number of units to remove.</param>
    /// <returns>True if units were removed successfully.</returns>
    public bool RemoveUnitFromPlayerArmy(UnitType type, int count)
    {
        if (count <= 0) return false;
        if (PlayerUnits == null) return false;

        var unit = PlayerUnits.Find(u => u.Type == type);
        if (unit == null) return false;

        unit.Count = Mathf.Max(0, unit.Count - count);
        if (unit.Count == 0)
        {
            PlayerUnits.Remove(unit);
        }
        return true;
    }
}