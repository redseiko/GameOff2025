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

    // Core Configuration (Stairwell)
    private const float CORE_WIDTH = 6.0f;
    private const float CORE_LENGTH = 5.0f;
    private const float CORE_WALL_THICKNESS = 0.2f;

    public override void Generate(GameObject root, Vector2 size, CityZone zone, CityMaterialProfile materials, int generationSeed) {
      InitializeRandom(generationSeed);

      // 1. Layout
      List<WingConfig> wings = DetermineLayout(size);

      // 2. Style
      float minDim = Mathf.Min(size.x, size.y);
      // Limit safe depth so we don't eat into the core area
      float maxSafeDepth = (minDim / 2.0f) - (Mathf.Max(CORE_WIDTH, CORE_LENGTH) / 2f) - 1.0f;
      // Fix: Clamp to user requested max of 1.5f
      float maxBalcony = Mathf.Min(1.5f, maxSafeDepth);

      BuildingStyle style = new BuildingStyle {
        hasDividers = Random.value > 0.2f,
        dividerSpacing = Random.Range(3.5f, 9.0f),
        balconyDepth = Mathf.Clamp(Random.Range(0.1f, 1.5f), 0.1f, Mathf.Max(0.1f, maxBalcony)),
        setbackFloors = (Random.value > 0.6f) ? Random.Range(1, 4) : 0,
        setbackDepth = Random.Range(1.5f, 4.0f),
        entranceWidth = Mathf.Clamp(size.x * 0.4f, 3.0f, 8.0f),
        hasCanopy = Random.value > 0.3f
      };

      float height = Random.Range(20f, 50f);
      float lobbyHeight = 5.0f;
      float floorHeight = 3.5f;
      int upperFloors = Mathf.FloorToInt((height - lobbyHeight) / floorHeight);

      if (style.setbackFloors >= upperFloors)
        style.setbackFloors = Mathf.Max(0, upperFloors - 1);

      float baseOffset = 0.2f;

      // 3. Build Wings
      foreach (var wing in wings) {
        GameObject wingRoot = new GameObject(wing.isPrimary ? "WingPrimary" : "WingSide");
        wingRoot.transform.parent = root.transform;
        wingRoot.transform.localPosition = wing.localPosition;

        // Foundation
        CreateBox(wingRoot, "Foundation", new Vector3(wing.size.x, baseOffset, wing.size.y), new Vector3(0, baseOffset / 2, 0), materials.Concrete);

        // Ground Floor
        if (wing.isPrimary) {
          BuildLobby(wingRoot, wing.size.x, wing.size.y, lobbyHeight, baseOffset, style, materials);
          // Core for Ground Floor
          BuildCoreSegment(wingRoot, 0, lobbyHeight, baseOffset, true, materials);
        } else {
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
            currentSize.x = Mathf.Max(CORE_WIDTH + 2f, currentSize.x - (style.setbackDepth * 2));
            currentSize.y = Mathf.Max(CORE_LENGTH + 2f, currentSize.y - (style.setbackDepth * 2));
          }

          BuildUpperFloor(wingRoot, i, currentSize, floorHeight, yPos, wing.isPrimary, style, materials);
        }

        // Roof
        float roofY = baseOffset + lobbyHeight + (upperFloors * floorHeight);
        Vector2 topSize = wing.size;
        if (style.setbackFloors > 0) {
          topSize.x = Mathf.Max(CORE_WIDTH + 2f, topSize.x - (style.setbackDepth * 2));
          topSize.y = Mathf.Max(CORE_LENGTH + 2f, topSize.y - (style.setbackDepth * 2));
        }

        CreateBox(wingRoot, "RoofSlab", new Vector3(topSize.x, 0.5f, topSize.y), new Vector3(0, roofY + 0.25f, 0), materials.Concrete);

        if (wing.isPrimary) {
          BuildRoofBulkhead(wingRoot, roofY, materials);
        }
      }
    }

    private List<WingConfig> DetermineLayout(Vector2 plotSize) {
      List<WingConfig> wings = new List<WingConfig>();
      float padding = 0f;
      Vector2 paddedSize = plotSize - new Vector2(padding, padding);
      bool allowLShape = (paddedSize.x > 22f && paddedSize.y > 22f);

      if (allowLShape && Random.value > 0.7f) {
        float mainLen = paddedSize.y * 0.65f;
        float sideLen = paddedSize.y - mainLen;
        float sideWidth = paddedSize.x * 0.55f;
        float mainZ = (paddedSize.y / 2) - (mainLen / 2);
        wings.Add(new WingConfig { size = new Vector2(paddedSize.x, mainLen), localPosition = new Vector3(0, 0, mainZ), isPrimary = true });
        float sideZ = -(paddedSize.y / 2) + (sideLen / 2);
        float sideX = (paddedSize.x / 2) - (sideWidth / 2);
        wings.Add(new WingConfig { size = new Vector2(sideWidth, sideLen), localPosition = new Vector3(sideX, 0, sideZ), isPrimary = false });
      } else {
        wings.Add(new WingConfig { size = paddedSize, localPosition = Vector3.zero, isPrimary = true });
      }
      return wings;
    }

    private void BuildUpperFloor(GameObject parent, int floorIndex, Vector2 size, float height, float yBase, bool hasCore, BuildingStyle style, CityMaterialProfile materials) {

      // 1. Floor Slab
      if (hasCore) {
        // Cut hole for stairs
        BuildSlabWithHole(parent, $"FloorSlab{floorIndex}", size, 0.3f, yBase + 0.15f, CORE_WIDTH, CORE_LENGTH, materials.Concrete);
        BuildSlabWithHole(parent, $"CeilingSlab{floorIndex}", size, 0.3f, yBase + height - 0.15f, CORE_WIDTH, CORE_LENGTH, materials.Concrete);

        // Build internal core for this floor
        BuildCoreSegment(parent, floorIndex + 1, height, yBase, false, materials);
      } else {
        CreateBox(parent, $"FloorSlab{floorIndex}", new Vector3(size.x, 0.3f, size.y), new Vector3(0, yBase + 0.15f, 0), materials.Concrete);
        CreateBox(parent, $"CeilingSlab{floorIndex}", new Vector3(size.x, 0.3f, size.y), new Vector3(0, yBase + height - 0.15f, 0), materials.Concrete);
      }

      // 2. Hollow Glass Shell
      float inset = style.balconyDepth;
      float glassH = height - 0.6f;
      float glassY = yBase + 0.3f + (glassH / 2);

      float shellW = Mathf.Max(1f, size.x - (inset * 2));
      float shellL = Mathf.Max(1f, size.y - (inset * 2));
      float shellThick = 0.1f;

      CreateBox(parent, "GlassFront", new Vector3(shellW, glassH, shellThick), new Vector3(0, glassY, (shellL / 2) - (shellThick / 2)), materials.GlassResidential);
      CreateBox(parent, "GlassBack", new Vector3(shellW, glassH, shellThick), new Vector3(0, glassY, -(shellL / 2) + (shellThick / 2)), materials.GlassResidential);
      float sideLen = shellL - (shellThick * 2);
      if (sideLen > 0) {
        CreateBox(parent, "GlassLeft", new Vector3(shellThick, glassH, sideLen), new Vector3(-(shellW / 2) + (shellThick / 2), glassY, 0), materials.GlassResidential);
        CreateBox(parent, "GlassRight", new Vector3(shellThick, glassH, sideLen), new Vector3((shellW / 2) - (shellThick / 2), glassY, 0), materials.GlassResidential);
      }

      // 3. Vertical Dividers
      if (style.hasDividers) {
        float dividerThick = 0.4f;
        float dividerDepth = inset + 0.5f;

        int divCountX = Mathf.FloorToInt(size.x / style.dividerSpacing);
        if (divCountX > 0) {
          float divStepX = size.x / divCountX;
          for (int i = 0; i <= divCountX; i++) {
            float xPos = -(size.x / 2) + (i * divStepX);
            CreateBox(parent, "DivFront", new Vector3(dividerThick, glassH, dividerDepth), new Vector3(xPos, yBase + height / 2, (size.y / 2) - (dividerDepth / 2)), materials.WallResidential);
            CreateBox(parent, "DivBack", new Vector3(dividerThick, glassH, dividerDepth), new Vector3(xPos, yBase + height / 2, -(size.y / 2) + (dividerDepth / 2)), materials.WallResidential);
          }
        }
        int divCountZ = Mathf.FloorToInt(size.y / style.dividerSpacing);
        if (divCountZ > 0) {
          float divStepZ = size.y / divCountZ;
          for (int i = 0; i <= divCountZ; i++) {
            float zPos = -(size.y / 2) + (i * divStepZ);
            CreateBox(parent, "DivRight", new Vector3(dividerDepth, glassH, dividerThick), new Vector3((size.x / 2) - (dividerDepth / 2), yBase + height / 2, zPos), materials.WallResidential);
            CreateBox(parent, "DivLeft", new Vector3(dividerDepth, glassH, dividerThick), new Vector3(-(size.x / 2) + (dividerDepth / 2), yBase + height / 2, zPos), materials.WallResidential);
          }
        }
      }
    }

    private void BuildCoreSegment(GameObject parent, int floorIndex, float height, float yBase, bool isLobby, CityMaterialProfile materials) {
      GameObject core = new GameObject($"Core_Floor{floorIndex}");
      core.transform.parent = parent.transform;
      core.transform.localPosition = new Vector3(0, yBase, 0);

      // Walls (Back Wall has door)
      float t = CORE_WALL_THICKNESS;
      float h = height;

      CreateBox(core, "CoreWallL", new Vector3(t, h, CORE_LENGTH), new Vector3(-CORE_WIDTH / 2 + t / 2, h / 2, 0), materials.Concrete);
      CreateBox(core, "CoreWallR", new Vector3(t, h, CORE_LENGTH), new Vector3(CORE_WIDTH / 2 - t / 2, h / 2, 0), materials.Concrete);
      CreateBox(core, "CoreWallF", new Vector3(CORE_WIDTH, h, t), new Vector3(0, h / 2, CORE_LENGTH / 2 - t / 2), materials.Concrete);

      float doorW = 1.5f;
      float doorH = 2.2f;
      float sideW = (CORE_WIDTH - doorW) / 2;
      CreateBox(core, "CoreWallB_L", new Vector3(sideW, h, t), new Vector3(-CORE_WIDTH / 2 + sideW / 2, h / 2, -CORE_LENGTH / 2 + t / 2), materials.Concrete);
      CreateBox(core, "CoreWallB_R", new Vector3(sideW, h, t), new Vector3(CORE_WIDTH / 2 - sideW / 2, h / 2, -CORE_LENGTH / 2 + t / 2), materials.Concrete);
      CreateBox(core, "CoreWallB_Top", new Vector3(doorW, h - doorH, t), new Vector3(0, doorH + (h - doorH) / 2, -CORE_LENGTH / 2 + t / 2), materials.Concrete);

      // Build Stairs + Floor Landing
      BuildStairs(core, height, materials);
    }

    private void BuildStairs(GameObject parent, float height, CityMaterialProfile materials) {
      // Layout: 
      // Back Landing (Z-) -> Entry/Exit
      // Front Landing (Z+) -> Turn
      // Left Ramp (Up), Right Ramp (Return)

      float backLandingDepth = 1.5f; // Floor level landing
      float frontLandingDepth = 1.2f; // Mid-turn landing

      // Available length for ramps
      float runLength = CORE_LENGTH - backLandingDepth - frontLandingDepth;
      float rampWidth = (CORE_WIDTH - 0.5f) / 2;
      float halfH = height / 2;

      // Centers relative to Core center (0,0)
      float zBackLanding = -CORE_LENGTH / 2 + backLandingDepth / 2;
      float zFrontLanding = CORE_LENGTH / 2 - frontLandingDepth / 2;
      float zRamp = -CORE_LENGTH / 2 + backLandingDepth + (runLength / 2);

      // 1. FLOOR LANDING (Back) - Essential for exiting the stairwell
      // We lift it slightly (0.05) to avoid z-fighting with the main building floor slab
      CreateBox(parent, "LandingFloor",
          new Vector3(CORE_WIDTH - 0.4f, 0.2f, backLandingDepth),
          new Vector3(0, 0.05f, zBackLanding),
          materials.Concrete);

      // 2. MID LANDING (Front)
      CreateBox(parent, "LandingMid",
          new Vector3(CORE_WIDTH - 0.4f, 0.2f, frontLandingDepth),
          new Vector3(0, halfH, zFrontLanding),
          materials.Concrete);

      // 3. UP RAMP (Left Side)
      // Stretches from Back Landing to Front Landing
      GameObject r1 = CreateBox(parent, "StairUp",
          new Vector3(rampWidth, 0.2f, runLength + 0.4f), // +0.4 to clip into landings
          new Vector3(-CORE_WIDTH / 4, halfH / 2, zRamp),
          materials.Concrete);
      float angle = Mathf.Atan(halfH / runLength) * Mathf.Rad2Deg;
      r1.transform.localRotation = Quaternion.Euler(-angle, 0, 0);

      // 4. RETURN RAMP (Right Side)
      // Stretches from Front Landing to Back (Top)
      GameObject r2 = CreateBox(parent, "StairReturn",
          new Vector3(rampWidth, 0.2f, runLength + 0.4f),
          new Vector3(CORE_WIDTH / 4, halfH + halfH / 2, zRamp),
          materials.Concrete);
      r2.transform.localRotation = Quaternion.Euler(angle, 0, 0);
    }

    private void BuildRoofBulkhead(GameObject parent, float yBase, CityMaterialProfile materials) {
      GameObject bulk = new GameObject("RoofBulkhead");
      bulk.transform.parent = parent.transform;
      bulk.transform.localPosition = new Vector3(0, yBase, 0);

      float h = 3.0f;
      float t = CORE_WALL_THICKNESS;

      // Walls
      CreateBox(bulk, "WallL", new Vector3(t, h, CORE_LENGTH), new Vector3(-CORE_WIDTH / 2 + t / 2, h / 2, 0), materials.WallResidential);
      CreateBox(bulk, "WallR", new Vector3(t, h, CORE_LENGTH), new Vector3(CORE_WIDTH / 2 - t / 2, h / 2, 0), materials.WallResidential);
      CreateBox(bulk, "WallF", new Vector3(CORE_WIDTH, h, t), new Vector3(0, h / 2, CORE_LENGTH / 2 - t / 2), materials.WallResidential);

      float doorW = 1.2f;
      float doorH = 2.1f;
      float sideW = (CORE_WIDTH - doorW) / 2;
      CreateBox(bulk, "WallB_L", new Vector3(sideW, h, t), new Vector3(-CORE_WIDTH / 2 + sideW / 2, h / 2, -CORE_LENGTH / 2 + t / 2), materials.WallResidential);
      CreateBox(bulk, "WallB_R", new Vector3(sideW, h, t), new Vector3(CORE_WIDTH / 2 - sideW / 2, h / 2, -CORE_LENGTH / 2 + t / 2), materials.WallResidential);
      CreateBox(bulk, "WallB_Top", new Vector3(doorW, h - doorH, t), new Vector3(0, doorH + (h - doorH) / 2, -CORE_LENGTH / 2 + t / 2), materials.WallResidential);

      CreateBox(bulk, "Roof", new Vector3(CORE_WIDTH + 0.4f, 0.3f, CORE_LENGTH + 0.4f), new Vector3(0, h, 0), materials.Concrete);

      // Generate the final top stair flight leading up to this door
      // Since this is a standalone object on the roof, we can just build the stairs inside it directly
      // Note: The previous floor's "Return Ramp" ended at yBase.
      // So we need a floor landing here at y=0.
      CreateBox(bulk, "ExitLanding",
          new Vector3(CORE_WIDTH - 0.4f, 0.2f, 2.0f), // Small landing pad
          new Vector3(0, 0.05f, -CORE_LENGTH / 2 + 1.0f),
          materials.Concrete);
    }

    private void BuildSlabWithHole(GameObject parent, string name, Vector2 size, float thick, float yPos, float holeW, float holeL, Material mat) {
      GameObject slab = new GameObject(name);
      slab.transform.parent = parent.transform;
      slab.transform.localPosition = new Vector3(0, yPos, 0);

      float frontLen = (size.y - holeL) / 2;
      CreateBox(slab, "SlabFront", new Vector3(size.x, thick, frontLen), new Vector3(0, 0, (size.y / 2) - (frontLen / 2)), mat);
      CreateBox(slab, "SlabBack", new Vector3(size.x, thick, frontLen), new Vector3(0, 0, -(size.y / 2) + (frontLen / 2)), mat);

      float sideW = (size.x - holeW) / 2;
      CreateBox(slab, "SlabLeft", new Vector3(sideW, thick, holeL), new Vector3(-(size.x / 2) + (sideW / 2), 0, 0), mat);
      CreateBox(slab, "SlabRight", new Vector3(sideW, thick, holeL), new Vector3((size.x / 2) - (sideW / 2), 0, 0), mat);
    }

    private void BuildLobby(GameObject parent, float w, float l, float h, float baseOffset, BuildingStyle style, CityMaterialProfile mats) {
      GameObject lobby = new GameObject("LobbyLevel");
      lobby.transform.parent = parent.transform;
      lobby.transform.localPosition = Vector3.zero;

      float wallThick = 0.3f;
      float doorWidth = style.entranceWidth;
      float doorHeight = 3.5f;
      float yCenter = baseOffset + (h / 2);

      CreateBox(lobby, "WallBack", new Vector3(w, h, wallThick), new Vector3(0, yCenter, -l / 2 + wallThick / 2), mats.WallResidential);
      CreateBox(lobby, "WallLeft", new Vector3(wallThick, h, l - wallThick * 2), new Vector3(-w / 2 + wallThick / 2, yCenter, 0), mats.WallResidential);
      CreateBox(lobby, "WallRight", new Vector3(wallThick, h, l - wallThick * 2), new Vector3(w / 2 - wallThick / 2, yCenter, 0), mats.WallResidential);

      float frontZ = l / 2 - wallThick / 2;
      float sideWidth = (w - doorWidth) / 2;
      CreateBox(lobby, "WallFrontLeft", new Vector3(sideWidth, h, wallThick), new Vector3(-w / 2 + sideWidth / 2, yCenter, frontZ), mats.WallResidential);
      CreateBox(lobby, "WallFrontRight", new Vector3(sideWidth, h, wallThick), new Vector3(w / 2 - sideWidth / 2, yCenter, frontZ), mats.WallResidential);
      CreateBox(lobby, "WallFrontLintel", new Vector3(doorWidth, h - doorHeight, wallThick), new Vector3(0, baseOffset + doorHeight + (h - doorHeight) / 2, frontZ), mats.WallResidential);

      // Lobby Floor (With Hole for Core)
      BuildSlabWithHole(lobby, "LobbyFloor", new Vector2(w - 0.6f, l - 0.6f), 0.1f, baseOffset + 0.05f, CORE_WIDTH, CORE_LENGTH, mats.Concrete);

      if (style.hasCanopy) {
        float canopyDepth = Random.Range(2.0f, 4.0f);
        float canopyHeight = baseOffset + doorHeight + 0.2f;
        float canopyWidth = doorWidth + 2.0f;
        CreateBox(lobby, "CanopyRoof", new Vector3(canopyWidth, 0.2f, canopyDepth), new Vector3(0, canopyHeight, frontZ + (canopyDepth / 2)), mats.WallResidential);
        float pillarSize = 0.2f;
        float pillarX = (canopyWidth / 2) - 0.4f;
        float pillarZ = frontZ + canopyDepth - 0.4f;
        CreateCylinder(lobby, "CanopyPillarL", new Vector3(pillarSize, canopyHeight / 2, pillarSize), new Vector3(-pillarX, canopyHeight / 2, pillarZ), mats.Concrete);
        CreateCylinder(lobby, "CanopyPillarR", new Vector3(pillarSize, canopyHeight / 2, pillarSize), new Vector3(pillarX, canopyHeight / 2, pillarZ), mats.Concrete);
      }
    }
  }
}
