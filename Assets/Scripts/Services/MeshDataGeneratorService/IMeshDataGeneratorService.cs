using Models;
using Services.PlaneGeneration.Impls;

namespace Services.MeshDataGeneratorService
{
    public interface IMeshDataGeneratorService
    {
        MeshDataVo GenerateMesh(int resolution, float size);
    }
}