using Enums;
using Models;
using UnityEngine;

namespace Strategies.HydraulicErosion.Impls
{
    public class CPUGridBasedErosionStrategy : IHydraulicErosionStrategy
    {
        public EHydraulicErosionType HydraulicErosionType => EHydraulicErosionType.GridCPU;
        
        struct VertexState
        {
            public float[] outFlow;
            public float[] inFlow;
            public float height;
            public float water;
            public float sediment;
        }
        
        public void Execute(HydraulicErosionIterationVo iterationData, MeshDataVo meshDataVo)
        {
            var verticesStates = new VertexState[meshDataVo.Resolution][];
            var deltaVerticesStates = new VertexState[meshDataVo.Resolution][];

            for (var x = 0; x < meshDataVo.Resolution; ++x)
            {
                verticesStates[x] = new VertexState[meshDataVo.Resolution];
                deltaVerticesStates[x] = new VertexState[meshDataVo.Resolution];

                for (var y = 0; y < meshDataVo.Resolution; ++y)
                {
                    verticesStates[x][y] = new VertexState()
                    {
                        height = meshDataVo.Vertices[x][y].y,
                        water = 1,
                        sediment = 0
                    };
                    deltaVerticesStates[x][y] = new VertexState();
                }
            }
            
            for (var i = 0; i < iterationData.IterationsCount; ++i)
            {
                // for (var x = 0; x < meshDataVo.Resolution; ++x)
                // {
                //     for (var y = 0; y < meshDataVo.Resolution; ++y)
                //     {
                //         verticesStates[x][y] = new VertexState()
                //         {
                //             height = meshDataVo.Vertices[x][y].y,
                //             water = 1,
                //             sediment = 0
                //         };
                //     }
                // }
                
                // if(i % 40 == 0)
                //     for (var y = 0; y < meshDataVo.Resolution; ++y)
                //     {
                //         for (var x = 0; x < meshDataVo.Resolution; ++x)
                //         {
                //             verticesStates[x][y].water += 1;
                //         }
                //     }

                for (var y = 0; y < meshDataVo.Resolution; ++y)
                {
                    for (var x = 0; x < meshDataVo.Resolution; ++x)
                    {
                        // GatherOutflowData(
                        //     ref verticesStates, ref deltaVerticesStates,
                        //     new Vector2Int(x, y), in iterationData, in meshDataVo);
                        
                        NeighborsCheck(
                            ref verticesStates, ref deltaVerticesStates,
                            new Vector2Int(x, y), in iterationData, in meshDataVo);
                    }
                }
                
                for (var y = 0; y < meshDataVo.Resolution; ++y)
                {
                    for (var x = 0; x < meshDataVo.Resolution; ++x)
                    {
                        verticesStates[x][y].sediment += deltaVerticesStates[x][y].sediment;
                        verticesStates[x][y].water += deltaVerticesStates[x][y].water;
                        verticesStates[x][y].height += deltaVerticesStates[x][y].height;

                        deltaVerticesStates[x][y].sediment = 0;
                        deltaVerticesStates[x][y].water = 0;
                        deltaVerticesStates[x][y].height = 0;
                        deltaVerticesStates[x][y].outFlow = new float[9];
                    }
                }
                
                for (var x = 0; x < meshDataVo.Resolution; ++x)
                {
                    for (var y = 0; y < meshDataVo.Resolution; ++y)
                    {
                        meshDataVo.Vertices[x][y].y = verticesStates[x][y].height;
                    }
                }
            }
            
            
        }

        private void GatherOutflowData(
            ref VertexState[][] map,
            ref VertexState[][] deltaMap,
            Vector2Int position,
            in HydraulicErosionIterationVo iterationData,
            in MeshDataVo meshDataVo)
        {
            float[] fullHeightDeltas = new float[9];
            float fullHeightDeltasAccumulative = 0;
            
            for(var x = -1; x <= 1; ++x)
            for (var y = -1; y <= 1; ++y)
            {
                if(y == 0 && x == 0)
                    continue;
                
                var neighborPosition = position + new Vector2Int(y, x);
                
                if(!IsInBounds(neighborPosition, meshDataVo.Resolution))
                    return;
                
                var readonlyCurrentCell = map[position.x][position.y];
                var readonlyNeighborCell = map[neighborPosition.x][neighborPosition.y];
                var currentFullHeight = readonlyCurrentCell.height + readonlyCurrentCell.water;
                var neighborFullHeight = readonlyNeighborCell.height + readonlyNeighborCell.water;
                var deltaWater = Mathf.Min(readonlyCurrentCell.water, currentFullHeight - neighborFullHeight);
                
                fullHeightDeltas[3 * y + x + 4] = deltaWater;
                
                if(deltaWater <= 0)
                    continue;

                fullHeightDeltasAccumulative += deltaWater;
            }

            deltaMap[position.x][position.y].outFlow = new float[9];

            for(var x = -1; x <= 1; ++x)
            for (var y = -1; y <= 1; ++y)
            {
                if(y == 0 && x == 0)
                    continue;

                var neighborPosition = position + new Vector2Int(y, x);
                
                if(!IsInBounds(neighborPosition, meshDataVo.Resolution))
                    return;
                
                var deltaWater = fullHeightDeltas[3 * y + x + 4];

                if(deltaWater <= 0)
                    continue;
                
                deltaWater /= fullHeightDeltasAccumulative;

                deltaMap[position.x][position.y].outFlow[3 * y + x + 4] = deltaWater;
            }
        }

        private void NeighborsCheck(
            ref VertexState[][] map, 
            ref VertexState[][] deltaMap, 
            Vector2Int position, 
            in HydraulicErosionIterationVo iterationData, 
            in MeshDataVo meshDataVo)
        {
            var fullHeightDeltas = new float[9];
            var fullHeightDeltasAccumulative = 0f;
            
            for(var x = -1; x <= 1; ++x)
            for (var y = -1; y <= 1; ++y)
            {
                if(y == 0 && x == 0)
                    continue;
                
                var neighborPosition = position + new Vector2Int(y, x);
                
                if(!IsInBounds(neighborPosition, meshDataVo.Resolution))
                    return;
                
                var readonlyCurrentCell = map[position.x][position.y];
                var readonlyNeighborCell = map[neighborPosition.x][neighborPosition.y];
                var currentFullHeight = readonlyCurrentCell.height + readonlyCurrentCell.water;
                var neighborFullHeight = readonlyNeighborCell.height + readonlyNeighborCell.water;
                var deltaWater = Mathf.Min(readonlyCurrentCell.water, currentFullHeight - neighborFullHeight);
                
                fullHeightDeltas[3 * y + x + 4] = deltaWater;
                
                if(deltaWater <= 0)
                    continue;

                fullHeightDeltasAccumulative += deltaWater;
            }

            for(var x = -1; x <= 1; ++x)
            for (var y = -1; y <= 1; ++y)
            {
                if(y == 0 && x == 0)
                    continue;

                var neighborPosition = position + new Vector2Int(y, x);
                
                if(!IsInBounds(neighborPosition, meshDataVo.Resolution))
                    return;
                
                var deltaWater = fullHeightDeltas[3 * y + x + 4];

                if (deltaWater <= 0) // Lower, than neighbor
                {
                    var deposition = iterationData.DepositionRate * map[position.x][position.y].sediment;

                    deltaMap[position.x][position.y].height += deposition;
                    deltaMap[position.x][position.y].sediment -= deposition;
                }
                else
                {
                    deltaWater /= fullHeightDeltasAccumulative;
                    
                    var potentiallyCarryingSediment = iterationData.SedimentCarryingCapacity * deltaWater;
                    
                    // var potentiallyCarryingSediment = iterationData.SedimentCarryingCapacity * 
                    //                                   deltaMap[position.x][position.y].outFlow[3 * y + x + 4];

                    if (map[position.x][position.y].sediment > potentiallyCarryingSediment)
                    {
                        var remainingSediment = map[position.x][position.y].sediment - potentiallyCarryingSediment;
                        
                        deltaMap[neighborPosition.x][neighborPosition.y].sediment += potentiallyCarryingSediment;
                        deltaMap[position.x][position.y].height += iterationData.DepositionRate * remainingSediment;
                        deltaMap[position.x][position.y].sediment -= iterationData.DepositionRate * remainingSediment;
                    }
                    else
                    {
                        deltaMap[neighborPosition.x][neighborPosition.y].sediment +=
                            map[position.x][position.y].sediment + iterationData.SoilSoftness *
                            (potentiallyCarryingSediment - map[position.x][position.y].sediment);
                        
                        deltaMap[position.x][position.y].height -= 
                            iterationData.SoilSoftness * (potentiallyCarryingSediment - map[position.x][position.y].sediment);
                            
                        deltaMap[position.x][position.y].sediment = 0;
                    }
                }
            }
        }

        private bool IsInBounds(Vector2Int position, int resolution)
        {
            if (position.x < 0)
                return false;

            if (position.x >= resolution)
                return false;
            
            if (position.y < 0)
                return false;

            if (position.y >= resolution)
                return false;

            return true;
        }
    }
}