using UnityEngine;
using System.Collections.Generic;

namespace GameJam {
  public class ResidentialWalkableGenerator : BuildingGeneratorBase {

    private struct WingConfig {
      public Vector3 localPosition;
      public Vector2 size;
      public bool isPrimary;
    }

    private struct BuildingStyle {
      public bool hasDividers;
      public float dividerSpacing;
      public float balconyDepth;
      public int setbackFloors;
      public float setbackDepth;
      public float entranceWidth;
      public bool hasCanopy;
    }

    public override void Generate(GameObject root, Vector2 size, CityZone zone, CityMaterialProfile materials, int generationSeed) {
      InitializeRandom(generationSeed);

      // 1. Determine Layout (Using full plot size now)
      List<WingConfig> wings = DetermineLayout(size);

      // 2. Determine Style
      float minDim = Mathf.Min(size.x, size.y);
      float maxSafeDepth = (minDim / 2.0f) - 1.0f;

      BuildingStyle style = new BuildingStyle {
        hasDividers = Random.value > 0.2f,
        dividerSpacing = Random.Range(3.5f, 9.0f),
        balconyDepth = Mathf.Clamp(Random.Range(0.1f, 1f), 0.1f, maxSafeDepth),
        setbackFloors = (Random.value > 0.6f) ? Random.Range(1, 4) : 0,
        setbackDepth = Random.Range(1.5f, 4.0f),
        entranceWidth = Mathf.Clamp(size.x * 0.4f, 3.0f, 8.0f),
        hasCanopy = Random.value > 0.3f
      };

      float height = Random.Range(20f, 50f);
      float lobbyHeight = 5.0f;
      float floorHeight = 3.2f;
      int upperFloors = Mathf.FloorToInt((height - lobbyHeight) / floorHeight);

      if (style.setbackFloors >= upperFloors)
        style.setbackFloors = Mathf.Max(0, upperFloors - 1);

      // Foundation Height (Base Offset)
      float baseOffset = 0.2f;

      // 3. Build Wings
      foreach (var wing in wings) {
        GameObject wingRoot = new GameObject(wing.isPrimary ? "WingPrimary" : "WingSide");
        wingRoot.transform.parent = root.transform;
        wingRoot.transform.localPosition = wing.localPosition;

        // Foundation (Sits at 0..0.2)
        CreateBox(wingRoot, "Foundation",
            new Vector3(wing.size.x, baseOffset, wing.size.y),
            new Vector3(0, baseOffset / 2, 0),
            materials.Concrete);

        // Ground Floor (Sits ON TOP of foundation, starting at baseOffset)
        if (wing.isPrimary) {
          BuildLobby(wingRoot, wing.size.x, wing.size.y, lobbyHeight, baseOffset, style, materials);
        } else {
          // Solid ground floor for side wings
          CreateBox(wingRoot, "GroundFloorSolid",
              new Vector3(wing.size.x - 0.2f, lobbyHeight, wing.size.y - 0.2f),
              new Vector3(0, baseOffset + (lobbyHeight / 2), 0),
              materials.WallResidential);
        }

        // Upper Floors
        for (int i = 0; i < upperFloors; i++) {
          float yPos = baseOffset + lobbyHeight + (i * floorHeight);

          Vector2 currentSize = wing.size;
          bool isSetback = i >= (upperFloors - style.setbackFloors);

          if (isSetback) {
            currentSize.x = Mathf.Max(5f, currentSize.x - (style.setbackDepth * 2));
            currentSize.y = Mathf.Max(5f, currentSize.y - (style.setbackDepth * 2));
          }

          BuildUpperFloor(wingRoot, i, currentSize, floorHeight, yPos, style, materials);
        }

        // Final Roof
        Vector2 topSize = wing.size;
        if (style.setbackFloors > 0) {
          topSize.x = Mathf.Max(5f, topSize.x - (style.setbackDepth * 2));
          topSize.y = Mathf.Max(5f, topSize.y - (style.setbackDepth * 2));
        }

        float roofY = baseOffset + lobbyHeight + (upperFloors * floorHeight);
        CreateBox(wingRoot, "RoofSlab",
            new Vector3(topSize.x, 0.5f, topSize.y),
            new Vector3(0, roofY + 0.25f, 0),
            materials.Concrete);

        if (wing.isPrimary) {
          CreateBox(wingRoot, "ElevatorHousing",
              new Vector3(4f, 2.5f, 4f),
              new Vector3(0, roofY + 1.25f, 0),
              materials.WallResidential);
        }
      }
    }

    private List<WingConfig> DetermineLayout(Vector2 plotSize) {
      List<WingConfig> wings = new List<WingConfig>();

      // Reduced padding to almost zero so buildings abut sidewalks/neighbors
      float padding = 0f;
      Vector2 paddedSize = plotSize - new Vector2(padding, padding);

      bool allowLShape = (paddedSize.x > 22f && paddedSize.y > 22f);

      if (allowLShape && Random.value > 0.7f) {
        // L-Shape
        float mainLen = paddedSize.y * 0.65f;
        float sideLen = paddedSize.y - mainLen;
        float sideWidth = paddedSize.x * 0.55f;

        float mainZ = (paddedSize.y / 2) - (mainLen / 2);
        wings.Add(new WingConfig {
          size = new Vector2(paddedSize.x, mainLen),
          localPosition = new Vector3(0, 0, mainZ),
          isPrimary = true
        });

        float sideZ = -(paddedSize.y / 2) + (sideLen / 2);
        float sideX = (paddedSize.x / 2) - (sideWidth / 2);
        wings.Add(new WingConfig {
          size = new Vector2(sideWidth, sideLen),
          localPosition = new Vector3(sideX, 0, sideZ),
          isPrimary = false
        });
      } else {
        // Standard Block
        wings.Add(new WingConfig {
          size = paddedSize,
          localPosition = Vector3.zero,
          isPrimary = true
        });
      }
      return wings;
    }

    private void BuildUpperFloor(GameObject parent, int floorIndex, Vector2 size, float height, float yBase, BuildingStyle style, CityMaterialProfile materials) {

      // 1. Floor Slab
      CreateBox(parent, $"FloorSlab{floorIndex}",
          new Vector3(size.x, 0.3f, size.y),
          new Vector3(0, yBase + 0.15f, 0),
          materials.Concrete);

      // 2. Ceiling Slab
      CreateBox(parent, $"CeilingSlab{floorIndex}",
          new Vector3(size.x, 0.3f, size.y),
          new Vector3(0, yBase + height - 0.15f, 0),
          materials.Concrete);

      // 3. Glass Core
      float inset = style.balconyDepth;
      float glassW = Mathf.Max(1f, size.x - (inset * 2));
      float glassL = Mathf.Max(1f, size.y - (inset * 2));
      float glassH = height - 0.6f;

      CreateBox(parent, $"GlassCore{floorIndex}",
          new Vector3(glassW, glassH, glassL),
          new Vector3(0, yBase + 0.3f + (glassH / 2), 0),
          materials.GlassResidential);

      // 4. Vertical Dividers
      if (style.hasDividers) {
        float dividerThick = 0.4f;
        float dividerDepth = inset + 0.5f; // Ensures overlap with slab edge

        int divCountX = Mathf.FloorToInt(size.x / style.dividerSpacing);
        if (divCountX > 0) {
          float divStepX = size.x / divCountX;
          for (int i = 0; i <= divCountX; i++) {
            float xPos = -(size.x / 2) + (i * divStepX);
            // Flush with outer edge
            CreateBox(parent, "DivFront",
                new Vector3(dividerThick, glassH, dividerDepth),
                new Vector3(xPos, yBase + height / 2, (size.y / 2) - (dividerDepth / 2)),
                materials.WallResidential);
            CreateBox(parent, "DivBack",
                new Vector3(dividerThick, glassH, dividerDepth),
                new Vector3(xPos, yBase + height / 2, -(size.y / 2) + (dividerDepth / 2)),
                materials.WallResidential);
          }
        }

        int divCountZ = Mathf.FloorToInt(size.y / style.dividerSpacing);
        if (divCountZ > 0) {
          float divStepZ = size.y / divCountZ;
          for (int i = 0; i <= divCountZ; i++) {
            float zPos = -(size.y / 2) + (i * divStepZ);
            CreateBox(parent, "DivRight",
                new Vector3(dividerDepth, glassH, dividerThick),
                new Vector3((size.x / 2) - (dividerDepth / 2), yBase + height / 2, zPos),
                materials.WallResidential);
            CreateBox(parent, "DivLeft",
                new Vector3(dividerDepth, glassH, dividerThick),
                new Vector3(-(size.x / 2) + (dividerDepth / 2), yBase + height / 2, zPos),
                materials.WallResidential);
          }
        }
      }
    }

    private void BuildLobby(GameObject parent, float w, float l, float h, float baseOffset, BuildingStyle style, CityMaterialProfile mats) {
      GameObject lobby = new GameObject("LobbyLevel");
      lobby.transform.parent = parent.transform;
      lobby.transform.localPosition = Vector3.zero;

      float wallThick = 0.3f;
      float doorWidth = style.entranceWidth;
      float doorHeight = 3.5f;

      // All Y positions shifted by baseOffset + h/2
      float yCenter = baseOffset + (h / 2);

      // Walls
      CreateBox(lobby, "WallBack", new Vector3(w, h, wallThick), new Vector3(0, yCenter, -l / 2 + wallThick / 2), mats.WallResidential);
      CreateBox(lobby, "WallLeft", new Vector3(wallThick, h, l - wallThick * 2), new Vector3(-w / 2 + wallThick / 2, yCenter, 0), mats.WallResidential);
      CreateBox(lobby, "WallRight", new Vector3(wallThick, h, l - wallThick * 2), new Vector3(w / 2 - wallThick / 2, yCenter, 0), mats.WallResidential);

      // Front (Doorway)
      float frontZ = l / 2 - wallThick / 2;
      float sideWidth = (w - doorWidth) / 2;
      CreateBox(lobby, "WallFrontLeft", new Vector3(sideWidth, h, wallThick), new Vector3(-w / 2 + sideWidth / 2, yCenter, frontZ), mats.WallResidential);
      CreateBox(lobby, "WallFrontRight", new Vector3(sideWidth, h, wallThick), new Vector3(w / 2 - sideWidth / 2, yCenter, frontZ), mats.WallResidential);
      CreateBox(lobby, "WallFrontLintel", new Vector3(doorWidth, h - doorHeight, wallThick), new Vector3(0, baseOffset + doorHeight + (h - doorHeight) / 2, frontZ), mats.WallResidential);

      // Floor & Pillar
      CreateBox(lobby, "LobbyFloor", new Vector3(w - wallThick * 2, 0.1f, l - wallThick * 2), new Vector3(0, baseOffset + 0.05f, 0), mats.Concrete);
      CreateBox(lobby, "LobbyPillar", new Vector3(1f, h, 1f), new Vector3(0, yCenter, 0), mats.Concrete);

      // Canopy
      if (style.hasCanopy) {
        float canopyDepth = Random.Range(2.0f, 4.0f);
        float canopyHeight = baseOffset + doorHeight + 0.2f;
        float canopyWidth = doorWidth + 2.0f;

        CreateBox(lobby, "CanopyRoof",
            new Vector3(canopyWidth, 0.2f, canopyDepth),
            new Vector3(0, canopyHeight, frontZ + (canopyDepth / 2)),
            mats.WallResidential);

        float pillarSize = 0.2f;
        float pillarX = (canopyWidth / 2) - 0.4f;
        float pillarZ = frontZ + canopyDepth - 0.4f;

        CreateCylinder(lobby, "CanopyPillarL",
            new Vector3(pillarSize, canopyHeight / 2, pillarSize),
            new Vector3(-pillarX, canopyHeight / 2, pillarZ),
            mats.Concrete);

        CreateCylinder(lobby, "CanopyPillarR",
            new Vector3(pillarSize, canopyHeight / 2, pillarSize),
            new Vector3(pillarX, canopyHeight / 2, pillarZ),
            mats.Concrete);
      }
    }
  }
}
