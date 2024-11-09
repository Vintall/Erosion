using Databases.CommonShadersDatabase;
using Models;
using UnityEngine;

namespace Services.GPUHydraulicErosionService.Impls
{
    public class GPUHydraulicErosionService : IGPUHydraulicErosionService
    {
        private static readonly int HeightMapPropertyId = Shader.PropertyToID("heightMap");
        private static readonly int WaterMapPropertyId = Shader.PropertyToID("waterMap");
        private static readonly int DeltaTimePropertyId = Shader.PropertyToID("deltaTime");
        private static readonly int ErosionRatePropertyId = Shader.PropertyToID("erosionRate");
        private static readonly int DepositionRatePropertyId = Shader.PropertyToID("depositionRate");
        private static readonly int EvaporationRatePropertyId = Shader.PropertyToID("evaporationRate");
        private static readonly int MinSlopePropertyId = Shader.PropertyToID("minSlope");
        private readonly ICommonShadersDatabase _commonShadersDatabase;

        public GPUHydraulicErosionService(
            ICommonShadersDatabase commonShadersDatabase)
        {
            _commonShadersDatabase = commonShadersDatabase;
        }

        public void SimulateErosionIteration(
            HydraulicErosionIterationVo iterationData,
            Texture heightMap,
            Texture waterMap)
        {
            var erosionShader = _commonShadersDatabase.HydraulicErosionComputeShader;
            var kernel = erosionShader.FindKernel("CSMain");

            erosionShader.SetTexture(kernel, HeightMapPropertyId, heightMap);
            erosionShader.SetTexture(kernel, WaterMapPropertyId, waterMap);

            erosionShader.SetFloat(DeltaTimePropertyId, iterationData.DeltaTime);
            erosionShader.SetFloat(ErosionRatePropertyId, iterationData.ErosionRate);
            erosionShader.SetFloat(DepositionRatePropertyId, iterationData.DepositionRate);
            erosionShader.SetFloat(EvaporationRatePropertyId, iterationData.EvaporationRate);
            erosionShader.SetFloat(MinSlopePropertyId, iterationData.MinSlope);

            erosionShader.Dispatch(kernel, heightMap.width / 8, heightMap.width / 8, 1);
        }
    }
}