using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using ExtensionMethods;
using System;

public class PlayerInputHandler : MonoBehaviour {
    public PlayerInput PlayerInputComponent;
    public InputMaster PlayerInputsMaster;

    public Camera mainCamera;

    public Vector2 RawMovementInput;
    public Vector2 NormMovementDirectionInput;
    public Vector2 MouseWorldPos;

    public bool LockInputs = false;
    public float InputHoldTime = 0.2f;

    private InputActionMap gameplayMap;
    private InputActionMap uiMap;

    private InputAction movementAction;
    private InputAction shootAction;
    private InputAction mousePositionAction;

    public static Action<Vector2> OnMoveStart;
    public static Action<Vector2> OnMoveStop;
    public static Action<bool> OnShootStart;
    public static Action<bool> OnShootStop;

    private void OnEnable() {
        // LevelManager.OnLevelStarted += EnableGameplayInputs;
        // LevelManager.OnLevelLoaded += ResetInputs;

        // LevelManager.OnLevelFinished += DisableGameplayInputs;
        // LevelManager.OnLevelFinished += ResetInputs;

        // LevelManager.OnPlayerSpawn += EnableGameplayInputs;
        // LevelManager.OnPlayerSpawn += ResetInputs;

        // LevelManager.OnGamePaused += DisableGameplayInputs;
        // LevelManager.OnGameUnpaused += EnableGameplayInputs;
    }

    private void OnDisable() {
        // LevelManager.OnLevelStarted -= EnableGameplayInputs;
        // LevelManager.OnLevelLoaded -= ResetInputs;

        // LevelManager.OnLevelFinished -= DisableGameplayInputs;
        // LevelManager.OnLevelFinished -= ResetInputs;

        // LevelManager.OnPlayerSpawn -= EnableGameplayInputs;
        // LevelManager.OnPlayerSpawn -= ResetInputs;

        // LevelManager.OnGamePaused -= DisableGameplayInputs;
        // LevelManager.OnGameUnpaused -= EnableGameplayInputs;

        movementAction.performed -= OnMoveInput;
        movementAction.canceled -= OnMoveInput;

        shootAction.started -= OnShootInputStart;
        shootAction.canceled -= OnShootInputStop;
    }

    private void Awake() {
        gameplayMap = PlayerInputComponent.actions.FindActionMap("Gameplay");
        uiMap = PlayerInputComponent.actions.FindActionMap("UI");

        movementAction = gameplayMap.FindAction("Movement");
        shootAction = gameplayMap.FindAction("Shoot");
        mousePositionAction = gameplayMap.FindAction("MousePosition");

        movementAction.performed += OnMoveInput;
        movementAction.canceled += OnMoveInput;

        shootAction.started += OnShootInputStart;
        shootAction.canceled += OnShootInputStop;
    }

    private void Start() {
        PlayerInputsMaster = new InputMaster();

        EnableGameplayInputs();
        // EnableUIInputs();
        
        ResetInputs();
    }

    private void Update() {
        MouseWorldPos = GetMousePosition();
    }

    public Vector2 GetMousePosition() {
        return mainCamera.ScreenToWorldPoint(mousePositionAction.ReadValue<Vector2>());
    }

    public Vector2 GetAimDirection(Vector2 position) {
        return MouseWorldPos - position;
    }

    public void OnMoveInput(InputAction.CallbackContext context) {
        // if (LockInputs) return;

        RawMovementInput = context.ReadValue<Vector2>();
        
        NormMovementDirectionInput = RawMovementInput.normalized;

        if (context.performed) {
            OnMoveStart?.Invoke(NormMovementDirectionInput);
        }
        else if (context.canceled) {
            OnMoveStop?.Invoke(Vector2.zero);
        }
    }

    public void OnShootInputStart(InputAction.CallbackContext context) {
        OnShootStart?.Invoke(true);
    }

    public void OnShootInputStop(InputAction.CallbackContext context) {
        OnShootStop?.Invoke(false);
    }

    public void UnlockGameplayInputs() {
        LockInputs = false;
    }

    public void LockGameplayInputs() {
        LockInputs = true;
    }

    private void ResetInputs() {
        RawMovementInput = Vector2.zero;
        NormMovementDirectionInput = Vector2.zero;
    }

    public void EnableGameplayInputs() {
        gameplayMap.Enable();
    }

    public void DisableGameplayInputs() {
        gameplayMap.Disable();
    }

    public void EnableUIInputs() {
        uiMap.Enable();
    }

    public void DisableUIInputs() {
        uiMap.Disable();
    }
}
