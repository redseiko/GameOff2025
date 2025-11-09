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
          .Append(this.gameObject.transform.DOBlendableLocalMoveBy(TranslateBy, duration))
          .Join(this.gameObject.transform.DOBlendableLocalRotateBy(RotateBy, duration).SetEase(Ease.Linear))
          .Join(this.gameObject.transform.DOBlendableScaleBy(ScaleBy, duration))
          .SetAutoKill(false)
          .SetUpdate(UpdateType.Fixed);

        if (IsContinuous) {
          _tweenSequence.AppendCallback(this.RepeatTween);
        }
      }
    }

    public void PauseTween() {
      if (!_tweenSequence.IsActive() || (!IsContinuous && _tweenSequence.IsComplete())) {
        return;
      }

      _tweenSequence.Pause();
    }

    private void RepeatTween() {
      _tweenSequence.PlayAgain();
    }
  }
}
