using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GameJam {
  // --- DATA STRUCTURES ---
  public enum CityZone { Empty, Commercial, Residential, Industrial, Park }
  public enum CellType { Empty, Road, Building }

  [System.Serializable]
  public struct CityConfig {
    [Header("Grid Dimensions")]
    public int cityWidth;
    public int cityLength;

    [Header("Block Shape Rules")]
    [Tooltip("Minimum size of a block (cells). Stops recursion.")]
    public int minBlockSize;
    [Tooltip("Maximum size of a block (cells). Forces recursion.")]
    public int maxBlockSize;

    [Header("Lot Shape Rules")]
    public int minLotSize;
    public float maxLotAspectRatio;

    [Header("Road Settings")]
    [Tooltip("Width of the roads in grid cells.")]
    public int mainRoadWidth;
  }

  [System.Serializable]
  public struct ZoningWeights {
    public bool useDistricts;
    [Range(0, 1)] public float commercial;
    [Range(0, 1)] public float residential;
    [Range(0, 1)] public float industrial;
    [Range(0, 1)] public float park;
  }

  public interface IPlotGenerator {
    void GeneratePlot(GameObject plotRoot, Vector2 size, CityZone zone, Dictionary<CityZone, Material> materials);
  }

  // --- STUB GENERATOR ---
  public class StubPlotGenerator : IPlotGenerator {
    public void GeneratePlot(GameObject plotRoot, Vector2 size, CityZone zone, Dictionary<CityZone, Material> materials) {
      GameObject stub = GameObject.CreatePrimitive(PrimitiveType.Cube);
      stub.name = $"Stub{zone}";
      stub.transform.parent = plotRoot.transform;

      float height = 0;
      switch (zone) {
        case CityZone.Commercial:
          height = Random.Range(20f, 80f);
          break;
        case CityZone.Residential:
          height = Random.Range(8f, 25f);
          break;
        case CityZone.Industrial:
          height = Random.Range(6f, 15f);
          break;
        case CityZone.Park:
          height = 0.2f;
          break;
      }

      if (zone != CityZone.Park)
        height *= Random.Range(0.8f, 1.2f);

      float padding = 1.0f;
      float w = Mathf.Max(1, size.x - padding);
      float l = Mathf.Max(1, size.y - padding);

      stub.transform.localScale = new Vector3(w, height, l);
      stub.transform.localPosition = new Vector3(0, height / 2, 0);

      if (materials.ContainsKey(zone)) {
        var rend = stub.GetComponent<Renderer>();
        if (rend)
          rend.sharedMaterial = materials[zone];
      }
    }
  }

  // --- MAIN CLASS ---
  public class CityLevelGenerator : MonoBehaviour {
    [Header("Grid Configuration")]
    public CityConfig config = new CityConfig {
      cityWidth = 60,
      cityLength = 60,
      minBlockSize = 8,
      maxBlockSize = 16,
      minLotSize = 2,
      maxLotAspectRatio = 2.0f,
      mainRoadWidth = 2 // Defaulting to 2 to show the effect
    };

    public float cellSize = 10.0f;

    [Header("Zoning Configuration")]
    public ZoningWeights zoning = new ZoningWeights {
      useDistricts = true,
      commercial = 0.2f,
      residential = 0.6f,
      industrial = 0.1f,
      park = 0.1f
    };

    [Header("Road Palette")]
    public GameObject roadStraight;
    public GameObject roadCorner;
    public GameObject road3Way;
    public GameObject road4Way;
    public GameObject roadEnd;

    [Header("Stub Colors")]
    public Color colorCommercial = Color.blue;
    public Color colorResidential = Color.green;
    public Color colorIndustrial = Color.gray;
    public Color colorPark = Color.yellow;

    private CellType[,] grid;
    private List<RectInt> cityBlocks = new List<RectInt>();
    private Dictionary<CityZone, Material> stubMaterials;

    [ContextMenu("Generate City")]
    public void GenerateCity() {
      Cleanup();
      InitializeGrid();
      InitializeMaterials();

      RectInt fullCity = new RectInt(0, 0, config.cityWidth, config.cityLength);
      SplitBlockForRoads(fullCity);

      BuildRoadNetwork();
      BuildPlots();
    }

    private void Cleanup() {
      Transform roadNet = transform.Find("RoadNetwork");
      Transform buildNet = transform.Find("BuildingNetwork");

      if (roadNet != null)
        Undo.DestroyObjectImmediate(roadNet.gameObject);
      if (buildNet != null)
        Undo.DestroyObjectImmediate(buildNet.gameObject);

      while (transform.childCount > 0) {
        DestroyImmediate(transform.GetChild(0).gameObject);
      }
    }

    private void InitializeGrid() {
      grid = new CellType[config.cityWidth, config.cityLength];
      cityBlocks.Clear();
    }

    // --- STEP 1: ROAD LAYOUT ---
    private void SplitBlockForRoads(RectInt area) {
      int roadW = Mathf.Max(1, config.mainRoadWidth); // Ensure at least 1

      // Can we split? We need room for LeftBlock + Road + RightBlock
      // Left/Right blocks must obey minBlockSize
      int minSpaceNeeded = (config.minBlockSize * 2) + roadW;

      bool forceSplit = false;
      bool splitHorizontal = false;

      // Rule 1: Too Big? Force split.
      if (area.width > config.maxBlockSize) {
        forceSplit = true;
        splitHorizontal = true;
      } else if (area.height > config.maxBlockSize) {
        forceSplit = true;
        splitHorizontal = false;
      }

      // Rule 2: Stop if too small (unless forced)
      if (!forceSplit) {
        if (area.width < minSpaceNeeded || area.height < minSpaceNeeded) {
          RegisterCityBlock(area);
          return;
        }
        if (Random.value > 0.7f) {
          RegisterCityBlock(area);
          return;
        }
      }

      // Rule 3: Determine Split Direction
      if (!forceSplit) {
        splitHorizontal = area.width > area.height;
        if (area.width > minSpaceNeeded && area.height > minSpaceNeeded) {
          if (Random.value > 0.5f)
            splitHorizontal = !splitHorizontal;
        }
      }

      // Rule 4: Check if split is physically possible given dimensions
      // If we are forced to split but dimensions are too small, we abort to avoid infinite loop/errors
      if (splitHorizontal && area.width < minSpaceNeeded) { RegisterCityBlock(area); return; }
      if (!splitHorizontal && area.height < minSpaceNeeded) { RegisterCityBlock(area); return; }

      // Execute Split
      if (splitHorizontal) {
        // Vertical Cut (Splitting the Width)
        int minSplit = config.minBlockSize;
        int maxSplit = area.width - config.minBlockSize - roadW; // Ensure room for road + right block

        int splitX;
        if (forceSplit)
          splitX = (area.width - roadW) / 2; // Center it
        else
          splitX = Random.Range(minSplit, maxSplit);

        // Mark Road Cells
        for (int w = 0; w < roadW; w++) {
          for (int y = area.y; y < area.yMax; y++) {
            grid[area.x + splitX + w, y] = CellType.Road;
          }
        }

        // Recurse (Jump over the road width)
        SplitBlockForRoads(new RectInt(area.x, area.y, splitX, area.height));
        SplitBlockForRoads(new RectInt(area.x + splitX + roadW, area.y, area.width - splitX - roadW, area.height));
      } else {
        // Horizontal Cut (Splitting the Height)
        int minSplit = config.minBlockSize;
        int maxSplit = area.height - config.minBlockSize - roadW;

        int splitY;
        if (forceSplit)
          splitY = (area.height - roadW) / 2;
        else
          splitY = Random.Range(minSplit, maxSplit);

        // Mark Road Cells
        for (int w = 0; w < roadW; w++) {
          for (int x = area.x; x < area.xMax; x++) {
            grid[x, area.y + splitY + w] = CellType.Road;
          }
        }

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

    // --- STEP 2: ROADS ---
    private void BuildRoadNetwork() {
      GameObject roadRoot = new GameObject("RoadNetwork");
      roadRoot.transform.parent = this.transform;
      roadRoot.transform.localPosition = Vector3.zero;
      Undo.RegisterCreatedObjectUndo(roadRoot, "Create Roads");

      for (int x = 0; x < config.cityWidth; x++) {
        for (int y = 0; y < config.cityLength; y++) {
          if (grid[x, y] != CellType.Road)
            continue;
          bool n = IsRoad(x, y + 1);
          bool s = IsRoad(x, y - 1);
          bool e = IsRoad(x + 1, y);
          bool w = IsRoad(x - 1, y);
          SpawnRoadTile(roadRoot, x, y, n, s, e, w);
        }
      }
    }

    private bool IsRoad(int x, int y) {
      if (x < 0 || x >= config.cityWidth || y < 0 || y >= config.cityLength)
        return false;
      return grid[x, y] == CellType.Road;
    }

    private void SpawnRoadTile(GameObject parent, int x, int y, bool n, bool s, bool e, bool w) {
      GameObject prefab = null;
      float rotation = 0;
      int connections = (n ? 1 : 0) + (s ? 1 : 0) + (e ? 1 : 0) + (w ? 1 : 0);

      if (connections == 4)
        prefab = road4Way;
      else if (connections == 3) {
        prefab = road3Way;
        if (!n)
          rotation = 90;
        else if (!e)
          rotation = 180;
        else if (!s)
          rotation = -90;
        else
          rotation = 0;
      } else if (connections == 2) {
        if (n && s) { prefab = roadStraight; rotation = 0; } else if (e && w) { prefab = roadStraight; rotation = 90; } else {
          prefab = roadCorner;
          if (n && e)
            rotation = 0;
          else if (e && s)
            rotation = 90;
          else if (s && w)
            rotation = 180;
          else if (w && n)
            rotation = -90;
        }
      } else if (connections == 1) {
        prefab = roadEnd;
        if (n)
          rotation = 0;
        else if (e)
          rotation = 90;
        else if (s)
          rotation = 180;
        else
          rotation = -90;
      } else
        prefab = roadStraight;

      if (prefab != null) {
        GameObject r = Instantiate(prefab, parent.transform);
        r.name = $"RoadX{x}Y{y}";
        r.transform.localPosition = new Vector3(x * cellSize, 0, y * cellSize);
        r.transform.localRotation = Quaternion.Euler(0, rotation, 0);
      }
    }

    // --- STEP 3: BUILD PLOTS ---
    private void BuildPlots() {
      GameObject plotRoot = new GameObject("BuildingNetwork");
      plotRoot.transform.parent = this.transform;
      plotRoot.transform.localPosition = Vector3.zero;
      Undo.RegisterCreatedObjectUndo(plotRoot, "Create Buildings");

      IPlotGenerator stubGenerator = new StubPlotGenerator();

      foreach (var block in cityBlocks) {
        CityZone blockZone = DetermineZone(block);
        List<RectInt> lots = new List<RectInt>();
        SubdivideBlockIntoLots(block, lots);

        foreach (var lot in lots) {
          float widthMeters = lot.width * cellSize;
          float lengthMeters = lot.height * cellSize;
          float xPos = (lot.x * cellSize) + (widthMeters / 2f) - (cellSize / 2);
          float zPos = (lot.y * cellSize) + (lengthMeters / 2f) - (cellSize / 2);

          GameObject pObj = new GameObject($"LotX{lot.x}Y{lot.y}{blockZone}");
          pObj.transform.parent = plotRoot.transform;
          pObj.transform.localPosition = new Vector3(xPos, 0, zPos);

          stubGenerator.GeneratePlot(pObj, new Vector2(widthMeters, lengthMeters), blockZone, stubMaterials);
        }
      }
    }

    private CityZone DetermineZone(RectInt block) {
      if (zoning.useDistricts) {
        float dist = Vector2.Distance(block.center, new Vector2(config.cityWidth / 2, config.cityLength / 2));
        if (dist < config.cityWidth * 0.2f)
          return CityZone.Commercial;
        else if (dist > config.cityWidth * 0.4f && Random.value > 0.7f)
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
      // Force Split Conditions
      bool aspectTooLong = (float) area.width / area.height > config.maxLotAspectRatio;
      bool aspectTooTall = (float) area.height / area.width > config.maxLotAspectRatio;
      bool tooBig = area.width > config.minLotSize * 3 || area.height > config.minLotSize * 3;

      bool shouldSplit = aspectTooLong || aspectTooTall || (tooBig && Random.value > 0.3f);

      // Stop condition
      if (!shouldSplit) {
        result.Add(area);
        return;
      }

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

    // --- MATERIALS ---
    private void InitializeMaterials() {
      stubMaterials = new Dictionary<CityZone, Material>();
      stubMaterials[CityZone.Commercial] = GetOrCreateMaterial("StubCommercial", colorCommercial);
      stubMaterials[CityZone.Residential] = GetOrCreateMaterial("StubResidential", colorResidential);
      stubMaterials[CityZone.Industrial] = GetOrCreateMaterial("StubIndustrial", colorIndustrial);
      stubMaterials[CityZone.Park] = GetOrCreateMaterial("StubPark", colorPark);
    }

    private Material GetOrCreateMaterial(string matName, Color color) {
#if UNITY_EDITOR
      string folderPath = "Assets/Materials/CityStubs";
      string fullPath = $"{folderPath}/{matName}.mat";

      if (!AssetDatabase.IsValidFolder("Assets/Materials"))
        AssetDatabase.CreateFolder("Assets", "Materials");
      if (!AssetDatabase.IsValidFolder(folderPath))
        AssetDatabase.CreateFolder("Assets/Materials", "CityStubs");

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

      AssetDatabase.CreateAsset(newMat, fullPath);
      AssetDatabase.SaveAssets();
      return newMat;
#else
            return new Material(Shader.Find("Universal Render Pipeline/Lit"));
#endif
    }
  }
}
