using DG.Tweening;

using UnityEngine;

namespace GameJam {
  public sealed class MenuOverlayController : MonoBehaviour {
    [field: Header("Overlay")]
    [field: SerializeField]
    public RectTransform OverlayRectTransform { get; private set; }

    [field: SerializeField]
    public CanvasGroup OverlayCanvasGroup { get; private set; }

    [field: Header("State")]
    [field: SerializeField]
    public bool IsOverlayVisible { get; private set; }

    void Start() {
      ResetOverlay();
    }

    public void ResetOverlay() {
      OverlayCanvasGroup.alpha = 0f;
      OverlayCanvasGroup.blocksRaycasts = false;
      IsOverlayVisible = false;
    }

    public void ToggleOverlay() {
      if (IsOverlayVisible) {
        HideOverlay();
      } else {
        ShowOverlay();
      }
    }

    public void ShowOverlay() {
      OverlayCanvasGroup.DOComplete(withCallbacks: true);

      OverlayCanvasGroup.blocksRaycasts = true;
      IsOverlayVisible = true;

      OverlayCanvasGroup
          .DOFade(1f, 0.5f)
          .SetUpdate(isIndependentUpdate: true);
    }

    public void HideOverlay() {
      OverlayCanvasGroup.DOComplete(withCallbacks: true);

      OverlayCanvasGroup.blocksRaycasts = false;
      IsOverlayVisible = false;

      OverlayCanvasGroup
          .DOFade(0f, 0.5f)
          .SetUpdate(isIndependentUpdate: true);
    }
  }
}
