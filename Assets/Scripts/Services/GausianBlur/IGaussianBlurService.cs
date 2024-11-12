using UnityEngine;

namespace Services.GausianBlur
{
    public interface IGaussianBlurService
    {
        void ApplyGaussianBlur(ref Vector3[][] heightMap, int resolution);
    }
}