using Enums;
using Models;
using UnityEngine;

namespace Strategies.HydraulicErosion.Impls
{
    public class CPUParticleBasedErosionStrategy : IHydraulicErosionStrategy
    {
        private readonly IErosionCellSimulator _erosionCellSimulator;
        public EHydraulicErosionType HydraulicErosionType => EHydraulicErosionType.ParticlesCPU;

        public CPUParticleBasedErosionStrategy(
            IErosionCellSimulator erosionCellSimulator)
        {
            _erosionCellSimulator = erosionCellSimulator;
        }
        
        public void Execute(HydraulicErosionIterationVo iterationData, MeshDataVo meshDataVo)
        {
            _erosionCellSimulator.SetupSimulator(meshDataVo.Vertices);

            for (var i = 0; i < iterationData.IterationsCount; ++i)
            {
                _erosionCellSimulator.SimulateDroplet(new Vector2(
                    Random.Range(0f, meshDataVo.Resolution),
                    Random.Range(0f, meshDataVo.Resolution)));
            }

            meshDataVo.Vertices = _erosionCellSimulator.HeightMap;
        }
    }
}