using UnityEngine;
using ExtensionMethods;
using Utilities;
using System;

public class Enemy : Entity, IDamageable, IDamageDealer {
    [field: SerializeField] public HealthSystem HealthSystem { get; set; }
    public GameObject ExplosionVFX;
    
    [field: SerializeField] public Collider2D HitboxTrigger { get; set; }
    [field: SerializeField] public LayerMask DamageablesLayers { get; set; }
    [field: SerializeField] public int DamageDealerLayer { get; set; }
    [field: SerializeField] public int DamageAmount { get; set; }

    [UnityEngine.Range(0.0f, 50.0f)] public float maxMoveSpeed = 5f;
    [UnityEngine.Range(0.0f, 100.0f)] public float moveAcceleration = 10f;
    [UnityEngine.Range(0.0f, 100.0f)] public float moveDecceleration = 10f;
    [UnityEngine.Range(0.0f, 100.0f)] public float turnSpeed = 0.2f;
    [UnityEngine.Range(0.0f, 100.0f)] public float directionalDrag = 1f;
    [UnityEngine.Range(0.0f, 10.0f)] public float directionChangeFactor = 5f;
    [UnityEngine.Range(0.0f, 10.0f)] public float maxMagnitude = 1f;

    float targetMoveSpeed;
    float currentMoveSpeed;
    Vector2 currentMoveDirection;
    Vector2 targetMoveDirection;

    public static Action OnEnemyDestroyed;

    protected override void OnEnable() {
        base.OnEnable();

        HealthSystem.OnLivesDepleted += DestroyShip;

        shootRateTimer = new CountdownTimer(shootRate);
        shootRateTimer.OnTimerStart += () => canShoot = false;
        shootRateTimer.OnTimerStop += () => canShoot = true;

        LevelManager.OnGameOver += KillShip;
        LevelManager.OnMainMenuLoadStart += KillShip;
    }

    protected override void OnDisable() {
        base.OnDisable();

        HealthSystem.OnLivesDepleted -= DestroyShip;

        shootRateTimer.OnTimerStart -= () => canShoot = false;
        shootRateTimer.OnTimerStop -= () => canShoot = true;

        LevelManager.OnGameOver -= KillShip;
        LevelManager.OnMainMenuLoadStart -= KillShip;
    }

    protected override void Awake() {
        base.Awake();

        if (HealthSystem.IsNull()) HealthSystem = this.GetComponentInChildren<HealthSystem>();
    }

    protected override void Start() {
        shootRateTimer.Start();
    }

    protected override void Update() {
        base.Update();

        UpdateDotProducts();
        SetAimDirection();
        SetMovementDirection(targetAimDirection);
        SetVelocity();
        RotateShip();
        CheckShootingConditions();
    }

    protected override void FixedUpdate() {
        base.FixedUpdate();

        MoveShip();
    }

    bool isChangingDirections = false;
    bool isMoving = false;

    void SetMovementDirection(Vector2 direction) {
        targetMoveDirection = direction;
    }

    void SetAimDirection() {
        aimInputDirection = LevelManager.PlayerInstance.transform.position - transform.position;
        targetAimDirection = aimInputDirection.normalized;

        currentAimDirection = RotateTowards(currentAimDirection, targetAimDirection, turnSpeed, maxMagnitude);
    }

    void RotateShip() {
        transform.right = currentAimDirection;
    }

    void SetVelocity() {
        targetMoveSpeed = maxMoveSpeed;
        currentMoveSpeed = Mathf.MoveTowards(currentMoveSpeed, targetMoveSpeed, Time.deltaTime * moveAcceleration);
        // if (isMoving) {
        //     targetMoveSpeed = maxMoveSpeed;
        //     currentMoveSpeed = Mathf.MoveTowards(currentMoveSpeed, targetMoveSpeed, Time.deltaTime * moveAcceleration);
        // }
        // else {
        //     targetMoveSpeed = 0.0f;
        //     targetMoveDirection = aimInputDirection;
        //     currentMoveSpeed = Mathf.MoveTowards(currentMoveSpeed, targetMoveSpeed, Time.deltaTime * moveDecceleration);
        // }

        if (!isChangingDirections)
            currentMoveDirection = Vector2.Lerp(currentMoveDirection, targetMoveDirection, Time.deltaTime * directionalDrag);
        else
            currentMoveDirection = Vector2.Lerp(currentMoveDirection, targetMoveDirection, Time.deltaTime * (directionalDrag / directionChangeFactor));
    }

    void MoveShip() {
        CurrentVelocity = currentMoveDirection * currentMoveSpeed;
        Rb.linearVelocity = CurrentVelocity;
    }

    [UnityEngine.Range(0.0f, 1.0f)] public float forwardDotLimit = 0.25f;
    [UnityEngine.Range(0.0f, 1.0f)] public float strafeDotLimit = 0.5f;
    [UnityEngine.Range(-1.0f, 0.0f)] public float backwardsDotLimit = -0.5f;
    [UnityEngine.Range(-1.0f, 1.0f)] public float directionChangeDotLimit = 0.25f;

    [UnityEngine.Range(0.0f, 1.0f)] public float strafeSpeedMult =  0.75f;
    [UnityEngine.Range(0.0f, 1.0f)] public float backwardsSpeedMult = 0.5f;
    [UnityEngine.Range(0.0f, 5.0f)] public float boostSpeedMult = 2.0f;

    public MoveDirection moveDirection = MoveDirection.Forward;
    private float lookMoveDotProduct = 1.0f;

    Vector2 aimInputDirection = new Vector2(0.0f, 0.0f);
    Vector2 currentAimDirection = new Vector2(0.0f, 0.0f);
    Vector2 targetAimDirection = new Vector2(0.0f, 0.0f);

    void UpdateDotProducts() {
        lookMoveDotProduct = Vector2.Dot(currentAimDirection.normalized, CurrentVelocity.normalized);

        if (lookMoveDotProduct <= directionChangeDotLimit) isChangingDirections = true;
        else isChangingDirections = false;
    }

    void OnTriggerEnter2D(Collider2D collision) {
        IDamageable damagedEntity = collision.GetComponentInHierarchy<IDamageable>();

        if (damagedEntity.IsNull()) return;

        HealthSystem entityHealthSystem = damagedEntity.HealthSystem;
        if (!entityHealthSystem.IsDamagedBy(DamageDealerLayer)) return;
        
        OnEntityDamagedEventArgs entityArgs = new OnEntityDamagedEventArgs(this, DamageAmount, collision.ClosestPoint(this.transform.position));
        damagedEntity.Damage(this, entityArgs);

        Debug.Log($"{this.gameObject.name} damaged {entityHealthSystem.Entity.name} for {DamageAmount} HP");
    }

    public void Damage(object sender, OnEntityDamagedEventArgs entityDamagedArgs) {
        HealthSystem.ReduceHealth(sender, entityDamagedArgs);
    }

    void DestroyShip(object sender, OnEntityDamagedEventArgs entityDamagedEventArgs) {
        OnEnemyDestroyed?.Invoke();
        Instantiate(ExplosionVFX, transform.position, Quaternion.identity);
        AudioManager.instance.PlaySFX("EnemyDeath");
        gameObject.Destroy();
    }

    void KillShip() {
        Instantiate(ExplosionVFX, transform.position, Quaternion.identity);
        AudioManager.instance.PlaySFX("EnemyDeath");
        gameObject.Destroy();
    }

    [UnityEngine.Range(0.01f, 2.0f)] public float shootRate = 0.2f;

    public GameObject ProjectilePrefab;

    CountdownTimer shootRateTimer;
    public bool canShoot = true;
    public bool lockShooting = false;

    void CheckShootingConditions() {
        shootRateTimer.Tick(Time.deltaTime);
        if (lockShooting) return;

        if (canShoot) {
            Shoot();
            shootRateTimer.Restart();
        }
    }

    void Shoot() {
        GameObject projectile = Instantiate(ProjectilePrefab, transform.position, transform.rotation);
        AudioManager.instance.PlaySFX("ProjectileShot");
    }
}
