using UnityEngine;

namespace NEXUS.Utilities
{
    using System.IO;
    public class JSONDataHandler
    {
        private string baseDirectory;

        public JSONDataHandler()
        {
            baseDirectory = Path.Combine(Application.dataPath, "Data/");
            // Ensure the directory exists
            if (!Directory.Exists(baseDirectory))
                Directory.CreateDirectory(baseDirectory);
        }

        // Generic method to save data
        public void SaveData<T>(T data, string fileName)
        {
            if (data == null)
            {
                Debug.LogError("Cannot save null data.");
                return;
            }

            string jsonFilePath = Path.Combine(baseDirectory, fileName);

            string jsonData = JsonUtility.ToJson(data, true);
            File.WriteAllText(jsonFilePath, jsonData);
            Debug.Log($"Data saved to {jsonFilePath}");
        }

        // Generic method to load data
        public T LoadData<T>(string fileName) where T : class
        {
            string jsonFilePath = Path.Combine(baseDirectory, fileName);

            if (!File.Exists(jsonFilePath))
            {
                Debug.LogError($"File not found: {jsonFilePath}");
                return null;
            }

            string jsonData = File.ReadAllText(jsonFilePath);
            T data = JsonUtility.FromJson<T>(jsonData);
            return data;
        }
    }
}