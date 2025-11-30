using UnityEngine;

namespace GameJam {
  public interface IPlotGenerator {
    /// <summary>
    /// Generates a building or structure within the given bounds.
    /// </summary>
    /// <param name="root">The parent GameObject for this plot. Position is usually (0,0,0) relative to the lot.</param>
    /// <param name="size">The Width (X) and Length (Z) of the plot in meters.</param>
    /// <param name="zone">The designated Zone type (Commercial, Residential, etc).</param>
    /// <param name="materials">The strongly-typed collection of city materials.</param>
    /// <param name="generationSeed">A unique seed to ensure deterministic generation per lot.</param>
    void Generate(GameObject root, Vector2 size, CityZone zone, CityMaterialProfile materials, int generationSeed);
  }
}
