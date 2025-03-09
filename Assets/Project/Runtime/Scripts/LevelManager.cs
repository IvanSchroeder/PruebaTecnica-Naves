using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using System;
using System.Linq;
using UnityEngine.SceneManagement;
using Utilities;

public enum SceneIndexes {
    MAIN_MENU = 1,
    GAMEPLAY = 2,
}

public enum GameState {
    None,
    MainMenu,
    Gameplay,
}

public class LevelManager : MonoBehaviour {
    public GameState CurrentGameState;

    public static LevelManager instance;
    // public Player PlayerInstance;
    public static Player PlayerInstance;
    public GameObject PlayerPrefab;
    public GameObject SpawnSystemPrefab;
    SpawnSystem SpawnSystem;

    public int playerDeathsCount = 0;
    public int totalEnemiesKilledCount = 0;
    public IntSO currentSessionEnemyKillCount;

    public float levelFinishDelaySeconds;
    private WaitForSeconds levelFinishDelay;

    private Coroutine levelHandlerCoroutine;

    public static Action OnLevelLoaded;
    public static Action OnLevelFinished;
    public static Action<bool> OnNewTimeRecord;
    public static Action OnLevelRestart;
    public static Action OnGamePaused;
    public static Action OnGameUnpaused;
    public static Action OnGameOver;
    public static Action OnPlayerSpawn;
    public static Action OnAllCoinsCollected;
    public static Action<GameState> OnGameStateChanged;

    public float frameFreezeDuration = 0.5f;
    private Coroutine frameFreezeCoroutine;
    private WaitForSecondsRealtime freezeWait;

    [SerializeField] private CanvasGroup loadingScreenCanvasGroup;
    [SerializeField] private float secondsToWaitInLoadingScreen;
    [SerializeField] private float fadeInSeconds = 1f;
    [SerializeField] private float fadeOutSeconds = 1f;

    private WaitForSecondsRealtime secondsInLoadingScreen;

    [SerializeField] private float secondsToWaitAfterLevelSpawn;
    private WaitForSecondsRealtime secondsAfterLevelSpawn;

    public static event Action OnGameSessionInitialized;
    public static event Action OnMainMenuLoadStart;
    public static event Action OnMainMenuLoadEnd;
    public static event Action OnGameplayScreenLoadStart;
    public static event Action OnGameplayScreenLoadEnd;

    CountdownTimer playerSpawnTimer;
    [UnityEngine.Range(0.0f, 10.0f)] public float spawnDelaySeconds = 1f;

    private void OnValidate() {
        levelFinishDelay = new WaitForSeconds(levelFinishDelaySeconds);
    }

    private void OnEnable() {
        UIManager.OnPause += PauseLevel;
        UIManager.OnPauseAnimationCompleted += UnpauseLevel;

        Player.OnPlayerDamaged += FrameFreeze;

        Player.OnPlayerKilled += GameOver;

        playerSpawnTimer.OnTimerStop += SpawnPlayer;

        Enemy.OnEnemyDestroyed += ScoreEnemySkill;
    }

    private void OnDisable() {
        UIManager.OnPause -= PauseLevel;
        UIManager.OnPauseAnimationCompleted -= UnpauseLevel;

        Player.OnPlayerDamaged -= FrameFreeze;

        Player.OnPlayerKilled -= GameOver;

        playerSpawnTimer.OnTimerStop -= SpawnPlayer;

        Enemy.OnEnemyDestroyed -= ScoreEnemySkill;
    }

    private void Awake() {
        if (instance.IsNull()) {
            instance = this;
        }
        else if (instance != this) {
            Destroy(gameObject);
        }

        secondsInLoadingScreen = new WaitForSecondsRealtime(secondsToWaitInLoadingScreen);
        secondsAfterLevelSpawn = new WaitForSecondsRealtime(secondsToWaitAfterLevelSpawn);
        freezeWait = new WaitForSecondsRealtime(frameFreezeDuration);

        playerSpawnTimer = new CountdownTimer(spawnDelaySeconds);
    }

    private void Start() {
        CurrentGameState = GameState.None;

        StartCoroutine(InitializeGameSession(ChangeGameState(GameState.MainMenu)));
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.KeypadEnter)) {
            PauseEditor();
        }
        else if (Input.GetKeyDown(KeyCode.M)) {
            BackToMainMenu();
        }

        playerSpawnTimer.Tick(Time.deltaTime);
    }

    public void ExitGame() {
        Application.Quit();
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    public void PauseEditor() {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPaused = UnityEditor.EditorApplication.isPaused.Toggle();
        #endif
    }

    public void StartGame() {
        StartCoroutine(LoadGameplayScene(true, (int)SceneIndexes.MAIN_MENU, ChangeGameState(GameState.Gameplay)));
    }

    public void BackToMainMenu() {
        StartCoroutine(LoadMainMenuScene(true, (int)SceneIndexes.GAMEPLAY, ChangeGameState(GameState.MainMenu)));
    }

    public void GameOver() {
        if (levelHandlerCoroutine.IsNotNull()) {
            StopCoroutine(levelHandlerCoroutine);
            levelHandlerCoroutine = null;
        }

        levelHandlerCoroutine = StartCoroutine(GameOverRoutine());
    }

    public void SpawnPlayer() {
        GameObject player = Instantiate(PlayerPrefab, Vector2.zero, Quaternion.identity);

        PlayerInstance = player.GetComponentInHierarchy<Player>();
    }

    public void PauseLevel(bool pause) {
        if (pause)
            SetTimeScale(0f);
    }

    public void UnpauseLevel(bool pause) {
        if (!pause)
            SetTimeScale(1f);
    }

    private IEnumerator ChangeGameState(GameState state) {
        if (CurrentGameState != state) {
            CurrentGameState = state;
            OnGameStateChanged?.Invoke(CurrentGameState);

            switch (CurrentGameState) {
                case GameState.Gameplay:
                    yield return StartCoroutine(LoadLevelScreenRoutine());
                break;
                case GameState.MainMenu:
                    yield return StartCoroutine(LoadMainMenuScreenRoutine());
                break;

            }

            yield return null;
        }
        else yield return null;
    }

    private IEnumerator InitializeGameSession(IEnumerator midLoadRoutine = null, IEnumerator endLoadRoutine = null) {
        int sceneToLoad = (int)SceneIndexes.MAIN_MENU;
        Debug.Log($"Loading {SceneManager.GetSceneByBuildIndex(sceneToLoad)} scene");

        loadingScreenCanvasGroup.alpha = 1f;
        AsyncOperation loadingOperation = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);

        while (!loadingOperation.isDone) {
            yield return null;
        }

        AudioManager.instance.StopMusic();

        yield return secondsInLoadingScreen;

        Time.timeScale = 1f;

        OnGameSessionInitialized?.Invoke();

        if (midLoadRoutine != null) yield return StartCoroutine(midLoadRoutine);

        yield return StartCoroutine(ScreenFade(loadingScreenCanvasGroup, 0f, fadeOutSeconds));

        if (endLoadRoutine != null) yield return StartCoroutine(endLoadRoutine);

        Debug.Log($"Finished loading {SceneManager.GetSceneByBuildIndex(sceneToLoad)} scene");
    }

    private IEnumerator LoadMainMenuScene(bool unloadCurrentScene = false, int sceneToUnload = default, IEnumerator midLoadRoutine = null, IEnumerator endLoadRoutine = null) {
        int sceneToLoad = (int)SceneIndexes.MAIN_MENU;

        yield return StartCoroutine(ScreenFade(loadingScreenCanvasGroup, 1f, fadeInSeconds));
        OnMainMenuLoadStart?.Invoke();
        AsyncOperation loadingOperation = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);

        while (!loadingOperation.isDone) {
            yield return null;
        }

        if (unloadCurrentScene) {
            AsyncOperation unloadingOperation = SceneManager.UnloadSceneAsync(sceneToUnload);

            while (!unloadingOperation.isDone) {
                yield return null;
            }
        }

        OnMainMenuLoadEnd?.Invoke();

        AudioManager.instance.StopMusic();

        yield return secondsInLoadingScreen;

        Time.timeScale = 1f;

        if (midLoadRoutine != null) yield return StartCoroutine(midLoadRoutine);
        
        yield return StartCoroutine(ScreenFade(loadingScreenCanvasGroup, 0f, fadeOutSeconds));
        if (endLoadRoutine != null) yield return StartCoroutine(endLoadRoutine);
    }

    private IEnumerator LoadGameplayScene(bool unloadCurrentScene = false, int sceneToUnload = default, IEnumerator midLoadRoutine = null, IEnumerator endLoadRoutine = null) {
        int sceneToLoad = (int)SceneIndexes.GAMEPLAY;

        yield return StartCoroutine(ScreenFade(loadingScreenCanvasGroup, 1f, fadeInSeconds));
        OnGameplayScreenLoadStart?.Invoke();
        AsyncOperation loadingOperation = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);

        while (!loadingOperation.isDone) {
            yield return null;
        }

        if (unloadCurrentScene) {
            AsyncOperation unloadingOperation = SceneManager.UnloadSceneAsync(sceneToUnload);

            while (!unloadingOperation.isDone) {
                yield return null;
            }
        }

        AudioManager.instance.StopMusic();

        yield return secondsInLoadingScreen;

        Time.timeScale = 1f;

        OnGameplayScreenLoadEnd?.Invoke();

        if (midLoadRoutine != null) yield return StartCoroutine(midLoadRoutine);

        yield return StartCoroutine(ScreenFade(loadingScreenCanvasGroup, 0f, fadeOutSeconds));

        if (endLoadRoutine != null) yield return StartCoroutine(endLoadRoutine);
    }

    public IEnumerator LoadMainMenuScreenRoutine() {
        KillPlayer();
        SpawnSystem?.gameObject.Destroy();

        AudioManager.instance.PlayMusic("MainMenuMusic");

        yield return null;
    }

    public IEnumerator LoadLevelScreenRoutine() {
        yield return new WaitForSecondsRealtime(2f);

        OnLevelLoaded?.Invoke();

        yield return secondsAfterLevelSpawn;

        SpawnSystem = Instantiate(SpawnSystemPrefab, transform.position, Quaternion.identity, transform).GetComponentInHierarchy<SpawnSystem>();
        AudioManager.instance.PlayMusic("GameplayMusic");
        currentSessionEnemyKillCount.Value = 0;
        playerSpawnTimer.Start();

        yield return null;
    }

    public void FrameFreeze() {
        if (frameFreezeCoroutine.IsNotNull()) {
            SetTimeScale(1f, true);
            StopCoroutine(frameFreezeCoroutine);
            frameFreezeCoroutine = null;
        }

        frameFreezeCoroutine = StartCoroutine(FrameFreezeRoutine());
    }

    public IEnumerator FrameFreezeRoutine() {
        SetTimeScale(0f, true);
        yield return freezeWait;
        SetTimeScale(1f, true);
    }

    public void SetTimeScale(float scale, bool instant = false) {
        Time.timeScale = scale;
        // DOTween.To(() => Time.timeScale, x => Time.timeScale = x, scale, instant == false ? pauseLerpSpeed : 0f).SetUpdate(true);
    }

    // public void SpawnPlayer() {
    //     Player player = FindObjectOfType<Player>();

    //     if (player.IsNotNull()) {
    //         PlayerInstance = player;
    //     }
    //     else {
    //         var playerObj = GameObject.Instantiate(PlayerPrefab, Vector3.zero, Quaternion.identity);
    //         playerObj.name = "Player";
    //         PlayerInstance = playerObj.GetComponent<Player>();
    //     }

    //     PlayerInstance.transform.SetParent(null);
    //     PlayerInstance.transform.position = currentCheckpoint.SpawnpointTransform.position;

    //     PlayerInstance.OnEntityDamaged += CheckPlayerHit;
    //     PlayerInstance.OnPlayerDeathEnd += RespawnPlayer;
    //     PlayerInstance.OnLivesDepleted += GameOver;

    //     PlayerInstance.HealthSystem.HasInfiniteLives = currentLevel.enableInfiniteLives;

    //     OnPlayerSpawn?.Invoke();

    //     Debug.Log($"Respawned player");
    // }

    public void KillPlayer() {
        if (PlayerInstance.IsNotNull()) {
            PlayerInstance.gameObject.Destroy();
        }
    }

    void ScoreEnemySkill() {
        currentSessionEnemyKillCount.Value++;
    }

    private IEnumerator ScreenFade(CanvasGroup canvasGroup, float targetAlpha, float duration = 1f) {
        float elapsedTime = 0f;
        float percentageComplete = elapsedTime / duration;
        float startValue = canvasGroup.alpha;

        while (elapsedTime < duration) {
            canvasGroup.alpha = Mathf.Lerp(startValue, targetAlpha, percentageComplete);
            elapsedTime += Time.unscaledDeltaTime;
            percentageComplete = elapsedTime / duration;

            yield return Utils.waitForEndOfFrame;
        }

        canvasGroup.alpha = targetAlpha;
    }

    public IEnumerator GameOverRoutine() {
        yield return levelFinishDelay;
        OnGameOver?.Invoke();
        yield return null;
    }
}
