using System;
using System.Collections.Generic;
using System.Diagnostics;
using Models;
using MonoBehavior;
using Pools.PlanePool;
using Services.GausianBlur;
using Services.NoiseGeneration;
using Services.HeightTextureDrawer;
using Services.MeshDataGeneratorService;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Services.PlaneGeneration.Impls
{
    public class TerrainChunkGeneratorService : ITerrainChunkGeneratorService
    {
        private readonly ITerrainChunkPool _terrainChunkPool;
        private readonly IMeshDataGeneratorService _meshDataGeneratorService;

        public TerrainChunkGeneratorService(
            ITerrainChunkPool terrainChunkPool,
            IMeshDataGeneratorService meshDataGeneratorService)
        {
            _terrainChunkPool = terrainChunkPool;
            _meshDataGeneratorService = meshDataGeneratorService;
        }
        
        public TerrainChunk GenerateTerrainChunk(int resolution, float size)
        {
            var meshData = _meshDataGeneratorService.GenerateMeshData(resolution, size);
            var terrainChunk = _terrainChunkPool.Spawn(null);
            
            ApplyPerlin(ref meshData.Vertices, meshData.Resolution, new NoiseLayerVo[]
            {
                new NoiseLayerVo(Vector2.one * 5, Vector2.zero, 5f),
                new NoiseLayerVo(Vector2.one * 2, Vector2.zero, 3f)
            });
            
            var newMesh = GenerateMeshFromMeshData(meshData);
            
            terrainChunk.MeshData = meshData;
            terrainChunk.MeshFilter.mesh = newMesh;
            
            return terrainChunk;
        }

        public Mesh GenerateMeshFromMeshData(MeshDataVo meshData)
        {
            var resultMesh = new Mesh();
            var heightMapLinear = new Vector3[meshData.Resolution * meshData.Resolution];

            for (var z = 0; z < meshData.Resolution; ++z)
            for (var x = 0; x < meshData.Resolution; ++x)
                heightMapLinear[z * meshData.Resolution + x] = meshData.Vertices[z][x];
            
            resultMesh.vertices = heightMapLinear;
            resultMesh.triangles = meshData.Triangles;
            resultMesh.RecalculateNormals();
            resultMesh.RecalculateTangents();
            resultMesh.RecalculateBounds();
            resultMesh.RecalculateUVDistributionMetrics();

            return resultMesh;
        }
        
        public void ApplyPerlin(ref Vector3[][] grid, int resolution, NoiseLayerVo[] noiseLayers)
        {
            var openSimplexNoise = new OpenSimplexNoise(3248);
            
            for (var z = 0; z < resolution; ++z)
            for (var x = 0; x < resolution; ++x)
            {
                var point = grid[z][x];

                double height = 0;
                for (var i = 0; i < noiseLayers.Length; ++i)
                {
                    var noiseLayer = noiseLayers[i];
                    height += openSimplexNoise.Evaluate((point.x + noiseLayer.Displacement.x) / noiseLayer.Scale.x,
                                  (point.z + noiseLayer.Displacement.y) / noiseLayer.Scale.y) *
                              noiseLayer.Influence;
                }
                point.y = (float)height;
                grid[z][x] = point;
            }
        }
    }
}