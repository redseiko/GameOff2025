using System;

using UnityEngine;
using UnityEngine.InputSystem;

using YoloBox;

namespace GameJam {
  public sealed class InteractManager : SingletonManager<InteractManager> {
    [field: Header("Agent")]
    [field: SerializeField]
    public GameObject InteractAgent { get; private set; }

    [field: SerializeField]
    public GameObject InteractCamera { get; private set; }

    [field: Header("Raycast")]
    [field: SerializeField, Min(0f)]
    public float InteractRange { get; set; } = 4f;

    [field: SerializeField, Min(0f)]
    public float SphereCastRadius { get; private set; } = 0.1f;

    [field: SerializeField]
    public LayerMask SphereCastLayerMask { get; private set; }

    [field: Header("Interact")]
    [field: SerializeField]
    public bool CanInteract { get; set; } = true;

    [field: SerializeField]
    public Interactable ClosestInteractable { get; private set; }

    void FixedUpdate() {
      UpdateRaycastHits(InteractAgent, InteractCamera, InteractRange);
      UpdateClosestInteractable();
    }

    public void ProcessInteractAction(InputAction.CallbackContext context = default) {
      Interactable interactable = ClosestInteractable;

      if (interactable) {
        interactable.Interact(InteractAgent);
      }
    }

    int _raycastHitsCount = 0;
    readonly RaycastHit[] _raycastHits = new RaycastHit[25];
    readonly float[] _hitDistanceCache = new float[25];

    void UpdateRaycastHits(GameObject agent, GameObject camera, float range) {
      if (!agent) {
        _raycastHitsCount = 0;
        return;
      }

      Transform origin = camera.transform;

      // TODO: remove me later.
      Debug.DrawRay(origin.position, origin.forward * InteractRange, Color.red);

      _raycastHitsCount =
        Physics.SphereCastNonAlloc(
          origin.position,
          SphereCastRadius,
          origin.forward,
          _raycastHits,
          range,
          SphereCastLayerMask,
          QueryTriggerInteraction.Ignore);

      for (int i = 0; i < _raycastHitsCount; i++) {
        _hitDistanceCache[i] = _raycastHits[i].distance;
      }

      Array.Sort(_hitDistanceCache, _raycastHits, 0, _raycastHitsCount);
    }

    void UpdateClosestInteractable() {
      Interactable interactable = CanInteract ? GetClosestInteractable() : default;

      if (interactable != ClosestInteractable) {
        if (ClosestInteractable) {
          ClosestInteractable.ToggleHighlight(toggleOn: false);
        }

        if (interactable) {
          interactable.ToggleHighlight(toggleOn: true);
        }

        ClosestInteractable = interactable;
      }

      UIManager.Instance.SetInteractable(ClosestInteractable);
    }

    Interactable GetClosestInteractable() {
      for (int i = 0; i < _raycastHitsCount; i++) {
        RaycastHit raycastHit = _raycastHits[i];
        Interactable interactable = raycastHit.collider.GetComponentInParent<Interactable>();

        if (!interactable) {
          return default;
        }

        if (interactable
            && interactable.enabled
            && interactable.CanInteract
            && raycastHit.distance <= interactable.InteractRange) {
          return interactable;
        }
      }

      return default;
    }
  }
}
