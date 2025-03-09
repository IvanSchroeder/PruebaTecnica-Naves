using UnityEngine;

public class PlayerState : State {
    protected Player player;
    
    public PlayerState(Player _player, StateMachine _stateMachine, string _animBoolName) {
        Init(_player, _stateMachine, _animBoolName);

        player.isDead = false;
    }

    void Init(Player _player, StateMachine _stateMachine, string _animBoolName) {
        player = _player;
        stateMachine = _stateMachine;
        animBoolName = _animBoolName;
    }

    public override void Enter() {
        base.Enter();
    }

    public override void Exit() {
        base.Exit();
    }

    public override void LogicUpdate() {
        base.LogicUpdate();

        if (player.isExitingState) return; 
    }

    public override void PhysicsUpdate() {
        base.PhysicsUpdate();
    }

    public override void AnimationTrigger() {
        base.AnimationTrigger();
    }

    public override void AnimationFinishTrigger() {
        base.AnimationFinishTrigger();

        player.isAnimationFinished = true;
    }
}
