using UnityEngine;
using UnityEngine.InputSystem;

using YoloBox;

namespace GameJam {
  public sealed class InputManager : SingletonManager<InputManager> {
    [field: Header("Actions")]
    [field: SerializeField]
    public InputActionProperty InteractAction { get; private set; }

    [field: SerializeField]
    public InputActionProperty ToggleMenuAction { get; private set; }

    [field: Header("State")]
    [field: SerializeField]
    public bool IsCursorLocked { get; private set; }

    void OnEnable() {
      InteractAction.action.performed += ProcessInteractAction;
      ToggleMenuAction.action.performed += ProcessToggleMenuAction;
    }

    void OnDisable() {
      InteractAction.action.performed -= ProcessInteractAction;
      ToggleMenuAction.action.performed -= ProcessToggleMenuAction;
    }

    public void ProcessInteractAction(InputAction.CallbackContext context = default) {
      Debug.Log($"Interact?");
    }

    public void ProcessToggleMenuAction(InputAction.CallbackContext context = default) {
      Debug.Log($"ToggleMenu?");
    }

    void LateUpdate() {
      UpdateCursorLockState();
    }

    void UpdateCursorLockState() {
      if (ShouldLockCursor()) {
        LockCursor();
      } else {
        UnlockCursor();
      }
    }

    public bool ShouldLockCursor() {
      return true;
    }

    public void LockCursor() {
      IsCursorLocked = true;

      if (Cursor.lockState != CursorLockMode.Locked) {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
      }
    }

    public void UnlockCursor() {
      IsCursorLocked = false;

      if (Cursor.lockState != CursorLockMode.Confined) {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
      }
    }
  }
}
