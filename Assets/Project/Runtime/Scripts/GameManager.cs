using UnityEngine;
using ExtensionMethods;
using Utilities;
using System;

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }

    public AudioManager AudioManager { get; private set; }
    public OptionsManager OptionsManager { get; private set; }

    private void Awake() {
        if (Instance.IsNull()) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log($"Setting GameManager Instance to {this}");
            InitializeManagers();
        }
        else if (Instance != this) {
            Destroy(gameObject);
        }
    }

    // private void Start() {
    //     StartGameplaySession();
    // }

    // private void StartGameplaySession() {
    //     Instantiate(SpawnSystemPrefab, transform.position, Quaternion.identity, transform);
    //     SpawnSystem = Instantiate(SpawnSystemPrefab, transform.position, Quaternion.identity, transform).GetComponentInHierarchy<SpawnSystem>();
    //     AudioManager.PlayMusic("GameplayMusic");
    //     currentSessionEnemyKillCount.Value = 0;
    //     playerSpawnTimer.Start();
    // }

    private void InitializeManagers() {
        AudioManager = GetComponentInChildren<AudioManager>();
        OptionsManager = GetComponentInChildren<OptionsManager>();
    }
}
