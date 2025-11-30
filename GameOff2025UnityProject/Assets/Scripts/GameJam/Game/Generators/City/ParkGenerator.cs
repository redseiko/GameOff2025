using UnityEngine;

namespace GameJam {
  public class ParkGenerator : BuildingGeneratorBase {
    public override void Generate(GameObject root, Vector2 size, CityZone zone, CityMaterialProfile materials, int generationSeed) {
      InitializeRandom(generationSeed);

      // 1. Grass Base
      CreateBox(root, "GrassPlane",
          new Vector3(size.x - 1f, 0.2f, size.y - 1f),
          new Vector3(0, 0.1f, 0),
          materials.ParkGrass);

      // 2. Central Feature (Statue or Fountain)
      // Just a concrete block for now, but distinguishes it from empty land
      CreateBox(root, "CenterFeature",
          new Vector3(3f, 2f, 3f),
          new Vector3(0, 1.2f, 0),
          materials.Concrete);

      // 3. Trees (Abstract Green Cylinders)
      // Place 4 trees in corners
      float treeX = (size.x / 4);
      float treeZ = (size.y / 4);
      float treeH = Random.Range(3f, 6f);

      CreateCylinder(root, "Tree_FL", new Vector3(1f, treeH / 2, 1f), new Vector3(-treeX, treeH / 2, -treeZ), materials.ParkGrass);
      CreateCylinder(root, "Tree_FR", new Vector3(1f, treeH / 2, 1f), new Vector3(treeX, treeH / 2, -treeZ), materials.ParkGrass);
      CreateCylinder(root, "Tree_BL", new Vector3(1f, treeH / 2, 1f), new Vector3(-treeX, treeH / 2, treeZ), materials.ParkGrass);
      CreateCylinder(root, "Tree_BR", new Vector3(1f, treeH / 2, 1f), new Vector3(treeX, treeH / 2, treeZ), materials.ParkGrass);
    }
  }
}
