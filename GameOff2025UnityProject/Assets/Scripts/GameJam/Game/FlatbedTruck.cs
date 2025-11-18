using System;

using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;

namespace GameJam {
  public sealed class FlatbedTruck : MonoBehaviour {
    [field: Header("Physics")]
    [field: SerializeField]
    public Rigidbody TruckRigidbody { get; private set; }

    [field: Header("Vehicle")]
    [field: SerializeField]
    public WheelControl[] Wheels { get; private set; } = Array.Empty<WheelControl>();

    [field: SerializeField]
    public float MotorTorque { get; set; } = 2000f;

    [field: SerializeField]
    public float BrakeTorque { get; set; } = 2000f;

    [field: SerializeField]
    public float MaxSpeed { get; set; } = 20f;

    [field: SerializeField]
    public float SteeringRange { get; set; } = 30f;

    [field: SerializeField]
    public float SteeringRangeAtMaxSpeed { get; set; } = 10f;

    [field: SerializeField]
    public InputActionProperty CarMovementInput { get; private set; }

    [field: Header("State")]
    [field: SerializeField]
    public bool CanMoveForward { get; private set; } = false;

    [field: SerializeField]
    public Vector2 InputVector { get; private set; }

    [field: SerializeField]
    public GameObject CurrentAttachedChild { get; private set; }

    public void ToggleTruck(GameObject interactAgent = default) {
      CanMoveForward = !CanMoveForward;

      if (interactAgent) {
        if (CanMoveForward && !CurrentAttachedChild) {
          ToggleParentConstraint(interactAgent);
        } else if (!CanMoveForward && CurrentAttachedChild) {
          ToggleParentConstraint(interactAgent);
        }
      }
    }

    public void ToggleParentConstraint(GameObject interactAgent) {
      if (!CurrentAttachedChild) {
        CurrentAttachedChild = interactAgent;

        if (!interactAgent.TryGetComponent(out ParentConstraint parentConstraint)) {
          parentConstraint = interactAgent.AddComponent<ParentConstraint>();
        }

        int constraintIndex =
            parentConstraint.AddSource(
                new ConstraintSource() {
                  sourceTransform = TruckRigidbody.transform,
                  weight = 1f,
                });

        Matrix4x4 inverse =
            Matrix4x4.TRS(TruckRigidbody.transform.position, TruckRigidbody.transform.rotation, Vector3.one).inverse;
        parentConstraint.SetTranslationOffset(
            constraintIndex, inverse.MultiplyPoint3x4(interactAgent.transform.position));
        parentConstraint.SetRotationOffset(
            constraintIndex, (Quaternion.Inverse(TruckRigidbody.transform.rotation) * transform.rotation).eulerAngles);

        parentConstraint.weight = 1f;
        parentConstraint.constraintActive = true;
        parentConstraint.locked = true;
      } else {
        if (CurrentAttachedChild.TryGetComponent(out ParentConstraint parentConstraint)) {
          Destroy(parentConstraint);
        }

        CurrentAttachedChild = default;
      }
    }

    void FixedUpdate() {
      if (!CanMoveForward) {
        return;
      }

      Vector2 inputVector = CarMovementInput.action.ReadValue<Vector2>();
      InputVector = inputVector;

      float vInput = inputVector.y;
      float hInput = inputVector.x;

      float forwardSpeed = Vector3.Dot(transform.forward, TruckRigidbody.linearVelocity);
      float speedFactor = Mathf.InverseLerp(0f, MaxSpeed, Mathf.Abs(forwardSpeed));

      float currentMotorTorque = Mathf.Lerp(MotorTorque, 0, speedFactor);
      float currentSteerRange = Mathf.Lerp(SteeringRange, SteeringRangeAtMaxSpeed, speedFactor);

      bool isAccelerating = Mathf.Sign(vInput) == Mathf.Sign(forwardSpeed);

      foreach (WheelControl wheel in Wheels) {
        if (wheel.IsSteerable) {
          wheel.WheelCollider.steerAngle = hInput * currentSteerRange;
        }

        if (isAccelerating) {
          if (wheel.IsMotorized) {
            wheel.WheelCollider.motorTorque = vInput * currentMotorTorque;
          }

          wheel.WheelCollider.brakeTorque = 0f;
        } else {
          wheel.WheelCollider.motorTorque = 0f;
          wheel.WheelCollider.brakeTorque = Mathf.Abs(vInput) * BrakeTorque;
        }
      }
    }
  }
}
