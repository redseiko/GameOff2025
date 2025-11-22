using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class WarehouseGenerator : MonoBehaviour {
  [Header("Warehouse Dimensions")]
  public float width = 80f;
  public float depth = 50f;
  public float height = 15f;
  public float dockHeight = 1.5f;
  public float apronDepth = 8f;

  [Header("Office Settings")]
  public float officeWidth = 25f; // Wider for more rooms
  public float officeDepth = 15f;
  public float officeHeight = 4f;

  [Header("Colors & Materials")]
  public Color concreteColor = new Color(0.7f, 0.7f, 0.7f);
  public Color wallColor = new Color(0.9f, 0.9f, 0.9f);
  public Color roofColor = new Color(0.1f, 0.2f, 0.4f);     // PSA Blue
  public Color shutterColor = new Color(0.25f, 0.3f, 0.35f);
  public Color safetyYellow = new Color(1f, 0.8f, 0f);      // Safety Bollards

  [Header("Office Interior")]
  public Color officeFloorColor = new Color(0.25f, 0.25f, 0.35f); // Carpet
  public Color officeWallColor = new Color(0.92f, 0.92f, 0.85f);  // Cream
  public Color furnitureColor = new Color(0.5f, 0.5f, 0.5f);      // Metal Cabinets
  public Color woodColor = new Color(0.55f, 0.4f, 0.25f);         // Desks

  [Header("Configuration")]
  public int numberOfBays = 8;
  [Range(0f, 1f)] public float shutterOpenAmount = 0.8f; // 0 = Closed, 1 = Fully Open

  // Material References
  private Material matConcrete, matWall, matRoof, matShutter, matSafety;
  private Material matOfficeFloor, matOfficeWall, matMetal, matWood;

  void Start() {
    BuildWarehouse();
  }

  [ContextMenu("Build Warehouse")]
  public void BuildWarehouse() {
    // 1. Cleanup
    while (transform.childCount > 0)
      DestroyImmediate(transform.GetChild(0).gameObject);

    // 2. Setup Materials
    InitializeMaterials();

    // 3. Build Structure
    BuildFoundation();
    BuildRoof();
    BuildBackWall();
    BuildFrontFacade(); // Loading Bays
    BuildSideWalls();   // Includes Employee Entrance

    // 4. Build Office
    BuildOfficeStructure();
  }

  private void InitializeMaterials() {
    matConcrete = GetOrCreateMaterial("Wh_Concrete", concreteColor);
    matWall = GetOrCreateMaterial("Wh_Wall", wallColor);
    matRoof = GetOrCreateMaterial("Wh_Roof", roofColor);
    matShutter = GetOrCreateMaterial("Wh_Shutter", shutterColor);
    matSafety = GetOrCreateMaterial("Wh_Safety", safetyYellow);

    matOfficeFloor = GetOrCreateMaterial("Office_Carpet", officeFloorColor);
    matOfficeWall = GetOrCreateMaterial("Office_Wall", officeWallColor);
    matMetal = GetOrCreateMaterial("Office_Metal", furnitureColor);
    matWood = GetOrCreateMaterial("Office_Wood", woodColor);
  }

  // -----------------------
  // EXTERIOR STRUCTURE
  // -----------------------

  private void BuildFoundation() {
    // Main Slab
    CreatePrimitive(PrimitiveType.Cube, "Foundation",
        new Vector3(width, dockHeight, depth + apronDepth),
        new Vector3(0, dockHeight / 2, (apronDepth / 2) - (apronDepth / 2)),
        matConcrete
    );
  }

  private void BuildRoof() {
    float roofDepth = depth + apronDepth + 3f;
    CreatePrimitive(PrimitiveType.Cube, "Roof",
        new Vector3(width + 2f, 0.5f, roofDepth),
        new Vector3(0, dockHeight + height + 0.25f, 0),
        matRoof
    );
  }

  private void BuildBackWall() {
    CreatePrimitive(PrimitiveType.Cube, "BackWall",
        new Vector3(width, height, 1),
        new Vector3(0, dockHeight + (height / 2), (depth / 2) - 0.5f),
        matWall
    );
  }

  private void BuildSideWalls() {
    // Left Wall (Solid)
    CreatePrimitive(PrimitiveType.Cube, "WallLeft",
        new Vector3(1, height, depth),
        new Vector3(-width / 2 + 0.5f, dockHeight + (height / 2), 0),
        matWall
    );

    // Right Wall (Contains Employee Entrance)
    // We need to "cut" a hole for the door near the back (where the office is)
    // The office is at the back-right. Let's put the door at Z = 15 (relative to center)

    float doorWidth = 1.5f;
    float doorHeight = 2.4f;
    float doorZPos = (depth / 2) - 5f; // 5 meters from the back corner

    GameObject rightWallRoot = new GameObject("WallRight_WithDoor");
    rightWallRoot.transform.parent = transform;
    rightWallRoot.transform.localPosition = Vector3.zero;

    // 1. Wall Segment (Front to Door)
    // Calculate length from front of building (-depth/2) to the door
    float frontSegmentLen = (depth) - 5f - doorWidth; // Approximation for simplicity
    float frontZCenter = (-depth / 2) + (frontSegmentLen / 2);

    CreatePrimitiveChild(rightWallRoot, PrimitiveType.Cube, "Wall_R_Front",
        new Vector3(1, height, frontSegmentLen),
        new Vector3(width / 2 - 0.5f, dockHeight + height / 2, frontZCenter),
        matWall);

    // 2. Wall Segment (Door to Back)
    float backSegmentLen = 5f;
    float backZCenter = (depth / 2) - (backSegmentLen / 2);

    CreatePrimitiveChild(rightWallRoot, PrimitiveType.Cube, "Wall_R_Back",
        new Vector3(1, height, backSegmentLen),
        new Vector3(width / 2 - 0.5f, dockHeight + height / 2, backZCenter),
        matWall);

    // 3. Lintel (Above Door)
    float lintelHeight = height - doorHeight;
    CreatePrimitiveChild(rightWallRoot, PrimitiveType.Cube, "Wall_R_Lintel",
        new Vector3(1, lintelHeight, doorWidth),
        new Vector3(width / 2 - 0.5f, dockHeight + height - (lintelHeight / 2), doorZPos - (doorWidth / 2)),
        matWall);

    // 4. Concrete Step (Outside)
    CreatePrimitiveChild(rightWallRoot, PrimitiveType.Cube, "Entrance_Step",
        new Vector3(2.5f, dockHeight, 3f),
        new Vector3(width / 2 + 1.25f, dockHeight / 2, doorZPos - (doorWidth / 2)),
        matConcrete);

    // 5. Awning (Above Door)
    CreatePrimitiveChild(rightWallRoot, PrimitiveType.Cube, "Entrance_Awning",
        new Vector3(2.5f, 0.1f, 3f),
        new Vector3(width / 2 + 1.25f, dockHeight + doorHeight + 0.5f, doorZPos - (doorWidth / 2)),
        matShutter);
  }

  private void BuildFrontFacade() {
    // Lintel
    CreatePrimitive(PrimitiveType.Cube, "FrontLintel",
        new Vector3(width, height / 3, 1),
        new Vector3(0, dockHeight + height - (height / 6), -depth / 2 + 0.5f),
        matWall
    );

    // Pillars & Bollards
    for (int i = 0; i <= numberOfBays; i++) {
      float xPos = Mathf.Lerp(-width / 2 + 1, width / 2 - 1, (float) i / numberOfBays);

      // Main Pillar
      CreatePrimitive(PrimitiveType.Cube, $"Pillar_{i}",
          new Vector3(1, height * 0.66f, 1),
          new Vector3(xPos, dockHeight + (height * 0.33f), -depth / 2 + 0.5f),
          matWall
      );

      // Safety Bollard (Cylinder)
      // Placed slightly in front of the pillar
      GameObject bollard = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
      bollard.name = $"Bollard_{i}";
      bollard.transform.parent = transform;
      bollard.transform.localScale = new Vector3(0.4f, 0.6f, 0.4f); // Unity Cylinder height is 2, so 0.6 scale = 1.2m tall
      bollard.transform.localPosition = new Vector3(xPos, dockHeight + 0.6f, -depth / 2 - 1.0f);
      if (bollard.GetComponent<Renderer>())
        bollard.GetComponent<Renderer>().sharedMaterial = matSafety;
    }

    // Shutters (Opened)
    // Visualized as a block stuck high up in the frame
    for (int i = 0; i < numberOfBays; i++) {
      float t = ((float) i / numberOfBays) + (0.5f / numberOfBays);
      float xPos = Mathf.Lerp(-width / 2 + 1, width / 2 - 1, t);

      float fullShutterHeight = height * 0.66f;
      float currentHeight = Mathf.Lerp(fullShutterHeight, 1f, shutterOpenAmount); // If open, height is small
      float yOffset = dockHeight + fullShutterHeight - (currentHeight / 2); // Anchor to top

      CreatePrimitive(PrimitiveType.Cube, $"Shutter_{i}",
          new Vector3(width / numberOfBays - 1.5f, currentHeight, 0.2f),
          new Vector3(xPos, yOffset, -depth / 2 + 0.5f),
          matShutter
      );
    }
  }

  // -----------------------
  // OFFICE INTERIOR
  // -----------------------

  private void BuildOfficeStructure() {
    float offX = (width / 2) - (officeWidth / 2) - 0.5f;
    float offZ = (depth / 2) - (officeDepth / 2) - 0.5f;
    Vector3 officeCenter = new Vector3(offX, dockHeight, offZ);

    GameObject officeRoot = new GameObject("Office_Interior");
    officeRoot.transform.parent = this.transform;
    officeRoot.transform.localPosition = Vector3.zero;

    // Floor
    CreatePrimitiveChild(officeRoot, PrimitiveType.Cube, "Floor",
        new Vector3(officeWidth, 0.1f, officeDepth),
        new Vector3(officeCenter.x, dockHeight + 0.05f, officeCenter.z), matOfficeFloor);

    // Ceiling
    CreatePrimitiveChild(officeRoot, PrimitiveType.Cube, "Ceiling",
        new Vector3(officeWidth, 0.2f, officeDepth),
        new Vector3(officeCenter.x, dockHeight + officeHeight, officeCenter.z), matOfficeWall);

    // --- WALLS ---
    // 1. Front Wall (with door to Warehouse)
    CreateWallWithDoor(officeRoot, "Office_Front",
        new Vector3(officeWidth, officeHeight, 0.2f),
        new Vector3(officeCenter.x, dockHeight + officeHeight / 2, officeCenter.z - officeDepth / 2),
        2f, 4f, matOfficeWall);

    // 2. Left Wall (Solid)
    CreatePrimitiveChild(officeRoot, PrimitiveType.Cube, "Office_Left",
        new Vector3(0.2f, officeHeight, officeDepth),
        new Vector3(officeCenter.x - officeWidth / 2, dockHeight + officeHeight / 2, officeCenter.z), matOfficeWall);

    // 3. Interior Partition (Manager's Office)
    // Create a room in the back corner of the office
    float partitionX = officeCenter.x + (officeWidth / 4);
    CreatePrimitiveChild(officeRoot, PrimitiveType.Cube, "Partition_Wall",
        new Vector3(0.1f, officeHeight, officeDepth / 2),
        new Vector3(partitionX, dockHeight + officeHeight / 2, officeCenter.z + officeDepth / 4), matOfficeWall);

    // --- FURNITURE ---
    BuildHeavyFurniture(officeRoot, officeCenter);
  }

  private void BuildHeavyFurniture(GameObject parent, Vector3 center) {
    // 1. Filing Cabinets (Along the Left Wall)
    // Made of stacked cubes to look like drawers
    for (int i = 0; i < 4; i++) {
      float zPos = center.z - (officeDepth / 2) + 2f + (i * 1.5f);
      float xPos = center.x - (officeWidth / 2) + 0.8f;
      Vector3 cabPos = new Vector3(xPos, dockHeight, zPos);
      CreateFilingCabinet(parent, cabPos);
    }

    // 2. Industrial Shelving (Middle of room)
    // Using Cylinders for posts and Cubes for shelves
    Vector3 shelfPos = new Vector3(center.x - 2f, dockHeight, center.z);
    CreateIndustrialShelf(parent, shelfPos);

    // 3. Manager's Desk (Behind Partition)
    Vector3 mgrPos = new Vector3(center.x + (officeWidth / 3), dockHeight, center.z + (officeDepth / 3));
    CreateLargeDesk(parent, mgrPos, true); // True = L-Shape

    // 4. General Staff Desks
    CreateLargeDesk(parent, new Vector3(center.x, dockHeight, center.z - 3f), false);
    CreateLargeDesk(parent, new Vector3(center.x + 4f, dockHeight, center.z - 3f), false);
  }

  // --- FURNITURE PREFABS ---

  private void CreateFilingCabinet(GameObject parent, Vector3 bottomPos) {
    GameObject cabinet = new GameObject("Cabinet");
    cabinet.transform.parent = parent.transform;
    cabinet.transform.localPosition = bottomPos;

    float w = 0.8f;
    float d = 0.6f;
    float h = 0.5f; // Drawer size
    for (int i = 0; i < 4; i++) {
      CreatePrimitiveChild(cabinet, PrimitiveType.Cube, "Drawer",
          new Vector3(w, h - 0.05f, d),
          new Vector3(0, (i * h) + h / 2, 0), matMetal);
    }
  }

  private void CreateIndustrialShelf(GameObject parent, Vector3 bottomPos) {
    GameObject shelf = new GameObject("ShelfUnit");
    shelf.transform.parent = parent.transform;
    shelf.transform.localPosition = bottomPos;

    float w = 3f;
    float d = 1f;
    float h = 3f;

    // 4 Posts (Cylinders)
    Material mat = matMetal;
    float postRadius = 0.05f;
    Vector3[] corners = {
            new Vector3(-w/2, 0, -d/2), new Vector3(w/2, 0, -d/2),
            new Vector3(-w/2, 0, d/2),  new Vector3(w/2, 0, d/2)
        };

    foreach (Vector3 corner in corners) {
      GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
      post.transform.parent = shelf.transform;
      post.transform.localPosition = corner + new Vector3(0, h / 2, 0);
      post.transform.localScale = new Vector3(postRadius * 2, h / 2, postRadius * 2); // Height is /2 because Cylinder default is 2
      if (post.GetComponent<Renderer>())
        post.GetComponent<Renderer>().sharedMaterial = mat;
    }

    // 4 Shelves (Planes/Thin Cubes)
    for (int i = 1; i <= 4; i++) {
      CreatePrimitiveChild(shelf, PrimitiveType.Cube, "ShelfLevel",
          new Vector3(w, 0.05f, d),
          new Vector3(0, (h / 4) * i, 0), mat);
    }
  }

  private void CreateLargeDesk(GameObject parent, Vector3 pos, bool isManager) {
    // Main Desk
    CreatePrimitiveChild(parent, PrimitiveType.Cube, "DeskTop",
        new Vector3(2.2f, 0.1f, 1.0f), pos + new Vector3(0, 0.75f, 0), matWood);

    // Legs (Cylinders)
    float legScale = 0.1f;
    GameObject leg1 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
    leg1.transform.parent = parent.transform;
    leg1.transform.localPosition = pos + new Vector3(-1f, 0.375f, 0.4f);
    leg1.transform.localScale = new Vector3(legScale, 0.375f, legScale);
    leg1.GetComponent<Renderer>().sharedMaterial = matMetal;

    GameObject leg2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
    leg2.transform.parent = parent.transform;
    leg2.transform.localPosition = pos + new Vector3(1f, 0.375f, 0.4f);
    leg2.transform.localScale = new Vector3(legScale, 0.375f, legScale);
    leg2.GetComponent<Renderer>().sharedMaterial = matMetal;

    if (isManager) {
      // L-Shape Return
      CreatePrimitiveChild(parent, PrimitiveType.Cube, "DeskReturn",
          new Vector3(0.8f, 0.1f, 2.0f), pos + new Vector3(1.5f, 0.75f, 0.5f), matWood);
    }
  }

  // --- HELPER BUILDERS ---

  private void CreateWallWithDoor(GameObject parent, string name, Vector3 wallSize, Vector3 wallPos, float doorWidth, float doorOffset, Material mat) {
    GameObject wallRoot = new GameObject(name);
    wallRoot.transform.parent = parent.transform;
    wallRoot.transform.localPosition = Vector3.zero;

    float doorHeight = 2.2f;
    float halfWidth = wallSize.x / 2;

    // 1. Left Part
    float leftWidth = doorOffset;
    CreatePrimitiveChild(wallRoot, PrimitiveType.Cube, "L",
        new Vector3(leftWidth, wallSize.y, wallSize.z),
        new Vector3(wallPos.x - halfWidth + (leftWidth / 2), wallPos.y, wallPos.z), mat);

    // 2. Right Part
    float rightWidth = wallSize.x - (doorOffset + doorWidth);
    CreatePrimitiveChild(wallRoot, PrimitiveType.Cube, "R",
        new Vector3(rightWidth, wallSize.y, wallSize.z),
        new Vector3(wallPos.x + halfWidth - (rightWidth / 2), wallPos.y, wallPos.z), mat);

    // 3. Top Part
    float lintelH = wallSize.y - doorHeight;
    float lintelY = wallPos.y - (wallSize.y / 2) + doorHeight + (lintelH / 2);
    CreatePrimitiveChild(wallRoot, PrimitiveType.Cube, "Top",
        new Vector3(doorWidth, lintelH, wallSize.z),
        new Vector3(wallPos.x - halfWidth + doorOffset + (doorWidth / 2), lintelY, wallPos.z), mat);
  }

  private void CreatePrimitive(PrimitiveType type, string name, Vector3 scale, Vector3 localPos, Material mat) {
    CreatePrimitiveChild(this.gameObject, type, name, scale, localPos, mat);
  }

  private void CreatePrimitiveChild(GameObject parent, PrimitiveType type, string name, Vector3 scale, Vector3 pos, Material mat) {
    GameObject obj = GameObject.CreatePrimitive(type);
    obj.name = name;
    obj.transform.parent = parent.transform;
    obj.transform.localScale = scale;
    obj.transform.localPosition = pos;

    if (obj.GetComponent<Renderer>())
      obj.GetComponent<Renderer>().sharedMaterial = mat;
  }

  private Material GetOrCreateMaterial(string matName, Color color) {
#if UNITY_EDITOR
    string folderPath = "Assets/Materials";
    if (!AssetDatabase.IsValidFolder(folderPath))
      AssetDatabase.CreateFolder("Assets", "Materials");
    string fullPath = $"{folderPath}/{matName}.mat";
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
        Material tempMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        tempMat.SetColor("_BaseColor", color);
        return tempMat;
#endif
  }
}
