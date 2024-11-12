using System.Runtime.InteropServices;
using Extensions;
using Models;
using MonoBehavior;
using Pools.PlanePool;
using Services.GPUHydraulicErosionService;
using Services.NoiseGeneration;
using Services.PlaneGeneration;
using Services.PlaneSpawnerService;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using Zenject;

namespace Services.TestInterfaceController.Impls
{
    public class TestInterfaceController : ITestInterfaceController, IInitializable
    {
        private readonly TestInterfaceView _view;
        private readonly IGPUHydraulicErosionService _gpuHydraulicErosionService;
        private readonly INoiseGeneratorService _noiseGeneratorService;
        private readonly IPlaneSpawnerService _planeSpawnerService;
        private readonly ITerrainChunkPool _terrainChunkPool;
        private readonly ITerrainChunkGeneratorService _terrainChunkGeneratorService;
        private readonly IErosionCellSimulator _erosionCellSimulator;

        private TerrainChunk _currentTerrainChunk;
        
        public TestInterfaceController(
            TestInterfaceView testInterfaceView,
            IGPUHydraulicErosionService gpuHydraulicErosionService,
            INoiseGeneratorService noiseGeneratorService,
            IPlaneSpawnerService planeSpawnerService,
            ITerrainChunkPool terrainChunkPool,
            ITerrainChunkGeneratorService terrainChunkGeneratorService,
            IErosionCellSimulator erosionCellSimulator)
        {
            _view = testInterfaceView;
            _gpuHydraulicErosionService = gpuHydraulicErosionService;
            _noiseGeneratorService = noiseGeneratorService;
            _planeSpawnerService = planeSpawnerService;
            _terrainChunkPool = terrainChunkPool;
            _terrainChunkGeneratorService = terrainChunkGeneratorService;
            _erosionCellSimulator = erosionCellSimulator;
        }

        public void Initialize()
        {
            _view.OnSimulateButtonPress += OnSimulateButtonPress;
            _view.OnResetButtonPress += OnResetButtonPress;
            
            OnResetButtonPress();
        }

        private void OnSimulateButtonPress()
        {
            _erosionCellSimulator.SetupSimulator(_currentTerrainChunk.MeshData.Vertices);
            
            
            for (var i = 0; i < _view.Iteration; ++i)
            {
                
                // _gpuHydraulicErosionService.SimulateErosionIteration(
                //     new HydraulicErosionIterationVo()
                //     {
                //         DeltaTime = 1,
                //         DepositionRate = 0.97f,
                //         ErosionRate = 0.03f,
                //         EvaporationRate = 0.01f,
                //         MinSlope = -4f
                //     },
                //     _currentTerrainChunk.MeshData);
                _erosionCellSimulator.SimulateDroplet(new Vector2(Random.Range(1, 255), Random.Range(1, 255)));
            }

            _currentTerrainChunk.MeshData.Vertices = _erosionCellSimulator.HeightMap;
            
            _currentTerrainChunk.MeshFilter.mesh = 
                _terrainChunkGeneratorService.GenerateMeshFromMeshData(_currentTerrainChunk.MeshData);
            
        }

        private void OnResetButtonPress()
        {
            if (_currentTerrainChunk != null)
            {
                _terrainChunkPool.Despawn(_currentTerrainChunk);
                _currentTerrainChunk = null;
            }

            _currentTerrainChunk = _terrainChunkGeneratorService.GenerateTerrainChunk(256, 10);
            //_noiseGeneratorService.GeneratePoint()
        }
    }
}