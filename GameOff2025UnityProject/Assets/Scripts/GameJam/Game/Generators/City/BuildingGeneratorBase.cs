using UnityEngine;

namespace GameJam {
  /// <summary>
  /// Abstract base class for all city building generators.
  /// Provides utility methods for spawning primitives and managing transforms.
  /// </summary>
  public abstract class BuildingGeneratorBase : IPlotGenerator {

    // --- INTERFACE CONTRACT ---
    public abstract void Generate(GameObject root, Vector2 size, CityZone zone, CityMaterialProfile materials, int generationSeed);

    // --- HELPER METHODS ---

    /// <summary>
    /// Creates a Cube primitive.
    /// </summary>
    protected GameObject CreateBox(GameObject parent, string name, Vector3 scale, Vector3 localPos, Material mat) {
      GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
      obj.name = name;
      obj.transform.parent = parent.transform;
      obj.transform.localPosition = localPos;
      obj.transform.localScale = scale;

      if (mat != null) {
        var rend = obj.GetComponent<Renderer>();
        if (rend)
          rend.sharedMaterial = mat;
      }

      return obj;
    }

    /// <summary>
    /// Creates a Cylinder primitive (Useful for pillars, tanks, vents).
    /// Note: Unity Cylinder default height is 2.0 units.
    /// </summary>
    protected GameObject CreateCylinder(GameObject parent, string name, Vector3 scale, Vector3 localPos, Material mat) {
      GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
      obj.name = name;
      obj.transform.parent = parent.transform;
      obj.transform.localPosition = localPos;
      obj.transform.localScale = scale;

      if (mat != null) {
        var rend = obj.GetComponent<Renderer>();
        if (rend)
          rend.sharedMaterial = mat;
      }

      return obj;
    }

    /// <summary>
    /// Helper to initialize the RNG with the specific seed for this lot.
    /// </summary>
    protected void InitializeRandom(int seed) {
      Random.InitState(seed);
    }
  }
}
