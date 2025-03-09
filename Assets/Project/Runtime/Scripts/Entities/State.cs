using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State {
    protected Entity entity;
    protected StateMachine stateMachine;
    protected string animBoolName;

    public bool isAnimationFinished { get; set; }
    public bool isExitingState { get; set; }

    public virtual void Enter() {
        entity.isAnimationFinished = false;
        entity.isExitingState = false;
        entity.Anim.SetBool(animBoolName, true);
    }

    public virtual void Exit() {
        entity.Anim.SetBool(animBoolName, false);
        entity.isExitingState = true;
    }

    public virtual void LogicUpdate() {}

    public virtual void PhysicsUpdate() {}

    public virtual void AnimationTrigger() {}

    public virtual void AnimationFinishTrigger() {}
}
