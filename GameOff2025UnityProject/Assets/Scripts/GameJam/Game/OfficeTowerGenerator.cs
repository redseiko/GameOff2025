using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GameJam {
  public sealed class OfficeTowerGenerator : MonoBehaviour {
    [Header("Tower Dimensions")]
    public int numberOfFloors = 5;
    public float floorHeight = 4.0f;
    public float width = 26f;
    public float length = 36f;

    [Header("Service Core Settings")]
    public float coreWidth = 7.0f;
    public float stairLength = 10.0f;
    public float elevLength = 5.0f;
    public float coreGap = 1.0f;
    public float coreWallThickness = 0.3f;

    [Header("Layout")]
    public float corridorWidth = 3.0f;
    public float exteriorWallThick = 0.3f;

    [Header("Colors")]
    public Color concreteColor = new Color(0.6f, 0.6f, 0.65f);
    public Color facadeColor = new Color(0.85f, 0.85f, 0.85f);
    public Color windowColor = new Color(0.2f, 0.5f, 0.7f, 0.3f);
    public Color floorColor = new Color(0.25f, 0.25f, 0.25f);
    public Color interiorColor = new Color(0.9f, 0.9f, 0.9f);
    public Color metalColor = new Color(0.4f, 0.4f, 0.45f);

    private Material matConcrete, matFacade, matGlass, matFloor, matInterior, matMetal;

    void Start() { } // Empty to prevent runtime execution

    [ContextMenu("Build Tower")]
    public void BuildTower() {
      Cleanup();
      InitializeMaterials();

      float totalCoreLen = elevLength + coreGap + stairLength;
      float coreGroupZ = 0;

      // Calculate Centers relative to Core Group Center
      float elevCenterZ = coreGroupZ - (totalCoreLen / 2) + (elevLength / 2);
      Vector3 elevPos = new Vector3((width / 2) - (coreWidth / 2), 0, elevCenterZ);

      float stairCenterZ = coreGroupZ + (totalCoreLen / 2) - (stairLength / 2);
      Vector3 stairPos = new Vector3((width / 2) - (coreWidth / 2), 0, stairCenterZ);

      for (int i = 0; i < numberOfFloors; i++) {
        BuildFloor(i, elevPos, stairPos, totalCoreLen);
      }

      BuildRoof(numberOfFloors, elevPos, stairPos, totalCoreLen);
    }

    private void Cleanup() {
      while (transform.childCount > 0)
        DestroyImmediate(transform.GetChild(0).gameObject);
    }

    // ---------------------------------------------------------
    // FLOOR LOGIC
    // ---------------------------------------------------------

    private void BuildFloor(int floorIndex, Vector3 elevPos, Vector3 stairPos, float totalCoreLen) {
      float currentY = floorIndex * floorHeight;
      GameObject floorRoot = new GameObject($"Floor_{floorIndex}");
      floorRoot.transform.parent = this.transform;
      floorRoot.transform.localPosition = new Vector3(0, currentY, 0);

      BuildFloorSlab(floorRoot, totalCoreLen);
      BuildCoreStructure(floorRoot, elevPos, stairPos, false);

      if (floorIndex == 0)
        BuildLobbyFacade(floorRoot);
      else
        BuildStandardFacade(floorRoot);

      if (floorIndex > 0)
        BuildCorridor(floorRoot);
    }

    private void BuildFloorSlab(GameObject parent, float totalCoreLen) {
      float slabThick = 0.2f;
      float mainWidth = width - coreWidth;

      // Main Slab
      CreatePrimitive(parent, "Slab_Main",
          new Vector3(mainWidth, slabThick, length),
          new Vector3(-coreWidth / 2, 0, 0), matFloor);

      // Front Strip (Elevator side)
      float coreStartZ = -(totalCoreLen / 2);
      float frontStripLen = (length / 2) + coreStartZ;
      if (frontStripLen > 0) {
        CreatePrimitive(parent, "Slab_Front",
            new Vector3(coreWidth, slabThick, frontStripLen),
            new Vector3((width / 2) - (coreWidth / 2), 0, -length / 2 + frontStripLen / 2), matFloor);
      }

      // Back Strip (Stair side)
      float coreEndZ = (totalCoreLen / 2);
      float backStripLen = (length / 2) - coreEndZ;
      if (backStripLen > 0) {
        CreatePrimitive(parent, "Slab_Back",
            new Vector3(coreWidth, slabThick, backStripLen),
            new Vector3((width / 2) - (coreWidth / 2), 0, length / 2 - backStripLen / 2), matFloor);
      }

      // Buffer Gap Floor
      float gapCenterZ = -(totalCoreLen / 2) + elevLength + (coreGap / 2);
      CreatePrimitive(parent, "Slab_Buffer",
          new Vector3(coreWidth, slabThick, coreGap),
          new Vector3((width / 2) - (coreWidth / 2), 0, gapCenterZ), matFloor);
    }

    // ---------------------------------------------------------
    // CORE STRUCTURE
    // ---------------------------------------------------------

    private void BuildCoreStructure(GameObject parent, Vector3 elevPos, Vector3 stairPos, bool isRoof) {
      BuildElevatorBox(parent, elevPos);
      BuildStairBox(parent, stairPos);

      if (!isRoof) {
        BuildStairRamps(parent, stairPos);
      }
    }

    private void BuildElevatorBox(GameObject parent, Vector3 pos) {
      GameObject elev = new GameObject("ElevatorShaft");
      elev.transform.parent = parent.transform;
      elev.transform.localPosition = pos;

      float h = floorHeight;
      float t = coreWallThickness;
      float w = coreWidth;
      float l = elevLength;

      // Standard Walls (Centered vertically by h/2 inside CreatePrimitive)
      CreatePrimitive(elev, "W_Ext", new Vector3(t, h, l), new Vector3(w / 2 - t / 2, h / 2, 0), matConcrete);
      CreatePrimitive(elev, "W_Back", new Vector3(w, h, t), new Vector3(0, h / 2, l / 2 - t / 2), matConcrete);
      CreatePrimitive(elev, "W_Front", new Vector3(w, h, t), new Vector3(0, h / 2, -l / 2 + t / 2), matConcrete);

      // *** FIX: Pass Y=0 for WallWithDoor container ***
      // The internal logic of CreateWallWithDoor handles the h/2 lift for the wall segments.
      CreateWallWithDoor(elev, "W_Int",
          new Vector3(t, h, l),
          new Vector3(-w / 2 + t / 2, 0, 0),
          1.6f, matConcrete);
    }

    private void BuildStairBox(GameObject parent, Vector3 pos) {
      GameObject stairs = new GameObject("StairShaft");
      stairs.transform.parent = parent.transform;
      stairs.transform.localPosition = pos;

      float h = floorHeight;
      float t = coreWallThickness;
      float w = coreWidth;
      float l = stairLength;

      CreatePrimitive(stairs, "W_Ext", new Vector3(t, h, l), new Vector3(w / 2 - t / 2, h / 2, 0), matConcrete);
      CreatePrimitive(stairs, "W_Back", new Vector3(w, h, t), new Vector3(0, h / 2, l / 2 - t / 2), matConcrete);
      CreatePrimitive(stairs, "W_Front", new Vector3(w, h, t), new Vector3(0, h / 2, -l / 2 + t / 2), matConcrete);

      // *** FIX: Pass Y=0 here as well ***
      float doorOffset = -(l / 2) + 2.0f;
      CreateWallWithDoorOffset(stairs, "W_Int",
          new Vector3(t, h, l),
          new Vector3(-w / 2 + t / 2, 0, 0),
          1.2f, doorOffset, matConcrete);
    }

    private void BuildStairRamps(GameObject parent, Vector3 pos) {
      // 3-Zone Ramp System
      float wallT = coreWallThickness;
      float innerW = coreWidth - (wallT * 2);
      float innerL = stairLength - (wallT * 2);
      float landingDepth = 2.5f;
      float rampRunLen = innerL - (2 * landingDepth);

      GameObject ramps = new GameObject("Ramps");
      ramps.transform.parent = parent.transform;
      ramps.transform.localPosition = pos;

      float zFrontLanding = -innerL / 2 + landingDepth / 2;
      float zBackLanding = innerL / 2 - landingDepth / 2;
      float zRampCenter = 0;
      float halfH = floorHeight / 2;

      // Bottom Landing
      CreatePrimitive(ramps, "Landing_Bot",
          new Vector3(innerW / 2, 0.2f, landingDepth),
          new Vector3(-innerW / 4, 0.1f, zFrontLanding), matFloor);

      // Ramp Up 1
      GameObject r1 = CreatePrimitive(ramps, "Ramp_Up_1",
          new Vector3(innerW / 2 - 0.1f, 0.2f, rampRunLen),
          new Vector3(-innerW / 4, halfH / 2, zRampCenter), matFloor);
      float angle = Mathf.Atan(halfH / rampRunLen) * Mathf.Rad2Deg;
      r1.transform.localRotation = Quaternion.Euler(-angle, 0, 0);

      // Mid Landing
      CreatePrimitive(ramps, "Landing_Mid",
          new Vector3(innerW, 0.2f, landingDepth),
          new Vector3(0, halfH, zBackLanding), matFloor);

      // Ramp Up 2
      GameObject r2 = CreatePrimitive(ramps, "Ramp_Up_2",
          new Vector3(innerW / 2 - 0.1f, 0.2f, rampRunLen),
          new Vector3(innerW / 4, halfH + halfH / 2, zRampCenter), matFloor);
      r2.transform.localRotation = Quaternion.Euler(angle, 0, 0);

      // Top Landing
      CreatePrimitive(ramps, "Landing_Top",
          new Vector3(innerW, 0.2f, landingDepth),
          new Vector3(0, floorHeight, zFrontLanding), matFloor);

      // Rail
      CreatePrimitive(ramps, "Rail_Divider",
          new Vector3(0.2f, floorHeight, rampRunLen),
          new Vector3(0, floorHeight / 2, zRampCenter), matMetal);
    }

    // ---------------------------------------------------------
    // UTILS (Facade, Roof, Walls)
    // ---------------------------------------------------------

    private void BuildRoof(int floors, Vector3 elevPos, Vector3 stairPos, float coreLen) {
      float y = floors * floorHeight;
      GameObject roof = new GameObject("Roof");
      roof.transform.parent = transform;
      roof.transform.localPosition = new Vector3(0, y, 0);

      BuildFloorSlab(roof, coreLen);

      float pH = 1.2f;
      CreatePrimitive(roof, "P_F", new Vector3(width, pH, 0.3f), new Vector3(0, pH / 2, -length / 2), matFacade);
      CreatePrimitive(roof, "P_B", new Vector3(width, pH, 0.3f), new Vector3(0, pH / 2, length / 2), matFacade);
      CreatePrimitive(roof, "P_L", new Vector3(0.3f, pH, length), new Vector3(-width / 2, pH / 2, 0), matFacade);
      CreatePrimitive(roof, "P_R", new Vector3(0.3f, pH, length), new Vector3(width / 2, pH / 2, 0), matFacade);

      BuildCoreStructure(roof, elevPos, stairPos, true);

      CreatePrimitive(roof, "Elev_Cap", new Vector3(coreWidth, 0.3f, elevLength), elevPos + new Vector3(0, floorHeight, 0), matConcrete);
      CreatePrimitive(roof, "Stair_Cap", new Vector3(coreWidth, 0.3f, stairLength), stairPos + new Vector3(0, floorHeight, 0), matConcrete);
    }

    private void BuildLobbyFacade(GameObject parent) {
      float sideW = (width - 4f) / 2;
      CreatePrimitive(parent, "G_L", new Vector3(sideW, floorHeight, 0.1f), new Vector3(-width / 2 + sideW / 2, floorHeight / 2, -length / 2), matGlass);
      CreatePrimitive(parent, "G_R", new Vector3(sideW, floorHeight, 0.1f), new Vector3(width / 2 - sideW / 2, floorHeight / 2, -length / 2), matGlass);
      CreatePrimitive(parent, "Canopy", new Vector3(6f, 0.2f, 3f), new Vector3(0, 3f, -length / 2 - 1.5f), matFacade);

      CreatePrimitive(parent, "W_B", new Vector3(width, floorHeight, 0.3f), new Vector3(0, floorHeight / 2, length / 2), matFacade);
      CreatePrimitive(parent, "W_L", new Vector3(0.3f, floorHeight, length), new Vector3(-width / 2, floorHeight / 2, 0), matFacade);
      CreatePrimitive(parent, "W_R", new Vector3(0.3f, floorHeight, length), new Vector3(width / 2, floorHeight / 2, 0), matFacade);
    }

    private void BuildStandardFacade(GameObject parent) {
      float t = exteriorWallThick;
      BuildBand(parent, new Vector3(0, 0, -length / 2), width, t);
      BuildBand(parent, new Vector3(0, 0, length / 2), width, t);
      BuildBand(parent, new Vector3(-width / 2, 0, 0), t, length);
      BuildBand(parent, new Vector3(width / 2, 0, 0), t, length);
    }

    private void BuildBand(GameObject p, Vector3 pos, float w, float d) {
      GameObject b = new GameObject("Band");
      b.transform.parent = p.transform;
      b.transform.localPosition = pos;
      CreatePrimitive(b, "B", new Vector3(w, 1, d), new Vector3(0, 0.5f, 0), matFacade);
      CreatePrimitive(b, "W", new Vector3(w, 2, d), new Vector3(0, 2f, 0), matGlass);
      CreatePrimitive(b, "T", new Vector3(w, 1, d), new Vector3(0, 3.5f, 0), matFacade);
    }

    private void BuildCorridor(GameObject parent) {
      CreatePrimitive(parent, "Div1", new Vector3(0.5f, floorHeight, 0.5f), new Vector3(-width / 4, floorHeight / 2, length / 4), matInterior);
      CreatePrimitive(parent, "Div2", new Vector3(0.5f, floorHeight, 0.5f), new Vector3(-width / 4, floorHeight / 2, -length / 4), matInterior);
    }

    private void CreateWallWithDoorOffset(GameObject parent, string name, Vector3 size, Vector3 pos, float doorWidth, float doorOffsetZ, Material mat) {
      GameObject w = new GameObject(name);
      w.transform.parent = parent.transform;
      w.transform.localPosition = pos;

      float fullLen = size.z;
      float thick = size.x;
      float h = size.y;
      float doorH = 2.2f;

      float doorStart = doorOffsetZ - (doorWidth / 2);
      float doorEnd = doorOffsetZ + (doorWidth / 2);
      float wallStart = -fullLen / 2;
      float wallEnd = fullLen / 2;

      // Segment 1: Wall
      float s1Len = doorStart - wallStart;
      if (s1Len > 0.05f) {
        CreatePrimitive(w, "S1", new Vector3(thick, h, s1Len), new Vector3(0, h / 2, wallStart + s1Len / 2), mat);
      }

      // Segment 2: Wall
      float s2Len = wallEnd - doorEnd;
      if (s2Len > 0.05f) {
        CreatePrimitive(w, "S2", new Vector3(thick, h, s2Len), new Vector3(0, h / 2, doorEnd + s2Len / 2), mat);
      }

      // Lintel
      CreatePrimitive(w, "Top", new Vector3(thick, h - doorH, doorWidth), new Vector3(0, doorH + (h - doorH) / 2, doorOffsetZ), mat);
    }

    private void CreateWallWithDoor(GameObject parent, string name, Vector3 size, Vector3 pos, float doorWidth, Material mat) {
      CreateWallWithDoorOffset(parent, name, size, pos, doorWidth, 0, mat);
    }

    private GameObject CreatePrimitive(GameObject parent, string name, Vector3 scale, Vector3 pos, Material mat) {
      GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
      obj.name = name;
      obj.transform.parent = parent.transform;
      obj.transform.localScale = scale;
      obj.transform.localPosition = pos;
      if (obj.GetComponent<Renderer>())
        obj.GetComponent<Renderer>().sharedMaterial = mat;
      return obj;
    }

    private void InitializeMaterials() {
      matConcrete = GetOrCreateMaterial("Office_Concrete", concreteColor, false);
      matFacade = GetOrCreateMaterial("Office_Facade", facadeColor, false);
      matGlass = GetOrCreateMaterial("Office_Glass", windowColor, true);
      matFloor = GetOrCreateMaterial("Office_Floor", floorColor, false);
      matInterior = GetOrCreateMaterial("Office_Interior", interiorColor, false);
      matMetal = GetOrCreateMaterial("Office_Metal", metalColor, false);
    }

    private Material GetOrCreateMaterial(string matName, Color color, bool isTransparent) {
#if UNITY_EDITOR
      string folder = "Assets/Materials/Office";
      if (!AssetDatabase.IsValidFolder("Assets/Materials"))
        AssetDatabase.CreateFolder("Assets", "Materials");
      if (!AssetDatabase.IsValidFolder(folder))
        AssetDatabase.CreateFolder("Assets/Materials", "Office");
      string fullPath = $"{folder}/{matName}.mat";
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
