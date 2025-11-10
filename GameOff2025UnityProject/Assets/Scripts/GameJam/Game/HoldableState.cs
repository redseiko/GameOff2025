using System;

using Unity.Cinemachine;

using UnityEngine;
using UnityEngine.Events;

namespace GameJam {
  public class HoldableState : MonoBehaviour {
    [field: Header("Progression")]
    [field: SerializeField]
    public float HoldDuration { get; private set; } = 1.0f;

    [field: SerializeField]
    public float OnHoldingTriggerInterval { get; private set; } = 0.25f;

    [field: SerializeField]
    public bool RewindOnInterrupt { get; private set; } = false;

    [field: SerializeField]
    public bool ResetTriggerIntervalsWhileRewinding { get; private set; } = false;

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
        if (_progress > HoldDuration) {
          _complete = true;
          _active = false;
          OnHoldComplete?.Invoke(this.gameObject);
        } else if (_active) {
          if (_progress > _nextTriggerTime) {
            OnHolding?.Invoke(this.gameObject);
            _nextTriggerTime += OnHoldingTriggerInterval;
          }
          _progress += Time.fixedDeltaTime;
        } else if (RewindOnInterrupt && _progress > 0) {
          _progress -= Time.fixedDeltaTime;
          if (ResetTriggerIntervalsWhileRewinding &&
            (_nextTriggerTime - _progress) > OnHoldingTriggerInterval) {
            _nextTriggerTime -= OnHoldingTriggerInterval;
          }
        }
      }
    }
  }
}
