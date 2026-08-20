using UnityEngine;

namespace NEXUS.Utilities
{
    using System.IO;

    /// <summary>
    /// Save/load for both save slots and read-only content data.
    ///
    /// Paths matter on mobile:
    ///   - Application.dataPath is the APK/Assets folder — read-only on device.
    ///     The old version wrote saves there; it worked in the Editor and would
    ///     have silently failed on every phone.
    ///   - Application.persistentDataPath is the writable per-app folder on
    ///     every platform. All writes go there now.
    ///
    /// Reads resolve in three tiers:
    ///   1. persistentDataPath          — saves, and any content the game patched
    ///   2. Assets/&lt;dir&gt; (Editor only)  — keeps the current Editor workflow
    ///   3. Resources/&lt;dir&gt;/&lt;file&gt;      — content shipped inside the build
    ///
    /// So "SourceData" content lives in Assets/Resources/SourceData and loads
    /// on device via tier 3, while save slots live in persistentDataPath.
    /// </summary>
    public class JSONDataHandler
    {
        private readonly string directoryName;
        private readonly string writeDirectory;

        public JSONDataHandler(int slot) : this("SaveSlot" + slot) { }

        /// <summary>
        /// Every folder a save slot can live in, newest location first.
        /// Deleting a slot has to clear all of them, otherwise a legacy save in
        /// Assets keeps coming back through tier 2.
        /// </summary>
        public static string[] GetSlotDirectories(int slot)
        {
            string folder = "SaveSlot" + slot;

            return new[]
            {
                Path.Combine(Application.persistentDataPath, folder),
                Path.Combine(Application.dataPath, folder)
            };
        }

        public JSONDataHandler(string directory)
        {
            directoryName = directory;
            writeDirectory = Path.Combine(Application.persistentDataPath, directory);

            if (!Directory.Exists(writeDirectory))
                Directory.CreateDirectory(writeDirectory);
        }

        public void SaveData<T>(T data, string fileName)
        {
            if (data == null)
            {
                Debug.LogError("Cannot save null data.");
                return;
            }

            string jsonFilePath = Path.Combine(writeDirectory, fileName);

            string jsonData = JsonUtility.ToJson(data, true);
            File.WriteAllText(jsonFilePath, jsonData);
            Debug.Log($"Data saved to {jsonFilePath}");
        }

        public T LoadData<T>(string fileName) where T : class
        {
            // Tier 1: writable folder (saves, patched content)
            string persistentPath = Path.Combine(writeDirectory, fileName);
            if (File.Exists(persistentPath))
                return JsonUtility.FromJson<T>(File.ReadAllText(persistentPath));

#if UNITY_EDITOR
            // Tier 2: legacy Assets folder, Editor only. Existing test saves in
            // Assets/SaveSlotX keep loading; on the next save they migrate to
            // persistentDataPath via tier 1.
            string legacyPath = Path.Combine(Application.dataPath, directoryName, fileName);
            if (File.Exists(legacyPath))
                return JsonUtility.FromJson<T>(File.ReadAllText(legacyPath));
#endif

            // Tier 3: content bundled into the build.
            // Resources.Load takes the path without extension.
            string resourcePath = directoryName + "/" + Path.GetFileNameWithoutExtension(fileName);
            TextAsset asset = Resources.Load<TextAsset>(resourcePath);
            if (asset != null)
                return JsonUtility.FromJson<T>(asset.text);

            Debug.LogWarning($"File not found in any tier: {directoryName}/{fileName}");
            return null;
        }

        /// <summary>True if the file exists in any tier.</summary>
        public bool Exists(string fileName)
        {
            if (File.Exists(Path.Combine(writeDirectory, fileName)))
                return true;

#if UNITY_EDITOR
            if (File.Exists(Path.Combine(Application.dataPath, directoryName, fileName)))
                return true;
#endif

            string resourcePath = directoryName + "/" + Path.GetFileNameWithoutExtension(fileName);
            return Resources.Load<TextAsset>(resourcePath) != null;
        }

        /// <summary>Deletes a file from the writable tier only.</summary>
        public void Delete(string fileName)
        {
            string path = Path.Combine(writeDirectory, fileName);
            if (File.Exists(path))
                File.Delete(path);
        }
    }
}
