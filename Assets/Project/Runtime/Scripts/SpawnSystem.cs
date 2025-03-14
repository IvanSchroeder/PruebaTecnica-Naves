using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using Utilities;
using NUnit.Framework.Internal.Commands;

public class SpawnSystem : MonoBehaviour {
    public List<GameObject> EnemyPrefabsList;

    public List<Transform> SpawnPointsList;

    public int maxEnemiesCount = 10;
    int currentAmountOfEnemies = 0;

    public float enemySpawnTimeSeconds = 5f;
    public float enemySpawnDelaySeconds = 2f;
    public int minEnemySpawnAmount = 1;
    public int maxEnemySpawnAmount = 5;
    CountdownTimer enemySpawnTimer;
    CountdownTimer enemySpawnTimerDelay;

    bool lockSpawning = false;

    void OnEnable() {
        enemySpawnTimerDelay.OnTimerStop += enemySpawnTimer.Start;
        enemySpawnTimerDelay.OnTimerStop += SpawnRandomEnemy;
        enemySpawnTimer.OnTimerStop += SpawnRandomEnemy;

        Enemy.OnEnemyDestroyed += () => currentAmountOfEnemies--;

        LevelManager.OnGameOver += enemySpawnTimer.Stop;
        LevelManager.OnGameOver += () => lockSpawning = true;
    }

    void OnDisable() {
        enemySpawnTimerDelay.OnTimerStop -= enemySpawnTimer.Start;
        enemySpawnTimerDelay.OnTimerStop -= SpawnRandomEnemy;
        enemySpawnTimer.OnTimerStop -= SpawnRandomEnemy;

        Enemy.OnEnemyDestroyed -= () => currentAmountOfEnemies--;

        LevelManager.OnGameOver -= enemySpawnTimer.Stop;
        LevelManager.OnGameOver -= () => lockSpawning = true;
    }

    void Awake() {
        enemySpawnTimer = new CountdownTimer(enemySpawnTimeSeconds);
        enemySpawnTimerDelay = new CountdownTimer(enemySpawnDelaySeconds);
    }

    void Update() {
        if (!lockSpawning) enemySpawnTimerDelay.Tick(Time.deltaTime);
        if (!lockSpawning)enemySpawnTimer.Tick(Time.deltaTime);
    }

    void Start() {
        currentAmountOfEnemies = 0;
        lockSpawning = false;
        enemySpawnTimerDelay.Start();
    }

    void SpawnRandomEnemy() {
        enemySpawnTimer.Restart();

        if (currentAmountOfEnemies >= maxEnemiesCount) {
            return;
        }

        int enemiesToSpawn = Random.Range(minEnemySpawnAmount, maxEnemySpawnAmount);

        for (int i = 0; i < enemiesToSpawn ; i++) {
            Transform spawnpoint = SelectRandomSpawnpoint();
            Instantiate(EnemyPrefabsList.GetRandomElement(), spawnpoint.position, Quaternion.identity);
            currentAmountOfEnemies++;

            if (currentAmountOfEnemies >= maxEnemiesCount) break;
        }
    }

    Transform SelectRandomSpawnpoint() {
        Transform transform = SpawnPointsList.GetRandomElement();
        return SpawnPointsList.GetRandomElement();
    }
}
