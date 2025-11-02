using UnityEngine;
using UnityEngine.InputSystem;

namespace GameJam {
  public sealed class PlayerStateMachine : StateMachine {
    [field: Header("Actions")]
    [field: SerializeField]
    public InputActionProperty MoveAction { get; private set; }

    [field: SerializeField]
    public InputActionProperty JumpAction { get; private set; }

    [field: Header("Movement")]
    [field: SerializeField]
    public Vector3 Velocity { get; set; }

    [field: SerializeField]
    public float MovementSpeed { get; private set; } = 5f;

    [field: SerializeField]
    public float JumpForce { get; private set; } = 5f;

    [field: SerializeField]
    public float LookRotationDampFactor { get; private set; } = 10f;

    [field: SerializeField]
    [field: Range(0f, 2f)]
    public float GravityMultiplier { get; set; } = 2f;

    [field: SerializeField]
    public Transform MainCamera { get; private set; }

    [field: SerializeField]
    public CharacterController CharacterController { get; private set; }

    void Start() {
      SwitchState(new PlayerMoveState(this));
    }
  }
}
