using DG.Tweening;

using TMPro;

using UnityEngine;

namespace GameJam {
  public sealed class PopupPanelController : MonoBehaviour {
    [field: Header("Panel")]
    [field: SerializeField]
    public RectTransform PanelRectTransform { get; private set; }

    [field: SerializeField]
    public CanvasGroup PanelCanvasGroup { get; private set; }

    [field: Header("Popup")]
    [field: SerializeField]
    public TextMeshProUGUI PopupText { get; private set; }

    [field: Header("State")]
    [field: SerializeField]
    public bool IsPanelVisible { get; private set; }

    void Start() {
      ResetPanel();
    }

    public void ResetPanel() {
      PanelCanvasGroup.alpha = 0f;
      PanelCanvasGroup.blocksRaycasts = false;

      IsPanelVisible = false;
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
      PanelCanvasGroup.DOComplete(withCallbacks: true);

      PanelCanvasGroup.blocksRaycasts = false;
      IsPanelVisible = false;

      PanelCanvasGroup
          .DOFade(0f, 0.2f)
          .SetUpdate(isIndependentUpdate: true);
    }

    public void ShowPopupText(string popupText, float popupDuration) {
      DOTween.Complete(gameObject, withCallbacks: false);

      PopupText.text = popupText;

      DOTween.Sequence()
          .InsertCallback(0f, ShowPanel)
          .Insert(0f, PopupText.transform.DOPunchPosition(new(0f, -10f, 0f), 0.4f, 0, 0f))
          .InsertCallback(popupDuration, HidePanel)
          .SetTarget(gameObject)
          .SetLink(gameObject);
    }
  }
}
