using UnityEngine;

public class PlayerStatHandler : MonoBehaviour
{

}
[System.Serializable]
public class PlayerData
{
    public int ID;
    public string Name;
    public int Level;
    public int Health;
    public int MaxHealth;
    public int Experience;
    public int MaxExperience;
    public int Strength;
    public int Dexterity;
    public int Constitution;
    public int Charisma;
    public int Rations;
    public int MaxExhaustion;
    public int CurrentExhaustion;

    public PlayerData(int saveSlot)
    {
        //Read the json file from the resources folder
        string jsonData = System.IO.File.ReadAllText(Application.dataPath + "/Base/PlayerData/SaveSlot_" + saveSlot + ".json");
        //Convert the json to player data
        JsonUtility.FromJsonOverwrite(jsonData, this);
    }

    public PlayerData(PlayerSO playerSO)
    {
        ID = playerSO.ID;
        Name = playerSO.Name;
        Level = playerSO.Level;
        Health = playerSO.CurrentHealth;
        MaxHealth = playerSO.MaxHealth;
        Experience = playerSO.CurrentExperience;
        MaxExperience = playerSO.MaxExperience;
        Strength = playerSO.Strength;
        Dexterity = playerSO.Dexterity;
        Constitution = playerSO.Constitution;
        Charisma = playerSO.Charisma;
        Rations = playerSO.Rations;
        MaxExhaustion = playerSO.Maxexhaustion;
        CurrentExhaustion = playerSO.Currentexhaustion;
    }

    //We will read/write this data to a json and we will handle those in here

    //We will create a method to save the data
    public void SaveData(int saveSlot)
    {
        // Convert the player data to a json
        string jsonData = JsonUtility.ToJson(this);
        //Write the json to Resources folder
        System.IO.File.WriteAllText(Application.dataPath + "/Base/PlayerData/SaveSlot_" + saveSlot + ".json", jsonData);
    }
}

