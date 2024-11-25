using UnityEngine;
using UnityEngine.Serialization;

namespace Databases.CommonShadersDatabase.Impls
{
    [CreateAssetMenu(menuName = "Databases/CommonShadersDatabase", fileName = "CommonShadersDatabase")]
    public class CommonShadersDatabase : ScriptableObject, ICommonShadersDatabase
    {
        [SerializeField] private ComputeShader gridBasedHydraulicErosionComputeShader;
        [SerializeField] private ComputeShader particleBasedHydraulicErosionComputeShader;
        
        public ComputeShader GridBasedHydraulicErosionComputeShader => gridBasedHydraulicErosionComputeShader;
        public ComputeShader ParticleBasedHydraulicErosionComputeShader => particleBasedHydraulicErosionComputeShader;
    }
}