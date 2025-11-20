using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GameJam {
  public sealed class InterlaceGenerator : MonoBehaviour {
    [Header("Identity")]
    public string apartmentName = "InterlaceBlockA";

    [Header("Block Dimensions (To Scale)")]
    public float blockLength = 70.0f;
    public float blockWidth = 16.5f;
    public float floorHeight = 3.5f;
    public int floorsPerBlock = 6;

    [Header("Internal Structure")]
    public float slabThickness = 0.3f;
    public float centralCoreWidth = 4.0f;
    public float endCapThickness = 1.0f;

    [Header("Facade Design")]
    [Range(0f, 1f)] public float balconyProbability = 0.35f;
    public float windowInset = 0.6f;
    public float finDepth = 0.6f;
    public int modulesAlongLength = 14;

    [Header("Colors")]
    public Color concreteColor = new Color(0.9f, 0.9f, 0.9f); // Exterior White
    public Color interiorColor = new Color(0.6f, 0.6f, 0.65f); // Darker Grey for inside walls
    public Color glassColor = new Color(0.2f, 0.3f, 0.35f, 0.4f);
    public Color balconyFloorColor = new Color(0.6f, 0.5f, 0.4f);
    public Color roofGardenColor = new Color(0.3f, 0.5f, 0.2f);

    private Material matConcrete, matInterior, matGlass, matBalcony, matRoof;

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
      for (int f = 0; f < floorsPerBlock; f++) {
        GenerateSingleFloor(f);
      }

      GenerateRoof();
      GenerateLongFacade(true);
      GenerateLongFacade(false);
      GenerateEndCaps();
    }

    private void GenerateSingleFloor(int floorIndex) {
      float yBase = floorIndex * floorHeight;
      GameObject floorRoot = new GameObject($"Floor{floorIndex}");
      floorRoot.transform.parent = this.transform;
      floorRoot.transform.localPosition = new Vector3(0, yBase, 0);

      // A. Floor Slab (Exterior visible band) -> Uses Concrete
      CreatePrimitive(floorRoot, "FloorSlab",
          new Vector3(blockLength, slabThickness, blockWidth),
          new Vector3(0, slabThickness / 2, 0),
          matConcrete);

      // B. Internal Spine (Visible only through windows) -> Uses Interior Material
      float spineHeight = floorHeight - slabThickness;
      float spineLength = blockLength - (endCapThickness * 2);

      CreatePrimitive(floorRoot, "InternalSpine",
          new Vector3(spineLength, spineHeight, centralCoreWidth),
          new Vector3(0, slabThickness + (spineHeight / 2), 0),
          matInterior); // <--- CHANGED HERE
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
          matConcrete);

      CreatePrimitive(roofRoot, "GardenSurface",
          new Vector3(blockLength - 0.5f, 0.1f, blockWidth - 0.5f),
          new Vector3(0, roofThick + 0.05f, 0),
          matRoof);

      float pH = 1.2f;
      float pThick = 0.3f;
      CreatePrimitive(roofRoot, "ParapetF", new Vector3(blockLength, pH, pThick), new Vector3(0, pH / 2, blockWidth / 2 - pThick / 2), matConcrete);
      CreatePrimitive(roofRoot, "ParapetB", new Vector3(blockLength, pH, pThick), new Vector3(0, pH / 2, -blockWidth / 2 + pThick / 2), matConcrete);
      CreatePrimitive(roofRoot, "ParapetL", new Vector3(pThick, pH, blockWidth), new Vector3(-blockLength / 2 + pThick / 2, pH / 2, 0), matConcrete);
      CreatePrimitive(roofRoot, "ParapetR", new Vector3(pThick, pH, blockWidth), new Vector3(blockLength / 2 - pThick / 2, pH / 2, 0), matConcrete);
    }

    private void GenerateLongFacade(bool isFront) {
      float zPos = isFront ? (blockWidth / 2) : -(blockWidth / 2);
      float zDir = isFront ? 1 : -1;

      GameObject facadeRoot = new GameObject(isFront ? "FacadeFront" : "FacadeBack");
      facadeRoot.transform.parent = this.transform;
      facadeRoot.transform.localPosition = Vector3.zero;

      float moduleLength = blockLength / modulesAlongLength;

      for (int f = 0; f < floorsPerBlock; f++) {
        float yPos = (f * floorHeight) + slabThickness;

        for (int m = 0; m < modulesAlongLength; m++) {
          float xStart = -(blockLength / 2);
          float xPos = xStart + (m * moduleLength) + (moduleLength / 2);

          if (Mathf.Abs(xPos) > (blockLength / 2) - endCapThickness)
            continue;

          float seed = (f * 100) + m + (isFront ? 0 : 5000);
          bool hasBalcony = (Mathf.PerlinNoise(seed * 0.1f, seed * 0.1f) < balconyProbability);

          if (hasBalcony) {
            GenerateBalcony(facadeRoot, xPos, yPos, zPos, zDir, moduleLength);
          } else {
            GenerateWindowUnit(facadeRoot, xPos, yPos, zPos, zDir, moduleLength);
          }
        }
      }
    }

    private void GenerateWindowUnit(GameObject parent, float x, float y, float z, float zDir, float width) {
      float h = floorHeight - slabThickness;
      float glassZ = z - (windowInset * zDir);

      CreatePrimitive(parent, "GlassWindow",
          new Vector3(width - 0.1f, h - 0.1f, 0.1f),
          new Vector3(x, y + (h / 2), glassZ),
          matGlass);

      CreatePrimitive(parent, "SunFin",
          new Vector3(0.15f, h, finDepth),
          new Vector3(x + (width / 2), y + (h / 2), z + (finDepth / 2 * zDir) - (0.2f * zDir)),
          matConcrete);
    }

    private void GenerateBalcony(GameObject parent, float x, float y, float z, float zDir, float width) {
      float protrusion = 2.0f;

      // Floor
      CreatePrimitive(parent, "BalconyFloor",
          new Vector3(width, 0.15f, protrusion),
          new Vector3(x, y + 0.075f, z + (protrusion / 2 * zDir)),
          matBalcony);

      // Rails
      float railH = 1.1f;
      float railT = 0.05f;
      float railY = y + 0.15f + (railH / 2);

      CreatePrimitive(parent, "RailFront",
          new Vector3(width, railH, railT),
          new Vector3(x, railY, z + (protrusion * zDir)),
          matGlass);
      CreatePrimitive(parent, "RailSideL",
          new Vector3(railT, railH, protrusion),
          new Vector3(x - (width / 2), railY, z + (protrusion / 2 * zDir)),
          matGlass);
      CreatePrimitive(parent, "RailSideR",
          new Vector3(railT, railH, protrusion),
          new Vector3(x + (width / 2), railY, z + (protrusion / 2 * zDir)),
          matGlass);

      // Wall Behind Balcony
      float wallZ = z - (windowInset * 1.5f * zDir);
      float h = floorHeight - slabThickness;

      CreatePrimitive(parent, "BalconyWallGlass",
          new Vector3(width - 0.1f, h - 0.1f, 0.1f),
          new Vector3(x, y + (h / 2), wallZ),
          matGlass);
    }

    private void GenerateEndCaps() {
      float totalH = floorsPerBlock * floorHeight;

      GameObject capR = new GameObject("EndCapRight");
      capR.transform.parent = this.transform;
      capR.transform.localPosition = new Vector3((blockLength / 2) - (endCapThickness / 2), totalH / 2, 0);
      CreatePrimitive(capR, "WallSolid", new Vector3(endCapThickness, totalH, blockWidth), Vector3.zero, matConcrete);

      GameObject capL = new GameObject("EndCapLeft");
      capL.transform.parent = this.transform;
      capL.transform.localPosition = new Vector3(-(blockLength / 2) + (endCapThickness / 2), totalH / 2, 0);
      CreatePrimitive(capL, "WallSolid", new Vector3(endCapThickness, totalH, blockWidth), Vector3.zero, matConcrete);
    }

    // ---------------------------------------------------------
    // HELPERS
    // ---------------------------------------------------------

    private void CreatePrimitive(GameObject parent, string name, Vector3 scale, Vector3 pos, Material mat) {
      GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
      obj.name = name;
      obj.transform.parent = parent.transform;
      obj.transform.localScale = scale;
      obj.transform.localPosition = pos;
      if (obj.GetComponent<Renderer>())
        obj.GetComponent<Renderer>().sharedMaterial = mat;
    }

    private void InitializeMaterials() {
      matConcrete = GetOrCreateMaterial("Concrete", concreteColor, false);
      matInterior = GetOrCreateMaterial("Interior", interiorColor, false); // New
      matGlass = GetOrCreateMaterial("Glass", glassColor, true);
      matBalcony = GetOrCreateMaterial("Balcony", balconyFloorColor, false);
      matRoof = GetOrCreateMaterial("RoofGarden", roofGardenColor, false);
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

      Shader shader = Shader.Find("Universal Render Pipeline/Lit");
      if (!shader)
        shader = Shader.Find("Standard");

      Material mat = new Material(shader);
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

      AssetDatabase.CreateAsset(mat, fullPath);
      AssetDatabase.SaveAssets();
      return mat;
#else
      return new Material(Shader.Find("Universal Render Pipeline/Lit")); 
#endif
    }
  }
}
