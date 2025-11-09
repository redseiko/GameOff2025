using UnityEngine;
using UnityEngine.Events;

namespace GameJam {
  public class CollideInteractable : MonoBehaviour {
    [field: Header("Events"), Space(10f)]
    [field: SerializeField]
    public UnityEvent<GameObject> OnCollide { get; private set; }

    [field: SerializeField]
    public UnityEvent<GameObject> OnStay { get; private set; }

    [field: SerializeField]
    public UnityEvent<GameObject> OnExit { get; private set; }

    void OnTriggerEnter(Collider collider) {
      if (collider.TryGetComponent(out CharacterController _)) {
        OnCollide?.Invoke(this.gameObject);
      }
    }

    private void OnTriggerStay(Collider collider) {
      if (collider.TryGetComponent(out CharacterController _)) {
        OnStay?.Invoke(this.gameObject);
      }
    }

    private void OnTriggerExit(Collider collider) {
      if (collider.TryGetComponent(out CharacterController _)) {
        OnExit?.Invoke(this.gameObject);
      }
    }
  }
}
