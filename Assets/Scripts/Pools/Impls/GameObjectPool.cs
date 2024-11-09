using UnityEngine;
using Zenject;

namespace Pools.Impls
{
    public class GameObjectPool<T> : MemoryPool<Transform, T>, IGameObjectPool<T> where T : MonoBehaviour
    {
        
    }
}