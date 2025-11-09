using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace YoloBox {
  public sealed class WebGlButton : Button {
    public override void OnPointerDown(PointerEventData eventData) {
      base.OnPointerDown(eventData);
      base.OnPointerClick(eventData);
    }

    public override void OnPointerClick(PointerEventData eventData) {
      // Do nothing.
    }
  }
}
