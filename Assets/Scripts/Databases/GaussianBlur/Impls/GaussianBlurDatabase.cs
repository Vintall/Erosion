using Models;
using UnityEngine;

namespace Databases.GaussianBlur.Impls
{
    [CreateAssetMenu(menuName = "Databases/GaussianBlurDatabase", fileName = "GaussianBlurDatabase")]
    public class GaussianBlurDatabase : ScriptableObject, IGaussianBlurDatabase
    {
        [SerializeField] private GaussianBlurVo defaultGaussianBlurVo;
        [SerializeField] private GaussianBlurVo gpuGaussianBlurVo;

        public GaussianBlurVo DefaultGaussianBlurVo => defaultGaussianBlurVo;
        public GaussianBlurVo GPUGaussianBlurVo => gpuGaussianBlurVo;
    }
}