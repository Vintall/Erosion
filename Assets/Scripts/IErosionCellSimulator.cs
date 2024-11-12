using UnityEngine;

public interface IErosionCellSimulator
{
    void SimulateStep();
    void SimulateDroplet(Vector2 currentPosition);
    void SetupSimulator(Vector3[][] heightMap);
    Vector3[][] HeightMap { get; }
}