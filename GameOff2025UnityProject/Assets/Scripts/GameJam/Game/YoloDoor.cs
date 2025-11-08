using System.Collections.Generic;

using DG.Tweening;

using UnityEngine;

namespace GameJam {
  public sealed class YoloDoor : MonoBehaviour {
    [field: SerializeField]
    public Transform DoorHinge { get; private set; }

    [field: SerializeField]
    public List<Collider> DoorColliders { get; private set; } = new();

    [field: Header("OpenDoor")]
    [field: SerializeField]
    public Vector3 OpenDoorRotation { get; set; } = new(0f, 90f, 0f);

    [field: SerializeField]
    public float OpenDoorDuration { get; set; } = 0.5f;

    [field: Header("State")]
    [field: SerializeField]
    public bool IsDoorOpen { get; private set; } = false;

    Sequence _openDoorSequence = default;

    public void OpenDoor() {
      if (_openDoorSequence.IsActive() && _openDoorSequence.IsComplete()) {
        return;
      }

      ToggleDoorColliders(toggleOn: false);

      Vector3 rotation = IsDoorOpen ? OpenDoorRotation * -1f : OpenDoorRotation;
      bool isDoorOpen = !IsDoorOpen;

      _openDoorSequence = DOTween.Sequence()
          .Append(DoorHinge.DOBlendableLocalRotateBy(rotation, OpenDoorDuration))
          .OnComplete(
              () => {
                ToggleDoorColliders(toggleOn: true);
                IsDoorOpen = isDoorOpen;
              });
    }

    void ToggleDoorColliders(bool toggleOn) {
      foreach (Collider collider in DoorColliders) {
        collider.enabled = toggleOn;
      }
    }
  }
}
