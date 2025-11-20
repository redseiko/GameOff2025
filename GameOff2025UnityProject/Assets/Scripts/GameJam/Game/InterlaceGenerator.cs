using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GameJam {
  public sealed class InterlaceGenerator : MonoBehaviour {
    [Header("Identity")]
    public string apartmentName = "InterlaceBlockA";

    [Header("Block Dimensions")]
    public float blockLength = 70.0f;
    public float blockWidth = 16.5f;
    public float floorHeight = 3.5f;
    public int floorsPerBlock = 6;

    [Header("Internal Structure")]
    public float slabThickness = 0.3f;
    public float centralCoreWidth = 4.0f;
    // EndCapThickness removed, replaced by dynamic window generation

    [Header("Facade Design")]
    [Range(0f, 1f)] public float balconyProbability = 0.35f;
    public float windowInset = 0.6f;
    public float finDepth = 0.6f;
    public int modulesAlongLength = 14; // Controls the "Grid" of the facade

    [Header("Colors")]
    public Color concreteColor = new Color(0.9f, 0.9f, 0.9f);
    public Color interiorColor = new Color(0.6f, 0.6f, 0.65f);
    public Color glassColor = new Color(0.2f, 0.3f, 0.35f, 0.4f);
    public Color balconyFloorColor = new Color(0.6f, 0.5f, 0.4f);
    public Color roofGardenColor = new Color(0.3f, 0.5f, 0.2f);

    private Material matConcrete, matInterior, matGlass, matGlassRail, matBalcony, matRoof;

    private enum FaceSide { Front, Back, Left, Right }

    [ContextMenu("Generate Block")]
    public void GenerateBlock() {
      Cleanup();
      InitializeMaterials();
      GenerateStructure();
    }

    private void Cleanup() {
      while (transform.childCount > 0)
        DestroyImmediate(transform.GetChild(0).gameObject);
    }

    private void GenerateStructure() {
      // 1. Generate Floors
      for (int f = 0; f < floorsPerBlock; f++) {
        GenerateSingleFloor(f);
      }

      // 2. Generate Roof
      GenerateRoof();

      // 3. Generate All 4 Facades
      GenerateFacade(FaceSide.Front); // Z+
      GenerateFacade(FaceSide.Back);  // Z-
      GenerateFacade(FaceSide.Right); // X+
      GenerateFacade(FaceSide.Left);  // X-
    }

    private void GenerateSingleFloor(int floorIndex) {
      float yBase = floorIndex * floorHeight;
      GameObject floorRoot = new GameObject($"Floor{floorIndex}");
      floorRoot.transform.parent = this.transform;
      floorRoot.transform.localPosition = new Vector3(0, yBase, 0);

      // Floor Slab
      CreatePrimitive(floorRoot, "FloorSlab",
          new Vector3(blockLength, slabThickness, blockWidth),
          new Vector3(0, slabThickness / 2, 0),
          Vector3.zero, matConcrete);

      // Internal Spine
      // We shorten it slightly (blockLength - 2m) so it doesn't hit the new End Windows
      float spineHeight = floorHeight - slabThickness;
      float spineLength = blockLength - 2.0f;

      CreatePrimitive(floorRoot, "InternalSpine",
          new Vector3(spineLength, spineHeight, centralCoreWidth),
          new Vector3(0, slabThickness + (spineHeight / 2), 0),
          Vector3.zero, matInterior);
    }

    private void GenerateRoof() {
      float totalHeight = floorsPerBlock * floorHeight;
      GameObject roofRoot = new GameObject("RoofSystem");
      roofRoot.transform.parent = this.transform;
      roofRoot.transform.localPosition = new Vector3(0, totalHeight, 0);

      float roofThick = 0.5f;
      CreatePrimitive(roofRoot, "RoofSlab",
          new Vector3(blockLength, roofThick, blockWidth),
          new Vector3(0, roofThick / 2, 0),
          Vector3.zero, matConcrete);

      CreatePrimitive(roofRoot, "GardenSurface",
          new Vector3(blockLength - 0.5f, 0.1f, blockWidth - 0.5f),
          new Vector3(0, roofThick + 0.05f, 0),
          Vector3.zero, matRoof);

      float pH = 1.2f;
      float pThick = 0.3f;
      CreatePrimitive(roofRoot, "ParapetF", new Vector3(blockLength, pH, pThick), new Vector3(0, pH / 2, blockWidth / 2 - pThick / 2), Vector3.zero, matConcrete);
      CreatePrimitive(roofRoot, "ParapetB", new Vector3(blockLength, pH, pThick), new Vector3(0, pH / 2, -blockWidth / 2 + pThick / 2), Vector3.zero, matConcrete);
      CreatePrimitive(roofRoot, "ParapetL", new Vector3(pThick, pH, blockWidth), new Vector3(-blockLength / 2 + pThick / 2, pH / 2, 0), Vector3.zero, matConcrete);
      CreatePrimitive(roofRoot, "ParapetR", new Vector3(pThick, pH, blockWidth), new Vector3(blockLength / 2 - pThick / 2, pH / 2, 0), Vector3.zero, matConcrete);
    }

    // ---------------------------------------------------------
    // UNIFIED FACADE LOGIC
    // ---------------------------------------------------------

    private void GenerateFacade(FaceSide side) {
      // Determine orientation and dimensions
      bool isLongAxis = (side == FaceSide.Front || side == FaceSide.Back);
      float totalLen = isLongAxis ? blockLength : blockWidth;

      // Calculate Module Size
      // We try to keep modules roughly the same size (approx 5m)
      float referenceModuleSize = blockLength / modulesAlongLength;
      int moduleCount = Mathf.Max(1, Mathf.RoundToInt(totalLen / referenceModuleSize));
      float actualModuleWidth = totalLen / moduleCount;

      // Determine Position & Rotation settings
      // Offset: Distance from center to the face
      float faceOffset = (isLongAxis ? blockWidth : blockLength) / 2.0f;

      // Z-fighting prevention: push balconies slightly different amounts?
      // For now, just precise math.

      Quaternion rotation = Quaternion.identity;
      Vector3 faceNormal = Vector3.forward; // Direction the face is looking

      switch (side) {
        case FaceSide.Front: // Z+
          faceNormal = Vector3.forward;
          rotation = Quaternion.Euler(0, 0, 0);
          break;
        case FaceSide.Back: // Z-
          faceNormal = Vector3.back;
          rotation = Quaternion.Euler(0, 180, 0);
          break;
        case FaceSide.Right: // X+
          faceNormal = Vector3.right;
          rotation = Quaternion.Euler(0, 90, 0);
          break;
        case FaceSide.Left: // X-
          faceNormal = Vector3.left;
          rotation = Quaternion.Euler(0, -90, 0);
          break;
      }

      GameObject facadeRoot = new GameObject($"Facade_{side}");
      facadeRoot.transform.parent = this.transform;
      facadeRoot.transform.localPosition = Vector3.zero;

      // Loop Floors
      for (int f = 0; f < floorsPerBlock; f++) {
        float yPos = (f * floorHeight) + slabThickness;

        // Loop Modules
        for (int m = 0; m < moduleCount; m++) {
          // Calculate lateral position along the face (centered)
          // We iterate from Left to Right relative to the face normal
          float lateralStart = -(totalLen / 2);
          float lateralPos = lateralStart + (m * actualModuleWidth) + (actualModuleWidth / 2);

          // Convert (Lateral, Vertical, Depth) into Local Space
          // Lateral = X (before rotation), Vertical = Y, Depth = Z (before rotation)
          // We start at (lateralPos, yPos, faceOffset) relative to the identity rotation
          // But since we rotate the object, we just place it at the correct world pos?
          // Easier: Place relative to face center, then rotate point.

          // Base pos: Center of the face
          Vector3 centerOfFace = faceNormal * faceOffset;
          // Lateral offset: Perpendicular to normal.
          // If Normal is Z, Lateral is X. If Normal is X, Lateral is -Z (based on rotation).
          // Let's rely on the rotation quaternion to do the work.
          Vector3 localOffset = new Vector3(lateralPos, 0, 0);
          Vector3 finalPos = centerOfFace + (rotation * localOffset);
          finalPos.y = yPos; // Y is constant

          // Randomize
          // Use a stable seed based on position to avoid tiling artifacts
          float seed = (f * 100) + m + (int) side * 500;
          bool hasBalcony = (Mathf.PerlinNoise(seed * 0.1f, seed * 0.13f) < balconyProbability);

          if (hasBalcony) {
            GenerateBalcony(facadeRoot, finalPos, rotation, actualModuleWidth);
          } else {
            GenerateWindowUnit(facadeRoot, finalPos, rotation, actualModuleWidth);
          }
        }
      }
    }

    private void GenerateWindowUnit(GameObject parent, Vector3 pos, Quaternion rot, float width) {
      float h = floorHeight - slabThickness;

      // Inset Vector (Local Z negative)
      Vector3 insetVec = rot * new Vector3(0, 0, -windowInset);

      // 1. Glass
      CreatePrimitive(parent, "GlassWindow",
          new Vector3(width - 0.1f, h - 0.1f, 0.1f),
          pos + insetVec + new Vector3(0, h / 2, 0),
          rot, matGlass);

      // 2. Sun Fin (Local X offset)
      Vector3 finOffset = rot * new Vector3((width / 2), 0, (finDepth / 2) - 0.2f);
      CreatePrimitive(parent, "SunFin",
          new Vector3(0.15f, h, finDepth),
          pos + finOffset + new Vector3(0, h / 2, 0),
          rot, matConcrete);
    }

    private void GenerateBalcony(GameObject parent, Vector3 pos, Quaternion rot, float width) {
      float protrusion = 2.0f;

      // Local offsets relative to the 'pos' on the face surface
      Vector3 floorCenter = rot * new Vector3(0, 0, protrusion / 2);
      Vector3 railFront = rot * new Vector3(0, 0, protrusion);
      Vector3 railLeft = rot * new Vector3(-width / 2, 0, protrusion / 2);
      Vector3 railRight = rot * new Vector3(width / 2, 0, protrusion / 2);

      // 1. Floor
      CreatePrimitive(parent, "BalconyFloor",
          new Vector3(width, 0.15f, protrusion),
          pos + floorCenter + new Vector3(0, 0.075f, 0),
          rot, matBalcony);

      // 2. Rails
      float railH = 1.1f;
      float railT = 0.05f;
      float railY = 0.15f + (railH / 2);

      CreatePrimitive(parent, "RailFront",
          new Vector3(width, railH, railT),
          pos + railFront + new Vector3(0, railY, 0),
          rot, matGlassRail);

      // Rotate side rails 90 deg local
      Quaternion sideRot = rot * Quaternion.Euler(0, 90, 0);

      CreatePrimitive(parent, "RailL",
          new Vector3(protrusion, railH, railT), // Note: Width is now the protrusion length
          pos + railLeft + new Vector3(0, railY, 0),
          sideRot, matGlassRail);

      CreatePrimitive(parent, "RailR",
          new Vector3(protrusion, railH, railT),
          pos + railRight + new Vector3(0, railY, 0),
          sideRot, matGlassRail);

      // 3. Wall Behind
      float h = floorHeight - slabThickness;
      Vector3 wallInset = rot * new Vector3(0, 0, -windowInset * 1.5f);

      CreatePrimitive(parent, "BalconyWall",
          new Vector3(width - 0.1f, h - 0.1f, 0.1f),
          pos + wallInset + new Vector3(0, h / 2, 0),
          rot, matGlass);
    }

    // ---------------------------------------------------------
    // HELPERS
    // ---------------------------------------------------------

    // Updated CreatePrimitive to accept Rotation
    private void CreatePrimitive(GameObject parent, string name, Vector3 scale, Vector3 pos, Vector3 eulerRot, Material mat) {
      CreatePrimitive(parent, name, scale, pos, Quaternion.Euler(eulerRot), mat);
    }

    private void CreatePrimitive(GameObject parent, string name, Vector3 scale, Vector3 pos, Quaternion rot, Material mat) {
      GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
      obj.name = name;
      obj.transform.parent = parent.transform;
      obj.transform.localScale = scale;
      obj.transform.SetLocalPositionAndRotation(pos, rot);
      if (obj.GetComponent<Renderer>())
        obj.GetComponent<Renderer>().sharedMaterial = mat;
    }

    private void InitializeMaterials() {
      matConcrete = GetOrCreateMaterial("Concrete", concreteColor, false);
      matInterior = GetOrCreateMaterial("Interior", interiorColor, false);
      matBalcony = GetOrCreateMaterial("Balcony", balconyFloorColor, false);
      matRoof = GetOrCreateMaterial("RoofGarden", roofGardenColor, false);

      // Custom Shaders
      matGlass = GetOrCreateMaterial("Glass", glassColor, false);
      matGlassRail = GetOrCreateMaterial("GlassRail", glassColor, true);
    }

    private Material GetOrCreateMaterial(string suffix, Color color, bool isTransparent) {
#if UNITY_EDITOR
      string folder = $"Assets/Materials/Apartments/{apartmentName}";
      string fileName = $"{apartmentName}{suffix}";
      string fullPath = $"{folder}/{fileName}.mat";

      if (!AssetDatabase.IsValidFolder("Assets/Materials"))
        AssetDatabase.CreateFolder("Assets", "Materials");
      if (!AssetDatabase.IsValidFolder("Assets/Materials/Apartments"))
        AssetDatabase.CreateFolder("Assets/Materials", "Apartments");
      if (!AssetDatabase.IsValidFolder(folder))
        AssetDatabase.CreateFolder("Assets/Materials/Apartments", apartmentName);

      Material existing = AssetDatabase.LoadAssetAtPath<Material>(fullPath);
      if (existing != null)
        return existing;

      Shader shader;
      if (suffix == "Glass") {
        shader = Shader.Find("GameJam/URPInteriorMapping");
        if (!shader)
          shader = Shader.Find("Universal Render Pipeline/Lit");
      } else {
        shader = Shader.Find("Universal Render Pipeline/Lit");
        if (!shader)
          shader = Shader.Find("Standard");
      }

      Material mat = new Material(shader);

      if (suffix == "Glass" && shader.name == "GameJam/URPInteriorMapping") {
        mat.SetColor("_BaseColor", new Color(0.9f, 0.9f, 0.9f));
        mat.SetColor("_EmissionColor", new Color(1f, 0.9f, 0.7f));
        mat.SetFloat("_EmissionStrength", 2.0f);
        mat.SetFloat("_RoomDepth", 1.0f);
      } else {
        mat.SetColor("_BaseColor", color);
        if (isTransparent) {
          mat.SetFloat("_Surface", 1);
          mat.SetOverrideTag("RenderType", "Transparent");
          mat.SetInt("_SrcBlend", (int) UnityEngine.Rendering.BlendMode.SrcAlpha);
          mat.SetInt("_DstBlend", (int) UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
          mat.SetInt("_ZWrite", 0);
          mat.renderQueue = 3000;
          mat.SetColor("_BaseColor", new Color(color.r, color.g, color.b, 0.3f));
        }
      }

      AssetDatabase.CreateAsset(mat, fullPath);
      AssetDatabase.SaveAssets();
      return mat;
#else
      return new Material(Shader.Find("Universal Render Pipeline/Lit")); 
#endif
    }
  }
}
