using UnityEngine;

namespace GameJam {
  public sealed class PlayerJumpState : PlayerBaseState {
    public PlayerJumpState(PlayerStateMachine stateMachine) : base(stateMachine) {
      // ...
    }

    public override void Enter() {
      Vector3 velocity = _stateMachine.Velocity;
      velocity.y = _stateMachine.JumpForce;

      _stateMachine.Velocity = velocity;
    }

    public override void Tick() {
      ApplyGravity();

      if (_stateMachine.Velocity.y <= 0f) {
        _stateMachine.SwitchState(new PlayerFallState(_stateMachine));
      }

      FaceMoveDirection();
      Move();
    }

    public override void Exit() {
      // ...
    }
  }
}
