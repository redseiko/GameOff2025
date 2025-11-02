using UnityEngine;

namespace GameJam {
  public abstract class StateMachine : MonoBehaviour {
    State _currentState;

    void Update() {
      _currentState?.Tick();
    }

    public void SwitchState(State state) {
      _currentState?.Exit();
      _currentState = state;
      _currentState.Enter();
    }
  }
}
