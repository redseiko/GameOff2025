using UnityEditor;

using UnityEngine;

namespace GameJam.Editor {
  [CustomEditor(typeof(DialogPanelController))]
  public sealed class DialogPanelControllerEditor : UnityEditor.Editor {
    DialogPanelController _controller = default;

    void OnEnable() {
      _controller = (DialogPanelController) target;
    }

    void OnDisable() {
      _controller = default;
    }

    public override void OnInspectorGUI() {
      base.OnInspectorGUI();
      EditorGUILayout.Separator();

      EditorGUILayout.LabelField(nameof(DialogPanelControllerEditor), EditorStyles.boldLabel);
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
