using UnityEngine;

using YoloBox;

namespace GameJam {
  public sealed class GameManager : SingletonManager<GameManager> {
    [field: Header("State")]
    [field: SerializeField]
    public bool IsGamePaused { get; private set; }

    void Start() {
      InputManager.Instance.LockCursor();
    }

    void LateUpdate() {
      UpdateGamePauseState();
    }

    void UpdateGamePauseState() {
      if (ShouldPauseGame()) {
        PauseGame();
      } else {
        UnpauseGame();
      }
    }

    bool ShouldPauseGame() {
      return UIManager.Instance.ShouldUnlockCursor();
    }

    public void PauseGame() {
      if (!IsGamePaused) {
        IsGamePaused = true;
        Time.timeScale = 0f;
      }
    }

    public void UnpauseGame() {
      if (IsGamePaused) {
        IsGamePaused = false;
        Time.timeScale = 1f;
      }
    }
  }
}
