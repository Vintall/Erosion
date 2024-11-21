using Databases.CommonShadersDatabase;
using Databases.CommonShadersDatabase.Impls;
using Databases.GaussianBlur;
using Databases.GaussianBlur.Impls;
using Databases.HeightTextureDrawer;
using Databases.HeightTextureDrawer.Impls;
using UnityEngine;
using Zenject;

namespace Installers.MainScene
{
    [CreateAssetMenu(menuName = "Installers/MainScene/DatabasesInstaller", fileName = "MainSceneDatabasesInstaller")]
    public class MainSceneDatabasesInstaller : ScriptableObjectInstaller<MainSceneDatabasesInstaller>
    {
        [SerializeField] private HeightTextureDrawerStyleDatabase heightTextureDrawerStyleDatabase;
        [SerializeField] private CommonShadersDatabase commonShadersDatabase;
        [SerializeField] private GaussianBlurDatabase gaussianBlurDatabase;
        
        public override void InstallBindings()
        {
            BindDatabase<IHeightTextureDrawerStyleDatabase>(heightTextureDrawerStyleDatabase);
            BindDatabase<ICommonShadersDatabase>(commonShadersDatabase);
            BindDatabase<IGaussianBlurDatabase>(gaussianBlurDatabase);
        }

        private void BindDatabase<T1>(T1 instance)
        {
            Container.Bind<T1>().FromInstance(instance).AsSingle();
        }
    }
}