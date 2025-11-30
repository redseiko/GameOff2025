using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GameJam {

  public class CityLevelGenerator : MonoBehaviour {
    [Header("Grid Configuration")]
    public CityConfig config = new CityConfig {
      cityWidth = 60,
      cityLength = 60,
      minBlockSize = 8,
      maxBlockSize = 16,
      minLotSize = 2,
      maxLotAspectRatio = 2.0f,
      mainRoadWidth = 2
    };

    public float cellSize = 10.0f;

    [Header("Zoning Configuration")]
    public ZoningWeights zoning = new ZoningWeights { useDistricts = true, commercial = 0.2f, residential = 0.6f, industrial = 0.1f, park = 0.1f };

    [Header("Visual Settings")]
    public Color roadColor = new Color(0.2f, 0.2f, 0.2f);
    public Color sidewalkColor = new Color(0.7f, 0.7f, 0.7f);
    public Color buildingConcrete = new Color(0.85f, 0.85f, 0.85f);
    public Color buildingBrick = new Color(0.7f, 0.5f, 0.4f);
    public Color buildingGlass = new Color(0.2f, 0.4f, 0.5f, 0.6f);
    public Color buildingIndustrial = new Color(0.5f, 0.5f, 0.55f);

    [Header("Specific Zone Colors")]
    public Color glassCommercial = new Color(0.2f, 0.4f, 0.6f, 0.6f);
    public Color glassResidential = new Color(0.7f, 0.6f, 0.4f, 0.6f);
    public Color glassIndustrial = new Color(0.3f, 0.35f, 0.4f, 0.6f);
    public Color wallResidential = new Color(0.8f, 0.75f, 0.7f);
    public Color wallIndustrial = new Color(0.4f, 0.4f, 0.45f);
    public Color parkGreen = new Color(0.2f, 0.5f, 0.2f);

    // --- Internal State ---
    private CellType[,] grid;
    private readonly List<RectInt> cityBlocks = new List<RectInt>();

    // The Data Object holding all our materials
    private CityMaterialProfile materialProfile;

    // The Strategy Dictionary (Zone -> Generator)
    private Dictionary<CityZone, IPlotGenerator> generators;

    [ContextMenu("Generate City")]
    public void GenerateCity() {
      Cleanup();
      InitializeGrid();
      InitializeMaterials();
      InitializeGenerators();

      // 1. Layout Data
      RectInt fullCity = new RectInt(0, 0, config.cityWidth, config.cityLength);
      SplitBlockForRoads(fullCity);

      // 2. Build Infrastructure
      BuildOptimizedRoads();

      // 3. Build Lots
      BuildPlots();
    }

    private void Cleanup() {
      Transform roadNet = transform.Find("RoadNetwork");
      Transform buildNet = transform.Find("BuildingNetwork");

      if (roadNet != null)
        Undo.DestroyObjectImmediate(roadNet.gameObject);
      if (buildNet != null)
        Undo.DestroyObjectImmediate(buildNet.gameObject);

      while (transform.childCount > 0)
        DestroyImmediate(transform.GetChild(0).gameObject);
    }

    private void InitializeGrid() {
      grid = new CellType[config.cityWidth, config.cityLength];
      cityBlocks.Clear();
    }

    private void InitializeGenerators() {
      generators = new Dictionary<CityZone, IPlotGenerator>();

      // 1. Residential: Mix of Old Stubs (70%) and New Walkables (30%)
      var resSelector = new RandomSelectorGenerator();
      resSelector.AddCandidate(new ResidentialGenerator(), 0.1f);        // The faceted block we made previously
      resSelector.AddCandidate(new ResidentialWalkableGenerator(), 0.9f); // The new one with the door
      generators.Add(CityZone.Residential, resSelector);

      // 2. Others: Keep as single generators for now
      generators.Add(CityZone.Commercial, new CommercialGenerator());
      generators.Add(CityZone.Industrial, new IndustrialGenerator());
      generators.Add(CityZone.Park, new ParkGenerator());
    }

    // --- LAYOUT ---
    private void SplitBlockForRoads(RectInt area) {
      int roadW = Mathf.Max(1, config.mainRoadWidth);
      int minSpaceNeeded = (config.minBlockSize * 2) + roadW;

      bool forceSplit = false;
      bool splitHorizontal = false;

      if (area.width > config.maxBlockSize) { forceSplit = true; splitHorizontal = true; } else if (area.height > config.maxBlockSize) { forceSplit = true; splitHorizontal = false; }

      if (!forceSplit) {
        if (area.width < minSpaceNeeded || area.height < minSpaceNeeded) { RegisterCityBlock(area); return; }
        if (Random.value > 0.7f) { RegisterCityBlock(area); return; }
      }

      if (!forceSplit) {
        splitHorizontal = area.width > area.height;
        if (area.width > minSpaceNeeded && area.height > minSpaceNeeded) {
          if (Random.value > 0.5f)
            splitHorizontal = !splitHorizontal;
        }
      }

      if (splitHorizontal) {
        if (area.width < minSpaceNeeded) { RegisterCityBlock(area); return; }
        int splitX = forceSplit ? (area.width - roadW) / 2 : Random.Range(config.minBlockSize, area.width - config.minBlockSize - roadW);

        for (int w = 0; w < roadW; w++)
          for (int y = area.y; y < area.yMax; y++)
            grid[area.x + splitX + w, y] = CellType.Road;
        SplitBlockForRoads(new RectInt(area.x, area.y, splitX, area.height));
        SplitBlockForRoads(new RectInt(area.x + splitX + roadW, area.y, area.width - splitX - roadW, area.height));
      } else {
        if (area.height < minSpaceNeeded) { RegisterCityBlock(area); return; }
        int splitY = forceSplit ? (area.height - roadW) / 2 : Random.Range(config.minBlockSize, area.height - config.minBlockSize - roadW);

        for (int w = 0; w < roadW; w++)
          for (int x = area.x; x < area.xMax; x++)
            grid[x, area.y + splitY + w] = CellType.Road;
        SplitBlockForRoads(new RectInt(area.x, area.y, area.width, splitY));
        SplitBlockForRoads(new RectInt(area.x, area.y + splitY + roadW, area.width, area.height - splitY - roadW));
      }
    }

    private void RegisterCityBlock(RectInt area) {
      for (int x = area.x; x < area.xMax; x++) {
        for (int y = area.y; y < area.yMax; y++) {
          if (grid[x, y] != CellType.Road)
            grid[x, y] = CellType.Building;
        }
      }
      cityBlocks.Add(area);
    }

    // --- ROADS ---
    private void BuildOptimizedRoads() {
      GameObject roadRoot = new GameObject("RoadNetwork");
      roadRoot.transform.parent = this.transform;
      roadRoot.transform.localPosition = Vector3.zero;
      Undo.RegisterCreatedObjectUndo(roadRoot, "Create Roads");

      bool[,] processed = new bool[config.cityWidth, config.cityLength];

      for (int z = 0; z < config.cityLength; z++) {
        for (int x = 0; x < config.cityWidth; x++) {
          if (grid[x, z] == CellType.Road && !processed[x, z]) {
            // Greedy Mesh
            int width = 0;
            while (x + width < config.cityWidth && grid[x + width, z] == CellType.Road && !processed[x + width, z])
              width++;

            int height = 1;
            bool canExpand = true;
            while (canExpand && z + height < config.cityLength) {
              for (int k = 0; k < width; k++) {
                if (grid[x + k, z + height] != CellType.Road || processed[x + k, z + height]) { canExpand = false; break; }
              }
              if (canExpand)
                height++;
            }

            for (int dz = 0; dz < height; dz++)
              for (int dx = 0; dx < width; dx++)
                processed[x + dx, z + dz] = true;

            CreateSmartRoadRect(roadRoot, new RectInt(x, z, width, height));
          }
        }
      }
    }

    private void CreateSmartRoadRect(GameObject parent, RectInt area) {
      float w = area.width * cellSize;
      float l = area.height * cellSize;
      float centerX = (area.x * cellSize) + (w / 2f) - (cellSize / 2);
      float centerZ = (area.y * cellSize) + (l / 2f) - (cellSize / 2);

      GameObject segment = new GameObject($"RoadX{area.x}Y{area.y}");
      segment.transform.parent = parent.transform;
      segment.transform.localPosition = new Vector3(centerX, 0, centerZ);

      CreateBox(segment, "Asphalt", new Vector3(w, 0.1f, l), Vector3.zero, materialProfile.Asphalt);

      float swWidth = 2.0f;
      float swHeight = 0.2f;
      float halfW = w / 2;
      float halfL = l / 2;

      if (area.x > 0)
        GenerateEdgeSidewalks(segment, area.y, area.height, true,
            (offset) => grid[area.x - 1, offset] != CellType.Road,
            (centerOffset, length) => new Vector3(-halfW + swWidth / 2, 0.05f, -halfL + centerOffset),
            (length) => new Vector3(swWidth, swHeight, length));

      if (area.x + area.width < config.cityWidth)
        GenerateEdgeSidewalks(segment, area.y, area.height, true,
            (offset) => grid[area.x + area.width, offset] != CellType.Road,
            (centerOffset, length) => new Vector3(halfW - swWidth / 2, 0.05f, -halfL + centerOffset),
            (length) => new Vector3(swWidth, swHeight, length));

      if (area.y > 0)
        GenerateEdgeSidewalks(segment, area.x, area.width, false,
            (offset) => grid[offset, area.y - 1] != CellType.Road,
            (centerOffset, length) => new Vector3(-halfW + centerOffset, 0.05f, -halfL + swWidth / 2),
            (length) => new Vector3(length, swHeight, swWidth));

      if (area.y + area.height < config.cityLength)
        GenerateEdgeSidewalks(segment, area.x, area.width, false,
            (offset) => grid[offset, area.y + area.height] != CellType.Road,
            (centerOffset, length) => new Vector3(-halfW + centerOffset, 0.05f, halfL - swWidth / 2),
            (length) => new Vector3(length, swHeight, swWidth));
    }

    private void GenerateEdgeSidewalks(GameObject parent, int startIdx, int count, bool vertical,
                                       System.Func<int, bool> isNeighborNonRoad,
                                       System.Func<float, float, Vector3> getPos,
                                       System.Func<float, Vector3> getScale) {

      float currentLen = 0;
      float startOffset = 0;
      bool building = false;

      for (int k = 0; k < count; k++) {
        int gridIndex = startIdx + k;
        bool solid = isNeighborNonRoad(gridIndex);

        if (solid) {
          if (!building) { building = true; startOffset = k * cellSize; }
          currentLen += cellSize;
        } else {
          if (building) {
            SpawnSidewalkPiece(parent, startOffset, currentLen, getPos, getScale);
            building = false;
            currentLen = 0;
          }
        }
      }
      if (building)
        SpawnSidewalkPiece(parent, startOffset, currentLen, getPos, getScale);
    }

    private void SpawnSidewalkPiece(GameObject parent, float startOffset, float length,
                                    System.Func<float, float, Vector3> getPos,
                                    System.Func<float, Vector3> getScale) {
      float centerPos = startOffset + (length / 2f);
      CreateBox(parent, "Sidewalk", getScale(length), getPos(centerPos, length), materialProfile.Sidewalk);
    }

    // --- BUILDINGS (Using Strategy Pattern) ---
    private void BuildPlots() {
      GameObject plotRoot = new GameObject("BuildingNetwork");
      plotRoot.transform.parent = this.transform;
      plotRoot.transform.localPosition = Vector3.zero;
      Undo.RegisterCreatedObjectUndo(plotRoot, "Create Buildings");

      foreach (var block in cityBlocks) {
        CityZone blockZone = DetermineZone(block);
        List<RectInt> lots = new List<RectInt>();
        SubdivideBlockIntoLots(block, lots);

        foreach (var lot in lots) {
          float w = lot.width * cellSize;
          float l = lot.height * cellSize;
          float x = (lot.x * cellSize) + (w / 2) - (cellSize / 2);
          float z = (lot.y * cellSize) + (l / 2) - (cellSize / 2);

          GameObject pObj = new GameObject($"LotX{lot.x}Y{lot.y}");
          pObj.transform.parent = plotRoot.transform;
          pObj.transform.localPosition = new Vector3(x, 0, z);

          // Use the Strategy Dictionary
          if (generators.ContainsKey(blockZone)) {
            // Generate a stable seed based on position
            int seed = (lot.x * 1000) + lot.y;
            generators[blockZone].Generate(pObj, new Vector2(w, l), blockZone, materialProfile, seed);
          }
        }
      }
    }

    private CityZone DetermineZone(RectInt block) {
      if (zoning.useDistricts) {
        float dist = Vector2.Distance(block.center, new Vector2(config.cityWidth / 2, config.cityLength / 2));
        if (dist < config.cityWidth * 0.25f)
          return CityZone.Commercial;
        else if (dist > config.cityWidth * 0.45f && Random.value > 0.7f)
          return CityZone.Industrial;
        else if (Random.value > 0.9f)
          return CityZone.Park;
        return CityZone.Residential;
      } else {
        float r = Random.value;
        float sum = 0;
        sum += zoning.commercial;
        if (r < sum)
          return CityZone.Commercial;
        sum += zoning.residential;
        if (r < sum)
          return CityZone.Residential;
        sum += zoning.industrial;
        if (r < sum)
          return CityZone.Industrial;
        return CityZone.Park;
      }
    }

    private void SubdivideBlockIntoLots(RectInt area, List<RectInt> result) {
      bool aspectTooLong = (float) area.width / area.height > config.maxLotAspectRatio;
      bool aspectTooTall = (float) area.height / area.width > config.maxLotAspectRatio;
      bool tooBig = area.width > config.minLotSize * 3 || area.height > config.minLotSize * 3;
      bool shouldSplit = aspectTooLong || aspectTooTall || (tooBig && Random.value > 0.3f);

      if (!shouldSplit) { result.Add(area); return; }

      bool splitH = area.width > area.height;
      if (aspectTooLong)
        splitH = true;
      else if (aspectTooTall)
        splitH = false;

      if (splitH) {
        if (area.width < config.minLotSize * 2) { result.Add(area); return; }
        int split = area.width / 2;
        SubdivideBlockIntoLots(new RectInt(area.x, area.y, split, area.height), result);
        SubdivideBlockIntoLots(new RectInt(area.x + split, area.y, area.width - split, area.height), result);
      } else {
        if (area.height < config.minLotSize * 2) { result.Add(area); return; }
        int split = area.height / 2;
        SubdivideBlockIntoLots(new RectInt(area.x, area.y, area.width, split), result);
        SubdivideBlockIntoLots(new RectInt(area.x, area.y + split, area.width, area.height - split), result);
      }
    }

    // --- UTILS & MATERIALS ---
    private void CreateBox(GameObject parent, string name, Vector3 scale, Vector3 pos, Material mat) {
      GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
      obj.name = name;
      obj.transform.parent = parent.transform;
      obj.transform.localScale = scale;
      obj.transform.localPosition = pos;
      if (obj.GetComponent<Renderer>())
        obj.GetComponent<Renderer>().sharedMaterial = mat;
    }

    private void InitializeMaterials() {
      materialProfile = new CityMaterialProfile {
        // Infrastructure
        Asphalt = GetOrCreateMaterial("RoadAsphalt", roadColor),
        Sidewalk = GetOrCreateMaterial("RoadSidewalk", sidewalkColor),

        // Base Construction
        Concrete = GetOrCreateMaterial("BuildConcrete", buildingConcrete),
        Brick = GetOrCreateMaterial("BuildBrick", buildingBrick),

        // Glass Types
        GlassCommercial = GetOrCreateMaterial("GlassCommercial", glassCommercial, true),
        GlassResidential = GetOrCreateMaterial("GlassResidential", glassResidential, true),
        GlassIndustrial = GetOrCreateMaterial("GlassIndustrial", glassIndustrial, true),

        // Wall Types
        WallResidential = GetOrCreateMaterial("WallResidential", wallResidential),
        WallIndustrial = GetOrCreateMaterial("WallIndustrial", wallIndustrial),

        // Misc
        ParkGrass = GetOrCreateMaterial("BuildPark", parkGreen)
      };
    }

    private Material GetOrCreateMaterial(string matName, Color color, bool transparent = false) {
#if UNITY_EDITOR
      string folderPath = "Assets/Materials/CityGen";
      string fullPath = $"{folderPath}/{matName}.mat";

      if (!AssetDatabase.IsValidFolder("Assets/Materials"))
        AssetDatabase.CreateFolder("Assets", "Materials");
      if (!AssetDatabase.IsValidFolder(folderPath))
        AssetDatabase.CreateFolder("Assets/Materials", "CityGen");

      Material existingMat = AssetDatabase.LoadAssetAtPath<Material>(fullPath);
      if (existingMat != null) {
        existingMat.color = color;
        existingMat.SetColor("_BaseColor", color);
        return existingMat;
      }

      Shader shader = Shader.Find("Universal Render Pipeline/Lit");
      if (shader == null)
        shader = Shader.Find("Standard");

      Material newMat = new Material(shader);
      newMat.SetColor("_BaseColor", color);
      if (transparent) {
        newMat.SetFloat("_Surface", 1);
        newMat.SetInt("_SrcBlend", (int) UnityEngine.Rendering.BlendMode.SrcAlpha);
        newMat.SetInt("_DstBlend", (int) UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        newMat.SetInt("_ZWrite", 0);
        newMat.renderQueue = 3000;
        newMat.SetColor("_BaseColor", new Color(color.r, color.g, color.b, 0.6f));
      }

      AssetDatabase.CreateAsset(newMat, fullPath);
      AssetDatabase.SaveAssets();
      return newMat;
#else
            return new Material(Shader.Find("Standard"));
#endif
    }
  }
}
