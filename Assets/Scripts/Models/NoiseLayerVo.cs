using System;
using UnityEngine;

namespace Models
{
    [Serializable]
    public class NoiseLayerVo
    {
        public Vector2 Scale;
        public Vector2 Displacement;
        public float Influence;

        public NoiseLayerVo(Vector2 scale, Vector2 displacement, float influence)
        {
            Scale = scale;
            Displacement = displacement;
            Influence = influence;
        }
    }
}