using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class FileDataHandler
{
    // Directory Path
    private string dataDirPath;
    // File Name
    private string dataFileName;

    public FileDataHandler(string dataDirPath, string dataFileName) {
        this.dataDirPath = dataDirPath;
        this.dataFileName = dataFileName;
    }

    public GameData Load() {
        // Path.Combine will account for different Operating Systems having different path separators.
        string fullPath = Path.Combine(dataDirPath, dataFileName);
        GameData loadedData = null;
        if (File.Exists(fullPath)) {
            try {
                // Load the serialized data from the file
                string dataToLoad = "";
                using (FileStream stream = new FileStream(fullPath, FileMode.Open)) {
                    using (StreamReader reader = new StreamReader(stream)) {
                        dataToLoad = reader.ReadToEnd();
                    }
                }

                // Deserialize the data from JSON back into C#
                loadedData = JsonUtility.FromJson<GameData>(dataToLoad);

            } catch (Exception e) {
                Debug.LogError("Error occurred when trying to load data from " + fullPath + " -> " + e);
            }
        }
        return loadedData;
    }

    public void Save(GameData data) {
        // Path.Combine will account for different Operating Systems having different path separators.
        string fullPath = Path.Combine(dataDirPath, dataFileName);
        try {
            // Create the directory the file will be written to if it doesn't already exist.
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            // serialize the c# game data object into JSON
            string dataToStore = JsonUtility.ToJson(data, false); //true -> format the JSON data.

            // write the serialized data to the file
            using (FileStream stream = new FileStream(fullPath, FileMode.Create)) {
                using (StreamWriter writer = new StreamWriter(stream)) {
                    writer.Write(dataToStore);
                }
            }
            
        } catch (Exception e) {
            Debug.LogError("Error occurred when trying to save data to file: " + fullPath + " -> " + e);
        }
    }
}
