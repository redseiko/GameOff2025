using UnityEngine;
using UnityEngine.Events;

namespace GameJam
{
    public class CollideInteractable : MonoBehaviour
    {
    [field: Header("Events"), Space(10f)]
    [field: SerializeField]
    public UnityEvent<GameObject> OnCollide { get; private set; }

    void OnTriggerEnter(Collider collider) {
      if (collider.TryGetComponent(out CharacterController _)) {
        OnCollide.Invoke(this.gameObject);
      }
    }
  }
}
