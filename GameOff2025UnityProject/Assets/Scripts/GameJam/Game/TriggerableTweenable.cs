using DG.Tweening;

using UnityEditor.Experimental.GraphView;

using UnityEngine;

using YoloBox;

namespace GameJam {
  public class TriggerableTweenable : MonoBehaviour {
    [field: SerializeField]
    public Vector3 TranslateBy { get; private set; } = Vector3.zero;

    [field: SerializeField]
    public Vector3 RotateBy { get; private set; } = Vector3.zero;

    [field: SerializeField]
    public Vector3 ScaleBy { get; private set; } = Vector3.zero;

    [field: SerializeField]
    public bool IsContinuous { get; private set; } = false;

    [field: SerializeField]
    public Ease EaseMethod { get; private set; } = Ease.Unset;

    Sequence _tweenSequence = default;

    public void TriggerTween(float duration) {
      if (_tweenSequence.IsActive()) {
        if (!IsContinuous && _tweenSequence.IsComplete()) {
          return;
        } else if (!_tweenSequence.IsPlaying()) {
          _tweenSequence.PlayForward();
        }
      } else {
        _tweenSequence = DOTween.Sequence()
          .Append(this.gameObject.transform.DOBlendableLocalMoveBy(TranslateBy, duration).SetEase(EaseMethod))
          .Join(this.gameObject.transform.DOBlendableLocalRotateBy(RotateBy, duration).SetEase(EaseMethod))
          .Join(this.gameObject.transform.DOBlendableScaleBy(ScaleBy, duration).SetEase(EaseMethod))
          .SetAutoKill(false)
          .SetUpdate(UpdateType.Fixed);

        if (IsContinuous) {
          _tweenSequence.AppendCallback(_tweenSequence.PlayAgain);
        }
      }
    }

    public void PauseTween() {
      if (!_tweenSequence.IsActive() || (!IsContinuous && _tweenSequence.IsComplete())) {
        return;
      }

      _tweenSequence.Pause();
    }

    public void OnDrawGizmosSelected() {
      Gizmos.color = new Color(0f, 0.75f, 0f, 0.25f);

      MeshFilter[] childMeshes = this.gameObject.GetComponentsInChildren<MeshFilter>();
      foreach (MeshFilter child in childMeshes) {
        Vector3 newScale = ScaleBy + new Vector3(1, 1, 1);
        Gizmos.DrawMesh(child.sharedMesh,
          this.gameObject.transform.position
          + Vector3.Scale(
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
