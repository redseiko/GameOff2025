using DG.Tweening;

using UnityEngine;
using UnityEngine.Animations;

namespace GameJam {
  public sealed class HandItem : MonoBehaviour {
    [field: Header("Pickup")]
    [field: SerializeField]
    public Vector3 PickupPositionOffset { get; set; } = Vector3.zero;

    [field: SerializeField]
    public Vector3 PickupRotationOffset { get; set; } = Vector3.zero;

    [field: SerializeField]
    public bool CanPickup { get; set; } = true;

    [field: Header("State")]
    [field: SerializeField]
    public bool IsPickedUp { get; private set; } = false;

    int _constraintIndex = -1;

    public void Pickup(GameObject interactAgent) {
      if (IsPickedUp) {
        return;
      }

      IsPickedUp = true;

      AddParentConstraint(interactAgent);
      SetIsKinematic(true);
      ToggleColliders(toggleOn: false);
    }

    public void Drop() {
      if (!IsPickedUp) {
        return;
      }

      RemoveParentConstraint();
      SetIsKinematic(false);
      ToggleColliders(toggleOn: true);

      IsPickedUp = false;
    }

    void SetIsKinematic(bool isKinematic) {
      foreach (Rigidbody rigidbody in GetComponentsInChildren<Rigidbody>()) {
        rigidbody.isKinematic = isKinematic;
      }
    }

    void ToggleColliders(bool toggleOn) {
      foreach (Collider collider in GetComponentsInChildren<Collider>()) {
        collider.enabled = toggleOn;
      }
    }

    void AddParentConstraint(GameObject interactAgent) {
      if (!gameObject.TryGetComponent(out ParentConstraint parentConstraint)) {
        parentConstraint = gameObject.AddComponent<ParentConstraint>();
      }

      _constraintIndex =
          parentConstraint.AddSource(
              new ConstraintSource() {
                sourceTransform = interactAgent.transform,
                weight = 1f,
              });

      parentConstraint.SetTranslationOffset(_constraintIndex, PickupPositionOffset);
      parentConstraint.SetRotationOffset(_constraintIndex, PickupRotationOffset);
      parentConstraint.weight = 0f;
      parentConstraint.constraintActive = true;

      DOVirtual
          .Float(0f, 1f, 0.5f, value => parentConstraint.weight = value)
          .SetEase(Ease.InQuad)
          .SetTarget(parentConstraint)
          .SetLink(parentConstraint.gameObject);
    }

    void RemoveParentConstraint() {
      if (!gameObject.TryGetComponent(out ParentConstraint parentConstraint) || _constraintIndex < 0) {
        return;
      }

      DOTween.Complete(parentConstraint, withCallbacks: true);

      parentConstraint.RemoveSource(_constraintIndex);
      _constraintIndex = -1;

      if (parentConstraint.sourceCount <= 0) {
        Destroy(parentConstraint);
      }
    }
  }
}
