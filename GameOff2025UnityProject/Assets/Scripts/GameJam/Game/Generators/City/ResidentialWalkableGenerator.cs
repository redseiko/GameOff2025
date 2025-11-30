using UnityEngine;

namespace GameJam {
  public class ResidentialWalkableGenerator : BuildingGeneratorBase {
    public override void Generate(GameObject root, Vector2 size, CityZone zone, CityMaterialProfile materials, int generationSeed) {
      InitializeRandom(generationSeed);

      // Dimensions
      float width = size.x - 2.0f; // Padding
      float length = size.y - 2.0f;
      float height = Random.Range(15f, 40f);

      float lobbyHeight = 4.5f;
      float floorHeight = 3.0f;
      int upperFloors = Mathf.FloorToInt((height - lobbyHeight) / floorHeight);

      // 1. Foundation Slab
      CreateBox(root, "Foundation", new Vector3(size.x, 0.2f, size.y), new Vector3(0, 0.1f, 0), materials.Concrete);

      // 2. The Lobby (Ground Floor) - Walkable Entrance
      BuildLobby(root, width, length, lobbyHeight, materials);

      // 3. Upper Floors (Stacked Stubs)
      // We start Y at lobby height
      for (int i = 0; i < upperFloors; i++) {
        float yPos = lobbyHeight + (i * floorHeight);

        // Floor Plate
        CreateBox(root, $"FloorPlate{i}",
            new Vector3(width, 0.3f, length),
            new Vector3(0, yPos + 0.15f, 0),
            materials.Concrete);

        // Living Volume (Slightly inset)
        CreateBox(root, $"ApartmentVolume{i}",
            new Vector3(width - 0.5f, floorHeight - 0.3f, length - 0.5f),
            new Vector3(0, yPos + 0.3f + (floorHeight - 0.3f) / 2, 0),
            materials.GlassResidential);

        // Random Balcony (Visual flair)
        if (Random.value > 0.6f) {
          float bW = 4f;
          float bD = 1.5f;
          // Stick it on the front face (Positive Z)
          CreateBox(root, $"Balcony{i}",
              new Vector3(bW, 1.0f, bD),
              new Vector3(Random.Range(-width / 4, width / 4), yPos + 1.0f, length / 2 + bD / 2),
              materials.Concrete);
        }
      }

      // 4. Roof
      float roofY = lobbyHeight + (upperFloors * floorHeight);
      CreateBox(root, "Roof", new Vector3(width, 0.5f, length), new Vector3(0, roofY, 0), materials.Concrete);
      CreateBox(root, "AC_Unit", new Vector3(3f, 2f, 3f), new Vector3(0, roofY + 1.25f, 0), materials.WallResidential);
    }

    private void BuildLobby(GameObject parent, float w, float l, float h, CityMaterialProfile mats) {
      GameObject lobby = new GameObject("LobbyLevel");
      lobby.transform.parent = parent.transform;
      lobby.transform.localPosition = Vector3.zero;

      float wallThick = 0.3f;
      float doorWidth = 2.5f;
      float doorHeight = 3.0f;

      // Back Wall (Solid)
      CreateBox(lobby, "WallBack",
          new Vector3(w, h, wallThick),
          new Vector3(0, h / 2, -l / 2 + wallThick / 2),
          mats.WallResidential);

      // Left Wall (Solid)
      CreateBox(lobby, "WallLeft",
          new Vector3(wallThick, h, l - (wallThick * 2)),
          new Vector3(-w / 2 + wallThick / 2, h / 2, 0),
          mats.WallResidential);

      // Right Wall (Solid)
      CreateBox(lobby, "WallRight",
          new Vector3(wallThick, h, l - (wallThick * 2)),
          new Vector3(w / 2 - wallThick / 2, h / 2, 0),
          mats.WallResidential);

      // Front Wall (With Entrance Hole)
      // We assume Front is +Z. We construct this out of 3 pieces (Left, Right, Top)
      float frontZ = l / 2 - wallThick / 2;

      // Left Panel
      float sideWidth = (w - doorWidth) / 2;
      CreateBox(lobby, "WallFrontLeft",
          new Vector3(sideWidth, h, wallThick),
          new Vector3(-w / 2 + sideWidth / 2, h / 2, frontZ),
          mats.WallResidential);

      // Right Panel
      CreateBox(lobby, "WallFrontRight",
          new Vector3(sideWidth, h, wallThick),
          new Vector3(w / 2 - sideWidth / 2, h / 2, frontZ),
          mats.WallResidential);

      // Top Lintel
      float lintelH = h - doorHeight;
      CreateBox(lobby, "WallFrontLintel",
          new Vector3(doorWidth, lintelH, wallThick),
          new Vector3(0, doorHeight + lintelH / 2, frontZ),
          mats.WallResidential);

      // Interior Floor (Lobby Floor)
      CreateBox(lobby, "LobbyFloor",
          new Vector3(w - wallThick * 2, 0.1f, l - wallThick * 2),
          new Vector3(0, 0.1f, 0),
          mats.Concrete);

      // Internal Central Pillar (Visual interest inside)
      CreateBox(lobby, "LobbyPillar",
          new Vector3(1f, h, 1f),
          new Vector3(0, h / 2, 0),
          mats.Concrete);
    }
  }
}
