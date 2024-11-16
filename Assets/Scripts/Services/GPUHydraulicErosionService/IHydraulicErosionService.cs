using Enums;
using Models;
using UnityEngine;

namespace Services.GPUHydraulicErosionService
{
    public interface IHydraulicErosionService
    {
        void SimulateErosion(
            HydraulicErosionIterationVo iterationData, 
            MeshDataVo meshDataVo,
            EHydraulicErosionType hydraulicErosionType);
    }
}