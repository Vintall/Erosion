using Services.GausianBlur.Impls;
using Services.GPUHydraulicErosionService.Impls;
using Services.HeightTextureDrawer.Impls;
using Services.MainInterfaceController.Impls;
using Services.MeshDataGeneratorService.Impls;
using Services.NoiseGeneration.Impls;
using Services.PlaneGeneration.Impls;
using Strategies.HydraulicErosion;
using Strategies.HydraulicErosion.Impls;
using Zenject;

namespace Installers.MainScene
{
    public class MainSceneInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            BindHydraulicErosionStrategies();
            BindServices();
        }

        private void BindServices()
        {
            BindService<MeshDataGeneratorService>();
            BindService<GaussianBlurService>();
            BindService<PerlinNoiseGeneratorService>();
            BindService<HeightTextureDrawer>();
            BindService<HydraulicErosionService>();
            BindService<MainInterfaceController>();
            BindService<TerrainChunkGeneratorService>();
        }

        private void BindService<T>() => 
            Container.BindInterfacesTo<T>().AsSingle();

        private void BindHydraulicErosionStrategies()
        {
            BindErosionStrategy<CPUGridBasedErosionStrategy>();
            BindErosionStrategy<CPUParticleBasedErosionStrategy>();
            BindErosionStrategy<GPUGridBasedErosionStrategy>();
            BindErosionStrategy<GPUParticleBasedErosionStrategy>();
            BindErosionStrategy<SnowballCPUErosionStrategy>();
        }

        private void BindErosionStrategy<T>() where T : IHydraulicErosionStrategy => 
            Container.Bind<IHydraulicErosionStrategy>().To<T>().AsCached();
    }
}