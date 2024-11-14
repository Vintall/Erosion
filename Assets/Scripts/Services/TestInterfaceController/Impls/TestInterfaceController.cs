using System.Runtime.InteropServices;
using Extensions;
using Models;
using MonoBehavior;
using Pools.PlanePool;
using Services.GPUHydraulicErosionService;
using Services.NoiseGeneration;
using Services.PlaneGeneration;
using Services.PlaneSpawnerService;
using UnityEditor;
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

            Selection.activeGameObject = _view.gameObject;
            
            OnResetButtonPress();
        }

        private void OnSimulateButtonPress()
        {
            for (var i = 0; i < _view.Iteration; ++i)
            {
                 _gpuHydraulicErosionService.SimulateErosionIteration(
                     new HydraulicErosionIterationVo
                     {
                         DepositionRate = _view.HydraulicErosionIterationVo.DepositionRate,
                         ErosionRate = _view.HydraulicErosionIterationVo.ErosionRate,
                         EvaporationRate = _view.HydraulicErosionIterationVo.EvaporationRate,
                         MinSlope = _view.HydraulicErosionIterationVo.MinSlope,
                         SedimentCarryingCapacity = _view.HydraulicErosionIterationVo.SedimentCarryingCapacity,
                         SoilSoftness = _view.HydraulicErosionIterationVo.SoilSoftness
                     },
                     _currentTerrainChunk.MeshData);
            }

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
        }
    }
}