using DG.Tweening;

using UnityEngine;

namespace GameJam {
  public sealed class YoloDoor : MonoBehaviour {
    [field: SerializeField]
    public Transform DoorHinge { get; private set; }

    [field: Header("OpenDoor")]
    [field: SerializeField]
    public Vector3 OpenDoorRotation { get; set; } = new(0f, 90f, 0f);

    [field: SerializeField]
    public float OpenDoorDuration { get; set; } = 0.5f;

    [field: Header("State")]
    public bool IsDoorOpen { get; private set; } = false;

    public void OpenDoor() {
      DoorHinge.DOBlendableRotateBy(IsDoorOpen ? OpenDoorRotation * -1f : OpenDoorRotation, OpenDoorDuration);
      IsDoorOpen = !IsDoorOpen;
    }
  }
}
