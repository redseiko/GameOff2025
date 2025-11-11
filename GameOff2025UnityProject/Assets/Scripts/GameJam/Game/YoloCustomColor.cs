using System;

using Unity.MaterialPropertyProvider;

using UnityEngine;

namespace GameJam {
  [ExecuteAlways]
  public sealed class YoloCustomColor : MaterialPropertyProviderBase {
    [field: SerializeField]
    [field: MaterialProperty("_Color")]
    public Color TargetColor { get; set; } = new Color(0.275f, 0.275f, 0.275f);

    [field: SerializeField]
    public Renderer[] TargetRenderers { get; set; } = Array.Empty<Renderer>();

    protected override Renderer[] Renderers {
      get => TargetRenderers;
    }
  }
}
