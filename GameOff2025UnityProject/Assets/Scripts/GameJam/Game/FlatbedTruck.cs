using UnityEngine;
using UnityEngine.Animations;

namespace GameJam {
  public sealed class FlatbedTruck : MonoBehaviour {
    [field: Header("Physics")]
    [field: SerializeField]
    public Rigidbody TruckRigidbody { get; private set; }

    [field: SerializeField]
    public float MotorForce { get; set; } = 10f;

    [field: SerializeField]
    public float SteerForce { get; set; } = 50f;

    [field: SerializeField]
    public float MaxVelocity { get; set; } = 10f;

    [field: Header("State")]
    [field: SerializeField]
    public bool CanMoveForward { get; private set; } = false;

    [field: SerializeField]
    public float CurrentVelocity { get; private set; }

    public void ToggleTruck(GameObject interactAgent = default) {
      CanMoveForward = !CanMoveForward;

      if (interactAgent) {
        ToggleParentConstraint(interactAgent);
      }
    }

    void ToggleParentConstraint(GameObject interactAgent) {
      if (CanMoveForward) {
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
        if (interactAgent.TryGetComponent(out ParentConstraint parentConstraint)) {
          Destroy(parentConstraint);
        }
      }
    }

    void FixedUpdate() {
      CurrentVelocity = TruckRigidbody.linearVelocity.magnitude;

      if (CanMoveForward && CurrentVelocity < MaxVelocity) {
        TruckRigidbody.AddRelativeForce(Vector3.forward * MotorForce, ForceMode.Acceleration);
      }
    }
  }
}
