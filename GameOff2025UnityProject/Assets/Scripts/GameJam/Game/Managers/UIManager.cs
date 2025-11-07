using UnityEngine;
using UnityEngine.InputSystem;

using YoloBox;

namespace GameJam {
  public sealed class UIManager : SingletonManager<UIManager> {
    [field: Header("Overlays")]
    [field: SerializeField]
    public MenuOverlayController MenuOverlay { get; private set; }

    [field: Header("Panels")]
    [field: SerializeField]
    public DialogPanelController DialogPanel { get; private set; }

    [field: SerializeField]
    public InteractPanelController InteractPanel { get; private set; }

    [field: SerializeField]
    public PopupPanelController PopupPanel { get; private set; }

    public void ProcessToggleMenuAction(InputAction.CallbackContext context = default) {
      ToggleMenu();
    }

    public void ToggleMenu() {
      MenuOverlay.ToggleOverlay();
    }

    public bool ShouldUnlockCursor() {
      return MenuOverlay.IsOverlayVisible || DialogPanel.IsPanelVisible;
    }

    public void SetInteractable(Interactable interactable, bool forceRefresh = false) {
      InteractPanel.SetInteractable(interactable, forceRefresh);
    }

    public void ShowPopupText(string popupText, float duration) {
      PopupPanel.ShowPopupText(popupText, duration);
    }
  }
}
