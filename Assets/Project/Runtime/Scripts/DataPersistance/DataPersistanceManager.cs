using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class DataPersistanceManager : MonoBehaviour {
    [Header("File Storage Config")]
    [SerializeField] private string fileName;

    public static DataPersistanceManager instance { get; private set; }
    private List<IDataPersistance> DataPersistanceObjects;
    private FileDataHandler dataHandler;

    private GameData gameData;

    private void Awake() {
        if (instance != null) {
            Debug.LogError("Found more than one Data Persistance Manager in the scene.");
        }
        instance = this;
    }

    private void Start() {
        this.dataHandler = new FileDataHandler(Application.persistentDataPath, fileName);
        this.DataPersistanceObjects = FindAllDataPersistanceObjects();
        LoadGame();
    }

    public void NewGame() {
        this.gameData = new GameData();
    }

    public void LoadGame() {
        this.gameData = dataHandler.Load();

        if (this.gameData == null) {
            Debug.Log("No data was found.");
            NewGame();
        }

        foreach (IDataPersistance dataPersistanceObj in DataPersistanceObjects) {
            dataPersistanceObj.LoadData(gameData);
        }
    }
    
    public void SaveGame() {
        foreach (IDataPersistance dataPersistanceObj in DataPersistanceObjects) {
            dataPersistanceObj.SaveData(ref gameData);
        }

        dataHandler.Save(gameData);
    }

    private void OnApplicationQuit() {
        SaveGame();
    }

    private List<IDataPersistance> FindAllDataPersistanceObjects() {
        IEnumerable<IDataPersistance> dataPersistenceObjects = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<IDataPersistance>();

        return new List<IDataPersistance>(dataPersistenceObjects);
    }
}
