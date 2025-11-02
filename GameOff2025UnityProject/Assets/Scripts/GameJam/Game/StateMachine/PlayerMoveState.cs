using UnityEngine;
using UnityEngine.InputSystem;

namespace GameJam {
  public sealed class PlayerMoveState : PlayerBaseState {

    public PlayerMoveState(PlayerStateMachine stateMachine) : base(stateMachine) {
      // ...
    }

    public override void Enter() {
      Vector3 velocity = _stateMachine.Velocity;
      velocity.y = Physics.gravity.y;

      _stateMachine.Velocity = velocity;

      _stateMachine.JumpAction.action.performed += SwitchToJumpState;
    }

    public override void Tick() {
      if (!_stateMachine.CharacterController.isGrounded) {
        _stateMachine.SwitchState(new PlayerFallState(_stateMachine));
      }

      CalculateMoveDirection();
      FaceMoveDirection();
      Move();
    }

    public override void Exit() {
      _stateMachine.JumpAction.action.performed -= SwitchToJumpState;
    }

    void SwitchToJumpState(InputAction.CallbackContext context) {
      _stateMachine.SwitchState(new PlayerJumpState(_stateMachine));
    }
  }
}
