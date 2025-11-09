using UnityEngine;
using UnityEngine.InputSystem;

using YoloBox;

namespace GameJam {
  public sealed class InputManager : SingletonManager<InputManager> {
    [field: Header("Actions")]
    [field: SerializeField]
    public InputActionProperty InteractAction { get; private set; }

    [field: SerializeField]
    public InputActionProperty HoldInteractAction { get; private set; }

    [field: SerializeField]
    public InputActionProperty PickupAction { get; private set; }

    [field: SerializeField]
    public InputActionProperty ToggleMenuAction { get; private set; }

    [field: Header("State")]
    [field: SerializeField]
    public bool IsCursorLocked { get; private set; }

    void Start() {
      InteractAction.action.performed += InteractManager.Instance.ProcessInteractAction;

      HoldInteractAction.action.performed += InteractManager.Instance.ProcessHoldInteractActionStart;
      HoldInteractAction.action.canceled += InteractManager.Instance.ProcessHoldInteractActionEnd;

      PickupAction.action.performed += HandManager.Instance.ProcessPickupAction;
      ToggleMenuAction.action.performed += UIManager.Instance.ProcessToggleMenuAction;
    }

    void LateUpdate() {
      UpdateCursorLockState();
    }

    void UpdateCursorLockState() {
      if (ShouldUnlockCursor()) {
        UnlockCursor();
      } else {
        LockCursor();
      }
    }

    public bool ShouldUnlockCursor() {
      return UIManager.Instance.ShouldUnlockCursor();
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
