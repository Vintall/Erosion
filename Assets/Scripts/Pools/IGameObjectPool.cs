using UnityEngine;
using Zenject;

namespace Pools
{
    public interface IGameObjectPool<T> : IMemoryPool<Transform, T> where T : MonoBehaviour
    {
        
    }
}