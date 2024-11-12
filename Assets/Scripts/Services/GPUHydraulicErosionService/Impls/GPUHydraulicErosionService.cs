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
            MeshDataVo meshDataVo)
        {
            var erosionShader = _commonShadersDatabase.HydraulicErosionComputeShader;
            var kernel = erosionShader.FindKernel("CSMain");
            var consecutiveVerticesFloat = new float[meshDataVo.Resolution * meshDataVo.Resolution];

            for(var i = 0;i<meshDataVo.Resolution;++i)
            for (var j = 0; j < meshDataVo.Resolution; ++j)
            {
                consecutiveVerticesFloat[i * meshDataVo.Resolution + j] = meshDataVo.Vertices[i][j].y;
            }
            
            ComputeBuffer heightBuffer = new ComputeBuffer(consecutiveVerticesFloat.Length, sizeof(float));
            heightBuffer.SetData(consecutiveVerticesFloat);
            erosionShader.SetBuffer(0, "heightMap", heightBuffer);

            ComputeBuffer waterBuffer = new ComputeBuffer(consecutiveVerticesFloat.Length, sizeof(float));
            waterBuffer.SetData(consecutiveVerticesFloat);
            erosionShader.SetBuffer(0, "waterMap", waterBuffer);

            erosionShader.SetInt("mapWidth", meshDataVo.Resolution);
            erosionShader.SetInt("mapHeight", meshDataVo.Resolution);
            
            erosionShader.Dispatch(kernel, meshDataVo.Resolution / 8, meshDataVo.Resolution / 8, 1);
            
            heightBuffer.GetData(consecutiveVerticesFloat);

            for (var i = 0; i < meshDataVo.Resolution; ++i)
            for (int j = 0; j < meshDataVo.Resolution; ++j)
            {
                meshDataVo.Vertices[i][j] = new Vector3(
                    meshDataVo.Vertices[i][j].x, 
                    consecutiveVerticesFloat[i * meshDataVo.Resolution + j], 
                    meshDataVo.Vertices[i][j].z);
            }
                
            //meshDataVo.Vertices = consecutiveVerticesVec.ConvertToMatrixArray(consecutiveVerticesVec.Length);
        }
    }
}