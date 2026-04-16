using System.Collections.Generic;

[System.Serializable]
public class TownHalls : Residentials
{
    public string TownHallId;
    public string RulerNpcId;
    public string StewardNpcId;
    public List<string> TownHallTags = new();

    public List<Job_SO_Constructor> Jobs = new();

    public TownHalls()
    {
        TownHallId = string.Empty;
        RulerNpcId = string.Empty;
        StewardNpcId = string.Empty;
        TownHallTags = new List<string>();
        Jobs = new List<Job_SO_Constructor>();
    }
}