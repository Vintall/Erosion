using Models;
using MonoBehavior;
using Pools.PlanePool;
using Services.GPUHydraulicErosionService;
using Services.NoiseGeneration;
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

        public TestInterfaceController(
            TestInterfaceView testInterfaceView,
            IGPUHydraulicErosionService gpuHydraulicErosionService,
            INoiseGeneratorService noiseGeneratorService,
            IPlaneSpawnerService planeSpawnerService,
            ITerrainChunkPool terrainChunkPool)
        {
            _view = testInterfaceView;
            _gpuHydraulicErosionService = gpuHydraulicErosionService;
            _noiseGeneratorService = noiseGeneratorService;
            _planeSpawnerService = planeSpawnerService;
            _terrainChunkPool = terrainChunkPool;
        }

        public void Initialize()
        {
            _view.OnGeneratePress += OnGeneratePress;
            
            heightMap = new RenderTexture(256, 256, GraphicsFormat.R16_SFloat, GraphicsFormat.None);
            waterMap = new RenderTexture(256, 256, GraphicsFormat.R16_SFloat, GraphicsFormat.None);
            
            //_planeSpawnerService.
            
        }

        private RenderTexture heightMap;
        private RenderTexture waterMap;
        private void OnGeneratePress()
        {
            Debug.Log("OnGeneratePress");

            return;
                
            _gpuHydraulicErosionService.SimulateErosionIteration(
                new HydraulicErosionIterationVo()
                {
                    DeltaTime = 1,
                    DepositionRate = 0.97f,
                    ErosionRate = 0.03f,
                    EvaporationRate = 0.01f,
                    MinSlope = -4f
                },
                heightMap, waterMap);
        }
    }
}