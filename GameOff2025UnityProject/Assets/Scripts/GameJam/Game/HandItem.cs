using System.Collections.Generic;

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

    [field: Header("OverlapBox")]
    [field: SerializeField]
    public bool HasOverlapBox { get; private set; } = false;

    [field: SerializeField]
    public Vector3 OverlapBoxCenter { get; private set; }

    [field: SerializeField]
    public Vector3 OverlapBoxSize { get; private set; }

    [field: Header("State")]
    [field: SerializeField]
    public bool IsPickedUp { get; private set; } = false;

    readonly List<HandItem> _childHandItems = new();

    public void Pickup(GameObject interactAgent) {
      if (IsPickedUp) {
        return;
      }

      IsPickedUp = true;

      HandManager.Instance.FindChildHandItems(this, _childHandItems);

      foreach (HandItem childHandItem in _childHandItems) {
        childHandItem.AddParentConstraint(gameObject, shouldSetOffsets: false, shouldTweenWeight: false);
        childHandItem.SetIsKinematic(true);
        childHandItem.ToggleColliders(toggleOn: false);
      }

      AddParentConstraint(interactAgent, shouldSetOffsets: true, shouldTweenWeight: true);
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

      foreach (HandItem childHandItem in _childHandItems) {
        childHandItem.RemoveParentConstraint();
        childHandItem.SetIsKinematic(false);
        childHandItem.ToggleColliders(true);
      }

      _childHandItems.Clear();

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

    void AddParentConstraint(GameObject parent, bool shouldSetOffsets, bool shouldTweenWeight) {
      if (!gameObject.TryGetComponent(out ParentConstraint parentConstraint)) {
        parentConstraint = gameObject.AddComponent<ParentConstraint>();
      }

      int constraintIndex =
          parentConstraint.AddSource(
              new ConstraintSource() {
                sourceTransform = parent.transform,
                weight = 1f,
              });

      if (shouldSetOffsets) {
        parentConstraint.SetTranslationOffset(constraintIndex, PickupPositionOffset);
        parentConstraint.SetRotationOffset(constraintIndex, PickupRotationOffset);
        parentConstraint.rotationAxis = Axis.None;
      } else {
        // Copied from: https://discussions.unity.com/t/218717/4
        Matrix4x4 inverse = Matrix4x4.TRS(parent.transform.position, parent.transform.rotation, Vector3.one).inverse;
        parentConstraint.SetTranslationOffset(constraintIndex, inverse.MultiplyPoint3x4(transform.position));
        parentConstraint.SetRotationOffset(
            constraintIndex, (Quaternion.Inverse(parent.transform.rotation) * transform.rotation).eulerAngles);
      }

      parentConstraint.weight = shouldTweenWeight ? 0f : 1f;
      parentConstraint.constraintActive = true;
      parentConstraint.locked = true;

      if (shouldTweenWeight) {
        DOVirtual
            .Float(0f, 1f, 0.5f, value => parentConstraint.weight = value)
            .SetEase(Ease.InQuad)
            .SetTarget(parentConstraint)
            .SetLink(parentConstraint.gameObject);
      }
    }

    void RemoveParentConstraint() {
      if (!gameObject.TryGetComponent(out ParentConstraint parentConstraint)) {
        return;
      }

      DOTween.Complete(parentConstraint, withCallbacks: true);

      parentConstraint.weight = 0f;
      parentConstraint.constraintActive = false;

      Destroy(parentConstraint);
    }
  }
}
