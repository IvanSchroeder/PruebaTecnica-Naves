using UnityEngine;

public class PlayerIdleState : PlayerState {
    public PlayerIdleState(Player _player, StateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName) {
    }

    public override void Enter() {
        Debug.Log("Entered Idle State");
    }

    public override void Exit() {
        Debug.Log("Exited Idle State");
    }
}
