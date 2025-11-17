using System.Collections;

using TMPro;

using UnityEngine;

namespace GameJam {
  public sealed class FPSPanelController : MonoBehaviour {
    [field: Header("UI")]
    [field: SerializeField]
    public TextMeshProUGUI FPSLabel { get; private set; }

    [field: Header("FPS")]
    [field: SerializeField]
    public float SmoothSpeed { get; set; } = 1f;

    float _fps = 0f;
    float _smoothFps = 0f;

    void Start() {
      StartCoroutine(UpdateFPS());
    }

    void Update() {
      _fps = 1f / Time.unscaledDeltaTime;

      if (Time.timeSinceLevelLoad < 0.1f) {
        _smoothFps = _fps;
      }

      _smoothFps += (_fps - _smoothFps) * Mathf.Clamp(Time.unscaledDeltaTime * SmoothSpeed, 0f, 1f);
    }

    IEnumerator UpdateFPS() {
      WaitForSeconds waitInterval = new(seconds: 1f);

      while (true) {
        yield return waitInterval;
        FPSLabel.text = Mathf.Ceil(_smoothFps).ToString();
      }
    }
  }
}
