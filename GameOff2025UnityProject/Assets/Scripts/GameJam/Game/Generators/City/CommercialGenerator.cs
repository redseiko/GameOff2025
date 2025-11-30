using UnityEngine;

namespace GameJam {
  public class CommercialGenerator : BuildingGeneratorBase {
    public override void Generate(GameObject root, Vector2 size, CityZone zone, CityMaterialProfile materials, int generationSeed) {
      InitializeRandom(generationSeed);

      // Commercial: Tall, Glassy, Vertical emphasis
      float height = Random.Range(30f, 80f);
      float w = size.x - 2f; // Padding from lot edge
      float l = size.y - 2f;

      // 1. Foundation
      CreateBox(root, "Foundation", new Vector3(size.x - 0.5f, 0.5f, size.y - 0.5f), new Vector3(0, 0.25f, 0), materials.Concrete);

      // 2. Concrete Core
      CreateBox(root, "Core", new Vector3(w, height, l), new Vector3(0, height / 2, 0), materials.Concrete);

      // 3. Glass Curtain Wall (Slightly wider than core to look attached)
      CreateBox(root, "GlassCurtain", new Vector3(w + 0.2f, height - 2f, l + 0.2f), new Vector3(0, height / 2, 0), materials.GlassCommercial);

      // 4. Corner Pillars (Fins)
      float finThick = 1.2f;
      CreateBox(root, "PillarFrontLeft", new Vector3(finThick, height, finThick), new Vector3(-w / 2, height / 2, -l / 2), materials.Concrete);
      CreateBox(root, "PillarFrontRight", new Vector3(finThick, height, finThick), new Vector3(w / 2, height / 2, -l / 2), materials.Concrete);
      CreateBox(root, "PillarBackLeft", new Vector3(finThick, height, finThick), new Vector3(-w / 2, height / 2, l / 2), materials.Concrete);
      CreateBox(root, "PillarBackRight", new Vector3(finThick, height, finThick), new Vector3(w / 2, height / 2, l / 2), materials.Concrete);

      // 5. Roof Cap
      CreateBox(root, "RoofCap", new Vector3(w, 1f, l), new Vector3(0, height + 0.5f, 0), materials.Concrete);
    }
  }
}
