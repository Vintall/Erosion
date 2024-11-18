using Enums;
using Models;
using MonoBehavior;
using Pools.PlanePool;
using Services.GausianBlur;
using Services.GPUHydraulicErosionService;
using Services.HeightTextureDrawer;
using Services.NoiseGeneration;
using Services.PlaneGeneration;
using Services.PlaneSpawnerService;
using UnityEditor;
using UnityEngine;
using Zenject;

namespace Services.TestInterfaceController.Impls
{
    public class TestInterfaceController : ITestInterfaceController, IInitializable
    {
        private readonly TestInterfaceView _view;
        private readonly IHydraulicErosionService _hydraulicErosionService;
        private readonly INoiseGeneratorService _noiseGeneratorService;
        private readonly IPlaneSpawnerService _planeSpawnerService;
        private readonly ITerrainChunkPool _terrainChunkPool;
        private readonly ITerrainChunkGeneratorService _terrainChunkGeneratorService;
        private readonly IGaussianBlurService _gaussianBlurService;
        private readonly IHeightTextureDrawer _heightTextureDrawer;

        private TerrainChunk _currentTerrainChunk;
        
        public TestInterfaceController(
            TestInterfaceView testInterfaceView,
            IHydraulicErosionService hydraulicErosionService,
            INoiseGeneratorService noiseGeneratorService,
            IPlaneSpawnerService planeSpawnerService,
            ITerrainChunkPool terrainChunkPool,
            ITerrainChunkGeneratorService terrainChunkGeneratorService,
            IGaussianBlurService gaussianBlurService,
            IHeightTextureDrawer heightTextureDrawer)
        {
            _view = testInterfaceView;
            _hydraulicErosionService = hydraulicErosionService;
            _noiseGeneratorService = noiseGeneratorService;
            _planeSpawnerService = planeSpawnerService;
            _terrainChunkPool = terrainChunkPool;
            _terrainChunkGeneratorService = terrainChunkGeneratorService;
            _gaussianBlurService = gaussianBlurService;
            _heightTextureDrawer = heightTextureDrawer;
        }

        public void Initialize()
        {
            _view.OnSimulateButtonPress += OnSimulateButtonPress;
            _view.OnResetButtonPress += OnResetButtonPress;
            _view.OnSampleToPNGPress += OnSampleToPNGPress;
            _view.OnApplyGaussianBlurPress += OnApplyGaussianBlurPress;

            Selection.activeGameObject = _view.gameObject;
            
            OnResetButtonPress();
        }

        private void OnSimulateButtonPress()
        {
            _hydraulicErosionService.SimulateErosion(
                _view.HydraulicErosionIterationVo,
                _currentTerrainChunk.MeshData,
                _view.HydraulicErosionType);

            if(_view.ApplyGaussianBlurAfterIterationsBlock)
                OnApplyGaussianBlurPress();
            
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

        private void OnSampleToPNGPress()
        {
            if(_currentTerrainChunk == null)
                return;
            
            _heightTextureDrawer.GenerateTexture(_currentTerrainChunk.MeshData.Vertices,
                _currentTerrainChunk.MeshData.Resolution);
        }

        private void OnApplyGaussianBlurPress()
        {
            if(_currentTerrainChunk == null)
                return;
            
            _gaussianBlurService.ApplyGaussianBlur(
                ref _currentTerrainChunk.MeshData.Vertices, 
                _currentTerrainChunk.MeshData.Resolution);
        }
    }
}