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
        private readonly INoiseGeneratorService _noiseGeneratorService;
        private readonly IHeightTextureDrawer _heightTextureDrawer;
        private readonly IGaussianBlurService _gaussianBlurService;
        private readonly IErosionCellSimulator _erosionCellSimulator;
        private readonly IMeshDataGeneratorService _meshDataGeneratorService;

        [SerializeField] private List<NoiseLayerVo> noiseLayers;
        
        [SerializeField] private bool useErrosion;
        [SerializeField] private bool applyGaussianBlur;
        [SerializeField] private int iterationsCount;

        public TerrainChunkGeneratorService(
            ITerrainChunkPool terrainChunkPool,
            INoiseGeneratorService noiseGeneratorService,
            IHeightTextureDrawer heightTextureDrawer,
            IGaussianBlurService gaussianBlurService,
            IErosionCellSimulator erosionCellSimulator,
            IMeshDataGeneratorService meshDataGeneratorService)
        {
            _terrainChunkPool = terrainChunkPool;
            _noiseGeneratorService = noiseGeneratorService;
            _heightTextureDrawer = heightTextureDrawer;
            _gaussianBlurService = gaussianBlurService;
            _erosionCellSimulator = erosionCellSimulator;
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
            //if (useErrosion)
            //{
            //    planeObject.name += " Erroded";

                //_erosionCellSimulator.SetupSimulator(meshData.Vertices);

            //    for (var i = 0; i < iterationsCount; ++i)
            //    {
            //        var position = new Vector2(Random.Range(0f, resolution - 1), Random.Range(0f, resolution - 1));
                    //_erosionCellSimulator.SimulateDroplet(position);
            //    }
            //}

            //if (applyGaussianBlur)
            //    _gaussianBlurService.ApplyGaussianBlur(ref meshData.Vertices, meshData.Resolution);

            //_heightTextureDrawer.GenerateTexture(meshData.Vertices, resolution);
            
            

            
            //
            //planeObject.MeshFilter.mesh = newMesh;

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