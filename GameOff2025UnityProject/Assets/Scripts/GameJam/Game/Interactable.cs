using QuickOutline;

using UnityEngine;
using UnityEngine.Events;

namespace GameJam {
  public sealed class Interactable : MonoBehaviour {
    [field: Header("Interact")]
    [field: SerializeField]
    public bool CanInteract { get; set; } = true;

    [field: SerializeField]
    public float InteractRange { get; set; } = 5f;

    [field: Header("UI")]
    [field: SerializeField]
    public bool ShouldShowUI { get; set; } = true;

    [field: SerializeField]
    public string InteractText { get; set; } = "...";

    [field: SerializeField]
    public Sprite InteractIcon { get; set; }

    [field: Header("Highlight")]
    [field: SerializeField]
    public Outline HighlightOutline { get; set; }

    [field: SerializeField]
    public bool CanHighlight { get; set; } = true;

    [field: Header("Events"), Space(10f)]
    [field: SerializeField]
    public UnityEvent<GameObject> OnInteract { get; private set; }

    void Start() {
      ToggleHighlight(toggleOn: false);
    }

    public void ToggleHighlight(bool toggleOn) {
      if (HighlightOutline) {
        HighlightOutline.enabled = toggleOn && CanHighlight;
      }
    }

    public void Interact() {
      OnInteract?.Invoke(default);
    }

    public void Interact(GameObject interactAgent) {
      OnInteract?.Invoke(interactAgent);
    }
  }
}
