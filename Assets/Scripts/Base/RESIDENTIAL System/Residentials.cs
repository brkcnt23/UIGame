using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public class Residentials
{
    public string Name;
    public int level;
    public int maxLevel;
    public int cost;
    public int income;
    public List<string> Resources;

    public int upgradeHour;
    
    public virtual void LevelUpResidential(ref PlayerData _Player)
    {
        if (level < maxLevel)
        {
            for (int i = 0; i < Resources.Count; i++)
            {
                if (Resources[i] == _Player.Items[i].Name)
                {
                    if (_Player.Items[i].Quantity >= RequiredResourceAmount())
                    {
                        _Player.Items[i].Quantity -= RequiredResourceAmount();
                    }
                    else
                    {
                        Debug.Log("Not enough resources");
                    }
                }
                else
                {
                    Debug.Log("Resource not found");
                }
            }
            level++;
        }
        else
        {
            Debug.Log("Max level reached");
        }
    }

    public int RequiredResourceAmount()
    {
        int baseAmount = 50;
        int requiredAmount = baseAmount * level;

        return requiredAmount;
    }

    public int CalculateUpgradeHour(PlayerData _Player)
    {
        int baseHour = 24;
        int skillLevels = _Player.CarpenterSkillLevel + _Player.MasonSkillLevel;
        int requiredHour = baseHour * level - (skillLevels * baseHour / 3);

        return math.max(baseHour,requiredHour);
    }

    public void ChangeIncome(int _income)
    {
        income = _income;
    }

    public void ChangeCost(int _cost)
    {
        cost = _cost;
    }

    public void ChangeMaxLevel(int _maxLevel)
    {
        maxLevel = _maxLevel;
    }
}