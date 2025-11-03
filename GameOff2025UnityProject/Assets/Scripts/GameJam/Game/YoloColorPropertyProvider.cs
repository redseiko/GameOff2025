using Unity.MaterialPropertyProvider;

using UnityEngine;

namespace GameJam {
  [ExecuteAlways]
  public sealed class YoloColorPropertyProvider : MaterialPropertyProviderBase {
    [field: SerializeField]
    [field: MaterialProperty("_Color")]
    public Color YoloColor { get; set; } = new Color(0.275f, 0.275f, 0.275f);

    Renderer[] _renderers;

    protected override Renderer[] Renderers { 
      get {
        if (_renderers == default) {
          _renderers = new Renderer[1] { GetComponent<Renderer>() };
        }

        return _renderers;
      }
    }
  }
}
