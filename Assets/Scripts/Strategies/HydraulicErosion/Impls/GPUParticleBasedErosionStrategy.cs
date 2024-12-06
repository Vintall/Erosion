using System;
using Databases.CommonShadersDatabase;
using Databases.GaussianBlur;
using Enums;
using Models;
using Unity.Collections;
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
            public float ULHeightDelta;
            public float UCHeightDelta;
            public float URHeightDelta;
            public float MLHeightDelta;
            public float MCHeightDelta;
            public float MRHeightDelta;
            public float BLHeightDelta;
            public float BCHeightDelta;
            public float BRHeightDelta;
            public int isActive;
        }
        
        public void Execute(HydraulicErosionIterationVo iterationData, 
            MeshDataVo meshDataVo, Action<int> iterationTimestamp)
        {
            var erosionShader = _commonShadersDatabase.ParticleBasedHydraulicErosionComputeShader;

            var pathPointsPerThread = 3;
            
            var inVertices = new Vector3[meshDataVo.Resolution * meshDataVo.Resolution];
            var pathPoints = new PathPoint[pathPointsPerThread * meshDataVo.Resolution * meshDataVo.Resolution];
            
            for (var i = 0; i < meshDataVo.Resolution; ++i)
            for (var j = 0; j < meshDataVo.Resolution; ++j)
                inVertices[i * meshDataVo.Resolution + j] = meshDataVo.Vertices[i][j];
            
            var inVerticesBuffer = new ComputeBuffer(inVertices.Length, sizeof(float) * 3);
            var pathPointsBuffer = new ComputeBuffer(pathPointsPerThread * meshDataVo.Resolution * meshDataVo.Resolution, sizeof(int) + sizeof(float) * 9 + sizeof(int));

            erosionShader.SetInt(Shader.PropertyToID("randomSeed"), Random.Range(1, 5000));
            erosionShader.SetInt(Shader.PropertyToID("pathPointsPerThread"), pathPointsPerThread);
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

            bool IsInBounds(Vector2Int pos)
            {
                return pos.x >= 0 &&
                       pos.y >= 0 &&
                       pos.x <= meshDataVo.Resolution - 1 &&
                       pos.y <= meshDataVo.Resolution - 1;
            }
            
            for (var l = 0; l < iterationData.IterationsCount; ++l)
            {
                erosionShader.Dispatch(0, meshDataVo.Resolution / 8, meshDataVo.Resolution / 8, 1);
                
                pathPointsBuffer.GetData(pathPoints);

                var gridCellsPopulatity = new int[meshDataVo.Resolution * meshDataVo.Resolution];
                
                for (var i = 0; i < pathPoints.Length; ++i) 
                    ++gridCellsPopulatity[pathPoints[i].vertexPosition];

                for (var i = 0; i < pathPoints.Length; ++i)
                {
                    var vertexPosition = pathPoints[i].vertexPosition;
                    var vertex2dPosition = new Vector2Int(
                        vertexPosition / meshDataVo.Resolution,
                        vertexPosition % meshDataVo.Resolution);

                    if (pathPoints[i].isActive == 0)
                        continue;
                    
                    Vector2Int neighborPos;
                    
                     neighborPos = vertex2dPosition + new Vector2Int(-1, -1);
                    
                     if (IsInBounds(neighborPos))
                         inVertices[neighborPos.y * meshDataVo.Resolution + neighborPos.x].y +=
                             pathPoints[i].ULHeightDelta / gridCellsPopulatity[pathPoints[i].vertexPosition];
                    
                     neighborPos = vertex2dPosition + new Vector2Int(0, -1);
                    
                     if (IsInBounds(neighborPos))
                         inVertices[neighborPos.y * meshDataVo.Resolution + neighborPos.x].y +=
                             pathPoints[i].UCHeightDelta / gridCellsPopulatity[pathPoints[i].vertexPosition];
                    
                     neighborPos = vertex2dPosition + new Vector2Int(1, -1);
                    
                     if (IsInBounds(neighborPos))
                         inVertices[neighborPos.y * meshDataVo.Resolution + neighborPos.x].y +=
                             pathPoints[i].URHeightDelta / gridCellsPopulatity[pathPoints[i].vertexPosition];
                    
                     neighborPos = vertex2dPosition + new Vector2Int(-1, 0);
                    
                     if (IsInBounds(neighborPos))
                         inVertices[neighborPos.y * meshDataVo.Resolution + neighborPos.x].y +=
                             pathPoints[i].MLHeightDelta / gridCellsPopulatity[pathPoints[i].vertexPosition];
                    
                    neighborPos = vertex2dPosition;

                    if (IsInBounds(neighborPos))
                        inVertices[neighborPos.y * meshDataVo.Resolution + neighborPos.x].y +=
                            pathPoints[i].MCHeightDelta / gridCellsPopulatity[pathPoints[i].vertexPosition];
                    
                    neighborPos = vertex2dPosition + new Vector2Int(1, 0);
                    
                    if (IsInBounds(neighborPos))
                        inVertices[neighborPos.y * meshDataVo.Resolution + neighborPos.x].y +=
                            pathPoints[i].MRHeightDelta / gridCellsPopulatity[pathPoints[i].vertexPosition];
                    
                    neighborPos = vertex2dPosition + new Vector2Int(-1, 1);
                    
                    if (IsInBounds(neighborPos))
                        inVertices[neighborPos.y * meshDataVo.Resolution + neighborPos.x].y +=
                            pathPoints[i].BLHeightDelta / gridCellsPopulatity[pathPoints[i].vertexPosition];
                    
                    neighborPos = vertex2dPosition + new Vector2Int(0, 1);
                    
                    if (IsInBounds(neighborPos))
                        inVertices[neighborPos.y * meshDataVo.Resolution + neighborPos.x].y +=
                            pathPoints[i].BCHeightDelta / gridCellsPopulatity[pathPoints[i].vertexPosition];
                    
                    neighborPos = vertex2dPosition + new Vector2Int(1, 1);
                    
                    if (IsInBounds(neighborPos))
                        inVertices[neighborPos.y * meshDataVo.Resolution + neighborPos.x].y +=
                            pathPoints[i].BRHeightDelta / gridCellsPopulatity[pathPoints[i].vertexPosition];
                }

                inVertices[0].y = inVertices[1].y;

                for (var x = 0; x < meshDataVo.Resolution; ++x)
                for (var y = 0; y < meshDataVo.Resolution; ++y)
                {
                    var linearPosition = y * meshDataVo.Resolution + x;
                    
                    if (x == 0 && y == 0)
                    {
                        inVertices[linearPosition] = inVertices[(y + 1) * meshDataVo.Resolution + x + 1];
                        continue;
                    }
                    
                    if (x == 0 && y == meshDataVo.Resolution - 1)
                    {
                        inVertices[linearPosition] = inVertices[(y - 1) * meshDataVo.Resolution + x + 1];
                        continue;
                    }
                        
                    if (x == meshDataVo.Resolution - 1 && y == 0)
                    {
                        inVertices[linearPosition] = inVertices[(y + 1) * meshDataVo.Resolution + x - 1];
                        continue;
                    }
                    
                    if (x == meshDataVo.Resolution - 1 && y == meshDataVo.Resolution - 1)
                    {
                        inVertices[linearPosition] = inVertices[(y - 1) * meshDataVo.Resolution + x - 1];
                        continue;
                    }

                    if (x == 0)
                    {
                        inVertices[linearPosition] = inVertices[y * meshDataVo.Resolution + x + 1];
                        continue;
                    }

                    if (y == 0)
                    {
                        inVertices[linearPosition] = inVertices[(y + 1) * meshDataVo.Resolution + x];
                        continue;
                    }
                    
                    if (x == meshDataVo.Resolution - 1)
                    {
                        inVertices[linearPosition] = inVertices[y * meshDataVo.Resolution + x - 1];
                        continue;
                    }

                    if (y == meshDataVo.Resolution - 1)
                    {
                        inVertices[linearPosition] = inVertices[(y - 1) * meshDataVo.Resolution + x];
                        continue;
                    }
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