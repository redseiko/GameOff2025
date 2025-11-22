using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GameJam {
  // --- DATA ---
  public enum CityZone { Empty, Commercial, Residential, Industrial, Park }
  public enum CellType { Empty, Road, Building }

  [System.Serializable]
  public struct CityConfig {
    [Header("Grid Dimensions")]
    public int cityWidth;
    public int cityLength;

    [Header("Block Rules")]
    public int minBlockSize;
    public int maxBlockSize;
    public int mainRoadWidth;

    [Header("Lot Rules")]
    public int minLotSize;
    public float maxLotAspectRatio;
  }

  [System.Serializable]
  public struct ZoningWeights {
    public bool useDistricts;
    [Range(0, 1)] public float commercial;
    [Range(0, 1)] public float residential;
    [Range(0, 1)] public float industrial;
    [Range(0, 1)] public float park;
  }

  // --- BUILDING GENERATOR ---
  public class ProceduralBuildingGenerator {
    private readonly Dictionary<string, Material> mats;

    public ProceduralBuildingGenerator(Dictionary<string, Material> materials) {
      this.mats = materials;
    }

    public void Generate(GameObject root, Vector2 size, CityZone zone) {
      // 1. Build Foundation (Concrete Slab)
      // FIX: Added padding so it doesn't clip into sidewalks
      if (zone != CityZone.Park) {
        CreateBox(root, "FoundationSlab",
            new Vector3(size.x - 0.5f, 0.2f, size.y - 0.5f),
            new Vector3(0, 0.1f, 0),
            mats["Concrete"]);
      }

      // 2. Build Structure
      switch (zone) {
        case CityZone.Commercial:
          BuildCommercial(root, size);
          break;
        case CityZone.Residential:
          BuildResidential(root, size);
          break;
        case CityZone.Industrial:
          BuildIndustrial(root, size);
          break;
        case CityZone.Park:
          BuildPark(root, size);
          break;
      }
    }

    private void BuildCommercial(GameObject root, Vector2 size) {
      // Style: Tall, Blue Glass, Vertical Fins
      float height = Random.Range(30f, 80f);
      float w = size.x - 2f;
      float l = size.y - 2f;

      CreateBox(root, "Core", new Vector3(w, height, l), new Vector3(0, height / 2, 0), mats["Concrete"]);
      CreateBox(root, "GlassMain", new Vector3(w + 0.2f, height - 2f, l + 0.2f), new Vector3(0, height / 2, 0), mats["GlassComm"]);

      float finThick = 1.0f;
      CreateBox(root, "PillarFL", new Vector3(finThick, height, finThick), new Vector3(-w / 2, height / 2, -l / 2), mats["Concrete"]);
      CreateBox(root, "PillarFR", new Vector3(finThick, height, finThick), new Vector3(w / 2, height / 2, -l / 2), mats["Concrete"]);
      CreateBox(root, "PillarBL", new Vector3(finThick, height, finThick), new Vector3(-w / 2, height / 2, l / 2), mats["Concrete"]);
      CreateBox(root, "PillarBR", new Vector3(finThick, height, finThick), new Vector3(w / 2, height / 2, l / 2), mats["Concrete"]);
    }

    private void BuildResidential(GameObject root, Vector2 size) {
      // Style: Medium, Warm Colors, Balconies/Bands
      float height = Random.Range(12f, 30f);
      float w = size.x - 3f;
      float l = size.y - 3f;
      float floorH = 3.0f;
      int floors = Mathf.FloorToInt(height / floorH);

      for (int i = 0; i < floors; i++) {
        float y = (i * floorH);
        // Floor Band
        CreateBox(root, $"Floor_{i}", new Vector3(w, 0.4f, l), new Vector3(0, y + 0.2f, 0), mats["WallRes"]);
        // Window Block
        CreateBox(root, $"Room_{i}", new Vector3(w - 0.5f, floorH - 0.4f, l - 0.5f), new Vector3(0, y + (floorH / 2) + 0.2f, 0), mats["GlassRes"]);

        // Simple Balcony Stub (Random side)
        if (Random.value > 0.5f) {
          CreateBox(root, $"Balcony_{i}", new Vector3(w + 1f, 1f, 2f), new Vector3(0, y + 1f, 0), mats["GlassRes"]);
        }
      }
      CreateBox(root, "Roof", new Vector3(w, 0.5f, l), new Vector3(0, (floors * floorH) + 0.25f, 0), mats["Concrete"]);
    }

    private void BuildIndustrial(GameObject root, Vector2 size) {
      // Style: Low, Grey/Dark, Sawtooth Roof
      float height = Random.Range(8f, 12f);
      float w = size.x - 1.5f;
      float l = size.y - 1.5f;

      // Main Block
      CreateBox(root, "FactoryWall", new Vector3(w, height, l), new Vector3(0, height / 2, 0), mats["WallInd"]);

      // Roof Details (Sawtooth or Vents)
      int segments = Mathf.FloorToInt(l / 5f);
      float segmentLen = l / segments;

      for (int i = 0; i < segments; i++) {
        float zPos = -l / 2 + (i * segmentLen) + (segmentLen / 2);
        // Triangle approx
        CreateBox(root, "RoofVent", new Vector3(w, 1.5f, segmentLen * 0.8f), new Vector3(0, height + 0.75f, zPos), mats["WallInd"]);
        // Skylight on side of vent
        CreateBox(root, "Skylight", new Vector3(w - 1f, 0.5f, segmentLen * 0.8f), new Vector3(0, height + 1.5f, zPos - 0.5f), mats["GlassInd"]);
      }
    }

    private void BuildPark(GameObject root, Vector2 size) {
      CreateBox(root, "Grass", new Vector3(size.x - 1f, 0.2f, size.y - 1f), new Vector3(0, 0.1f, 0), mats["Park"]);
      CreateBox(root, "Feature", new Vector3(3f, 2f, 3f), new Vector3(0, 1f, 0), mats["Concrete"]);
    }

    private void CreateBox(GameObject parent, string name, Vector3 scale, Vector3 pos, Material mat) {
      GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
      obj.name = name;
      obj.transform.parent = parent.transform;
      obj.transform.localScale = scale;
      obj.transform.localPosition = pos;
      if (obj.GetComponent<Renderer>())
        obj.GetComponent<Renderer>().sharedMaterial = mat;
    }
  }

  // --- MAIN GENERATOR ---
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

    // Re-added these to fix compile errors
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

    private CellType[,] grid;
    private readonly List<RectInt> cityBlocks = new List<RectInt>();
    private Dictionary<string, Material> generatedMaterials;

    [ContextMenu("Generate City")]
    public void GenerateCity() {
      Cleanup();
      InitializeGrid();
      InitializeMaterials();

      RectInt fullCity = new RectInt(0, 0, config.cityWidth, config.cityLength);
      SplitBlockForRoads(fullCity);

      BuildOptimizedRoads();
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
      for (int x = area.x; x < area.xMax; x++)
        for (int y = area.y; y < area.yMax; y++)
          if (grid[x, y] != CellType.Road)
            grid[x, y] = CellType.Building;
      cityBlocks.Add(area);
    }

    // --- ROADS (SMART SIDEWALKS) ---
    private void BuildOptimizedRoads() {
      GameObject roadRoot = new GameObject("RoadNetwork");
      roadRoot.transform.parent = this.transform;
      roadRoot.transform.localPosition = Vector3.zero;
      Undo.RegisterCreatedObjectUndo(roadRoot, "Create Roads");

      bool[,] processed = new bool[config.cityWidth, config.cityLength];

      for (int z = 0; z < config.cityLength; z++) {
        for (int x = 0; x < config.cityWidth; x++) {
          if (grid[x, z] == CellType.Road && !processed[x, z]) {
            // Greedy Mesh Expansion
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
      // 1. Asphalt (One big slab)
      float w = area.width * cellSize;
      float l = area.height * cellSize;
      float centerX = (area.x * cellSize) + (w / 2f) - (cellSize / 2);
      float centerZ = (area.y * cellSize) + (l / 2f) - (cellSize / 2);

      GameObject segment = new GameObject($"Road_{area.x}_{area.y}");
      segment.transform.parent = parent.transform;
      segment.transform.localPosition = new Vector3(centerX, 0, centerZ);

      CreateBox(segment, "Asphalt", new Vector3(w, 0.1f, l), Vector3.zero, generatedMaterials["Asphalt"]);

      // 2. Sidewalks (Scan perimeter for connections)
      // We scan the cells along the edge. If neighbor is NOT road, we add sidewalk length.
      // If neighbor IS road, we break the sidewalk mesh to create an opening.

      float swWidth = 2.0f;
      float swHeight = 0.2f;
      float halfW = w / 2;
      float halfL = l / 2;

      // Left Edge
      if (area.x > 0)
        GenerateEdgeSidewalks(segment, area.y, area.height, true,
            (offset) => grid[area.x - 1, offset] != CellType.Road,
            (centerOffset, length) => new Vector3(-halfW + swWidth / 2, 0.05f, -halfL + centerOffset),
            (length) => new Vector3(swWidth, swHeight, length));

      // Right Edge
      if (area.x + area.width < config.cityWidth)
        GenerateEdgeSidewalks(segment, area.y, area.height, true,
            (offset) => grid[area.x + area.width, offset] != CellType.Road,
            (centerOffset, length) => new Vector3(halfW - swWidth / 2, 0.05f, -halfL + centerOffset),
            (length) => new Vector3(swWidth, swHeight, length));

      // Bottom Edge
      if (area.y > 0)
        GenerateEdgeSidewalks(segment, area.x, area.width, false,
            (offset) => grid[offset, area.y - 1] != CellType.Road,
            (centerOffset, length) => new Vector3(-halfW + centerOffset, 0.05f, -halfL + swWidth / 2),
            (length) => new Vector3(length, swHeight, swWidth));

      // Top Edge
      if (area.y + area.height < config.cityLength)
        GenerateEdgeSidewalks(segment, area.x, area.width, false,
            (offset) => grid[offset, area.y + area.height] != CellType.Road,
            (centerOffset, length) => new Vector3(-halfW + centerOffset, 0.05f, halfL - swWidth / 2),
            (length) => new Vector3(length, swHeight, swWidth));
    }

    // Helper to generate broken sidewalk strips
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
            // End of segment, build it
            SpawnSidewalkPiece(parent, startOffset, currentLen, getPos, getScale);
            building = false;
            currentLen = 0;
          }
        }
      }
      // Flush last segment
      if (building)
        SpawnSidewalkPiece(parent, startOffset, currentLen, getPos, getScale);
    }

    private void SpawnSidewalkPiece(GameObject parent, float startOffset, float length,
                                    System.Func<float, float, Vector3> getPos,
                                    System.Func<float, Vector3> getScale) {
      // Logic to center the piece relative to the startOffset + length
      float centerPos = startOffset + (length / 2f) + (cellSize / 2f) - (cellSize / 2f); // (cellSize/2) adj because offset 0 is corner
                                                                                         // Wait, offset 0 in loop means distance 0 from corner.
                                                                                         // Center of first cell (k=0) is at distance 5.
      //centerPos = startOffset + (length / 2f);

      CreateBox(parent, "Sidewalk", getScale(length), getPos(centerPos, length), generatedMaterials["Sidewalk"]);
    }

    // --- PLOTS ---
    private void BuildPlots() {
      GameObject plotRoot = new GameObject("BuildingNetwork");
      plotRoot.transform.parent = this.transform;
      plotRoot.transform.localPosition = Vector3.zero;
      Undo.RegisterCreatedObjectUndo(plotRoot, "Create Buildings");

      ProceduralBuildingGenerator builder = new ProceduralBuildingGenerator(generatedMaterials);

      foreach (RectInt block in cityBlocks) {
        CityZone blockZone = DetermineZone(block);
        List<RectInt> lots = new List<RectInt>();
        SubdivideBlockIntoLots(block, lots);

        foreach (RectInt lot in lots) {
          float w = lot.width * cellSize;
          float l = lot.height * cellSize;
          float x = (lot.x * cellSize) + (w / 2) - (cellSize / 2);
          float z = (lot.y * cellSize) + (l / 2) - (cellSize / 2);

          GameObject pObj = new GameObject($"Lot_X{lot.x}Y{lot.y}");
          pObj.transform.parent = plotRoot.transform;
          pObj.transform.localPosition = new Vector3(x, 0, z);

          builder.Generate(pObj, new Vector2(w, l), blockZone);
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
      generatedMaterials = new Dictionary<string, Material> {
        // Infrastructure
        ["Asphalt"] = GetOrCreateMaterial("RoadAsphalt", roadColor),
        ["Sidewalk"] = GetOrCreateMaterial("RoadSidewalk", sidewalkColor),

        // Base Materials
        ["Concrete"] = GetOrCreateMaterial("BuildConcrete", buildingConcrete), // Fixed variable name
        ["Brick"] = GetOrCreateMaterial("BuildBrick", buildingBrick),

        // Specific Zones
        ["GlassComm"] = GetOrCreateMaterial("GlassCommercial", glassCommercial, true),
        ["GlassRes"] = GetOrCreateMaterial("GlassResidential", glassResidential, true),
        ["GlassInd"] = GetOrCreateMaterial("GlassIndustrial", glassIndustrial, true),

        ["WallRes"] = GetOrCreateMaterial("WallResidential", wallResidential),
        ["WallInd"] = GetOrCreateMaterial("WallIndustrial", wallIndustrial),

        // Fallbacks / Legacy
        ["Glass"] = GetOrCreateMaterial("BuildGlass", buildingGlass, true),
        ["Industrial"] = GetOrCreateMaterial("BuildIndustrial", buildingIndustrial),
        ["Park"] = GetOrCreateMaterial("BuildPark", parkGreen)
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
