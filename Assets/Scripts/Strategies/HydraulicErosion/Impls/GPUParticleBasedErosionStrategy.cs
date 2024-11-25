using System;
using Databases.CommonShadersDatabase;
using Databases.GaussianBlur;
using Enums;
using Models;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Strategies.HydraulicErosion.Impls
{
    public class GPUParticleBasedErosionStrategy : IHydraulicErosionStrategy
    {
        private readonly ICommonShadersDatabase _commonShadersDatabase;
        private readonly IGaussianBlurDatabase _gaussianBlurDatabase;
        private static readonly int InVerticesPropertyId = Shader.PropertyToID("inVertices");
        private static readonly int OutVertexStatesPropertyId = Shader.PropertyToID("outVertexStates");
        private static readonly int ErosionRatePropertyId = Shader.PropertyToID("erosionRate");
        private static readonly int DepositionRatePropertyId = Shader.PropertyToID("depositionRate");
        private static readonly int EvaporationRatePropertyId = Shader.PropertyToID("evaporationRate");
        private static readonly int MinSlopePropertyId = Shader.PropertyToID("minSlope");
        private static readonly int MapWidthPropertyId = Shader.PropertyToID("mapWidth");
        private static readonly int MapHeightPropertyId = Shader.PropertyToID("mapHeight");
        private static readonly int SoilSoftnessPropertyId = Shader.PropertyToID("soilSoftness");
        private static readonly int SedimentCarryingCapacityPropertyId = Shader.PropertyToID("sedimentCarryingCapacity");
        
        public EHydraulicErosionType HydraulicErosionType => EHydraulicErosionType.ParticlesGPU;

        public GPUParticleBasedErosionStrategy(
            ICommonShadersDatabase commonShadersDatabase,
            IGaussianBlurDatabase gaussianBlurDatabase)
        {
            _commonShadersDatabase = commonShadersDatabase;
            _gaussianBlurDatabase = gaussianBlurDatabase;
        }

        private struct PathPoint
        {
            public int vertexPosition;
            public float heightDifference;
            public int isActive;
        }
        
        public void Execute(HydraulicErosionIterationVo iterationData, 
            MeshDataVo meshDataVo, Action<int> iterationTimestamp)
        {
            var erosionShader = _commonShadersDatabase.ParticleBasedHydraulicErosionComputeShader;
            
            var inVertices = new Vector3[meshDataVo.Resolution * meshDataVo.Resolution];
            var pathPoints = new PathPoint[4 * meshDataVo.Resolution * meshDataVo.Resolution];
            
            for (var i = 0; i < meshDataVo.Resolution; ++i)
            for (var j = 0; j < meshDataVo.Resolution; ++j)
                inVertices[i * meshDataVo.Resolution + j] = meshDataVo.Vertices[i][j];
            
            var inVerticesBuffer = new ComputeBuffer(inVertices.Length, sizeof(float) * 3);
            var pathPointsBuffer = new ComputeBuffer(15 * meshDataVo.Resolution * meshDataVo.Resolution, sizeof(int) + sizeof(float) + sizeof(int));

            erosionShader.SetInt(Shader.PropertyToID("randomSeed"), Random.Range(1, 5000));
            erosionShader.SetInt(Shader.PropertyToID("pathPointsPerThread"), 4);
            erosionShader.SetInt(MapWidthPropertyId, meshDataVo.Resolution);
            erosionShader.SetInt(MapHeightPropertyId, meshDataVo.Resolution);
            erosionShader.SetFloat(DepositionRatePropertyId, iterationData.DepositionRate);
            erosionShader.SetFloat(EvaporationRatePropertyId, iterationData.EvaporationRate);
            erosionShader.SetFloat(Shader.PropertyToID("blurCenterModifier"), _gaussianBlurDatabase.GPUGaussianBlurVo.CenterModifier);
            erosionShader.SetFloat(Shader.PropertyToID("blurAdjacentModifier"), _gaussianBlurDatabase.GPUGaussianBlurVo.AdjacentModifier);
            erosionShader.SetFloat(Shader.PropertyToID("blurDiagonalModifier"), _gaussianBlurDatabase.GPUGaussianBlurVo.DiagonalModifier);
            
            inVerticesBuffer.SetData(inVertices);
            pathPointsBuffer.SetData(pathPoints);
            
            erosionShader.SetBuffer(0, InVerticesPropertyId, inVerticesBuffer);
            erosionShader.SetBuffer(1, InVerticesPropertyId, inVerticesBuffer);
            erosionShader.SetBuffer(2, InVerticesPropertyId, inVerticesBuffer);
            
            erosionShader.SetBuffer(0, Shader.PropertyToID("pathPoints"), pathPointsBuffer);
            erosionShader.SetBuffer(1, Shader.PropertyToID("pathPoints"), pathPointsBuffer);
            erosionShader.SetBuffer(2, Shader.PropertyToID("pathPoints"), pathPointsBuffer);

            for (var l = 0; l < iterationData.IterationsCount; ++l)
            {
                erosionShader.Dispatch(0, meshDataVo.Resolution / 8, meshDataVo.Resolution / 8, 1);
                
                pathPointsBuffer.GetData(pathPoints);

                var cellsPopularity = new int[meshDataVo.Resolution * meshDataVo.Resolution];

                for (var i = 0; i < pathPoints.Length; ++i)
                {
                    var vertexPosition = pathPoints[i].vertexPosition;
                    
                    ++cellsPopularity[vertexPosition];
                }

                for (var i = 0; i < pathPoints.Length; ++i)
                {
                    var vertexPosition = pathPoints[i].vertexPosition;
                    var vertex2dPosition = new Vector2Int(
                        vertexPosition / meshDataVo.Resolution,
                        vertexPosition % meshDataVo.Resolution);
                    
                    if(pathPoints[i].isActive == 0)
                        continue;
                    
                    if(vertex2dPosition.x == 0 || vertex2dPosition.y == 0 ||
                       vertex2dPosition.x == meshDataVo.Resolution - 1 ||
                       vertex2dPosition.y == meshDataVo.Resolution - 1)
                        continue;
                    
                    inVertices[vertexPosition].y += pathPoints[i].heightDifference / cellsPopularity[vertexPosition];
                }
                
                inVerticesBuffer.SetData(inVertices);
                
                //erosionShader.Dispatch(1, meshDataVo.Resolution / 8, meshDataVo.Resolution / 8, 1);
                erosionShader.Dispatch(2, meshDataVo.Resolution / 8, meshDataVo.Resolution / 8, 1);
            }
            
            inVerticesBuffer.GetData(inVertices);
            inVerticesBuffer.Release();
            pathPointsBuffer.Release();
            
            for (var i = 0; i < meshDataVo.Resolution; ++i)
            for (var j = 0; j < meshDataVo.Resolution; ++j)
            {
                meshDataVo.Vertices[i][j].y = inVertices[i * meshDataVo.Resolution + j].y;
            }
        }
    }
}