using DG.Tweening;

using UnityEngine;

namespace GameJam {
  public sealed class DialogPanelController : MonoBehaviour {
    [field: Header("Panel")]
    [field: SerializeField]
    public RectTransform PanelRectTransform { get; private set; }

    [field: SerializeField]
    public CanvasGroup PanelCanvasGroup { get; private set; }

    [field: Header("State")]
    public bool IsPanelVisible { get; private set; }

    public void ResetPanel() {
      PanelCanvasGroup.alpha = 0f;
      PanelCanvasGroup.blocksRaycasts = false;

      IsPanelVisible = false;
    }

    public void TogglePanel() {
      if (IsPanelVisible) {
        HidePanel();
      } else {
        ShowPanel();
      }
    }

    public void ShowPanel() {
      PanelCanvasGroup.DOComplete(withCallbacks: true);

      PanelCanvasGroup.blocksRaycasts = true;
      IsPanelVisible = true;

      PanelCanvasGroup
          .DOFade(1f, 0.4f)
          .SetUpdate(isIndependentUpdate: true);
    }

    public void HidePanel() {
      PanelCanvasGroup.DOComplete(withCallbacks: false);

      PanelCanvasGroup.blocksRaycasts = false;
      IsPanelVisible = false;

      PanelCanvasGroup
          .DOFade(0f, 0.4f)
          .SetUpdate(isIndependentUpdate: true);
    }
  }
}
