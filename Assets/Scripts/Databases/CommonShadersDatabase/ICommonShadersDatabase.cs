using UnityEngine;

namespace Databases.CommonShadersDatabase
{
    public interface ICommonShadersDatabase
    {
        ComputeShader HydraulicErosionComputeShader { get; }
    }
}