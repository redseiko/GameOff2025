using UnityEditor;

using UnityEngine;

namespace GameJam.Editor {
  [CustomEditor(typeof(MenuOverlayController))]
  public sealed class MenuOverlayControllerEditor : UnityEditor.Editor {
    MenuOverlayController _controller = default;

    void OnEnable() {
      _controller = (MenuOverlayController) target;
    }

    void OnDisable() {
      _controller = default;
    }

    public override void OnInspectorGUI() {
      base.OnInspectorGUI();
      EditorGUILayout.Separator();

      EditorGUILayout.LabelField(nameof(MenuOverlayControllerEditor), EditorStyles.boldLabel);
      EditorGUILayout.Separator();

      EditorGUI.BeginDisabledGroup(disabled: !EditorApplication.isPlaying);
      DrawControllerControls();
      EditorGUI.EndDisabledGroup();
    }

    void DrawControllerControls() {
      EditorGUILayout.BeginHorizontal();

      if (GUILayout.Button("ShowOverlay")) {
        _controller.ShowOverlay();
      }

      if (GUILayout.Button("HideOverlay")) {
        _controller.HideOverlay();
      }

      EditorGUILayout.EndHorizontal();
    }
  }
}
