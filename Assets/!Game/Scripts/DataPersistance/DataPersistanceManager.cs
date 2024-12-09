using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using System.Linq;
using Unity.Netcode;

public class DataPersistanceManager : MonoBehaviour
{
    [Header("File Storage Config")]
    [SerializeField] private string FileName;
    [SerializeField] private bool useEncryption;


    private ProfileData gameData;
    private List<IDataPersistance> dataPersistanceObjects;
    private List<IDataPersistance> dataPersistanceNetworkObjects;
    private FileDataHandeler dataHandeler;

    public static DataPersistanceManager Instance { get; private set; }

    private void Awake()
    {

        if (Instance != null)
        {
            Debug.LogError("Found more than one Data Persistance Manager in the scene. Please make sure there is only one Data Peristance Manger at a time.");
        }
        Instance = this;
    }

    private void Start()
    {
        this.dataHandeler = new FileDataHandeler(Application.persistentDataPath, FileName, useEncryption);
        this.dataPersistanceObjects = FindAllDataPersitanceObjects();
        this.dataPersistanceNetworkObjects = FindAllNetworkDataPersitanceObjects();
        LoadGame();
    }

    public ProfileData GetData()
    { return gameData; }

    public void NewGame()
    {
        this.gameData = new ProfileData();
    }
    
    public void LoadGame()
    {
        this.gameData = dataHandeler.Load();

        if (this.gameData == null)
        {
            Debug.Log("No save game found, initalizing to new game.");
            NewGame();
        }

        foreach (IDataPersistance dataPersistancsObj in dataPersistanceObjects)
        {
            dataPersistancsObj.LoadData(gameData);
        }
        foreach(IDataPersistance dataPersistanceObj in dataPersistanceNetworkObjects)
        {
            dataPersistanceObj.LoadData(gameData);
        }
    }

    public void SaveGame() 
    {
        foreach (IDataPersistance dataPersistancsObj in dataPersistanceObjects)
        {
            dataPersistancsObj.SaveData(gameData);
        }
        foreach (IDataPersistance dataPersistanceObj in dataPersistanceNetworkObjects)
        {
            dataPersistanceObj.SaveData(gameData);
        }

        dataHandeler.Save(gameData);
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }

    private List<IDataPersistance> FindAllDataPersitanceObjects()
    {
        IEnumerable<IDataPersistance> dataPersistanceObjects = FindObjectsOfType<MonoBehaviour>()
            .OfType<IDataPersistance>();

        return new List<IDataPersistance>(dataPersistanceObjects);
    }

    private List<IDataPersistance> FindAllNetworkDataPersitanceObjects()
    {
        IEnumerable<IDataPersistance> dataPersistanceObjects = FindObjectsOfType<NetworkBehaviour>()
            .OfType<IDataPersistance>();

        return new List<IDataPersistance>(dataPersistanceObjects);
    }
}
