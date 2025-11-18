using UnityEngine;

namespace GameJam {
  public sealed class WheelControl : MonoBehaviour {
    [field: SerializeField]
    public Transform WheelTransform { get; private set; }

    [field: SerializeField]
    public WheelCollider WheelCollider { get; private set; }

    [field: SerializeField]
    public bool IsSteerable { get; set; }

    [field: SerializeField]
    public bool IsMotorized { get; set; }

    Vector3 _position;
    Quaternion _rotation;

    void Update() {
      WheelCollider.GetWorldPose(out _position, out _rotation);
      WheelTransform.position = _position;     
    }
  }
}
