using UnityEngine;
using System.Collections.Generic;

namespace GameJam {
  /// <summary>
  /// A composite generator that randomly selects one of its child generators to execute.
  /// Allows mixing "Old Stubs" with "New Hero Buildings" in the same zone.
  /// </summary>
  public class RandomSelectorGenerator : IPlotGenerator {

    private struct Candidate {
      public IPlotGenerator generator;
      public float weight;
    }

    private readonly List<Candidate> candidates = new List<Candidate>();
    private float totalWeight = 0;

    public void AddCandidate(IPlotGenerator gen, float weight) {
      candidates.Add(new Candidate { generator = gen, weight = weight });
      totalWeight += weight;
    }

    public void Generate(GameObject root, Vector2 size, CityZone zone, CityMaterialProfile materials, int generationSeed) {
      // Determine which generator to use based on the seed
      Random.InitState(generationSeed);
      float roll = Random.Range(0f, totalWeight);

      float currentSum = 0;
      IPlotGenerator selected = null;

      foreach (var c in candidates) {
        currentSum += c.weight;
        if (roll <= currentSum) {
          selected = c.generator;
          break;
        }
      }

      // Fallback
      if (selected == null && candidates.Count > 0)
        selected = candidates[0].generator;

      // Execute the chosen one
      selected?.Generate(root, size, zone, materials, generationSeed);
    }
  }
}
