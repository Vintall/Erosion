using System;
using System.Collections.Generic;
using Enums;
using Models;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Strategies.HydraulicErosion.Impls
{
    public class CPUParticleBasedErosionStrategy : IHydraulicErosionStrategy
    {
        public EHydraulicErosionType HydraulicErosionType => EHydraulicErosionType.ParticlesCPU;

        public CPUParticleBasedErosionStrategy()
        {
            
        }

        public void Execute(HydraulicErosionIterationVo iterationData,
            MeshDataVo meshDataVo, Action<int> iterationTimestamp)
        {
            for (var i = 0; i < iterationData.IterationsCount; ++i)
            {
                SimulateDroplet(ref meshDataVo.Vertices, iterationData, meshDataVo.Resolution, new Vector2(
                    Random.Range(0f, meshDataVo.Resolution),
                    Random.Range(0f, meshDataVo.Resolution)));
            }
        }

        public struct Droplet
        {
            public Vector3 Position;
            public Vector3 Speed;
            public float WaterVolume;
            public float SedimentConcentration;
            public float SedimentCapacity;
        }

        public void SimulateDroplet(
            ref Vector3[][] heightMap, in HydraulicErosionIterationVo iterationData,
            int resolution, Vector2 currentPosition)
        {
            var iterations = 50;
            var droplet = new Droplet()
            {
                Position = new Vector3(currentPosition.x, 0, currentPosition.y),
                Speed = Vector3.zero,
                WaterVolume = 1,
                SedimentConcentration = 0
            };

            var friction = 0.1f;

            while (iterations > 0 && droplet.WaterVolume > 0)
            {
                var flooredPosition = Vector2Int.FloorToInt(new Vector2(droplet.Position.x, droplet.Position.z));

                if (flooredPosition.x < 0 ||
                    flooredPosition.x >= resolution - 1 ||
                    flooredPosition.y < 0 ||
                    flooredPosition.y >= resolution - 1)
                    return;

                var normal = GetSurfaceNormal(ref heightMap, flooredPosition, new Vector2(droplet.Position.x, droplet.Position.z));

                //Accelerate particle using newtonian mechanics using the surface normal.
                var acceleration = normal;
                droplet.Speed += acceleration; //F = ma, so a = F/m
                droplet.Position += droplet.Speed;
                droplet.Speed *= 1.0f - friction; //Friction Factor

                if (droplet.Position.x < 0 ||
                    droplet.Position.x > resolution - 1 ||
                    droplet.Position.z < 0 ||
                    droplet.Position.z > resolution - 1)
                    return;

                //Compute sediment capacity difference
                var heightDifference = heightMap[flooredPosition.y][flooredPosition.x].y -
                                       heightMap[(int)droplet.Position.z][(int)droplet.Position.x].y;

                if (heightDifference < 0)
                    heightDifference = 0;

                var maxsediment = droplet.WaterVolume * droplet.Speed.magnitude * heightDifference;


                if (maxsediment < 0.0)
                    maxsediment = 0;

                var sdiff = maxsediment - droplet.SedimentConcentration;

                droplet.SedimentConcentration += iterationData.DepositionRate * sdiff;

                var resultVector = Vector3.up * Mathf.Min(
                    droplet.WaterVolume * iterationData.DepositionRate * sdiff, heightDifference);

                heightMap[flooredPosition.y][flooredPosition.x] -= resultVector;

                var eligiblePositions = new List<Vector2Int>(8);

                for (var x = -1; x <= 1; ++x)
                for (var y = -1; y <= 1; ++y)
                {
                    if (x == 0 && y == 0)
                        continue;

                    var xx = x + flooredPosition.x;
                    var yy = y + flooredPosition.y;

                    if (IsPointInBounds(new Vector2Int(xx, yy), 1, resolution))
                        eligiblePositions.Add(new Vector2Int(xx, yy));
                }

                var eligiblePositionsCount = eligiblePositions.Count;

                for (var i = 0; i < eligiblePositionsCount; ++i)
                    heightMap[eligiblePositions[i].x][eligiblePositions[i].y] += resultVector / eligiblePositionsCount;

                droplet.WaterVolume *= (1.0f - iterationData.EvaporationRate);

                --iterations;
            }

            Vector3 GetSurfaceNormal(ref Vector3[][] heightMap, Vector2Int flooredPosition, Vector2 particlePosition) // x = x, y = z
            {
                var squareGrid = new Vector3[][]
                {
                    new Vector3[]
                    {
                        heightMap[flooredPosition.y][flooredPosition.x],
                        heightMap[flooredPosition.y + 1][flooredPosition.x]
                    },
                    new Vector3[]
                    {
                        heightMap[flooredPosition.y][flooredPosition.x + 1],
                        heightMap[flooredPosition.y + 1][flooredPosition.x + 1]
                    }
                };

                var distanceToFloor = Vector2.Distance(flooredPosition, particlePosition);
                var distanceToCeil = Vector2.Distance(squareGrid[1][1], particlePosition);


                if (distanceToFloor < distanceToCeil)
                    return Vector3.Cross(squareGrid[0][1] - squareGrid[0][0], squareGrid[1][0] - squareGrid[0][0])
                        .normalized;

                if (distanceToFloor > distanceToCeil)
                    return Vector3.Cross(squareGrid[1][0] - squareGrid[1][1], squareGrid[0][1] - squareGrid[1][1])
                        .normalized;

                var normal = ((squareGrid[1][0] + squareGrid[0][1] - squareGrid[0][0] - squareGrid[1][1]) / 2)
                    .normalized;

                if (normal.y < 0)
                    normal *= -1;

                return normal;
            }
        }
        
        public bool IsPointInBounds(Vector2Int point, float pointRadius, int resolution)
        {
            var pointLeftX = point.x - pointRadius;
            var pointRightX = point.x + pointRadius;
            var pointTopY = point.y - pointRadius;
            var pointBottomY = point.y + pointRadius;

            return pointLeftX >= 0 &&
                   pointRightX < resolution &&
                   pointTopY >= 0 &&
                   pointBottomY < resolution;
        }

        public Vector2Int[] GetPositionsInRadius(Vector2Int position, int pointRadius, int resolution)
        {
            var squareVolume = pointRadius * pointRadius * 4;
            var result = new List<Vector2Int>(squareVolume);

            for (var x = position.x - pointRadius; x < position.x + pointRadius; ++x)
            {
                for (var y = position.y - pointRadius; y < position.y + pointRadius; ++y)
                {
                    var positionInBound = new Vector2Int(x, y);

                    if (Vector2Int.Distance(position, positionInBound) <= pointRadius &&
                        IsPointInBounds(positionInBound, 1, resolution))
                        result.Add(positionInBound);
                }
            }
            
            return result.ToArray();
        }
    }
}