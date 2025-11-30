using UnityEngine;

namespace GameJam {
  public class ResidentialGenerator : BuildingGeneratorBase {
    public override void Generate(GameObject root, Vector2 size, CityZone zone, CityMaterialProfile materials, int generationSeed) {
      InitializeRandom(generationSeed);

      // Residential: Medium height, Horizontal banding
      float height = Random.Range(12f, 35f);
      float w = size.x - 3f;
      float l = size.y - 3f;
      float floorHeight = 3.0f;
      int floorCount = Mathf.FloorToInt(height / floorHeight);

      // 1. Foundation
      CreateBox(root, "Foundation", new Vector3(size.x - 0.5f, 0.5f, size.y - 0.5f), new Vector3(0, 0.25f, 0), materials.Concrete);

      // 2. Stacked Floors
      for (int i = 0; i < floorCount; i++) {
        float currentY = (i * floorHeight);

        // Floor Slab Band (The structural concrete line)
        CreateBox(root, $"FloorSlab{i}",
            new Vector3(w, 0.4f, l),
            new Vector3(0, currentY + 0.2f, 0),
            materials.WallResidential);

        // Living Space (Inset Glass Block)
        // We assume the glass represents windows + balconies recessed in
        CreateBox(root, $"ApartmentBlock{i}",
            new Vector3(w - 0.5f, floorHeight - 0.4f, l - 0.5f),
            new Vector3(0, currentY + (floorHeight / 2) + 0.2f, 0),
            materials.GlassResidential);
      }

      // 3. Roof (Penthouse/Mechanical)
      CreateBox(root, "Roof", new Vector3(w, 0.5f, l), new Vector3(0, (floorCount * floorHeight) + 0.25f, 0), materials.Concrete);

      // Elevator Overrun on roof
      CreateBox(root, "ElevatorOverrun", new Vector3(w * 0.3f, 2f, l * 0.3f), new Vector3(0, (floorCount * floorHeight) + 1.5f, 0), materials.Concrete);
    }
  }
}
