using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

using YoloBox;

namespace GameJam {
  public sealed class HandManager : SingletonManager<HandManager> {
    [field: Header("State")]
    [field: SerializeField]
    public List<Interactable> CurrentInteractables { get; private set; } = new();

    public void ProcessPickupAction(InputAction.CallbackContext context = default) {
      if (CurrentInteractables.Count > 0) {
        DropCurrentInteractable(out _);
        return;
      }

      Interactable interactable = InteractManager.Instance.ClosestInteractable;

      if (interactable
          && CurrentInteractables.Count <= 0
          && interactable.TryGetComponent(out HandItem handItem)
          && handItem.CanPickup) {
        PickupCurrentInteractable(interactable);
      }
    }

    public bool HasCurrentInteractable() {
      return CurrentInteractables.Count > 0;
    }

    public bool GetCurrentInteractable(out Interactable interactable) {
      if (CurrentInteractables.Count > 0) {
        interactable = CurrentInteractables[0];
        return true;
      }

      interactable = default;
      return false;
    }

    public void PickupCurrentInteractable(Interactable interactable) {
      PickupInteractable(interactable);
      CurrentInteractables.Add(interactable);
    }

    public void PickupInteractable(Interactable interactable) {
      interactable.CanInteract = false;

      if (interactable.TryGetComponent(out HandItem handItem)) {
        handItem.Pickup(InteractManager.Instance.InteractAgent);
      }
    }

    public bool DropCurrentInteractable(out Interactable interactable) {
      if (CurrentInteractables.Count <= 0) {
        interactable = default;
        return false;
      }

      interactable = CurrentInteractables[0];
      CurrentInteractables.RemoveAt(0);

      DropInteractable(interactable);

      return true;
    }

    public void DropInteractable(Interactable interactable) {
      if (interactable.TryGetComponent(out HandItem handItem)) {
        handItem.Drop();
      }

      interactable.CanInteract = true;
    }
  }
}
