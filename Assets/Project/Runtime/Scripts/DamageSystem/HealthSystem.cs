using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using System;
using Utilities;

public class HealthSystem : MonoBehaviour {
    [field: SerializeField] public Entity Entity { get; private set; }
    [field: SerializeField] public EntityData EntityData { get; private set; }
    [field: SerializeField] public CircleCollider2D HurtboxTrigger { get; set; }

    public IntSO maxHealthCopy;
    [field: SerializeField] public LayerMask DamagedBy { get; set; }
    [field: SerializeField] public bool IsRespawneable { get; set; }
    [field: SerializeField] public bool CanRespawn { get; set; }
    [field: SerializeField] public bool HasInfiniteLives { get; set; }
    [field: SerializeField] public int CurrentLives { get; set; }
    [field: SerializeField] public int MaxLives { get; set; }
    [field: SerializeField] public int MaxHealth { get; set; }
    [field: SerializeField] public int CurrentHealth { get; set; }
    [field: SerializeField] public bool IsDead { get; set; }
    [field: SerializeField] public bool HasInvulnerabilityFrames { get; set; }
    [field: SerializeField] public bool IsInvulnerable { get; set; }

    public EventHandler<OnEntityDamagedEventArgs> OnEntityDamaged;
    public EventHandler<OnEntityDamagedEventArgs> OnEntityDeath;
    public EventHandler<OnEntityDamagedEventArgs> OnInvulnerabilityStart;
    public EventHandler OnInvulnerabilityEnd;
    public EventHandler<OnEntityDamagedEventArgs> OnLivesDepleted;

    public bool startsInvulnerable = false;
    private Coroutine invulnerabilityCoroutine;
    private WaitForSeconds invulnerabilityDelay;
    public float invulnerabilitySeconds = 1f;

    private CountdownTimer invulnerabilityTimer;

    private void OnEnable() {
        invulnerabilityTimer.OnTimerStop += SetVulnerability;
    }

    private void OnDisable() {
        invulnerabilityTimer.OnTimerStop -= SetVulnerability;
    }

    private void Awake() {
        if (Entity == null) Entity = this.GetComponentInHierarchy<Entity>();
        if (HurtboxTrigger == null) HurtboxTrigger = GetComponent<CircleCollider2D>();

        invulnerabilityTimer = new CountdownTimer(invulnerabilitySeconds);
    }

    private void Start() {
        Init();
    }

    private void Update() {
        invulnerabilityTimer.Tick(Time.deltaTime);
    }

    public void Init() {
        if (EntityData == null) EntityData = Entity.entityData;

        DamagedBy = Entity.damagedBy;

        CurrentLives = MaxLives;
        if (CurrentLives > 1) CanRespawn = true;

        invulnerabilityDelay = new WaitForSeconds(invulnerabilitySeconds);
        if (startsInvulnerable) invulnerabilityTimer.Start();

        InitializeHealth();
    }

    public void InitializeHealth() {
        IsDead = false;
        IsInvulnerable = false;
        IsRespawneable = true;

        if (EntityData.copyHealthSO) {
            string s = EntityData.EntityMaxHealth.name;
            maxHealthCopy = Instantiate(EntityData.EntityMaxHealth);
            maxHealthCopy.name = s;
            MaxHealth = maxHealthCopy.Value;
        }
        else {
            MaxHealth = EntityData.EntityMaxHealth.Value;
        }

        CurrentHealth = MaxHealth;
        UpdateHealthSO();
    }

    void UpdateHealthSO() {
        if (!EntityData.copyHealthSO) {
            EntityData.EntityHealth.Value = CurrentHealth;
        }
    }

    public bool IsDamagedBy(int layer) {
        if (DamagedBy.HasLayer(layer)) {
            return true;
        }

        return false;
    }

    public void AddHealth(int amount) {
        CurrentHealth += amount;

        if (CurrentHealth >= MaxHealth) CurrentHealth = MaxHealth;

        UpdateHealthSO();
    }

    public void ReduceHealth(object sender, OnEntityDamagedEventArgs entityDamagedEventArgs) {
        if (IsInvulnerable || IsDead || !IsDamagedBy(entityDamagedEventArgs.DamageDealerSource.DamageDealerLayer)) return;

        if (entityDamagedEventArgs.DamageAmount == 0f) return;

        CurrentHealth -= entityDamagedEventArgs.DamageAmount;

        if (CurrentHealth <= 0) {
            CurrentHealth = 0;
            IsDead = true;
        }

        UpdateHealthSO();

        if (HasInvulnerabilityFrames) {
            IsInvulnerable = true;
            HurtboxTrigger.enabled = false;
        }

        if (IsDead) {
            ReduceLives(entityDamagedEventArgs);
            OnEntityDeath?.Invoke(sender, entityDamagedEventArgs);
        }
        else {
            if (HasInvulnerabilityFrames) SetInvulnerability();
            OnEntityDamaged?.Invoke(sender, entityDamagedEventArgs);
        }
    }

    public void ReduceLives(OnEntityDamagedEventArgs entityArgs) {
        if (HasInfiniteLives) return;
        
        CurrentLives--;

        if (CurrentLives <= 0) {
            CurrentLives = 0;
            CanRespawn = false;
            OnLivesDepleted?.Invoke(this, entityArgs);
        }
    }

    public void SetInvulnerability() {
        IsInvulnerable = true;
        HurtboxTrigger.enabled = false;
        invulnerabilityTimer.Restart();
    }

    public void SetVulnerability() {
        IsInvulnerable = false;
        HurtboxTrigger.enabled = true;
    }

    private IEnumerator InvulnerabilityFramesRoutine() {
        IsInvulnerable = true;
        HurtboxTrigger.enabled = false;
        OnEntityDamagedEventArgs args = new OnEntityDamagedEventArgs();
        OnInvulnerabilityStart?.Invoke(null, args);
        yield return invulnerabilityDelay;
        IsInvulnerable = false;
        HurtboxTrigger.enabled = true;
        OnInvulnerabilityEnd?.Invoke(this, null);
        yield return null;
    }
}
