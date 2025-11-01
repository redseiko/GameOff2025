using UnityEngine;

using YoloBox;

namespace GameJam {
  public sealed class UIManager : SingletonManager<UIManager> {
    [field: Header("Overlays")]
    [field: SerializeField]
    public MenuOverlayController MenuOverlay { get; private set; }

    public void ToggleMenu() {
      MenuOverlay.ToggleOverlay();
    }
  }
}
