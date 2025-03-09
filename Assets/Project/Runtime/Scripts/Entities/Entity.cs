using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using System;

public abstract class Entity : MonoBehaviour {
    public EntityData entityData;

    [field: SerializeField] public StateMachine EntityStateMachine { get; protected set; }
    [field: SerializeField] public Rigidbody2D Rb { get; protected set; }
    [field: SerializeField] public CircleCollider2D MovementCollider { get; protected set; }
    [field: SerializeField] public Animator Anim { get; protected set; }
    [field: SerializeField] public SpriteRenderer Sprite { get; protected set; }
    public LayerMask damagedBy;

    [field: SerializeField] public Vector2 CurrentVelocity { get; protected set; }

    public bool isAnimationFinished { get; set; }
    public bool isExitingState { get; set; }

    protected virtual void OnEnable() {}

    protected virtual void OnDisable() {}

    protected virtual void Awake() {
        EntityStateMachine = new StateMachine();
        Rb = this.GetComponentInChildren<Rigidbody2D>();
        Anim = this.GetComponentInChildren<Animator>();
        Sprite = this.GetComponentInChildren<SpriteRenderer>();
    }

    protected virtual void Start() {}

    protected virtual void Update() {
        EntityStateMachine?.CurrentState?.LogicUpdate();
    }

    protected virtual void FixedUpdate() {
        EntityStateMachine?.CurrentState?.PhysicsUpdate();
    }

    protected virtual void AnimationTrigger() => EntityStateMachine?.CurrentState?.AnimationTrigger();

    protected virtual void AnimationFinishTrigger() => EntityStateMachine?.CurrentState?.AnimationFinishTrigger();

    // public virtual void SetVelocity(float speed, int direction, bool lerpVelocity = false, float accelAmount = 30f) {}

    public Vector2 RotateTowards(Vector2 current, Vector2 target, float maxRadiansDelta, float maxMagnitudeDelta) {
        if (current.x + current.y == 0)
            return target.normalized * maxMagnitudeDelta;

        float signedAngle = Vector2.SignedAngle(current, target);
        float stepAngle = Mathf.MoveTowardsAngle(0, signedAngle, maxRadiansDelta * Mathf.Rad2Deg) * Mathf.Deg2Rad;
        Vector2 rotated = new Vector2(
            current.x * Mathf.Cos(stepAngle) - current.y * Mathf.Sin(stepAngle),
            current.x * Mathf.Sin(stepAngle) + current.y * Mathf.Cos(stepAngle)
        );
        if (maxMagnitudeDelta == 0)
            return rotated;

        float magnitude = current.magnitude;
        float targetMagnitude = target.magnitude;
        targetMagnitude = Mathf.MoveTowards(magnitude, targetMagnitude, maxMagnitudeDelta);
        return rotated.normalized * targetMagnitude;
    }
}
