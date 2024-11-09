using System;

namespace Models
{
    [Serializable]
    public class HydraulicErosionIterationVo
    {
        public float DeltaTime;
        public float ErosionRate;
        public float DepositionRate;
        public float EvaporationRate;
        public float MinSlope;
    }
}