﻿using System;

namespace Models
{
    [Serializable]
    public class HydraulicErosionIterationVo
    {
        public int IterationsCount;
        public float ErosionRate;
        public float DepositionRate;
        public float EvaporationRate;
        public float MinSlope;
        public float SedimentCarryingCapacity;
        public float SoilSoftness;
    }
}