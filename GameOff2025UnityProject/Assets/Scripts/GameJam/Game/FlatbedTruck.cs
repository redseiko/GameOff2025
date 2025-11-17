using UnityEngine;

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
    }

    void FixedUpdate() {
      CurrentVelocity = TruckRigidbody.linearVelocity.magnitude;

      if (CanMoveForward && CurrentVelocity < MaxVelocity) {
        TruckRigidbody.AddRelativeForce(Vector3.forward * MotorForce, ForceMode.Acceleration);
      }
    }
  }
}
