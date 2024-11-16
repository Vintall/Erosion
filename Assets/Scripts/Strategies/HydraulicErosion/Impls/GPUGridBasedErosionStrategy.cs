using Databases.CommonShadersDatabase;
using Enums;
using Models;
using UnityEngine;

namespace Strategies.HydraulicErosion.Impls
{
    public class GPUGridBasedErosionStrategy : IHydraulicErosionStrategy
    {
        private static readonly int InVertexStatesPropertyId = Shader.PropertyToID("inVertexStates");
        private static readonly int OutVertexStatesPropertyId = Shader.PropertyToID("outVertexStates");
        private static readonly int ErosionRatePropertyId = Shader.PropertyToID("erosionRate");
        private static readonly int DepositionRatePropertyId = Shader.PropertyToID("depositionRate");
        private static readonly int EvaporationRatePropertyId = Shader.PropertyToID("evaporationRate");
        private static readonly int MinSlopePropertyId = Shader.PropertyToID("minSlope");
        private readonly ICommonShadersDatabase _commonShadersDatabase;
        
        public EHydraulicErosionType HydraulicErosionType => EHydraulicErosionType.GridGPU;

        public GPUGridBasedErosionStrategy(
            ICommonShadersDatabase commonShadersDatabase)
        {
            _commonShadersDatabase = commonShadersDatabase;
        }
        
        struct VertexState
        {
            public float height;
            public float water;
            public float sediment;
        }
        
        public void Execute(HydraulicErosionIterationVo iterationData, MeshDataVo meshDataVo)
        {
            var erosionShader = _commonShadersDatabase.HydraulicErosionComputeShader;
            var kernel = 0;
            var verticesStates = new VertexState[meshDataVo.Resolution * meshDataVo.Resolution];
            //var deltaHeightMap = new float[meshDataVo.Resolution * meshDataVo.Resolution];
            
            for (var i = 0; i < meshDataVo.Resolution; ++i)
            for (var j = 0; j < meshDataVo.Resolution; ++j)
                verticesStates[i * meshDataVo.Resolution + j] = new VertexState()
                {
                    height = meshDataVo.Vertices[i][j].y,
                    sediment = 0,
                    water = Random.Range(0, 1f)
                };
            
            var inVertexStatesBuffer = new ComputeBuffer(verticesStates.Length, sizeof(float) * 3);
            var outVertexStatesBuffer = new ComputeBuffer(verticesStates.Length, sizeof(float) * 3);

            erosionShader.SetInt(Shader.PropertyToID("mapWidth"), meshDataVo.Resolution);
            erosionShader.SetInt(Shader.PropertyToID("mapHeight"), meshDataVo.Resolution);
            erosionShader.SetFloat(Shader.PropertyToID("soilSoftness"), iterationData.SoilSoftness);
            erosionShader.SetFloat(Shader.PropertyToID("sedimentCarryingCapacity"),
                iterationData.SedimentCarryingCapacity);
            erosionShader.SetFloat(ErosionRatePropertyId, iterationData.ErosionRate);
            erosionShader.SetFloat(DepositionRatePropertyId, iterationData.DepositionRate);
            erosionShader.SetFloat(EvaporationRatePropertyId, iterationData.EvaporationRate);
            erosionShader.SetFloat(MinSlopePropertyId, iterationData.MinSlope);
            
            for (var k = 0; k < 100; ++k)
            {
                //var heightDeltaBuffer = new ComputeBuffer(deltaHeightMap.Length, sizeof(float));

                inVertexStatesBuffer.SetData(verticesStates);
                outVertexStatesBuffer.SetData(verticesStates);
                //heightDeltaBuffer.SetData(deltaHeightMap);
                erosionShader.SetBuffer(kernel, InVertexStatesPropertyId, inVertexStatesBuffer);
                erosionShader.SetBuffer(kernel, OutVertexStatesPropertyId, outVertexStatesBuffer);
                //erosionShader.SetBuffer(kernel, Shader.PropertyToID("deltaHeightMap"), heightDeltaBuffer);
                erosionShader.Dispatch(kernel, meshDataVo.Resolution / 8, meshDataVo.Resolution / 8, 1);

                outVertexStatesBuffer.GetData(verticesStates);
                //heightDeltaBuffer.GetData(deltaHeightMap);

                //heightDeltaBuffer.Release();
                

                
            }
            
            inVertexStatesBuffer.Release();
            outVertexStatesBuffer.Release();
            
            for (var i = 0; i < meshDataVo.Resolution; ++i)
            for (var j = 0; j < meshDataVo.Resolution; ++j)
            {
                meshDataVo.Vertices[i][j] += Vector3.up *
                                             (verticesStates[i * meshDataVo.Resolution + j].height -
                                              meshDataVo.Vertices[i][j].y);
            }
        }
    }
}