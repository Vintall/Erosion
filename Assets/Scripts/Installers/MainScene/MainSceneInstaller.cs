using Services.GausianBlur.Impls;
using Services.GPUHydraulicErosionService.Impls;
using Services.HeightTextureDrawer.Impls;
using Services.MeshDataGeneratorService.Impls;
using Services.NoiseGeneration.Impls;
using Services.PlaneGeneration.Impls;
using Services.PlaneSpawnerService.Impls;
using Services.TestInterfaceController.Impls;
using Strategies.HydraulicErosion;
using Strategies.HydraulicErosion.Impls;
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
            BindHydraulicErosionStrategies();
            
            BindService<MeshDataGeneratorService>();
            BindService<GaussianBlurService>();
            BindService<PerlinNoiseGeneratorService>();
            BindService<HeightTextureDrawer>();
            BindService<PlaneSpawnerService>();
            BindService<HydraulicErosionService>();
            BindService<TestInterfaceController>();
            BindService<TerrainChunkGeneratorService>();
            BindService<ErosionCellSimulator>();
        }

        private void BindService<T>()
        {
            Container.BindInterfacesTo<T>().AsSingle();
        }

        private void BindHydraulicErosionStrategies()
        {
            Container.Bind<IHydraulicErosionStrategy>().To<CPUGridBasedErosionStrategy>().AsCached();
            Container.Bind<IHydraulicErosionStrategy>().To<CPUParticleBasedErosionStrategy>().AsCached();
            Container.Bind<IHydraulicErosionStrategy>().To<GPUGridBasedErosionStrategy>().AsCached();
            Container.Bind<IHydraulicErosionStrategy>().To<GPUParticleBasedErosionStrategy>().AsCached();
        }
    }
}