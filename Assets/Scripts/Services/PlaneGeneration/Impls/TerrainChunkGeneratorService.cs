using System;
using System.Collections.Generic;
using System.Diagnostics;
using Models;
using MonoBehavior;
using Pools.PlanePool;
using Services.GausianBlur;
using Services.NoiseGeneration;
using Services.HeightTextureDrawer;
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

        [SerializeField] private List<NoiseLayerVo> noiseLayers;
        
        [SerializeField] private bool useErrosion;
        [SerializeField] private bool applyGaussianBlur;
        
        [SerializeField] private float planeSize;
        [SerializeField] private int resolution;
        [SerializeField] private int iterationsCount;
        [SerializeField] private MeshDataGeneratorService.Impls.MeshDataGeneratorService _meshDataGeneratorService;
        [SerializeField] private Transform terrainChunksHolder;
        public TerrainChunk LastGenerated;

        public TerrainChunkGeneratorService(
            ITerrainChunkPool terrainChunkPool,
            INoiseGeneratorService noiseGeneratorService,
            IHeightTextureDrawer heightTextureDrawer,
            IGaussianBlurService gaussianBlurService,
            IErosionCellSimulator erosionCellSimulator)
        {
            _terrainChunkPool = terrainChunkPool;
            _noiseGeneratorService = noiseGeneratorService;
            _heightTextureDrawer = heightTextureDrawer;
            _gaussianBlurService = gaussianBlurService;
            _erosionCellSimulator = erosionCellSimulator;
        }
        
        public void ApplyPerlin(ref Vector3[][] grid)
        {
            var openSimplexNoise = new OpenSimplexNoise(3248);
            
            for (var z = 0; z < resolution; ++z)
            for (var x = 0; x < resolution; ++x)
            {
                var point = grid[z][x];

                double height = 0;
                for (var i = 0; i < noiseLayers.Count; ++i)
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

        public TerrainChunk GeneratePlane()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            
            var meshData = _meshDataGeneratorService.GenerateMesh(resolution, planeSize);
            
            ApplyPerlin(ref meshData.Vertices);

            var planeObject = _terrainChunkPool.Spawn(terrainChunksHolder);
            var newMesh = new Mesh();


            if (useErrosion)
            {
                planeObject.name += " Erroded";

                _erosionCellSimulator.SetupSimulator(meshData.Vertices);

                for (var i = 0; i < iterationsCount; ++i)
                {
                    var position = new Vector2(Random.Range(0f, resolution - 1), Random.Range(0f, resolution - 1));
                    _erosionCellSimulator.SimulateDroplet(position);
                }
            }

            if (applyGaussianBlur)
                gaussianBlurService.ApplyGaussianBlur(ref meshData.Vertices, meshData.Resolution);

            heightTextureDrawer.GenerateTexture(meshData.Vertices, resolution);
            
            var heightMapLinear = new Vector3[resolution * resolution];

            for (var z = 0; z < resolution; ++z)
            for (var x = 0; x < resolution; ++x)
                heightMapLinear[z * resolution + x] = meshData.Vertices[z][x];

            newMesh.vertices = heightMapLinear;
            newMesh.triangles = meshData.Triangles;
            newMesh.RecalculateNormals();
            newMesh.RecalculateTangents();
            newMesh.RecalculateBounds();
            newMesh.RecalculateUVDistributionMetrics();
            
            planeObject.MeshFilter.mesh = newMesh;
            planeObject.MeshDataVo = meshData;

            LastGenerated = planeObject;

            stopwatch.Stop();
            Debug.Log($"Time: {Math.Round(stopwatch.ElapsedMilliseconds / 1000f, 2)} sec");
            
            return planeObject;
        }
        
        public TerrainChunk GeneratePlane(TerrainChunk simulatableTerrainChunk)
        {
            if (!simulatableTerrainChunk)
                return null;
            
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            
            var meshData = simulatableTerrainChunk.MeshDataVo;
            var newMesh = simulatableTerrainChunk.MeshFilter.mesh;
            
            if (useErrosion)
            {
                _erosionCellSimulator.SetupSimulator(meshData.Vertices);

                for (var i = 0; i < iterationsCount; ++i)
                {
                    var position = new Vector2(Random.Range(0f, resolution - 1), Random.Range(0f, resolution - 1));
                    _erosionCellSimulator.SimulateDroplet(position);
                }
            }
			
			if (applyGaussianBlur)
                _gaussianBlurService.ApplyGaussianBlur(ref meshData.Vertices, meshData.Resolution);

            _heightTextureDrawer.GenerateTexture(meshData.Vertices, resolution);
            
            var heightMapLinear = new Vector3[resolution * resolution];

            for (var z = 0; z < resolution; ++z)
            for (var x = 0; x < resolution; ++x)
                heightMapLinear[z * resolution + x] = meshData.Vertices[z][x];

            newMesh.vertices = heightMapLinear;
            newMesh.triangles = meshData.Triangles;
            newMesh.RecalculateNormals();
            newMesh.RecalculateTangents();
            newMesh.RecalculateBounds();
            newMesh.RecalculateUVDistributionMetrics();
            
            simulatableTerrainChunk.MeshFilter.mesh = newMesh;

            stopwatch.Stop();
            Debug.Log($"Time: {Math.Round(stopwatch.ElapsedMilliseconds / 1000f, 2)} sec");
            
            return simulatableTerrainChunk;
        }
    }
}