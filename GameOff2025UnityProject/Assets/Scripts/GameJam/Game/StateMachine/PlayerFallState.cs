using UnityEngine;

namespace GameJam {
  public sealed class PlayerFallState : PlayerBaseState {
    public PlayerFallState(PlayerStateMachine stateMachine) : base(stateMachine) {
      // ...
    }

    public override void Enter() {
      Vector3 velocity = _stateMachine.Velocity;
      velocity.y = 0f;

      _stateMachine.Velocity = velocity;
    }

    public override void Tick() {
      ApplyGravity();
      Move();

      if (_stateMachine.CharacterController.isGrounded) {
        _stateMachine.SwitchState(new PlayerMoveState(_stateMachine));
      }
    }

    public override void Exit() {
      // ...
    }
  }
}
