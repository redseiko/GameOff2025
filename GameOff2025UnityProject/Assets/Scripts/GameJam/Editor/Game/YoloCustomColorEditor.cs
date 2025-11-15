using UnityEditor;

using UnityEngine;

namespace GameJam.Editor {
  [CustomEditor(typeof(YoloCustomColor))]
  public sealed class YoloCustomColorEditor : UnityEditor.Editor {
    YoloCustomColor _instance = default;

    void OnEnable() {
      _instance = (YoloCustomColor) target;
    }

    void OnDisable() {
      _instance = default;
    }

    public override void OnInspectorGUI() {
      base.OnInspectorGUI();
      EditorGUILayout.Separator();

      EditorGUILayout.LabelField(nameof(YoloCustomColorEditor), EditorStyles.boldLabel);
      EditorGUILayout.Separator();
    }
  }
}
