using UnityEngine;

namespace GameJam {
  public abstract class PlayerBaseState : State {
    protected readonly PlayerStateMachine _stateMachine;

    protected PlayerBaseState(PlayerStateMachine stateMachine) {
      _stateMachine = stateMachine;
    }

    protected void CalculateMoveDirection() {
      Vector3 cameraForward = _stateMachine.MainCamera.forward;
      cameraForward.y = 0;

      Vector3 cameraRight = _stateMachine.MainCamera.right;
      cameraRight.y = 0;

      Vector2 moveComposite = _stateMachine.MoveAction.action.ReadValue<Vector2>();
      Vector3 moveDirection = (cameraForward.normalized * moveComposite.y) + (cameraRight.normalized * moveComposite.x);

      Vector3 velocity = _stateMachine.Velocity;
      velocity.x = moveDirection.x * _stateMachine.MovementSpeed;
      velocity.z = moveDirection.z * _stateMachine.MovementSpeed;

      _stateMachine.Velocity = velocity;
    }

    protected void FaceMoveDirection() {
      Vector3 faceDirection = _stateMachine.MainCamera.forward;
      faceDirection.y = 0;

      if (faceDirection == Vector3.zero) {
        return;
      }

      _stateMachine.transform.rotation =
          Quaternion.Slerp(
              _stateMachine.transform.rotation,
              Quaternion.LookRotation(faceDirection),
              _stateMachine.LookRotationDampFactor * Time.deltaTime);
    }

    protected void ApplyGravity() {
      Vector3 velocity = _stateMachine.Velocity;

      if (_stateMachine.CharacterController.isGrounded && velocity.y < 0f) {
        velocity = new Vector3(velocity.x, -_stateMachine.CharacterController.stepOffset, velocity.z);
      } else {
        velocity += (_stateMachine.GravityMultiplier * Time.deltaTime * Physics.gravity);
      }

      _stateMachine.Velocity = velocity;
    }

    protected void Move() {
      _stateMachine.CharacterController.Move(_stateMachine.Velocity * Time.deltaTime);
    }
  }
}
