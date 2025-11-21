using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GameJam {
  public class CargoShipGenerator : MonoBehaviour {
    [Header("Identity")]
    public string shipName = "TradeWingsGen2";

    [Header("Dimensions")]
    public float length = 190f;
    public float beam = 32f;
    public float depth = 16f;
    public float draft = 10f;

    [Header("Superstructure")]
    [Range(0.1f, 0.9f)] public float towerPosition = 0.2f;
    public int decks = 5;
    public float deckHeight = 3.5f;
    public float towerWidth = 24f;
    public float towerLength = 18f;

    [Header("Colors")]
    public Color hullRed = new Color(0.4f, 0.1f, 0.1f);
    public Color hullTop = new Color(0.1f, 0.15f, 0.2f);
    public Color deckColor = new Color(0.3f, 0.4f, 0.35f);
    public Color towerColor = new Color(0.9f, 0.9f, 0.9f);
    public Color interiorWall = new Color(0.8f, 0.8f, 0.75f);
    public Color floorColor = new Color(0.4f, 0.4f, 0.4f);
    public Color glassColor = new Color(0.2f, 0.4f, 0.5f, 0.4f);

    private Material matHullRed, matHullTop, matDeck, matTower, matInterior, matFloor, matGlass;

    [ContextMenu("Build Ship")]
    public void BuildShip() {
      Cleanup();
      InitializeMaterials();
      BuildHull();
      BuildSuperstructure();
    }

    private void Cleanup() {
      while (transform.childCount > 0)
        DestroyImmediate(transform.GetChild(0).gameObject);
    }

    // ---------------------------------------------------------
    // HULL
    // ---------------------------------------------------------
    private void BuildHull() {
      GameObject hullRoot = new GameObject("Hull");
      hullRoot.transform.parent = transform;
      hullRoot.transform.localPosition = Vector3.zero;

      float bowLen = length * 0.15f;
      float sternLen = length * 0.1f;
      float midLen = length - bowLen - sternLen;

      // Mid Section
      CreatePrimitive(hullRoot, "Hull_Mid_Red", new Vector3(beam, draft, midLen), new Vector3(0, draft / 2, 0), matHullRed);
      CreatePrimitive(hullRoot, "Hull_Mid_Top", new Vector3(beam, depth - draft, midLen), new Vector3(0, draft + (depth - draft) / 2, 0), matHullTop);
      CreatePrimitive(hullRoot, "MainDeck", new Vector3(beam - 1f, 0.2f, midLen - 0.5f), new Vector3(0, depth, 0), matDeck);

      // Bow
      GameObject bowRoot = new GameObject("Bow");
      bowRoot.transform.parent = hullRoot.transform;
      bowRoot.transform.localPosition = new Vector3(0, 0, midLen / 2);
      float angle = 18f;
      float sideLen = bowLen / Mathf.Cos(angle * Mathf.Deg2Rad) + 2f;

      CreatePrimitive(bowRoot, "Bow_L", PrimitiveType.Cube, new Vector3(beam / 2, depth, sideLen), new Vector3(-beam / 4, depth / 2, bowLen / 2), Quaternion.Euler(0, angle, 0), matHullTop);
      CreatePrimitive(bowRoot, "Bow_R", PrimitiveType.Cube, new Vector3(beam / 2, depth, sideLen), new Vector3(beam / 4, depth / 2, bowLen / 2), Quaternion.Euler(0, -angle, 0), matHullTop);
      CreatePrimitive(bowRoot, "Bulb", PrimitiveType.Sphere, new Vector3(6f, 6f, 10f), new Vector3(0, 2f, bowLen), Quaternion.identity, matHullRed);
      CreatePrimitive(bowRoot, "BowDeck", new Vector3(beam * 0.8f, 0.2f, bowLen * 0.8f), new Vector3(0, depth, bowLen / 3), matDeck);

      // Stern
      GameObject sternRoot = new GameObject("Stern");
      sternRoot.transform.parent = hullRoot.transform;
      sternRoot.transform.localPosition = new Vector3(0, 0, -midLen / 2);
      CreatePrimitive(sternRoot, "Stern_Block", new Vector3(beam, depth, sternLen), new Vector3(0, depth / 2, -sternLen / 2), matHullTop);
      CreatePrimitive(sternRoot, "Stern_Red", new Vector3(beam + 0.1f, draft, sternLen + 0.1f), new Vector3(0, draft / 2, -sternLen / 2), matHullRed);
      CreatePrimitive(sternRoot, "SternDeck", new Vector3(beam, 0.2f, sternLen), new Vector3(0, depth, -sternLen / 2), matDeck);
    }

    // ---------------------------------------------------------
    // SUPERSTRUCTURE
    // ---------------------------------------------------------
    private void BuildSuperstructure() {
      float totalL = length;
      float zPos = Mathf.Lerp(totalL / 2 - 25f, -totalL / 2 + 25f, 1f - towerPosition);

      GameObject towerRoot = new GameObject("Superstructure");
      towerRoot.transform.parent = transform;
      towerRoot.transform.localPosition = new Vector3(0, depth, zPos);

      float stairHoleL = 6.0f;
      float stairHoleW = 5.0f;
      float stairZOffset = -towerLength / 4;

      for (int i = 0; i < decks; i++) {
        bool isBridge = (i == decks - 1);
        BuildDeck(towerRoot, i, isBridge, stairHoleL, stairHoleW, stairZOffset);
      }
    }

    private void BuildDeck(GameObject parent, int floorIndex, bool isBridge, float holeL, float holeW, float holeZ) {
      float yPos = floorIndex * deckHeight;
      GameObject deckRoot = new GameObject($"Deck_{floorIndex}");
      deckRoot.transform.parent = parent.transform;
      deckRoot.transform.localPosition = new Vector3(0, yPos, 0);

      float w = isBridge ? beam + 6f : towerWidth;
      float l = towerLength;
      float h = deckHeight;
      float wallT = 0.3f;

      // 1. FLOOR SLAB (With Stair Hole)
      // Front Slab 
      float holeStartZ = holeZ + (holeL / 2);
      float frontLen = (l / 2) - holeStartZ;
      CreatePrimitive(deckRoot, "Floor_Front", new Vector3(w, 0.2f, frontLen), new Vector3(0, 0, (l / 2) - (frontLen / 2)), matFloor);

      // Back Slab
      float holeEndZ = holeZ - (holeL / 2);
      float backLen = holeEndZ - (-l / 2);
      if (backLen > 0) {
        CreatePrimitive(deckRoot, "Floor_Back", new Vector3(w, 0.2f, backLen), new Vector3(0, 0, (-l / 2) + (backLen / 2)), matFloor);
      }

      // Side Strips
      float sideStripWidth = (w - holeW) / 2;
      CreatePrimitive(deckRoot, "Floor_L", new Vector3(sideStripWidth, 0.2f, holeL), new Vector3(-w / 2 + sideStripWidth / 2, 0, holeZ), matFloor);
      CreatePrimitive(deckRoot, "Floor_R", new Vector3(sideStripWidth, 0.2f, holeL), new Vector3(w / 2 - sideStripWidth / 2, 0, holeZ), matFloor);


      // 2. EXTERIOR WALLS
      if (isBridge) {
        // BRIDGE SPECIFIC WALLS
        // Dashboard / Lower Wall
        float dashH = 1.0f;
        float glassH = 2.0f;
        float glassY = dashH + (glassH / 2);

        // Front Dashboard
        CreatePrimitive(deckRoot, "Dash_F", new Vector3(w, dashH, wallT), new Vector3(0, dashH / 2, l / 2), matTower);
        // Front Glass
        CreatePrimitive(deckRoot, "Glass_F", new Vector3(w, glassH, 0.1f), new Vector3(0, glassY, l / 2), matGlass);

        // Back Wall
        CreatePrimitive(deckRoot, "Wall_B", new Vector3(w, h, wallT), new Vector3(0, h / 2, -l / 2), matTower);

        // Left Side Glass & Dash
        CreatePrimitive(deckRoot, "Dash_L", new Vector3(wallT, dashH, l), new Vector3(-w / 2, dashH / 2, 0), matTower);
        CreatePrimitive(deckRoot, "Glass_L", new Vector3(0.1f, glassH, l), new Vector3(-w / 2, glassY, 0), matGlass);

        // Right Side Glass & Dash
        CreatePrimitive(deckRoot, "Dash_R", new Vector3(wallT, dashH, l), new Vector3(w / 2, dashH / 2, 0), matTower);
        CreatePrimitive(deckRoot, "Glass_R", new Vector3(0.1f, glassH, l), new Vector3(w / 2, glassY, 0), matGlass);

        // Roof
        CreatePrimitive(deckRoot, "Roof", new Vector3(w + 1f, 0.3f, l + 1f), new Vector3(0, h, 0), matTower);

        // Railing for Stairs
        BuildStairRailings(deckRoot, holeL, holeW, holeZ);
      } else {
        // STANDARD DECK WALLS
        // Front
        CreatePrimitive(deckRoot, "Wall_F", new Vector3(w, h, wallT), new Vector3(0, h / 2, l / 2), matTower);

        // Back (Entry Door on Floor 0)
        if (floorIndex == 0)
          CreateWallWithDoor(deckRoot, "Wall_B_Entry", new Vector3(w, h, wallT), new Vector3(0, h / 2, -l / 2), 2.0f, matTower);
        else
          CreatePrimitive(deckRoot, "Wall_B", new Vector3(w, h, wallT), new Vector3(0, h / 2, -l / 2), matTower);

        // Sides (Solid)
        CreatePrimitive(deckRoot, "Wall_L", new Vector3(wallT, h, l), new Vector3(-w / 2, h / 2, 0), matTower);
        CreatePrimitive(deckRoot, "Wall_R", new Vector3(wallT, h, l), new Vector3(w / 2, h / 2, 0), matTower);
      }

      // 4. INTERIOR
      BuildInterior(deckRoot, w, l, h, isBridge, holeZ);
    }

    private void BuildInterior(GameObject parent, float w, float l, float h, bool isBridge, float stairZ) {
      float corrW = 2.0f;

      if (!isBridge) {
        //float stairGap = 3f;

        //// Front Corridor Walls
        //float frontLen = (l / 2) - (stairZ + stairGap / 2);
        //CreatePrimitive(parent, "Corr_L_F", new Vector3(0.2f, h, frontLen), new Vector3(-corrW / 2, h / 2, (l / 2) - frontLen / 2), matInterior);
        //CreatePrimitive(parent, "Corr_R_F", new Vector3(0.2f, h, frontLen), new Vector3(corrW / 2, h / 2, (l / 2) - frontLen / 2), matInterior);

        //// Back Corridor Walls
        //float backLen = (stairZ - stairGap / 2) - (-l / 2);
        //if (backLen > 0) {
        //  CreatePrimitive(parent, "Corr_L_B", new Vector3(0.2f, h, backLen), new Vector3(-corrW / 2, h / 2, -l / 2 + backLen / 2), matInterior);
        //  CreatePrimitive(parent, "Corr_R_B", new Vector3(0.2f, h, backLen), new Vector3(corrW / 2, h / 2, -l / 2 + backLen / 2), matInterior);
        //}

        // Dividers with Doors
        for (int z = -1; z <= 1; z++) {
          float roomZ = z * 4f;
          if (Mathf.Abs(roomZ - stairZ) < 3f)
            continue;

          CreateWallWithDoor(parent, "Div_L", new Vector3(w / 2 - corrW / 2, h, 0.2f), new Vector3(-w / 4 - corrW / 4, h / 2, roomZ), 1.2f, matInterior);
          CreateWallWithDoor(parent, "Div_R", new Vector3(w / 2 - corrW / 2, h, 0.2f), new Vector3(w / 4 + corrW / 4, h / 2, roomZ), 1.2f, matInterior);
        }
      }

      // Stairwell 
      if (!isBridge) {
        // Standard Deck gets the stairs going up
        GameObject stairRoot = new GameObject("Stairwell");
        stairRoot.transform.parent = parent.transform;
        stairRoot.transform.localPosition = new Vector3(0, 0, stairZ);

        // Pass in dimensions to build stairs
        // Hole size was 6.0 (L) x 5.0 (W)
        BuildSwitchbackStairs(stairRoot, 6.0f, 5.0f, h);
      }
    }

    private void BuildSwitchbackStairs(GameObject parent, float holeL, float holeW, float deckH) {
      float landingDepth = 1.5f;
      float rampWidth = (holeW / 2) - 0.2f;
      float rampRun = holeL - landingDepth;
      float halfH = deckH / 2;

      // --- FIX 1: Elongate Ramp to close gap ---
      float rampLength = rampRun + 0.5f; // Extra length to clip into floor/landing

      // 1. Bottom Ramp (Left, Up-Forward)
      GameObject r1 = CreatePrimitive(parent, "Ramp_Up",
          new Vector3(rampWidth, 0.2f, rampLength),
          new Vector3(-holeW / 4, halfH / 2, -holeL / 2 + rampRun / 2), matFloor);
      float angle = Mathf.Atan(halfH / rampRun) * Mathf.Rad2Deg;
      r1.transform.localRotation = Quaternion.Euler(-angle, 0, 0);

      // 2. Landing (Front)
      CreatePrimitive(parent, "Landing_Mid",
          new Vector3(holeW, 0.2f, landingDepth),
          new Vector3(0, halfH, holeL / 2 - landingDepth / 2), matFloor);

      // 3. Top Ramp (Right, Up-Backward)
      GameObject r2 = CreatePrimitive(parent, "Ramp_Return",
          new Vector3(rampWidth, 0.2f, rampLength),
          new Vector3(holeW / 4, halfH + halfH / 2, -holeL / 2 + rampRun / 2), matFloor);
      r2.transform.localRotation = Quaternion.Euler(angle, 0, 0);

      // 4. Rail
      CreatePrimitive(parent, "Rail_Center",
          new Vector3(0.1f, deckH, rampRun),
          new Vector3(0, deckH / 2, -holeL / 2 + rampRun / 2), matInterior);
    }

    private void BuildStairRailings(GameObject parent, float holeL, float holeW, float holeZ) {
      GameObject railRoot = new GameObject("StairRailings");
      railRoot.transform.parent = parent.transform;
      railRoot.transform.localPosition = new Vector3(0, 0, holeZ);

      float rH = 1.1f;
      float rT = 0.1f;

      // --- FIX 2: Bridge Access Opening ---
      // The Switchback stair ends on the Right side, going towards the Back (Z-).
      // So we need to leave the Back-Right quadrant open for the player to step off.

      // Front (Solid)
      CreatePrimitive(railRoot, "R_Front", new Vector3(holeW, rH, rT), new Vector3(0, rH / 2, holeL / 2), matInterior);

      // Back (Partial - Only cover the Left side where the down ramp is)
      CreatePrimitive(railRoot, "R_Back_L",
          new Vector3(holeW / 2, rH, rT),
          new Vector3(-holeW / 4, rH / 2, -holeL / 2), matInterior);

      // Left (Solid)
      CreatePrimitive(railRoot, "R_Left", new Vector3(rT, rH, holeL), new Vector3(-holeW / 2, rH / 2, 0), matInterior);

      // Right (Solid - along the side of the hole)
      CreatePrimitive(railRoot, "R_Right", new Vector3(rT, rH, holeL), new Vector3(holeW / 2, rH / 2, 0), matInterior);
    }

    // ---------------------------------------------------------
    // UTILS
    // ---------------------------------------------------------
    private void CreateWallWithDoor(GameObject parent, string name, Vector3 size, Vector3 pos, float doorWidth, Material mat) {
      GameObject w = new GameObject(name);
      w.transform.parent = parent.transform;
      w.transform.localPosition = pos;

      bool wallRunsAlongX = size.x > size.z;
      float fullLen = wallRunsAlongX ? size.x : size.z;
      float thick = wallRunsAlongX ? size.z : size.x;
      float h = size.y;
      float doorH = 2.1f;
      float sideLen = (fullLen - doorWidth) / 2;

      Vector3 posL = wallRunsAlongX ? new Vector3(-fullLen / 2 + sideLen / 2, 0, 0) : new Vector3(0, 0, -fullLen / 2 + sideLen / 2);
      Vector3 scaleL = wallRunsAlongX ? new Vector3(sideLen, h, thick) : new Vector3(thick, h, sideLen);
      CreatePrimitive(w, "Wall_S1", scaleL, posL, mat);

      Vector3 posR = wallRunsAlongX ? new Vector3(fullLen / 2 - sideLen / 2, 0, 0) : new Vector3(0, 0, fullLen / 2 - sideLen / 2);
      Vector3 scaleR = wallRunsAlongX ? new Vector3(sideLen, h, thick) : new Vector3(thick, h, sideLen);
      CreatePrimitive(w, "Wall_S2", scaleR, posR, mat);

      Vector3 posTop = new Vector3(0, doorH + (h - doorH) / 2, 0);
      Vector3 scaleTop = wallRunsAlongX ? new Vector3(doorWidth, h - doorH, thick) : new Vector3(thick, h - doorH, doorWidth);
      CreatePrimitive(w, "Wall_Lintel", scaleTop, posTop, mat);
    }

    private GameObject CreatePrimitive(GameObject parent, string name, Vector3 scale, Vector3 pos, Material mat) {
      return CreatePrimitive(parent, name, PrimitiveType.Cube, scale, pos, Quaternion.identity, mat);
    }

    private GameObject CreatePrimitive(GameObject parent, string name, PrimitiveType type, Vector3 scale, Vector3 pos, Quaternion rot, Material mat) {
      GameObject obj = GameObject.CreatePrimitive(type);
      obj.name = name;
      obj.transform.parent = parent.transform;
      obj.transform.localScale = scale;
      obj.transform.SetLocalPositionAndRotation(pos, rot);
      if (obj.GetComponent<Renderer>())
        obj.GetComponent<Renderer>().sharedMaterial = mat;
      return obj;
    }

    private void InitializeMaterials() {
      matHullRed = GetOrCreateMaterial("HullRed", hullRed);
      matHullTop = GetOrCreateMaterial("HullTop", hullTop);
      matDeck = GetOrCreateMaterial("Deck", deckColor);
      matTower = GetOrCreateMaterial("Superstructure", towerColor);
      matInterior = GetOrCreateMaterial("Interior", interiorWall);
      matFloor = GetOrCreateMaterial("Floor", floorColor);
      matGlass = GetOrCreateMaterial("Glass", glassColor, true);
    }

    private Material GetOrCreateMaterial(string suffix, Color color, bool isTransparent = false) {
#if UNITY_EDITOR
      string folder = $"Assets/Materials/Ships/{shipName}";
      string fileName = $"{shipName}_{suffix}";
      string fullPath = $"{folder}/{fileName}.mat";

      if (!AssetDatabase.IsValidFolder("Assets/Materials"))
        AssetDatabase.CreateFolder("Assets", "Materials");
      if (!AssetDatabase.IsValidFolder("Assets/Materials/Ships"))
        AssetDatabase.CreateFolder("Assets/Materials", "Ships");
      if (!AssetDatabase.IsValidFolder(folder))
        AssetDatabase.CreateFolder("Assets/Materials/Ships", shipName);

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
