using UnityEngine;

namespace NEXUS.Utilities
{
    using System.Collections.Generic;
    using System.IO;

    public class JSONDataHandler
    {
        private string baseDirectory;

        // Directories that are only searched when a file is missing in baseDirectory.
        // Used to keep older saves (which lived under Assets/) readable.
        private readonly List<string> fallbackDirectories = new List<string>();

        /// <summary>
        /// Save slot directory. Slots live in persistentDataPath so they survive
        /// builds and never end up inside the Assets folder.
        /// </summary>
        public static string GetSlotDirectory(int slot)
        {
            return Path.Combine(Application.persistentDataPath, "SaveSlot" + slot);
        }

        /// <summary>
        /// Where save slots used to be written (Assets/SaveSlotX).
        /// Kept for reading old saves only.
        /// </summary>
        public static string GetLegacySlotDirectory(int slot)
        {
            return Path.Combine(Application.dataPath, "SaveSlot" + slot);
        }

        public JSONDataHandler(int slot)
        {
            baseDirectory = GetSlotDirectory(slot);
            fallbackDirectories.Add(GetLegacySlotDirectory(slot));

            EnsureBaseDirectory();
        }

        /// <summary>
        /// Read-only source data (Assets/SourceData in the editor,
        /// StreamingAssets in a build).
        /// </summary>
        public JSONDataHandler(string folderName)
        {
            baseDirectory = Path.Combine(Application.dataPath, folderName);
            fallbackDirectories.Add(Path.Combine(Application.streamingAssetsPath, folderName));
            fallbackDirectories.Add(Path.Combine(Application.persistentDataPath, folderName));

            EnsureBaseDirectory();
        }

        private void EnsureBaseDirectory()
        {
            try
            {
                if (!Directory.Exists(baseDirectory))
                    Directory.CreateDirectory(baseDirectory);
            }
            catch (IOException e)
            {
                Debug.LogError($"Could not create data directory '{baseDirectory}': {e.Message}");
            }
        }

        // Generic method to save data
        public void SaveData<T>(T data, string fileName)
        {
            if (data == null)
            {
                Debug.LogError("Cannot save null data.");
                return;
            }

            EnsureBaseDirectory();

            string jsonFilePath = Path.Combine(baseDirectory, fileName);

            string jsonData = JsonUtility.ToJson(data, true);
            File.WriteAllText(jsonFilePath, jsonData);
            Debug.Log($"Data saved to {jsonFilePath}");
        }

        // Generic method to load data
        public T LoadData<T>(string fileName) where T : class
        {
            string jsonFilePath = ResolveFilePath(fileName);

            if (jsonFilePath == null)
            {
                Debug.LogWarning($"File not found: {Path.Combine(baseDirectory, fileName)}");
                return null;
            }

            string jsonData = File.ReadAllText(jsonFilePath);

            if (string.IsNullOrWhiteSpace(jsonData))
            {
                Debug.LogWarning($"File is empty: {jsonFilePath}");
                return null;
            }

            try
            {
                return JsonUtility.FromJson<T>(jsonData);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Could not parse '{jsonFilePath}': {e.Message}");
                return null;
            }
        }

        private string ResolveFilePath(string fileName)
        {
            string path = Path.Combine(baseDirectory, fileName);

            if (File.Exists(path))
                return path;

            foreach (string directory in fallbackDirectories)
            {
                string fallbackPath = Path.Combine(directory, fileName);

                if (File.Exists(fallbackPath))
                    return fallbackPath;
            }

            return null;
        }
    }
}
