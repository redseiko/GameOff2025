using UnityEditor;

using UnityEngine;

namespace GameJam.Editor {
  [CustomEditor(typeof(PopupPanelController))]
  public sealed class PopupPanelControllerEditor : UnityEditor.Editor {
    PopupPanelController _controller = default;

    void OnEnable() {
      _controller = (PopupPanelController) target;
    }

    void OnDisable() {
      _controller = default;
    }

    public override void OnInspectorGUI() {
      base.OnInspectorGUI();
      EditorGUILayout.Separator();

      EditorGUILayout.LabelField(nameof(PopupPanelControllerEditor), EditorStyles.boldLabel);
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
