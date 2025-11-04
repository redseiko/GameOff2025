using UnityEngine;

namespace GameJam {
  public sealed class FlavorText : MonoBehaviour {
    [field: Header("Popup")]
    [field: SerializeField, TextArea(minLines: 3, maxLines: 3)]
    public string PopupText { get; set; } = "...";

    [field: SerializeField, Min(0f)]
    public float PopupDuration { get; set; } = 4f;

    public void ShowPopupText() {
      UIManager.Instance.ShowPopupText(PopupText, PopupDuration);
    }
  }
}
