using UnityEngine;

namespace Services.HeightTextureDrawer
{
    public interface IHeightTextureDrawer
    {
        void SetPath(string path);
        void GenerateTexture(Vector3[][] vertices, int resolution);
        void GenerateTexture(float[][] heightMap, int resolution);
    }
}