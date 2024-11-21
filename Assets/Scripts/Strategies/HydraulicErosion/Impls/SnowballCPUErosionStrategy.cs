using System;
using Enums;
using Models;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Strategies.HydraulicErosion.Impls
{
    public class SnowballCPUErosionStrategy : IHydraulicErosionStrategy
    {
        public EHydraulicErosionType HydraulicErosionType => EHydraulicErosionType.SnowballCPU;

        public void Execute(HydraulicErosionIterationVo iterationData,
            MeshDataVo meshDataVo, Action<int> iterationTimestamp)
        {
            var floatVertices = new float[meshDataVo.Resolution][];
            
            for(var i = 0; i < meshDataVo.Resolution; ++i)
            {
                floatVertices[i] = new float[meshDataVo.Resolution];
                
                for (var j = 0; j < meshDataVo.Resolution; ++j)
                {
                    floatVertices[i][j] = meshDataVo.Vertices[i][j].y;
                }
            }

            for (int i = 0; i < iterationData.IterationsCount; i++)
            {
                Trace(ref floatVertices, Random.Range(0, meshDataVo.Resolution),
                    Random.Range(0, meshDataVo.Resolution),
                    in iterationData);
            }
            
            for(var i = 0; i < meshDataVo.Resolution; ++i)
            {
                for (var j = 0; j < meshDataVo.Resolution; ++j)
                {
                    meshDataVo.Vertices[i][j].y = floatVertices[i][j];
                }
            }
        }

        private float radius = 1.0f;
        private float friction = 0.9f;
        private float speed = 0.1f;

        void Trace(ref float[][] heightMap, float x, float y, in HydraulicErosionIterationVo iterationData)
        {
            float ox = Random.Range(-radius, radius); // The X offset
            float oy = Random.Range(-radius, radius); // The Y offset
            float sediment = 0; // The amount of carried sediment
            float xp = x; // The previous X position
            float yp = y; // The previous Y position
            float vx = 0; // The horizontal velocity
            float vy = 0; // The vertical velocity

            for (int i = 0; i < 100; i++)
            {
                // Get the surface normal of the terrain at the current location
                Vector3 surfaceNormal = SampleNormal(in heightMap, x + ox, y + oy, heightMap.Length);

                // If the terrain is flat, stop simulating, the snowball cannot roll any further
                if (surfaceNormal.y == 1)
                    break;

                // Calculate the deposition and erosion rate
                float deposit = sediment * iterationData.DepositionRate * surfaceNormal.y;
                float erosion = iterationData.ErosionRate * (1 - surfaceNormal.y) * Mathf.Min(1, i * 0.01f);

                // Change the sediment on the place this snowball came from
                ChangeHeightMap(xp, yp, deposit - erosion, heightMap.Length, ref heightMap);
                sediment += erosion - deposit;
                

                vx = friction * vx + surfaceNormal.x * speed;
                vy = friction * vy + surfaceNormal.z * speed;
                xp = x;
                yp = y;
                x += vx;
                y += vy;
            }
        }

        Vector3 SampleNormal(in float[][] heightMap, float x, float y, int resolution)
        {
            // Ensure x and y are within bounds
            int ix = Mathf.Clamp(Mathf.FloorToInt(x), 0, resolution - 1);
            int iy = Mathf.Clamp(Mathf.FloorToInt(y), 0, resolution - 1);

            // Compute gradient based on neighboring heights
            float left = ix > 0 ? heightMap[ix - 1][iy] : heightMap[ix][iy];
            float right = ix < resolution - 1 ? heightMap[ix + 1][iy] : heightMap[ix][iy];
            float down = iy > 0 ? heightMap[ix][iy - 1] : heightMap[ix][iy];
            float up = iy < resolution - 1 ? heightMap[ix][iy + 1] : heightMap[ix][iy];

            Vector3 gradient = new Vector3(left - right, 2.0f, down - up).normalized;
            return gradient;
        }

        void ChangeHeightMap(float x, float y, float delta, int resolution, ref float[][] heightMap)
        {
            int ix = Mathf.Clamp(Mathf.FloorToInt(x), 0, resolution - 1);
            int iy = Mathf.Clamp(Mathf.FloorToInt(y), 0, resolution - 1);

            heightMap[ix][iy] += delta;
        }
    }
}