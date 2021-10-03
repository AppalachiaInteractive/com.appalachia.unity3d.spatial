#region

using System;
using Sirenix.OdinInspector;

#endregion

namespace Appalachia.Spatial.Optimization
{
    [Serializable]
    public class SmacOptimizationParameters
    {
        [PropertyRange(1, 500)] public int maxIterations = 15;

        [PropertyRange(1, 500)] public int randomStartingPointCount = 20;

        [PropertyRange(1, 10)] public int functionEvaluationsPerIterationCount = 1;

        [PropertyRange(1, 100)] public int localSearchPointCount = 10;

        [PropertyRange(50, 2500)]
        public int randomSearchPointCount = 1000;
    }
}
