using MonoBehavior;
using Pools.PlanePool;
using Pools.PlanePool.Impls;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace Installers.MainScene
{
    [CreateAssetMenu(menuName = "Installers/MainScene/PrefabsInstaller", fileName = "MainScenePrefabsInstaller")]
    public class MainScenePrefabsInstaller : ScriptableObjectInstaller<MainScenePrefabsInstaller>
    {
        [SerializeField] private TestInterfaceView testInterfaceView;
        [SerializeField] private TerrainChunk terrainChunk;
        
        private Transform _parent;
        
        public override void InstallBindings()
        {
            _parent = new GameObject("World").transform;
            BindPrefab(testInterfaceView, null);
            BindPool<TerrainChunk, TerrainChunkPool, ITerrainChunkPool>(terrainChunk, 1);
        }
        
        private void BindPrefab<TContent>(TContent prefab, Transform parent)
            where TContent : Object
        {
            Container.BindInterfacesAndSelfTo<TContent>()
                .FromComponentInNewPrefab(prefab)
#if UNITY_EDITOR
                .UnderTransform(parent)
#endif
                .AsSingle()
                .OnInstantiated((_, o) => ((MonoBehaviour) o).gameObject.SetActive(true))
                .NonLazy();
        }
        
        private void BindPool<TItemContract, TPoolConcrete, TPoolContract>(TItemContract prefab, int size = 1)
            where TItemContract : MonoBehaviour
            where TPoolConcrete : TPoolContract, IMemoryPool
            where TPoolContract : IMemoryPool
        {
            var poolContainerName = "[MemoryPool] " + prefab.name;
            Container.BindMemoryPoolCustomInterface<TItemContract, TPoolConcrete, TPoolContract>()
                .WithInitialSize(size)
                .FromComponentInNewPrefab(prefab)
#if UNITY_EDITOR
                .UnderTransformGroup(poolContainerName)
#endif
                .AsCached();
        }
    }
}