using UnityEngine;

namespace Services.NoiseGeneration
{
    public interface INoiseGeneratorService
    {
        float GeneratePoint(int seed, int span, Vector3 point);
    }
}