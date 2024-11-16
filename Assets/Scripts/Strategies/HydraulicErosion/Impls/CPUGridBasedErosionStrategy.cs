using Enums;
using Models;

namespace Strategies.HydraulicErosion.Impls
{
    public class CPUGridBasedErosionStrategy : IHydraulicErosionStrategy
    {
        public EHydraulicErosionType HydraulicErosionType => EHydraulicErosionType.GridCPU;
        
        public void Execute(HydraulicErosionIterationVo iterationData, MeshDataVo meshDataVo)
        {
            throw new System.NotImplementedException();
        }
    }
}