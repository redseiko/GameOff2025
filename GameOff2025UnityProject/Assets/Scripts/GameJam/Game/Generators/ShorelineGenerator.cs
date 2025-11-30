using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GameJam {
  public class ShorelineGenerator : MonoBehaviour {
    [Header("Terrain References")]
    public Terrain terrain;

    [Header("Port Levels (Y Axis)")]
    [Tooltip("The seabed level (0 in your setup).")]
    public float deepWaterLevel = 0f;

    [Tooltip("The height of the land/concrete (25 in your setup).")]
    public float shoreLevel = 25f;

    [Header("Gradient Settings")]
    [Tooltip("Direction of the slope. (1,0) = Low Left to High Right. (-1,0) = Low Right to High Left.")]
    public Vector2 slopeDirection = new Vector2(1, 0);

    public AnimationCurve slopeCurve = new AnimationCurve(
        new Keyframe(0f, 0f),
        new Keyframe(0.4f, 0.1f),
        new Keyframe(0.8f, 0.9f),
        new Keyframe(1f, 1f)
    );

    [Header("Noise (Seabed Variation)")]
    public float noiseScale = 0.05f;
    public float noiseStrength = 2.0f;
    public int octaves = 3;
    public float persistence = 0.5f;

    [ContextMenu("Generate Shoreline")]
    public void GenerateShoreline() {
      if (terrain == null)
        terrain = GetComponent<Terrain>();
      if (terrain == null) {
        Debug.LogError("No Terrain found!");
        return;
      }

      TerrainData data = terrain.terrainData;
      if (data.size.y < shoreLevel) {
        Debug.LogError($"Terrain Height ({data.size.y}) is too low! Increase it to > {shoreLevel}.");
        return;
      }

#if UNITY_EDITOR
      Undo.RegisterCompleteObjectUndo(data, "Generate Shoreline");
#endif

      int res = data.heightmapResolution;
      float[,] heights = new float[res, res];
      float terrainMaxHeight = data.size.y;
      float terrainYPos = terrain.transform.position.y;

      // --- FIX START: CALCULATE PROJECTION BOUNDS ---
      Vector2 dir = slopeDirection.normalized;
      if (dir.sqrMagnitude < 0.001f)
        dir = Vector2.right; // Safety default

      // We project the 4 corners of the UV square onto the direction vector
      // to see what the minimum and maximum 'dot' values will be.
      // Corners: (0,0), (1,0), (0,1), (1,1)
      float d0 = 0f;                     // Dot(0,0)
      float d1 = dir.x;                  // Dot(1,0)
      float d2 = dir.y;                  // Dot(0,1)
      float d3 = dir.x + dir.y;          // Dot(1,1)

      float minProj = Mathf.Min(d0, Mathf.Min(d1, Mathf.Min(d2, d3)));
      float maxProj = Mathf.Max(d0, Mathf.Max(d1, Mathf.Max(d2, d3)));
      float range = maxProj - minProj;

      if (Mathf.Abs(range) < 0.0001f)
        range = 1f; // Prevent divide by zero
                    // --- FIX END ---

      for (int y = 0; y < res; y++) {
        for (int x = 0; x < res; x++) {
          float u = (float) x / (res - 1);
          float v = (float) y / (res - 1);

          // 1. Calculate raw projection
          float rawDot = Vector2.Dot(new Vector2(u, v), dir);

          // 2. Remap based on the calculated bounds
          // This converts the range [minProj ... maxProj] to [0 ... 1]
          float progress = (rawDot - minProj) / range;

          progress = Mathf.Clamp01(progress);

          // 3. Base Height
          float curveValue = slopeCurve.Evaluate(progress);
          float targetY = Mathf.Lerp(deepWaterLevel, shoreLevel, curveValue);

          // 4. Noise (Dampened near shore)
          float shoreDampening = 1.0f - (curveValue * curveValue);
          float noiseVal = CalculateFractalNoise(x, y, octaves, noiseScale, persistence);
          float noiseOffset = (noiseVal * noiseStrength) * shoreDampening;

          float finalWorldY = targetY + noiseOffset;

          // 5. Apply to array
          heights[y, x] = (finalWorldY - terrainYPos) / terrainMaxHeight;
        }
      }

      data.SetHeights(0, 0, heights);

#if UNITY_EDITOR
      EditorUtility.SetDirty(data);
#endif
      Debug.Log($"Shoreline Generated. Direction: {slopeDirection}");
    }

    private float CalculateFractalNoise(float x, float y, int octaves, float scale, float persistence) {
      float total = 0;
      float frequency = scale;
      float amplitude = 1;
      float maxValue = 0;

      for (int i = 0; i < octaves; i++) {
        float xCoord = (x * frequency) + (i * 132.5f);
        float yCoord = (y * frequency) + (i * 132.5f);

        float noise = Mathf.PerlinNoise(xCoord, yCoord) - 0.5f;

        total += noise * amplitude;
        maxValue += amplitude;

        amplitude *= persistence;
        frequency *= 2;
      }
      return total;
    }
  }
}
