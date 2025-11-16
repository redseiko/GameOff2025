using System.Collections;
using System.Collections.Generic;

using Eflatun.SceneReference;

using UnityEngine;
using UnityEngine.SceneManagement;

using YoloBox;

namespace GameJam {
  public sealed class GameManager : SingletonManager<GameManager> {
    [field: Header("Player")]
    [field: SerializeField]
    public YoloCapsuleController PlayerCapsuleController { get; private set; }

    [field: SerializeField]
    public CharacterController PlayerCharacterController { get; private set; }

    [field: SerializeField]
    public PlayerStateMachine PlayerStateMachine { get; private set; }

    [field: Header("Levels")]
    [field: SerializeField]
    public SceneReference[] StartingGameLevels { get; private set; }

    [field: Header("State")]
    [field: SerializeField]
    public bool IsGamePaused { get; private set; }

    void Start() {
      InputManager.Instance.LockCursor();
      StartCoroutine(LoadGameLevels(StartingGameLevels));
    }

    void LateUpdate() {
      UpdateGamePauseState();
    }

    public void TogglePlayerControls(bool toggleOn) {
      PlayerCapsuleController.enabled = toggleOn;
      PlayerCharacterController.enabled = toggleOn;
      PlayerStateMachine.enabled = toggleOn;

      InteractManager.Instance.CanInteract = toggleOn;
    }

    IEnumerator LoadGameLevels(IEnumerable<SceneReference> targetScenes) {
      TogglePlayerControls(toggleOn: false);

      List<Coroutine> loadCoroutines = new();

      foreach (SceneReference targetScene in targetScenes) {
        if (!IsActiveGameLevel(targetScene)) {
          loadCoroutines.Add(StartCoroutine(LoadGameLevel(targetScene)));
        }
      }

      foreach (Coroutine loadCoroutine in loadCoroutines) {
        yield return loadCoroutine;
      }

      TogglePlayerControls(toggleOn: true);
    }

    IEnumerator LoadGameLevel(SceneReference targetScene) {
      AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetScene.Path, LoadSceneMode.Additive);

      while (!asyncLoad.isDone) {
        yield return null;
      }
    }

    bool IsActiveGameLevel(SceneReference targetScene) {
      return
          targetScene != default
          && targetScene.State != SceneReferenceState.Unsafe
          && targetScene.LoadedScene.isLoaded;
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
