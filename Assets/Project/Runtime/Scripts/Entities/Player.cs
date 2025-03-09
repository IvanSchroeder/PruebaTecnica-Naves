using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using Utilities;
using System;

public enum MoveDirection {
    Forward,
    Strafe,
    Backwards
}

public class Player : Entity, IDamageable {
    public PlayerIdleState IdleState;
    public PlayerMoveState MoveState;
    public PlayerDeathState DeathState;

    public PlayerInputHandler playerInputHandler;
    [field: SerializeField] public HealthSystem HealthSystem { get; set; }

    [Header("Parameters")]
    [Space(5f)]

    [UnityEngine.Range(0.0f, 100.0f)] public float maxMoveSpeed = 50f;
    [UnityEngine.Range(0.0f, 100.0f)] public float moveAcceleration = 10f;
    [UnityEngine.Range(0.0f, 100.0f)] public float moveDecceleration = 10f;
    [UnityEngine.Range(0.0f, 100.0f)] public float turnSpeed = 10f;
    [UnityEngine.Range(0.0f, 100.0f)] public float directionalDrag = 10f;
    [UnityEngine.Range(0.0f, 10.0f)] public float directionChangeFactor = 5f;
    [UnityEngine.Range(0.0f, 10.0f)] public float maxMagnitude = 10f;

    [UnityEngine.Range(0.0f, 1.0f)] public float forwardDotLimit = 0.25f;
    [UnityEngine.Range(0.0f, 1.0f)] public float strafeDotLimit = 0.5f;
    [UnityEngine.Range(-1.0f, 0.0f)] public float backwardsDotLimit = -0.5f;
    [UnityEngine.Range(-1.0f, 1.0f)] public float directionChangeDotLimit = 0.25f;

    [UnityEngine.Range(0.0f, 1.0f)] public float strafeSpeedMult =  0.75f;
    [UnityEngine.Range(0.0f, 1.0f)] public float backwardsSpeedMult = 0.5f;
    [UnityEngine.Range(0.0f, 5.0f)] public float boostSpeedMult = 2.0f;

    Vector2 movementInputDirection = new Vector2(0.0f, 0.0f);
    Vector2 lastMoveInputDirection = new Vector2(0.0f, 1.0f);
    Vector2 aimInputDirection = new Vector2(0.0f, 0.0f);
    Vector2 currentAimDirection = new Vector2(0.0f, 0.0f);
    Vector2 targetAimDirection = new Vector2(0.0f, 0.0f);

    [Space(10f)]

    [Header("Info")]
    [Space(5f)]

    public MoveDirection moveDirection = MoveDirection.Forward;
    private float moveDotProduct = 1.0f;
    private float lookMoveDotProduct = 1.0f;
    float targetMoveSpeed;
    float currentMoveSpeed;
    Vector2 currentMoveDirection;
    Vector2 targetMoveDirection;
    float currentSpeedMult = 1.0f;

    bool isIdle = true;
    bool isMoving = false;
    public bool isChangingDirections = false;
    public bool isDead = false;

    public Action OnInvulnerability;
    public static Action OnPlayerDamaged;
    public static Action OnPlayerHealed;
    public static Action OnPlayerKilled;

    protected override void OnEnable() {
        base.OnEnable();

        PlayerInputHandler.OnMoveStart += SetMovementDirection;
        PlayerInputHandler.OnMoveStop += SetMovementDirection;

        OnInvulnerability += HealthSystem.SetInvulnerability;

        this.HealthSystem.OnEntityDeath += PlayerDeath;
    }

    protected override void OnDisable() {
        base.OnDisable();

        PlayerInputHandler.OnMoveStart -= SetMovementDirection;
        PlayerInputHandler.OnMoveStop -= SetMovementDirection;

        OnInvulnerability -= HealthSystem.SetInvulnerability;

        this.HealthSystem.OnEntityDeath -= PlayerDeath;
    }

    protected override void Awake() {
        base.Awake();

        IdleState = new PlayerIdleState(this, EntityStateMachine, "idle");
        MoveState = new PlayerMoveState(this, EntityStateMachine, "move");
        DeathState = new PlayerDeathState(this, EntityStateMachine, "death");
        if (playerInputHandler.IsNull()) playerInputHandler = this.GetComponentInHierarchy<PlayerInputHandler>();
        if (HealthSystem.IsNull()) HealthSystem = this.GetComponentInHierarchy<HealthSystem>();

        playerInputHandler.mainCamera = this.GetMainCamera();
    }

    protected override void Start() {
        base.Start();

        EntityStateMachine.Initialize(IdleState);
    }

    protected override void Update() {
        base.Update();

        SetAimDirection();
        UpdateDotProducts();
        SetVelocity();
        RotateShip();
    }

    protected override void FixedUpdate() {
        base.FixedUpdate();

        MoveShip();
    }

    void SetVelocity() {
        if (isMoving) {
            targetMoveSpeed = maxMoveSpeed * currentSpeedMult;
            currentMoveSpeed = Mathf.MoveTowards(currentMoveSpeed, targetMoveSpeed, Time.deltaTime * moveAcceleration);
        }
        else {
            targetMoveSpeed = 0.0f;
            targetMoveDirection = lastMoveInputDirection;
            currentMoveSpeed = Mathf.MoveTowards(currentMoveSpeed, targetMoveSpeed, Time.deltaTime * moveDecceleration);
        }

        isIdle = currentMoveSpeed > 0.0f ? false : true;

        if (isIdle) currentMoveDirection = Vector2.zero;

        if (!isChangingDirections)
            currentMoveDirection = Vector2.Lerp(currentMoveDirection, targetMoveDirection, Time.deltaTime * directionalDrag);
        else
            currentMoveDirection = Vector2.Lerp(currentMoveDirection, targetMoveDirection, Time.deltaTime * (directionalDrag / directionChangeFactor));
    }

    void SetMovementDirection(Vector2 direction) {
        targetMoveDirection = direction;
        movementInputDirection = direction;

        if (direction == Vector2.zero) {
            isMoving = false;
        }
        else {
            isMoving = true;
            lastMoveInputDirection = direction;
        }
    }

    void MoveShip() {
        CurrentVelocity = currentMoveDirection * currentMoveSpeed;
        Rb.linearVelocity = CurrentVelocity;
    }

    void SetAimDirection() {
        aimInputDirection = playerInputHandler.GetAimDirection(transform.position);
        targetAimDirection = aimInputDirection;

        currentAimDirection = RotateTowards(currentAimDirection, targetAimDirection, turnSpeed, maxMagnitude);
    }
    
    void RotateShip() {
        transform.right = currentAimDirection;
    }

    void UpdateDotProducts() {
        lookMoveDotProduct = Vector2.Dot(currentAimDirection.normalized, lastMoveInputDirection.normalized);
        moveDotProduct = Vector2.Dot(CurrentVelocity, targetMoveDirection * targetMoveSpeed);

        if (lookMoveDotProduct >= strafeDotLimit || isIdle) {
            moveDirection = MoveDirection.Forward;
            currentSpeedMult = 1.0f;
        }
        else if (lookMoveDotProduct >= backwardsDotLimit) {
            moveDirection = MoveDirection.Strafe;
            currentSpeedMult = strafeSpeedMult;
        }
        else {
            moveDirection = MoveDirection.Backwards;
            currentSpeedMult = backwardsSpeedMult;
        }

        if (moveDotProduct <= directionChangeDotLimit) isChangingDirections = true;
        else isChangingDirections = false;
    }
  
    public void Damage(object sender, OnEntityDamagedEventArgs entityDamagedArgs) {
        AudioManager.instance.PlaySFX("PlayerHit");
        HealthSystem.ReduceHealth(sender, entityDamagedArgs);
        OnPlayerDamaged?.Invoke();
    }

    void PlayerDeath(object sender, OnEntityDamagedEventArgs entityDamagedArgs) {
        AudioManager.instance.PlaySFX("PlayerDeath");
        gameObject.SetActive(false);
        OnPlayerKilled?.Invoke();
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, currentMoveDirection * 2f);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, targetMoveDirection * 2f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, CurrentVelocity.normalized * 2f);
    }
}
