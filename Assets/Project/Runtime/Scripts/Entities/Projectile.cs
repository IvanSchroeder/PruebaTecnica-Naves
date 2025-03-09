using ExtensionMethods;
using UnityEngine;
using Utilities;

public class Projectile : MonoBehaviour, IDamageDealer {
    public Animator ProjectileAnimator;
    public Rigidbody2D ProjectileRb;
    public GameObject SingleExplosionVFX;
    public AnimationCurve ProjectileAcceleration = new AnimationCurve();

    [field: SerializeField] public Collider2D HitboxTrigger { get; set; }
    [field: SerializeField] public LayerMask DamageablesLayers { get; set; }
    [field: SerializeField] public int DamageDealerLayer { get; set; }
    [field: SerializeField] public int DamageAmount { get; set; }

    [UnityEngine.Range(0.0f, 100.0f)] public float targetProjectileSpeed;
    [UnityEngine.Range(0.0f, 100.0f)] public float projectileAcceleration;
    [UnityEngine.Range(0.1f, 3.0f)] public float projectileTime = 1.0f;
    CountdownTimer projectileLifeTimer;

    private float currentProjectileSpeed;
    private Vector2 projectileDirection;
    private Vector2 projectileVelocity;

    protected void OnEnable() {
        projectileLifeTimer = new CountdownTimer(projectileTime);
        projectileLifeTimer.OnTimerStop += DestroyProjectile;
    }

    protected void OnDisable() {
        projectileLifeTimer.OnTimerStop -= DestroyProjectile;
    }

    void Start() {
        projectileLifeTimer.Start();
        projectileDirection = transform.right;
    }

    void Update() {
        projectileLifeTimer.Tick(Time.deltaTime);
        CalculateProjectileVelocity();
    }

    void FixedUpdate() {
        SetProjectileVelocity();
    }

    void CalculateProjectileVelocity() {
        currentProjectileSpeed = Mathf.MoveTowards(currentProjectileSpeed, targetProjectileSpeed, ProjectileAcceleration.Evaluate(projectileLifeTimer.Progress * projectileAcceleration));
        projectileVelocity = projectileDirection * currentProjectileSpeed;
    }

    void SetProjectileVelocity() {
        ProjectileRb.linearVelocity = projectileVelocity;
    }

    void DestroyProjectile() {
        Instantiate(SingleExplosionVFX, transform.position, Quaternion.identity);
        gameObject.Destroy();
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        IDamageable damagedEntity = collision.GetComponentInHierarchy<IDamageable>();

        if (damagedEntity.IsNull()) return;

        HealthSystem entityHealthSystem = damagedEntity.HealthSystem;
        if (!entityHealthSystem.IsDamagedBy(DamageDealerLayer)) return;
        
        OnEntityDamagedEventArgs entityArgs = new OnEntityDamagedEventArgs(this, DamageAmount, collision.ClosestPoint(this.transform.position));
        damagedEntity.Damage(this, entityArgs);

        Debug.Log($"{this.gameObject.name} damaged {entityHealthSystem.Entity.name} for {DamageAmount} HP");

        AudioManager.instance.PlaySFX("ProjectileHit");
        projectileLifeTimer.Stop();
    }
}
