using System;
using Enums;
using Models;

namespace Services.GPUHydraulicErosionService
{
    public interface IHydraulicErosionService
    {
        void SimulateErosion(
            HydraulicErosionIterationVo iterationData, 
            MeshDataVo meshDataVo,
            EHydraulicErosionType hydraulicErosionType,
            Action<int> iterationTimestamp);
    }
}