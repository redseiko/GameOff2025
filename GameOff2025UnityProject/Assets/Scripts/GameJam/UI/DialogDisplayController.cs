using DG.Tweening;

using DS.ScriptableObjects;

using TMPro;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using YoloBox;

namespace GameJam {
  public sealed class DialogDisplayController : MonoBehaviour {
    [field: Header("Display")]
    [field: SerializeField]
    public RectTransform DisplayRectTransform { get; private set; }

    [field: SerializeField]
    public CanvasGroup DisplayCanvasGroup { get; private set; }

    [field: Header("UI")]
    [field: SerializeField]
    public Image Background { get; private set; }

    [field: SerializeField]
    public TextMeshProUGUI DialogText { get; private set; }

    [field: Header("Confirm")]
    [field: SerializeField]
    public WebGlButton ConfirmButton { get; private set; }

    [field: SerializeField]
    public TextMeshProUGUI ConfirmLabel { get; private set; }

    [field: Header("State")]
    [field: SerializeField]
    public bool IsDisplayVisible { get; private set; }

    Sequence _showContentTween;

    void Start() {
      CreateTweens();
      ResetDisplay();
    }

    void CreateTweens() {
      _showContentTween =
          DOTween.Sequence()
              .Insert(0f, DialogText.transform.DOBlendableLocalMoveBy(new(0f, 15f, 0f), 0.4f).From(true))
              .SetTarget(DialogText)
              .SetLink(gameObject)
              .SetAutoKill(false)
              .SetUpdate(isIndependentUpdate: true)
              .Pause();
    }

    public void ResetDisplay() {
      DisplayCanvasGroup.alpha = 0f;
      DisplayCanvasGroup.blocksRaycasts = false;

      IsDisplayVisible = false;
    }

    public void ToggleDisplay() {
      if (IsDisplayVisible) {
        HideDisplay();
      } else {
        ShowDisplay();
      }
    }

    public void ShowDisplay() {
      DisplayCanvasGroup.DOComplete(withCallbacks: true);

      DisplayCanvasGroup.blocksRaycasts = true;
      IsDisplayVisible = true;

      EventSystem.current.SetSelectedGameObject(ConfirmButton.gameObject);

      DisplayCanvasGroup
          .DOFade(1f, 0.4f)
          .SetUpdate(isIndependentUpdate: true);
    }

    public void HideDisplay() {
      DisplayCanvasGroup.DOComplete(withCallbacks: true);

      DisplayCanvasGroup.blocksRaycasts = false;
      IsDisplayVisible = false;

      DisplayCanvasGroup
          .DOFade(0f, 0.4f)
          .SetUpdate(isIndependentUpdate: true);
    }

    public void SetupDisplay(DSDialogueSO dialogNode) {
      SetupText(dialogNode);
      SetupChoices(dialogNode);
    }

    void SetupText(DSDialogueSO dialogNode) {
      DialogText.text = dialogNode.Text;
    }

    void SetupChoices(DSDialogueSO dialogNode) {
      ConfirmLabel.text =
          dialogNode.Choices.Count > 0
              ? dialogNode.Choices[0].HasCustomChoiceText()
                  ? dialogNode.Choices[0].Text
                  : "Next"
              : "Close";
    }

    public void ShowContent() {
      _showContentTween.PlayComplete();
    }
  }
}
