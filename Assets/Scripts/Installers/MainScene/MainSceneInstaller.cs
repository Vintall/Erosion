using Services.GausianBlur.Impls;
using Services.GPUHydraulicErosionService.Impls;
using Services.HeightTextureDrawer.Impls;
using Services.MeshDataGeneratorService.Impls;
using Services.NoiseGeneration.Impls;
using Services.PlaneGeneration.Impls;
using Services.PlaneSpawnerService.Impls;
using Services.TestInterfaceController.Impls;
using Zenject;

namespace Installers.MainScene
{
    public class MainSceneInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            BindServices();
        }

        private void BindServices()
        {
            BindService<MeshDataGeneratorService>();
            BindService<GaussianBlurService>();
            BindService<PerlinNoiseGeneratorService>();
            BindService<HeightTextureDrawer>();
            BindService<PlaneSpawnerService>();
            BindService<GPUHydraulicErosionService>();
            BindService<TestInterfaceController>();
            BindService<TerrainChunkGeneratorService>();
        }

        private void BindService<T>()
        {
            Container.BindInterfacesTo<T>().AsSingle();
        }
    }
}