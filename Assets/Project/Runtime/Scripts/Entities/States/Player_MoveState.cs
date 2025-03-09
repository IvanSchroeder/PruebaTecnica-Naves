using UnityEngine;

// [CreateAssetMenu(fileName = "NewPlayerMoveState", menuName = "States/Player States/Move State")]
public class PlayerMoveState : PlayerState {
    public PlayerMoveState(Player _player, StateMachine _stateMachine, string _animBoolName) : base(_player, _stateMachine, _animBoolName) {
    }

    public override void Enter() {
        Debug.Log("Entered Move State");
    }

    public override void Exit() {
        Debug.Log("Exited Move State");
    }
}
