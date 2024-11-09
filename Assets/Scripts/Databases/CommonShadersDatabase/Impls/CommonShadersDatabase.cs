using UnityEngine;

namespace Databases.CommonShadersDatabase.Impls
{
    [CreateAssetMenu(menuName = "Databases/CommonShadersDatabase", fileName = "CommonShadersDatabase")]
    public class CommonShadersDatabase : ScriptableObject, ICommonShadersDatabase
    {
        [SerializeField] private ComputeShader hydraulicErosionComputeShader;
        
        public ComputeShader HydraulicErosionComputeShader => hydraulicErosionComputeShader;
    }
}