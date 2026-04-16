using System.Collections.Generic;
[System.Serializable]
public class Taverns : Residentials
{
    public string TavernId;
    public string OwnerNpcId;
    public List<string> TavernTags = new();

    public List<Quest_SO_Constructor> Quests = new();
    public List<Companion> Companions = new();

    public Taverns()
    {
        TavernId = string.Empty;
        OwnerNpcId = string.Empty;
        TavernTags = new List<string>();
        Quests = new List<Quest_SO_Constructor>();
        Companions = new List<Companion>();
    }
}