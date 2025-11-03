using UnityEditor;

using UnityEngine;

namespace GameJam.Editor {
  [CustomEditor(typeof(InteractPanelController))]
  public sealed class InteractPanelControllerEditor : UnityEditor.Editor {
    InteractPanelController _controller = default;

    void OnEnable() {
      _controller = (InteractPanelController) target;
    }

    void OnDisable() {
      _controller = default;
    }

    public override void OnInspectorGUI() {
      base.OnInspectorGUI();
      EditorGUILayout.Separator();

      EditorGUILayout.LabelField(nameof(InteractPanelControllerEditor), EditorStyles.boldLabel);
      EditorGUILayout.Separator();

      EditorGUI.BeginDisabledGroup(disabled: !EditorApplication.isPlaying);
      DrawControllerControls();
      EditorGUI.EndDisabledGroup();
    }

    void DrawControllerControls() {
      EditorGUILayout.BeginHorizontal();

      if (GUILayout.Button("ShowPanel")) {
        _controller.ShowPanel();
      }

      if (GUILayout.Button("HidePanel")) {
        _controller.HidePanel();
      }

      EditorGUILayout.EndHorizontal();
    }
  }
}
