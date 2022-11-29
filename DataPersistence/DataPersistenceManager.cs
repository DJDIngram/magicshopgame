using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

//Singleton Class
public class DataPersistenceManager : MonoBehaviour
{
    [Header("File Storage Config")]
    [SerializeField] private string fileName;

    // Our Game Data Class.
    private GameData gameData;
    // All our Monobehaviour scripts that require access to saving/loading  
    //      will inherit from IDataPersistence, and will be saved here:
    private List<IDataPersistence> dataPersistenceObjects;
    
    private FileDataHandler dataHandler;

    public static DataPersistenceManager instance { get; private set; }

    private void Awake() {
        // There should only ever be one instance of the DataPersistenceManager.
        if (instance != null) {
            Debug.LogError("Found more than one DataPersistenceManager");
        }
        instance = this;
    }

    private void Start() {
        this.dataHandler = new FileDataHandler(Application.persistentDataPath, fileName);
        this.dataPersistenceObjects = FindAllDataPersistenceObjects();
        LoadGame();
    }

    public void NewGame() {
        this.gameData = new GameData();
    }

    public void LoadGame() {
        // Load any saved data from a file using the data handler
        this.gameData = dataHandler.Load();
        //      If no data can be Loaded, initialize to a new game.
        if (this.gameData == null) {
            Debug.Log("No data was found. Initializing data to default (NewGame)");
            NewGame();
        }

        // Push the loaded data to all other scripts that need it.
        foreach (IDataPersistence dataPersistenceObject in dataPersistenceObjects) {
            dataPersistenceObject.LoadData(gameData);
        }
    }

    public void SaveGame() {
        // Pass the data to other scripts so they can update it
        foreach (IDataPersistence dataPersistenceObject in dataPersistenceObjects) {
            dataPersistenceObject.SaveData(gameData);
        }
        // TODO - save the data to a file using the data handler
        dataHandler.Save(gameData);
    }

    private List<IDataPersistence> FindAllDataPersistenceObjects() {
        // NOTE THAT THIS ONLY WORKS WITH MONOBEHAVIOUR SCRIPTS!
        IEnumerable<IDataPersistence> dataPersistenceObjects = FindObjectsOfType<MonoBehaviour>()
            .OfType<IDataPersistence>();
        return new List<IDataPersistence>(dataPersistenceObjects);
    }

}
