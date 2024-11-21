using Databases.GaussianBlur;
using UnityEngine;
using Zenject;

namespace Services.GausianBlur.Impls
{
    public class GaussianBlurService : IGaussianBlurService
    {
        private readonly IGaussianBlurDatabase _gaussianBlurDatabase;
        
        public GaussianBlurService(
            IGaussianBlurDatabase gaussianBlurDatabase)
        {
            _gaussianBlurDatabase = gaussianBlurDatabase;
        }

        public void ApplyGaussianBlur(ref Vector3[][] heightMap, int resolution)
        {
            var newHeightMap = new Vector3[resolution][];
            var blurModifiers = _gaussianBlurDatabase.DefaultGaussianBlurVo;
            
            for (var z = 0; z < resolution; ++z)
            {
                newHeightMap[z] = new Vector3[resolution];
                
                for (var x = 0; x < resolution; ++x) 
                    newHeightMap[z][x] = heightMap[z][x];
            }
            
            for (var z = 1; z < resolution - 1; ++z)
            {
                for (var x = 1; x < resolution - 1; ++x)
                {
                    var bluredValue =
                        heightMap[z][x].y * blurModifiers.CenterModifier +
                        heightMap[z][x + 1].y * blurModifiers.AdjacentModifier +
                        heightMap[z + 1][x].y * blurModifiers.AdjacentModifier +
                        heightMap[z][x - 1].y * blurModifiers.AdjacentModifier +
                        heightMap[z - 1][x].y * blurModifiers.AdjacentModifier +
                        heightMap[z + 1][x + 1].y * blurModifiers.DiagonalModifier +
                        heightMap[z + 1][x - 1].y * blurModifiers.DiagonalModifier +
                        heightMap[z - 1][x + 1].y * blurModifiers.DiagonalModifier +
                        heightMap[z - 1][x - 1].y * blurModifiers.DiagonalModifier;

                    newHeightMap[z][x] = new Vector3(heightMap[z][x].x, bluredValue, heightMap[z][x].z);
                }
            }

            heightMap = newHeightMap;
        }
    }
}