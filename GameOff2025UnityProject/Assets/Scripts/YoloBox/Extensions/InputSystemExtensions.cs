using System;

using UnityEngine.InputSystem;

namespace YoloBox {
  public static class InputSystemExtensions {
    public static void OnActionPerformed(
        this InputActionProperty inputActionProperty, Action<InputAction.CallbackContext> callback) {
      inputActionProperty.action.performed += callback;
    }

    public static void OnActionPerformed(this InputActionProperty inputActionProperty, Action callback) {
      inputActionProperty.action.performed += _ => callback();
    }
  }
}
