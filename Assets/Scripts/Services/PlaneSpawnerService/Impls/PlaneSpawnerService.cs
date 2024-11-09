using Databases.CommonShadersDatabase;
using Databases.HeightTextureDrawer;
using MonoBehavior;
using UnityEngine;
using Zenject;

namespace Services.PlaneSpawnerService.Impls
{
    public class PlaneSpawnerService : IPlaneSpawnerService, IInitializable
    {
        private readonly IHeightTextureDrawerStyleDatabase _heightTextureDrawerStyleDatabase;
        private readonly ICommonShadersDatabase _commonShadersDatabase;

        public PlaneSpawnerService(
            IHeightTextureDrawerStyleDatabase heightTextureDrawerStyleDatabase,
            ICommonShadersDatabase commonShadersDatabase)
        {
            _heightTextureDrawerStyleDatabase = heightTextureDrawerStyleDatabase;
            _commonShadersDatabase = commonShadersDatabase;
        }

        public void Initialize()
        {
            SpawnPlane();
        }

        private void SpawnPlane()
        {
            
        }
    }
}