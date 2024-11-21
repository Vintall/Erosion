using System.Diagnostics;
using System.Timers;
using MonoBehavior;
using Pools.PlanePool;
using Services.GausianBlur;
using Services.GPUHydraulicErosionService;
using Services.HeightTextureDrawer;
using Services.NoiseGeneration;
using Services.PlaneGeneration;
using Services.TestInterfaceController;
using UnityEditor;
using Zenject;
using Debug = UnityEngine.Debug;

namespace Services.MainInterfaceController.Impls
{
    public class MainInterfaceController : IMainInterfaceController, IInitializable
    {
        private readonly MainInterfaceView _view;
        private readonly IHydraulicErosionService _hydraulicErosionService;
        private readonly INoiseGeneratorService _noiseGeneratorService;
        private readonly ITerrainChunkPool _terrainChunkPool;
        private readonly ITerrainChunkGeneratorService _terrainChunkGeneratorService;
        private readonly IGaussianBlurService _gaussianBlurService;
        private readonly IHeightTextureDrawer _heightTextureDrawer;

        private TerrainChunk _currentTerrainChunk;
        
        public MainInterfaceController(
            MainInterfaceView mainInterfaceView,
            IHydraulicErosionService hydraulicErosionService,
            INoiseGeneratorService noiseGeneratorService,
            ITerrainChunkPool terrainChunkPool,
            ITerrainChunkGeneratorService terrainChunkGeneratorService,
            IGaussianBlurService gaussianBlurService,
            IHeightTextureDrawer heightTextureDrawer)
        {
            _view = mainInterfaceView;
            _hydraulicErosionService = hydraulicErosionService;
            _noiseGeneratorService = noiseGeneratorService;
            _terrainChunkPool = terrainChunkPool;
            _terrainChunkGeneratorService = terrainChunkGeneratorService;
            _gaussianBlurService = gaussianBlurService;
            _heightTextureDrawer = heightTextureDrawer;
        }

        public void Initialize()
        {
            _view.OnSimulateButtonPress += OnSimulateButtonPress;
            _view.OnSimulateButtonPress += GenerateMesh;
            
            _view.OnResetButtonPress += OnResetButtonPress;
            _view.OnSampleToPNGPress += OnSampleToPNGPress;
            _view.OnOpenPNGFolderPress += OnOpenPNGFolderPress;
            
            _view.OnApplyGaussianBlurPress += OnApplyGaussianBlurPress;
            _view.OnApplyGaussianBlurPress += GenerateMesh;

            Selection.activeGameObject = _view.gameObject;
            
            OnResetButtonPress();
        }

        private void OnSimulateButtonPress()
        {
            Stopwatch stopwatch = new Stopwatch();
            
            stopwatch.Start();
            
            _hydraulicErosionService.SimulateErosion(
                _view.HydraulicErosionIterationVo,
                _currentTerrainChunk.MeshData,
                _view.HydraulicErosionType,
                iteration => 
                    UpdateProgressBar(iteration, _view.HydraulicErosionIterationVo.IterationsCount));
            
            stopwatch.Stop();

            Debug.Log($"Execution time ms: {stopwatch.ElapsedMilliseconds}ms\n" +
                      $"Execution time s: {stopwatch.ElapsedMilliseconds / 1000f}s\n" +
                      $"Iterations count: {_view.HydraulicErosionIterationVo.IterationsCount}\n" +
                      $"Mode: {_view.HydraulicErosionType}\n" +
                      $"Iterations per second: {_view.HydraulicErosionIterationVo.IterationsCount / (stopwatch.ElapsedMilliseconds / 1000f)}\n" +
                      $"Iterations per millisecond: {_view.HydraulicErosionIterationVo.IterationsCount / (stopwatch.ElapsedMilliseconds)}");
            
            if(_view.ApplyBlurAutomaticly)
                OnApplyGaussianBlurPress();

            var previewTexture = _heightTextureDrawer.GetTexture(
                _currentTerrainChunk.MeshData.Vertices,
                _currentTerrainChunk.MeshData.Resolution);
            
            _view.UpdatePreviewTexture(previewTexture);
        }

        private void UpdateProgressBar(int currentIteration, int iterationsCount)
        {
            _view.SetupProgressBar(true, currentIteration, iterationsCount);
        }

        private void OnResetButtonPress()
        {
            if (_currentTerrainChunk != null)
            {
                _terrainChunkPool.Despawn(_currentTerrainChunk);
                _currentTerrainChunk = null;
            }

            _currentTerrainChunk =
                _terrainChunkGeneratorService.GenerateTerrainChunk(_view.TerrainResolution, _view.TerrainSize);
            _view.UpdatePreviewTexture(_heightTextureDrawer.GetTexture(
                _currentTerrainChunk.MeshData.Vertices,
                _currentTerrainChunk.MeshData.Resolution));
        }

        private void OnSampleToPNGPress()
        {
            if(_currentTerrainChunk == null)
                return;
            
            _heightTextureDrawer.GenerateTexture(_currentTerrainChunk.MeshData.Vertices,
                _currentTerrainChunk.MeshData.Resolution);
        }

        private void OnOpenPNGFolderPress()
        {
            Process.Start("explorer.exe", "C:\\Users\\Vintall\\Desktop\\Maps\\");
        }

        private void OnApplyGaussianBlurPress()
        {
            if(_currentTerrainChunk == null)
                return;
            
            _gaussianBlurService.ApplyGaussianBlur(
                ref _currentTerrainChunk.MeshData.Vertices, 
                _currentTerrainChunk.MeshData.Resolution);
            
            var previewTexture = _heightTextureDrawer.GetTexture(
                _currentTerrainChunk.MeshData.Vertices,
                _currentTerrainChunk.MeshData.Resolution);
            
            _view.UpdatePreviewTexture(previewTexture);
        }

        private void GenerateMesh()
        {
            _currentTerrainChunk.MeshFilter.mesh =
                _terrainChunkGeneratorService.GenerateMeshFromMeshData(_currentTerrainChunk.MeshData);
        }
    }
}