using System;

using DS.ScriptableObjects;

using UnityEngine;

namespace GameJam {
  public sealed class YoloBook : MonoBehaviour {
    [field: Header("Book")]
    [field: SerializeField]
    public DSDialogueSO[] BookDialogNodes { get; private set; } = Array.Empty<DSDialogueSO>();

    public void ShowBookDialogNode(int nodeIndex = 0) {
      if (BookDialogNodes.Length <= 0 || nodeIndex < 0 || nodeIndex >= BookDialogNodes.Length) {
        return;
      }

      UIManager.Instance.ShowDialogNode(BookDialogNodes[nodeIndex]);
    }
  }
}
