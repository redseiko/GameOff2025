using DG.Tweening;

using TMPro;

using UnityEngine;

namespace YoloBox {
  public static class DOTWeenExtensions {
    public static void PlayComplete(this Tween tween) {
      tween.Complete();
      tween.Restart();
    }

    public static void PlayAgain(this Tween tween) {
      if (tween.IsPlaying()) {
        tween.PlayForward();
      } else {
        tween.Restart();
      }
    }

    public static void PlayOrRewind(this Tween tween, bool condition) {
      if (condition) {
        tween.PlayAgain();
      } else {
        tween.SmoothRewind();
      }
    }

    public static Tweener DOCounter(this TMP_Text target, int fromValue, int toValue, float duration) {
      return DOVirtual
          .Int(fromValue, toValue, duration, x => target.SetText(x.ToString()))
          .SetTarget(target);
    }

    public static Tweener DOPercentCounter(this TMP_Text target, float fromValue, float toValue, float duration) {
      return DOVirtual
          .Float(fromValue, toValue, duration, x => target.SetText($"{(x * 100f):F0}%"))
          .SetTarget(target);
    }

    public static Tweener DOPlayOneShot(this AudioSource target, AudioClip audioClip, float volumeScale) {
      float duration = audioClip.length;

      return DOVirtual
          .Float(0f, duration, duration, EmptyFloatCallback)
          .OnPlay(() => target.PlayOneShot(audioClip, volumeScale))
          .SetTarget(target);
    }

    static void EmptyFloatCallback(float x) {
      // ...
    }
  }
}
