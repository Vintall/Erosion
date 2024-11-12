using Models;
using MonoBehavior;
using UnityEngine;

namespace Services.PlaneGeneration
{
    public interface ITerrainChunkGeneratorService
    {
        TerrainChunk GenerateTerrainChunk(int resolution, float size);
        Mesh GenerateMeshFromMeshData(MeshDataVo meshData);
    }
}