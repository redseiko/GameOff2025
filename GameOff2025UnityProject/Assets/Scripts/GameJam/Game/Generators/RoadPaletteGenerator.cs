using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GameJam {
  public sealed class RoadPaletteGenerator : MonoBehaviour {
    [Header("Settings")]
    public float tileSize = 10.0f;
    public float roadWidth = 6.0f;
    public float curbHeight = 0.2f;

    [Header("Materials")]
    public Color asphaltColor = new Color(0.2f, 0.2f, 0.2f);
    public Color sidewalkColor = new Color(0.7f, 0.7f, 0.7f);

    // Public references to hold the generated objects
    [HideInInspector] public GameObject prefabStraight;
    [HideInInspector] public GameObject prefabCorner;
    [HideInInspector] public GameObject prefabT;
    [HideInInspector] public GameObject prefabCross;
    [HideInInspector] public GameObject prefabEnd;

    [ContextMenu("Generate Road Palette")]
    public void GeneratePalette() {
      Cleanup();

      // 1. Create Persistent Materials
      Material matAsphalt = GetOrCreateMaterial("RoadAsphalt", asphaltColor);
      Material matSidewalk = GetOrCreateMaterial("RoadSidewalk", sidewalkColor);

      // 2. Generate Meshes
      // Straight (Road N-S, Sidewalks E-W)
      prefabStraight = BuildTile(this.gameObject, "RoadStraight", matAsphalt, matSidewalk, true, true, false, false);

      // Corner (Road N-E, Sidewalk inner/outer)
      prefabCorner = BuildTile(this.gameObject, "RoadCorner", matAsphalt, matSidewalk, true, false, true, false);

      // T-Junction (Road N-E-W)
      prefabT = BuildTile(this.gameObject, "Road3Way", matAsphalt, matSidewalk, true, false, true, true);

      // Cross (Road N-S-E-W)
      prefabCross = BuildTile(this.gameObject, "Road4Way", matAsphalt, matSidewalk, true, true, true, true);

      // Dead End (Road N only)
      prefabEnd = BuildTile(this.gameObject, "RoadEnd", matAsphalt, matSidewalk, true, false, false, false);
    }

    private void Cleanup() {
      while (transform.childCount > 0) {
        DestroyImmediate(transform.GetChild(0).gameObject);
      }
    }

    private GameObject BuildTile(GameObject parent, string name, Material matRoad, Material matWalk, bool n, bool s, bool e, bool w) {
      GameObject tile = new GameObject(name);
      tile.transform.parent = parent.transform;
      tile.transform.localPosition = Vector3.zero;

      // Center Hub
      CreateBox(tile, "RoadCenter", new Vector3(roadWidth, 0.1f, roadWidth), Vector3.zero, matRoad);

      float spokeLen = (tileSize - roadWidth) / 2;
      float spokePos = (roadWidth / 2) + (spokeLen / 2);

      if (n)
        CreateBox(tile, "RoadNorth", new Vector3(roadWidth, 0.1f, spokeLen), new Vector3(0, 0, spokePos), matRoad);
      if (s)
        CreateBox(tile, "RoadSouth", new Vector3(roadWidth, 0.1f, spokeLen), new Vector3(0, 0, -spokePos), matRoad);
      if (e)
        CreateBox(tile, "RoadEast", new Vector3(spokeLen, 0.1f, roadWidth), new Vector3(spokePos, 0, 0), matRoad);
      if (w)
        CreateBox(tile, "RoadWest", new Vector3(spokeLen, 0.1f, roadWidth), new Vector3(-spokePos, 0, 0), matRoad);

      float cornerSize = (tileSize - roadWidth) / 2;
      float cPos = (roadWidth / 2) + (cornerSize / 2);

      if (!n && !e)
        CreateBox(tile, "SidewalkNorthEast", new Vector3(cornerSize, curbHeight, cornerSize), new Vector3(cPos, 0.05f, cPos), matWalk);
      if (!s && !e)
        CreateBox(tile, "SidewalkSouthEast", new Vector3(cornerSize, curbHeight, cornerSize), new Vector3(cPos, 0.05f, -cPos), matWalk);
      if (!s && !w)
        CreateBox(tile, "SidewalkSouthWest", new Vector3(cornerSize, curbHeight, cornerSize), new Vector3(-cPos, 0.05f, -cPos), matWalk);
      if (!n && !w)
        CreateBox(tile, "SidewalkNorthWest", new Vector3(cornerSize, curbHeight, cornerSize), new Vector3(-cPos, 0.05f, cPos), matWalk);

      return tile;
    }

    private void CreateBox(GameObject p, string n, Vector3 s, Vector3 pos, Material m) {
      GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
      obj.name = n;
      obj.transform.parent = p.transform;
      obj.transform.localScale = s;
      obj.transform.localPosition = pos;
      if (obj.GetComponent<Renderer>())
        obj.GetComponent<Renderer>().sharedMaterial = m;
    }

    private Material GetOrCreateMaterial(string matName, Color color) {
#if UNITY_EDITOR
      string folderPath = "Assets/Materials/Roads";
      string fullPath = $"{folderPath}/{matName}.mat";

      if (!AssetDatabase.IsValidFolder("Assets/Materials"))
        AssetDatabase.CreateFolder("Assets", "Materials");
      if (!AssetDatabase.IsValidFolder(folderPath))
        AssetDatabase.CreateFolder("Assets/Materials", "Roads");

      Material existingMat = AssetDatabase.LoadAssetAtPath<Material>(fullPath);
      if (existingMat != null)
        return existingMat;

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
