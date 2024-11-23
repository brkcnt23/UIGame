using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class JSONDataHandler
{
    private string baseDirectory;
    private string settlementsFileName = "settlements.json";

    public JSONDataHandler()
    {
        baseDirectory = Path.Combine(Application.dataPath, "Scripts/Base/JSON Data/");
    }

    // Saves a list of settlement objects
    public void SaveSettlements(List<Settlement> settlements)
    {
        if (settlements == null || settlements.Count == 0)
        {
            Debug.LogError("Cannot save null or empty settlements list.");
            return;
        }

        string jsonFilePath = Path.Combine(baseDirectory, settlementsFileName);

        // Wrap the list in a serializable class
        SettlementListWrapper wrapper = new SettlementListWrapper { settlements = settlements };

        string jsonData = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(jsonFilePath, jsonData);
        Debug.Log($"Settlements saved to {jsonFilePath}");
    }

    // Loads a list of settlement objects
    public List<Settlement> LoadSettlements()
    {
        string jsonFilePath = Path.Combine(baseDirectory, settlementsFileName);

        if (!File.Exists(jsonFilePath))
        {
            Debug.LogError($"File not found: {jsonFilePath}");
            return new List<Settlement>();
        }

        string jsonData = File.ReadAllText(jsonFilePath);
        SettlementListWrapper wrapper = JsonUtility.FromJson<SettlementListWrapper>(jsonData);
        return wrapper.settlements;
    }

    // Wrapper class for serialization
    [System.Serializable]
    private class SettlementListWrapper
    {
        public List<Settlement> settlements;
    }
}