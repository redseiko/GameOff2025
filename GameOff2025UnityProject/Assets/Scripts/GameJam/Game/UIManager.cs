using UnityEngine;

using YoloBox;

namespace GameJam {
  public sealed class UIManager : SingletonManager<UIManager> {
    [field: Header("Overlays")]
    [field: SerializeField]
    public MenuOverlayController MenuOverlay { get; private set; }

    [field: Header("Panels")]
    [field: SerializeField]
    public DialogPanelController DialogPanel { get; private set; }

    public bool ShouldUnlockCursor() {
      return MenuOverlay.IsOverlayVisible || DialogPanel.IsPanelVisible;
    }

    public void ToggleMenu() {
      MenuOverlay.ToggleOverlay();
    }
  }
}
