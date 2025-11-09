using UnityEngine;

namespace GameJam {
  public sealed class YoloLamp : MonoBehaviour {
    [field: Header("Light")]
    [field: SerializeField]
    public Light LampLight { get; private set; }

    [field: SerializeField]
    public GameObject LampBulb { get; private set; }

    [field: Header("State")]
    [field: SerializeField]
    public bool IsLightOn { get; private set; } = true;

    public void ToggleLampLight() {
      ToggleLampLight(!IsLightOn);
    }

    public void ToggleLampLight(bool toggleOn) {
      LampLight.enabled = toggleOn;
      LampBulb.SetActive(toggleOn);
      IsLightOn = toggleOn;
    }
  }
}
