using Models;
using UnityEngine;

namespace Services.GPUHydraulicErosionService
{
    public interface IGPUHydraulicErosionService
    {
        void SimulateErosionIteration(
            HydraulicErosionIterationVo iterationData,
            Texture heightMap,
            Texture waterMap);
    }
}