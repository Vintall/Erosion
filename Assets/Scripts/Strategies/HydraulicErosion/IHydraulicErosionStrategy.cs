using Enums;
using Models;

namespace Strategies.HydraulicErosion
{
    public interface IHydraulicErosionStrategy
    {
        EHydraulicErosionType HydraulicErosionType { get; }
        void Execute(HydraulicErosionIterationVo iterationData, MeshDataVo meshDataVo);
    }
}