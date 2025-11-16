using System.Collections;

using TMPro;

using UnityEngine;

namespace GameJam {
  public sealed class FPSPanelController : MonoBehaviour {
    [field: Header("UI")]
    [field: SerializeField]
    public TextMeshProUGUI FPSLabel { get; private set; }

    void Start() {
      StartCoroutine(UpdateFPS());
    }

    IEnumerator UpdateFPS() {
      WaitForSeconds waitInterval = new(seconds: 1f);

      while (true) {
        yield return waitInterval;
        FPSLabel.text = Mathf.Ceil(1f / Time.unscaledDeltaTime).ToString();
      }
    }
  }
}
