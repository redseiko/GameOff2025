using DG.Tweening;

using DS.Enumerations;
using DS.ScriptableObjects;

using UnityEngine;

namespace GameJam {
  public sealed class DialogPanelController : MonoBehaviour {
    [field: Header("Panel")]
    [field: SerializeField]
    public RectTransform PanelRectTransform { get; private set; }

    [field: SerializeField]
    public CanvasGroup PanelCanvasGroup { get; private set; }

    [field: Header("Display")]
    [field: SerializeField]
    public DialogDisplayController BookDisplay { get; private set; }

    [field: Header("State")]
    public bool IsPanelVisible { get; private set; }

    [field: SerializeField]
    public DSDialogueSO CurrentDialogNode { get; private set; }

    [field: SerializeField]
    public DialogDisplayController CurrentDialogDisplay { get; private set; }

    void Start() {
      ResetPanel();
    }

    public void ResetPanel() {
      PanelCanvasGroup.alpha = 0f;
      PanelCanvasGroup.blocksRaycasts = false;

      IsPanelVisible = false;
    }

    public void TogglePanel() {
      if (IsPanelVisible) {
        HidePanel();
      } else {
        ShowPanel();
      }
    }

    public void ShowPanel() {
      PanelCanvasGroup.DOComplete(withCallbacks: true);

      PanelCanvasGroup.blocksRaycasts = true;
      IsPanelVisible = true;

      PanelCanvasGroup
          .DOFade(1f, 0.4f)
          .SetUpdate(isIndependentUpdate: true);
    }

    public void HidePanel() {
      PanelCanvasGroup.DOComplete(withCallbacks: true);

      PanelCanvasGroup.blocksRaycasts = false;
      IsPanelVisible = false;

      PanelCanvasGroup
          .DOFade(0f, 0.4f)
          .SetUpdate(isIndependentUpdate: true);
    }

    public void OnChoiceButtonClick(int choiceIndex) {
      ShowDialogNode(GetNextDialogNode(CurrentDialogNode, choiceIndex));
    }

    DSDialogueSO GetNextDialogNode(DSDialogueSO dialogNode, int choiceIndex) {
      if (!dialogNode || dialogNode.Choices.Count < choiceIndex) {
        return default;
      }

      return dialogNode.Choices[choiceIndex].NextDialogue;
    }

    public void ShowDialogNode(DSDialogueSO dialogNode) {
      CurrentDialogNode = dialogNode;

      if (!CurrentDialogNode) {
        HidePanel();
        return;
      }

      SetupDialogDisplay(dialogNode);
    }

    void SetupDialogDisplay(DSDialogueSO dialogNode) {
      DOTween.Complete(PanelRectTransform, withCallbacks: true);

      Sequence sequence =
          DOTween.Sequence()
              .SetTarget(PanelRectTransform)
              .SetUpdate(isIndependentUpdate: true);

      if (!IsPanelVisible) {
        sequence.AppendCallback(ShowPanel);
      }

      DialogDisplayController previousDialogDisplay = CurrentDialogDisplay;
      DialogDisplayController nextDialogDisplay = CurrentDialogDisplay;

      if (dialogNode.PortraitType == DSPortraitType.NoPortrait) {
        nextDialogDisplay = BookDisplay;
      } else {
        Debug.LogError($"Unsupported DSPortraitType! {dialogNode.PortraitType}");
      }

      if (previousDialogDisplay
          && previousDialogDisplay != nextDialogDisplay
          && previousDialogDisplay.IsDisplayVisible) {
        sequence
            .AppendCallback(previousDialogDisplay.HideDisplay)
            .AppendInterval(0.15f);
      }

      nextDialogDisplay.SetupDisplay(dialogNode);

      sequence.AppendCallback(
          nextDialogDisplay.IsDisplayVisible
              ? nextDialogDisplay.ShowContent
              : nextDialogDisplay.ShowDisplay);

      CurrentDialogDisplay = nextDialogDisplay;
    }
  }
}
