using UnityEngine;

namespace GameJam {
  public class YoloDebuggable : MonoBehaviour {
    [field: SerializeField]
    public string Message { get; private set; }

    public void Log() {
      Debug.Log(Message);
    }
  }
}
