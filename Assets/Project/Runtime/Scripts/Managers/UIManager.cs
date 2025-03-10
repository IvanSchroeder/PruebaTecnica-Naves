using System;
using System.Collections;
using ExtensionMethods;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {
    [Header("General References")]
    [Space(5)]
    public static UIManager instance;
    public IntSO MusicVolume;
    public IntSO SFXVolume;
    public IntSO PlayerHealth;
    public IntSO PlayerMaxHealth;
    public IntSO EnemiesKilled;
    public IntSO EnemiesKilledRecord;
    public IntSO DeathsCount;
    public float panelFadeSpeed = 1f;

    [Space(20)]

    [Header("Background References")]
    [Space(5)]
    public Image backgroundImage;
    public Sprite vortexTexture;
    public Sprite nebulaTexture;

    [Space(20)]

    [Header("Canvas Groups")]
    [Space(5)]
    public CanvasGroup MainMenuGroup;
    public CanvasGroup InGameGroup;
    [Space(5)]
    public CanvasGroup TitleScreenMenuGroup;
    public CanvasGroup SettingsMenuGroup;
    [Space(5)]
    public CanvasGroup GameplayGUIGroup;
    public CanvasGroup PauseMenuGroup;
    public CanvasGroup GameOverMenuGroup;

    [Space(20)]

    [Header("Texts")]
    [Space(5)]
    public TMP_Text PlayerHPText;
    public TMP_Text EnemyKillCountText;
    public TMP_Text EnemyKillTotalText;
    public TMP_Text EnemyKillRecordText;
    public TMP_Text EnemyKillRecordMainMenuText;
    public TMP_Text PlayerDeathsCountText;
    public TMP_Text MusicVolumeText;
    public TMP_Text SFXVolumeText;

    public static Action<bool> OnPause;
    public static Action<bool> OnPauseAnimationCompleted;
    private Coroutine gamePausedCoroutine;

    private void OnEnable() {
        PlayerHealth.OnValueChange += UpdatePlayerHealth;

        EnemiesKilled.OnValueChange += UpdateKillCounter;
        EnemiesKilled.OnValueChange += UpdateKillTotal;
        EnemiesKilledRecord.OnValueChange += UpdateKillRecord;
        DeathsCount.OnValueChange += UpdateDeathsCount;

        LevelManager.OnGameOver += ShowGameOverUI;
        LevelManager.OnMainMenuLoadEnd += InitializeMainMenuUI;
        LevelManager.OnGameplayScreenLoadEnd += InitializeGameplayUI;

        LevelManager.OnGameSessionInitialized += InitializeMainMenuUI;
    }

    private void OnDisable() {
        PlayerHealth.OnValueChange -= UpdatePlayerHealth;

        EnemiesKilled.OnValueChange -= UpdateKillCounter;
        EnemiesKilled.OnValueChange -= UpdateKillTotal;
        EnemiesKilledRecord.OnValueChange -= UpdateKillRecord;
        DeathsCount.OnValueChange -= UpdateDeathsCount;

        LevelManager.OnGameOver -= ShowGameOverUI;
        LevelManager.OnMainMenuLoadEnd -= InitializeMainMenuUI;
        LevelManager.OnGameplayScreenLoadEnd -= InitializeGameplayUI;

        LevelManager.OnGameSessionInitialized -= InitializeMainMenuUI;
    }

    private void Awake() {
        if (instance == null) {
            instance = this;
        }
        else if (instance != this) {
            Destroy(gameObject);
        }
    }

    private void Start() {
        UpdatePlayerHealth();
        UpdateKillCounter();
    }

    private CanvasGroup currentScreen;
    private CanvasGroup lastScreen;

    public void ChangeScreen(CanvasGroup screenToChange) {
        lastScreen = currentScreen;

        if (lastScreen.IsNotNull()) SetPanelMenuUI(lastScreen, false);

        currentScreen = screenToChange;

        if (currentScreen.IsNotNull()) SetPanelMenuUI(currentScreen, true);
    }

    private void InitializeMainMenuUI() {
        backgroundImage.sprite = nebulaTexture;

        SetPanelMenuUI(MainMenuGroup, true);
        SetPanelMenuUI(InGameGroup, false);

        lastScreen = null;

        ChangeScreen(TitleScreenMenuGroup);

        SetPanelMenuUI(SettingsMenuGroup, false);
    }

    private void InitializeGameplayUI() {
        backgroundImage.sprite = vortexTexture;

        SetPanelMenuUI(MainMenuGroup, false);
        SetPanelMenuUI(InGameGroup, true);

        lastScreen = null;

        ChangeScreen(GameplayGUIGroup);

        SetPanelMenuUI(PauseMenuGroup, false);
        SetPanelMenuUI(GameOverMenuGroup, false);
    }

    private void SetPanelMenuUI(CanvasGroup group, bool _enabled) {
        if (_enabled)
            group.alpha = 1f;
        else
            group.alpha = 0f;

        group.blocksRaycasts = _enabled;
    }

    public void SetPausedState(bool pause) {
        if (gamePausedCoroutine.IsNotNull()) {
            StopCoroutine(gamePausedCoroutine);
            gamePausedCoroutine = null;
        }

        gamePausedCoroutine = StartCoroutine(GamePauseRoutine(pause));
    }

    private void ShowGameOverUI() {
        SetPanelMenuUI(GameplayGUIGroup, false);
        SetPanelMenuUI(GameOverMenuGroup, true);
    }

    void UpdatePlayerHealth() {
        PlayerHPText.text = $"HP: {PlayerHealth.Value}/{PlayerMaxHealth.Value}";
    }

    void UpdatePlayerHealth(ValueSO<int> amount) {
        PlayerHPText.text = $"HP: {amount.Value}/{PlayerMaxHealth.Value}";
    }

    void UpdateKillCounter() {
        EnemyKillCountText.text = $"Enemies Killed: {EnemiesKilled.Value}";
    }

    void UpdateKillCounter(ValueSO<int> amount) {
        EnemyKillCountText.text = $"Enemigos matados: {amount.Value}";
    }

    void UpdateKillTotal(ValueSO<int> amount) {
        EnemyKillTotalText.text = $"Eliminaciones: {amount.Value}";
    }

    void UpdateKillRecord(ValueSO<int> amount) {
        EnemyKillRecordText.text = $"Record: {amount.Value}";
        EnemyKillRecordMainMenuText.text = $"Record: {amount.Value} enemigos";
    }

    public void UpdateMusicVolumeSlider(ValueSO<int> volume) {
        MusicVolumeText.text = $"{volume.Value}";
    }

    public void UpdateSFXVolumeSlider(ValueSO<int> volume) {
        SFXVolumeText.text = $"{volume.Value}";
    }

    public void UpdateMusicVolumeSlider(int volume) {
        MusicVolumeText.text = $"{volume}";
        MusicVolume.Value = volume;
    }

    public void UpdateSFXVolumeSlider(int volume) {
        SFXVolumeText.text = $"{volume}";
        SFXVolume.Value = volume;
    }

    void UpdateDeathsCount(ValueSO<int> amount) {
        PlayerDeathsCountText.text = $"Muertes totales: {amount.Value}";
    }

    private IEnumerator GamePauseRoutine(bool pause) {
        float gameplayAlpha = GameplayGUIGroup.alpha;
        float pauseAlpha = PauseMenuGroup.alpha;

        float elapsedTime = 0f;
        float percentage = elapsedTime / panelFadeSpeed;

        if (pause) {
            OnPause?.Invoke(true);

            SetPanelMenuUI(GameplayGUIGroup, false);
            SetPanelMenuUI(PauseMenuGroup, true);

            AudioManager.instance.PlaySFX("PauseOn");
            AudioManager.instance.TogglePauseMusic(false);
        }
        else {
            SetPanelMenuUI(GameplayGUIGroup, true);
            SetPanelMenuUI(PauseMenuGroup, false);

            AudioManager.instance.PlaySFX("PauseOff");

            yield return new WaitForSecondsRealtime(panelFadeSpeed);
            OnPauseAnimationCompleted?.Invoke(false);
            AudioManager.instance.TogglePauseMusic(true);
        }

        yield return null;
    }
}
