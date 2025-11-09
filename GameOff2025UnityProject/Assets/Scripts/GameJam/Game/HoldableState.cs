using System;

using UnityEngine;
using UnityEngine.Events;

namespace GameJam {
  public class HoldableState : MonoBehaviour {
    [field: Header("Duration")]
    [field: SerializeField]
    public float HoldDuration { get; private set; } = 1.0f;

    [field: SerializeField]
    public float OnHoldingTriggerInterval { get; private set; } = 0.25f;

    [field: Header("Events")]
    [field: SerializeField]
    public UnityEvent<GameObject> OnHoldStart { get; private set; }

    [field: SerializeField]
    public UnityEvent<GameObject> OnHolding { get; private set; }

    [field: SerializeField]
    public UnityEvent<GameObject> OnHoldInterrupted { get; private set; }

    [field: SerializeField]
    public UnityEvent<GameObject> OnHoldComplete { get; private set; }

    private bool _active = false;

    private bool _complete = false;

    private float _progress = 0.0f;

    private float _nextTriggerTime = 0.0f;

    public void StartHold() {
      if (!_complete) {
        _active = true;
        OnHoldStart?.Invoke(this.gameObject);
      }
    }

    public void StopHold() {
      _active = false;
      if (!_complete) {
        OnHoldInterrupted?.Invoke(this.gameObject);
      }
    }

    public void FixedUpdate() {
      if (!_complete) {
        if (Mathf.Approximately(_progress, HoldDuration)) {
          _complete = true;
          _active = false;
          OnHoldComplete?.Invoke(this.gameObject);
        } else if (_active) {
          if (_progress > _nextTriggerTime) {
            OnHolding?.Invoke(this.gameObject);
            _nextTriggerTime += OnHoldingTriggerInterval;
          }
          _progress += Time.fixedDeltaTime;
        }
      }
    }
  }
}
