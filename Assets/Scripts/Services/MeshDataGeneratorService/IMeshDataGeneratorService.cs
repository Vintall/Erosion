using Models;
using Services.PlaneGeneration.Impls;

namespace Services.MeshDataGeneratorService
{
    public interface IMeshDataGeneratorService
    {
        MeshDataVo GenerateMeshData(int resolution, float size);
    }
}