using System;

using DG.Tweening;

using UnityEngine;

namespace GameJam {
  public sealed class YoloDrawer : MonoBehaviour {
    [field: SerializeField]
    public Transform Drawer { get; private set; }

    [field: SerializeField]
    public Collider[] DrawerColliders { get; private set; } = Array.Empty<Collider>();

    [field: Header("OpenDrawer")]
    [field: SerializeField]
    public Vector3 OpenDrawerTranslation { get; set; } = Vector3.zero;

    [field: SerializeField, Min(0f)]
    public float OpenDrawerDuration { get; set; } = 0.5f;

    [field: Header("State")]
    [field: SerializeField]
    public bool IsDrawerOpen { get; private set; } = false;

    Sequence _openDrawerSequence = default;

    public void OpenDrawer(GameObject interactAgent = default) {
      if (_openDrawerSequence.IsActive() && !_openDrawerSequence.IsComplete()) {
        return;
      }

      ToggleDrawerColliders(interactAgent, toggleOn: false);

      Vector3 translation = IsDrawerOpen ? -OpenDrawerTranslation : OpenDrawerTranslation;
      bool isDrawerOpen = !IsDrawerOpen;

      _openDrawerSequence =
          DOTween.Sequence()
              .Append(Drawer.DOBlendableLocalMoveBy(translation, OpenDrawerDuration))
              .OnComplete(
                  () => {
                    ToggleDrawerColliders(interactAgent, toggleOn: true);
                    IsDrawerOpen = isDrawerOpen;
                  })
              .SetLink(Drawer.gameObject);
    }

    void ToggleDrawerColliders(GameObject interactAgent, bool toggleOn) {
      if (!interactAgent || !interactAgent.TryGetComponent(out Collider agentCollider)) {
        return;
      }

      foreach (Collider collider in DrawerColliders) {
        Physics.IgnoreCollision(collider, agentCollider, ignore: !toggleOn);
      }
    }
  }
}
