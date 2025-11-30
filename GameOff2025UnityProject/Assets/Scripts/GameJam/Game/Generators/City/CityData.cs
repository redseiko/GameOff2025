using UnityEngine;

namespace GameJam {

  // --- ENUMS ---
  public enum CityZone {
    Empty,
    Commercial,
    Residential,
    Industrial,
    Park
  }

  public enum CellType {
    Empty,
    Road,
    Building
  }

  // --- CONFIGURATION STRUCTS ---
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

  // --- MATERIAL PROFILE ---
  /// <summary>
  /// Holds references to all standard materials required by city generators.
  /// Passed to generators so they don't need to generate their own materials.
  /// </summary>
  public class CityMaterialProfile {
    // Infrastructure
    public Material Asphalt;
    public Material Sidewalk;

    // Base Construction
    public Material Concrete;
    public Material Brick;

    // Glass Types
    public Material GlassCommercial;
    public Material GlassResidential;
    public Material GlassIndustrial;

    // Wall Types
    public Material WallResidential;
    public Material WallIndustrial;

    // Misc
    public Material ParkGrass;
  }
}
