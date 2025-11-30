using UnityEngine;

namespace GameJam {
  public class IndustrialGenerator : BuildingGeneratorBase {
    public override void Generate(GameObject root, Vector2 size, CityZone zone, CityMaterialProfile materials, int generationSeed) {
      InitializeRandom(generationSeed);

      // Industrial: Low, Wide, Functional
      float height = Random.Range(8f, 14f);
      float w = size.x - 1.5f;
      float l = size.y - 1.5f;

      // 1. Foundation
      CreateBox(root, "Foundation", new Vector3(size.x - 0.5f, 0.5f, size.y - 0.5f), new Vector3(0, 0.25f, 0), materials.Concrete);

      // 2. Main Warehouse Body
      CreateBox(root, "WarehouseBody", new Vector3(w, height, l), new Vector3(0, height / 2, 0), materials.WallIndustrial);

      // 3. Loading Dock (Small extension on one side)
      // We place it at the "Front" (negative Z)
      CreateBox(root, "LoadingDock", new Vector3(w * 0.6f, 4f, 2f), new Vector3(0, 2f, -l / 2 - 1f), materials.Concrete);

      // 4. Sawtooth Roof / Skylights
      // We divide the length into segments
      int segments = Mathf.FloorToInt(l / 6f); // Every 6 meters
      if (segments < 1)
        segments = 1;
      float segmentLen = l / segments;

      for (int i = 0; i < segments; i++) {
        float zPos = -l / 2 + (i * segmentLen) + (segmentLen / 2);

        // The triangular vent shape (Approximated by a box for now)
        CreateBox(root, $"RoofVent{i}",
            new Vector3(w, 1.5f, segmentLen * 0.8f),
            new Vector3(0, height + 0.75f, zPos),
            materials.WallIndustrial);

        // The skylight window on the face of the vent
        CreateBox(root, $"Skylight{i}",
            new Vector3(w - 1f, 0.5f, segmentLen * 0.8f),
            new Vector3(0, height + 1.55f, zPos - 0.5f),
            materials.GlassIndustrial);
      }
    }
  }
}
