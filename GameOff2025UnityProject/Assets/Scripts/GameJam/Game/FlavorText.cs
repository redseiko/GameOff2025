using System;

using UnityEngine;

namespace GameJam {
  public sealed class FlavorText : MonoBehaviour {
    [field: Header("Popup")]
    [field: SerializeField, TextArea(minLines: 3, maxLines: 3)]
    public string[] PopupTexts { get; set; } = Array.Empty<string>();

    [field: SerializeField, Min(0f)]
    public float PopupDuration { get; set; } = 4f;

    public void ShowPopupText(int textIndex = 0) {
      UIManager.Instance.ShowPopupText(PopupTexts[textIndex], PopupDuration);
    }
  }
}
