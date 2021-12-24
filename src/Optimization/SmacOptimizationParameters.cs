#region

using System;
using Appalachia.Core.Objects.Root;
using Sirenix.OdinInspector;

#endregion

namespace Appalachia.Spatial.Optimization
{
    [Serializable]
    public class SmacOptimizationParameters : AppalachiaSimpleBase
    {
        [PropertyRange(1, 500)] public int maxIterations = 15;

        [PropertyRange(1, 500)] public int randomStartingPointCount = 20;

        [PropertyRange(1, 10)] public int functionEvaluationsPerIterationCount = 1;

        [PropertyRange(1, 100)] public int localSearchPointCount = 10;

        [PropertyRange(50, 2500)]
        public int randomSearchPointCount = 1000;
    }
}
