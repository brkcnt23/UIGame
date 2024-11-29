using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Army
{
    public List<Unit> Units { get; private set; }

    public Army()
    {
        Units = new List<Unit>();
    }

    /// <summary>
    /// Yeni bir birlik ekler. Eğer aynı türden bir birlik varsa sayısını artırır.
    /// </summary>
    /// <param name="unit">Eklemek istediğiniz birlik.</param>
    public void AddUnit(Unit unit)
    {
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
    /// Toplam birlik sayısını döndürür.
    /// </summary>
    /// <returns>Toplam birlik sayısı.</returns>
    public int GetTotalUnits()
    {
        return Units.Sum(unit => unit.Count);
    }

    /// <summary>
    /// Belirli bir türdeki birliklerden belirli sayıda azaltır.
    /// </summary>
    /// <param name="type">Birlik türü.</param>
    /// <param name="count">Azaltılacak sayıda birlik.</param>
    public void RemoveUnit(UnitType type, int count)
    {
        var unit = Units.Find(u => u.Type == type);
        if (unit != null)
        {
            unit.Count = Mathf.Max(unit.Count - count, 0);
            if (unit.Count == 0)
            {
                Units.Remove(unit);
            }
        }
    }

    /// <summary>
    /// Ordunun birliklerini konsola yazdırır.
    /// </summary>
    /// <param name="armyName">Ordunun adı.</param>
    public void DisplayUnits(string armyName)
    {
        Debug.Log($"{armyName} Birlikleri:");
        foreach (var unit in Units)
        {
            Debug.Log($"{unit.Type}: {unit.Count}");
        }
    }
}
