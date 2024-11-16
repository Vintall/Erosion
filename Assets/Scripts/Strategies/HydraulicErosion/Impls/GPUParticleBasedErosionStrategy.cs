using Enums;
using Models;

namespace Strategies.HydraulicErosion.Impls
{
    public class GPUParticleBasedErosionStrategy : IHydraulicErosionStrategy
    {
        public EHydraulicErosionType HydraulicErosionType => EHydraulicErosionType.ParticlesGPU;
        
        public void Execute(HydraulicErosionIterationVo iterationData, MeshDataVo meshDataVo)
        {
            throw new System.NotImplementedException();
        }
    }
}