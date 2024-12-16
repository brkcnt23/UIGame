using System.Collections.Generic;
[System.Serializable]
public class Taverns : Residentials
{
    public List<Quest_SO_Constructor> Quests;

    public List<Companion> Companions;

    public Taverns()
    {
        Quests = new List<Quest_SO_Constructor>();
        Companions = new List<Companion>();
    }

    public void AddQuest(Quest_SO_Constructor quest)
    {
        Quests.Add(quest);
    }

    public void RemoveQuest(Quest_SO_Constructor quest)
    {
        if (Quests.Contains(quest))
        {
            Quests.Remove(quest);
        }
    }

    public void AddCompanion(Companion companion)
    {
        Companions.Add(companion);
    }

    public void RemoveCompanion(Companion companion)
    {
        if (Companions.Contains(companion))
        {
            Companions.Remove(companion);
        }
    }

    public override void LevelUpResidential(ref PlayerData _Player)
    {
        base.LevelUpResidential(ref _Player);
        upgradeHour = CalculateUpgradeHour(_Player);

        
    }
}