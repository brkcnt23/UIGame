using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class TownHalls : Residentials
{
    public List<Job_SO_Constructor> Jobs;

    public TownHalls()
    {
        Jobs = new List<Job_SO_Constructor>();
    }

    public void AddJob(Job_SO_Constructor job)
    {
        Jobs.Add(job);
    }

    public void RemoveJob(Job_SO_Constructor job)
    {
        if (Jobs.Contains(job))
        {
            Jobs.Remove(job);
        }
    }

    public override void LevelUpResidential(ref PlayerData _Player)
    {
        base.LevelUpResidential(ref _Player);
        upgradeHour = CalculateUpgradeHour(_Player);
    }
}