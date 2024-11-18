using UnityEngine;
using Zenject;

namespace Services.GausianBlur.Impls
{
    public class GaussianBlurService : IGaussianBlurService, IInitializable
    {
        private float centerModifier;
        private float adjacentModifier;
        private float diagonalModifier;
        
        public GaussianBlurService() { }

        public void Initialize()
        {
            centerModifier = 0.6f; // TODO replace to GaussianBlurDatabase
            adjacentModifier = 0.075f;
            diagonalModifier = 0.025f;
        }
        
        public void ApplyGaussianBlur(ref Vector3[][] heightMap, int resolution)
        {
            var newHeightMap = new Vector3[resolution][];

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
                        heightMap[z][x].y * centerModifier +
                        heightMap[z][x + 1].y * adjacentModifier +
                        heightMap[z + 1][x].y * adjacentModifier +
                        heightMap[z][x - 1].y * adjacentModifier +
                        heightMap[z - 1][x].y * adjacentModifier +
                        heightMap[z + 1][x + 1].y * diagonalModifier +
                        heightMap[z + 1][x - 1].y * diagonalModifier +
                        heightMap[z - 1][x + 1].y * diagonalModifier +
                        heightMap[z - 1][x - 1].y * diagonalModifier;

                    newHeightMap[z][x] = new Vector3(heightMap[z][x].x, bluredValue, heightMap[z][x].z);
                }
            }

            heightMap = newHeightMap;
        }
    }
}