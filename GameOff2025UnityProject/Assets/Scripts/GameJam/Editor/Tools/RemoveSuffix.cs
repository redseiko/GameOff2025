using System;
using System.Text.RegularExpressions;

using UnityEditor;

namespace GameJam.Editor {
  public sealed class RemoveSuffix : ScriptableWizard {
    public static readonly Regex NumberSuffixRegex =
        new Regex(
            @"^((?<base>.+)\.\d+|(?<base>.+)_\d+|(?<base>.+) \(\d+\))$",
            RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.CultureInvariant,
            TimeSpan.FromMilliseconds(500));

    [MenuItem("Edit/Remove Suffix...")]
    static void CreateWizard() {
      DisplayWizard("Remove Suffix", typeof(RemoveSuffix), "Remove");
    }

    void OnEnable() {
      UpdateHelpString();
    }

    void OnSelectionChange() {
      UpdateHelpString();
    }

    void UpdateHelpString() {
      if (Selection.objects == default) {
        helpString = "No objects selected.";
      } else {
        helpString = $"Selected {Selection.objects.Length} objects.";
      }
    }

    void OnWizardCreate() {
      if (Selection.objects == default) {
        return;
      }

      foreach (UnityEngine.Object unityObject in Selection.objects) {
        Match match = NumberSuffixRegex.Match(unityObject.name);

        if (match.Success) {
          unityObject.name = match.Groups["base"].Value;
        }
      }
    }
  }
}
