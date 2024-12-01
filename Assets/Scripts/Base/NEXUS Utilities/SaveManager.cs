using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }
    private string saveDirectory;
    private const string SlotPrefix = "SaveSlot_";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            saveDirectory = Application.persistentDataPath + "/Saves/";
            if (!Directory.Exists(saveDirectory))
            {
                Directory.CreateDirectory(saveDirectory);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveGame(int slot, SaveSlot saveSlot)
    {
        string filePath = saveDirectory + SlotPrefix + slot + ".json";
        string jsonData = JsonUtility.ToJson(saveSlot, true);
        File.WriteAllText(filePath, jsonData);
        Debug.Log($"Game saved to slot {slot}");
    }

    public SaveSlot LoadGame(int slot)
    {
        string filePath = saveDirectory + SlotPrefix + slot + ".json";

        if (File.Exists(filePath))
        {
            string jsonData = File.ReadAllText(filePath);
            return JsonUtility.FromJson<SaveSlot>(jsonData);
        }
        Debug.LogWarning($"No save file found in slot {slot}");
        return null;
    }

    public bool DoesSlotExist(int slot)
    {
        string filePath = saveDirectory + SlotPrefix + slot + ".json";
        return File.Exists(filePath);
    }
}
