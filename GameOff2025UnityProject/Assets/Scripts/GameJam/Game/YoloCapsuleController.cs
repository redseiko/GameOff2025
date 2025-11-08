using UnityEngine;

namespace GameJam {
  public sealed class YoloCapsuleController : MonoBehaviour {
    [field: Header("CharacterController")]
    [field: SerializeField]
    public CharacterController CharacterController { get; private set; }

    [field: SerializeField]
    public bool ShouldDetectCollisions { get; private set; } = true;

    [field: Header("Push")]
    [field: SerializeField]
    public bool CapsuleCanPush { get; set; } = false;

    [field: SerializeField]
    public float CapsulePushMass { get; set; } = 60f;

    [field: SerializeField]
    public float CapsulePushForce { get; set; } = 0.1f;

    void Start() {
      CharacterController.detectCollisions = ShouldDetectCollisions;  
    }

    void OnControllerColliderHit(ControllerColliderHit hit) {
      if (!CapsuleCanPush) {
        return;
      }

      if (hit.collider.transform.root == transform.root) {
        return;
      }

      Rigidbody rigidbody = hit.collider.attachedRigidbody;

      if (!rigidbody || rigidbody.isKinematic) {
        return;
      }

      Vector3 pushDirection = new Vector3(hit.moveDirection.x, 0f, hit.moveDirection.z).normalized;

      float massRatio = CapsulePushMass / rigidbody.mass;
      massRatio = Mathf.Clamp(massRatio, 0.1f, 10f);

      rigidbody.AddForce(CapsulePushForce * massRatio * pushDirection, ForceMode.Impulse);
    }
  }
}
