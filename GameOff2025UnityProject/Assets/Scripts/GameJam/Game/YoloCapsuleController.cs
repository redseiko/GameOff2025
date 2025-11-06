using UnityEngine;

namespace GameJam {
  public sealed class YoloCapsuleController : MonoBehaviour {
    [field: SerializeField]
    public float CapsuleMass { get; set; } = 80f;

    [field: SerializeField]
    public bool CapsuleCanPush { get; set; } = false;

    [field: SerializeField]
    public float CapsulePushForce { get; set; } = 0.1f;

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

      float massRatio = CapsuleMass / rigidbody.mass;
      massRatio = Mathf.Clamp(massRatio, 0.1f, 10f);

      rigidbody.AddForce(CapsulePushForce * massRatio * pushDirection, ForceMode.Impulse);
    }
  }
}
