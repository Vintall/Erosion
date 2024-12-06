using System;
using Databases.CommonShadersDatabase;
using Databases.GaussianBlur;
using Enums;
using Models;
using UnityEngine;
using Random = UnityEngine.Random;

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
        private static readonly int MapWidthPropertyId = Shader.PropertyToID("mapWidth");
        private static readonly int MapHeightPropertyId = Shader.PropertyToID("mapHeight");
        private static readonly int SoilSoftnessPropertyId = Shader.PropertyToID("soilSoftness");
        private static readonly int SedimentCarryingCapacityPropertyId = Shader.PropertyToID("sedimentCarryingCapacity");
        private readonly ICommonShadersDatabase _commonShadersDatabase;
        private readonly IGaussianBlurDatabase _gaussianBlurDatabase;

        public EHydraulicErosionType HydraulicErosionType => EHydraulicErosionType.GridGPU;

        public GPUGridBasedErosionStrategy(
            ICommonShadersDatabase commonShadersDatabase,
            IGaussianBlurDatabase gaussianBlurDatabase)
        {
            _commonShadersDatabase = commonShadersDatabase;
            _gaussianBlurDatabase = gaussianBlurDatabase;
        }
        
        struct VertexState
        {
            public float height;
            public float water;
            public float sediment;
        }
        
        public void Execute(HydraulicErosionIterationVo iterationData, 
            MeshDataVo meshDataVo, Action<int> iterationTimestamp)
        {
            var erosionShader = _commonShadersDatabase.GridBasedHydraulicErosionComputeShader;
            var verticesStates = new VertexState[meshDataVo.Resolution * meshDataVo.Resolution];
            
            for (var i = 0; i < meshDataVo.Resolution; ++i)
            for (var j = 0; j < meshDataVo.Resolution; ++j)
                verticesStates[i * meshDataVo.Resolution + j] = new VertexState()
                {
                    height = meshDataVo.Vertices[i][j].y,
                    sediment = 0,
                    water = 1
                };
            
            var inVertexStatesBuffer = new ComputeBuffer(verticesStates.Length, sizeof(float) * 3);
            var outVertexStatesBuffer = new ComputeBuffer(verticesStates.Length, sizeof(float) * 3);
            
            erosionShader.SetInt(MapWidthPropertyId, meshDataVo.Resolution);
            erosionShader.SetInt(MapHeightPropertyId, meshDataVo.Resolution);
            erosionShader.SetFloat(SoilSoftnessPropertyId, iterationData.SoilSoftness);
            erosionShader.SetFloat(SedimentCarryingCapacityPropertyId, iterationData.SedimentCarryingCapacity);
            erosionShader.SetFloat(ErosionRatePropertyId, iterationData.ErosionRate);
            erosionShader.SetFloat(DepositionRatePropertyId, iterationData.DepositionRate);
            erosionShader.SetFloat(EvaporationRatePropertyId, iterationData.EvaporationRate);
            erosionShader.SetFloat(MinSlopePropertyId, iterationData.MinSlope);
            erosionShader.SetFloat(Shader.PropertyToID("blurCenterModifier"), _gaussianBlurDatabase.GPUGaussianBlurVo.CenterModifier);
            erosionShader.SetFloat(Shader.PropertyToID("blurAdjacentModifier"), _gaussianBlurDatabase.GPUGaussianBlurVo.AdjacentModifier);
            erosionShader.SetFloat(Shader.PropertyToID("blurDiagonalModifier"), _gaussianBlurDatabase.GPUGaussianBlurVo.DiagonalModifier);
            
            inVertexStatesBuffer.SetData(verticesStates);
            outVertexStatesBuffer.SetData(verticesStates);
            erosionShader.SetBuffer(0, InVertexStatesPropertyId, inVertexStatesBuffer);
            erosionShader.SetBuffer(0, OutVertexStatesPropertyId, outVertexStatesBuffer);
            erosionShader.SetBuffer(1, InVertexStatesPropertyId, inVertexStatesBuffer);
            erosionShader.SetBuffer(1, OutVertexStatesPropertyId, outVertexStatesBuffer);
            erosionShader.SetBuffer(2, InVertexStatesPropertyId, inVertexStatesBuffer);
            erosionShader.SetBuffer(2, OutVertexStatesPropertyId, outVertexStatesBuffer);

            var threadGroups = 16;
            
            for (var l = 0; l < iterationData.IterationsCount; ++l)
            {
                if (l % 20 == 0)
                {
                    inVertexStatesBuffer.GetData(verticesStates);
                    
                    for (var i = 0; i < meshDataVo.Resolution; ++i)
                    for (var j = 0; j < meshDataVo.Resolution; ++j)
                        verticesStates[i * meshDataVo.Resolution + j].water = Random.Range(0, 5f);
                    
                    inVertexStatesBuffer.SetData(verticesStates);
                }

                for (var k = 0; k < 20; ++k)
                {
                    erosionShader.Dispatch(0, meshDataVo.Resolution / threadGroups, meshDataVo.Resolution / threadGroups, 1);
                    erosionShader.Dispatch(1, meshDataVo.Resolution / threadGroups, meshDataVo.Resolution / threadGroups, 1);
                }

                if (l % 4 == 0)
                    erosionShader.Dispatch(2, meshDataVo.Resolution / threadGroups, meshDataVo.Resolution / threadGroups, 1);
            }
            
            
            outVertexStatesBuffer.GetData(verticesStates);
            inVertexStatesBuffer.Release();
            outVertexStatesBuffer.Release();
            
            for (var i = 0; i < meshDataVo.Resolution; ++i)
            for (var j = 0; j < meshDataVo.Resolution; ++j)
            {
                meshDataVo.Vertices[i][j].y = verticesStates[i * meshDataVo.Resolution + j].height;
            }
        }
    }
}