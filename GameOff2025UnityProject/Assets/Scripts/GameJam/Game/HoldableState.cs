using System;

using Unity.Cinemachine;

using UnityEditor.Experimental.GraphView;

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

    [field: SerializeField]
    public bool CompleteAtFullDuration { get; private set; } = true;

    [field: Header("Events")]
    [field: SerializeField]
    public UnityEvent<GameObject> OnHoldStart { get; private set; }

    [field: SerializeField]
    public UnityEvent<GameObject> OnHolding { get; private set; }

    [field: SerializeField]
    public UnityEvent<GameObject> OnHoldInterrupted { get; private set; }

    [field: SerializeField]
    public UnityEvent<GameObject> OnHoldComplete { get; private set; }

    private enum InternalState {
      // Not playing and waiting for a trigger.
      Waiting,

      // Triggered and progressing forward.
      Playing,

      // Progressing backwards.
      Rewinding,

      // No longer listening to triggers.
      Complete
    }

    private InternalState _state = InternalState.Waiting;

    private float _progress = 0.0f;

    private float _nextTriggerTime = 0.0f;

    public void StartHold() {
      if (_state == InternalState.Complete) {
        return;
      }

      _state = InternalState.Playing;
      OnHoldStart?.Invoke(this.gameObject);
    }

    public void StopHold() {
      if (_state == InternalState.Complete) {
        return;
      }

      if (RewindOnInterrupt) {
        _state = InternalState.Rewinding;
      } else {
        _state = InternalState.Waiting;
      }
      OnHoldInterrupted?.Invoke(this.gameObject);
    }

    public void FixedUpdate() {
      switch (_state) {
        case InternalState.Playing:
          if (_progress > _nextTriggerTime) {
            OnHolding?.Invoke(this.gameObject);
            _nextTriggerTime += OnHoldingTriggerInterval;
          }
          _progress += Time.fixedDeltaTime;
          if (_progress > HoldDuration) {
            OnHoldComplete?.Invoke(this.gameObject);
            if (CompleteAtFullDuration) {
              _state = InternalState.Complete;
            } else {
              _state = InternalState.Waiting;
            }
          }
          break;
        case InternalState.Rewinding:
          _progress -= Time.fixedDeltaTime;
          if (ResetTriggerIntervalsWhileRewinding &&
            (_nextTriggerTime - _progress) > OnHoldingTriggerInterval) {
            _nextTriggerTime -= OnHoldingTriggerInterval;
          }
          if (_progress < 0) {
            _state = InternalState.Waiting;
          }
          break;
        default:
          return;
      }
    }
  }
}
