using System;
using System.Collections.Generic;
using System.Linq;
using Enums;
using Models;
using Strategies.HydraulicErosion;

namespace Services.GPUHydraulicErosionService.Impls
{
    public class HydraulicErosionService : IHydraulicErosionService
    {
        private readonly Dictionary<EHydraulicErosionType, IHydraulicErosionStrategy> _hydraulicErosionStrategiesDictionary;
        
        public HydraulicErosionService(
            IHydraulicErosionStrategy[] hydraulicErosionStrategies)
        {
            _hydraulicErosionStrategiesDictionary = hydraulicErosionStrategies.ToDictionary(entry => entry.HydraulicErosionType);
        }

        public void SimulateErosion(
            HydraulicErosionIterationVo iterationData,
            MeshDataVo meshDataVo,
            EHydraulicErosionType hydraulicErosionType)
        {
            IHydraulicErosionStrategy resultStrategy;

            if (!_hydraulicErosionStrategiesDictionary.TryGetValue(hydraulicErosionType, out resultStrategy))
                throw new Exception($"Cannot find {typeof(IHydraulicErosionStrategy)} with key {hydraulicErosionType}");

            resultStrategy.Execute(iterationData, meshDataVo);
        }
    }
}