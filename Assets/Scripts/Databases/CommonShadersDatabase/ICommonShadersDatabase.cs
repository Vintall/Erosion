using UnityEngine;

namespace Databases.CommonShadersDatabase
{
    public interface ICommonShadersDatabase
    {
        ComputeShader GridBasedHydraulicErosionComputeShader { get; }
        ComputeShader ParticleBasedHydraulicErosionComputeShader { get; }
    }
}