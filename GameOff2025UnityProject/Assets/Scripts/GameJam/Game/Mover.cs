using DG.Tweening;

using UnityEditor.Experimental.GraphView;

using UnityEngine;

using YoloBox;

namespace GameJam {
  public class Mover : MonoBehaviour {
    [field: Header("Movement")]
    [field: SerializeField]
    public Vector3 TranslateBy { get; private set; } = Vector3.zero;

    [field: SerializeField]
    public Vector3 RotateBy { get; private set; } = Vector3.zero;

    [field: SerializeField]
    public Vector3 ScaleBy { get; private set; } = Vector3.zero;

    [field: SerializeField]
    public float Duration { get; private set; } = 1f;

    [field: Header("Behaviour")]
    [field: SerializeField]
    public bool Continuous { get; private set; } = false;

    [field: SerializeField]
    public bool AutoStart { get; private set; } = false;

    [field: SerializeField]
    public Ease EaseMethod { get; private set; } = Ease.Unset;

    public enum CompleteAction {
      // Complete the movement and stop
      End,

      // Begin moving backwards and end once it returns to the beginning
      Reverse,
    }

    [field: SerializeField]
    public CompleteAction OnComplete { get; private set; } = CompleteAction.End;

    Sequence _tweenSequence = default;

    public void Start() {
      if (AutoStart) {
        TriggerTween();
      }
    }

    public void TriggerTween() {
      TriggerTween(Duration);
    }

    public void TriggerTween(float duration) {
      if (_tweenSequence.IsActive()) {
        if (!Continuous && _tweenSequence.IsComplete()) {
          return;
        } else if (!_tweenSequence.IsPlaying() || _tweenSequence.IsBackwards()) {
          _tweenSequence.PlayForward();
        }
      } else {
        _tweenSequence = DOTween.Sequence()
          .Append(this.gameObject.transform.DOBlendableLocalMoveBy(TranslateBy, duration).SetEase(EaseMethod))
          .Join(this.gameObject.transform.DOBlendableLocalRotateBy(RotateBy, duration).SetEase(EaseMethod))
          .Join(this.gameObject.transform.DOBlendableScaleBy(ScaleBy, duration).SetEase(EaseMethod))
          .SetAutoKill(false)
          .SetUpdate(UpdateType.Fixed);

        if (Continuous) {
          _tweenSequence.SetLoops(-1,
            OnComplete == CompleteAction.Reverse ? LoopType.Yoyo : LoopType.Incremental);
        }
      }
    }

    public void PauseTween() {
      if (!_tweenSequence.IsActive() || (!Continuous && _tweenSequence.IsComplete())) {
        return;
      }

      _tweenSequence.Pause();
    }

    public void RewindTween() {
      if (!_tweenSequence.IsActive()) {
        return;
      }

      _tweenSequence.PlayBackwards();
    }

    public void OnDrawGizmosSelected() {
      Gizmos.color = new Color(0f, 0.75f, 0f, 0.25f);

      MeshFilter[] childMeshes = this.gameObject.GetComponentsInChildren<MeshFilter>();
      foreach (MeshFilter child in childMeshes) {
        Vector3 newScale = ScaleBy + new Vector3(1, 1, 1);
        Gizmos.DrawMesh(child.sharedMesh,
          Vector3.Scale(
            child.transform.localPosition,
            newScale)
          + TranslateBy,
          this.gameObject.transform.localRotation,
          Vector3.Scale(
            Vector3.Scale(
              this.gameObject.transform.localScale,
              child.transform.localScale),
            newScale));
      }
    }
  }
}
