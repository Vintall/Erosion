using MonoBehavior;
using Pools.Impls;
using UnityEngine;

namespace Pools.PlanePool.Impls
{
    public class TerrainChunkPool : GameObjectPool<TerrainChunk>, ITerrainChunkPool
    {
        protected override void OnSpawned(TerrainChunk item)
        {
            base.OnSpawned(item);
            
            item.gameObject.SetActive(true);
        }

        protected override void OnDespawned(TerrainChunk item)
        {
            base.OnDespawned(item);
            
            item.gameObject.SetActive(false);
        }

        protected override void OnCreated(TerrainChunk item)
        {
            base.OnCreated(item);
            
            item.gameObject.SetActive(false);
        }
    }
}