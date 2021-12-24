#region

using System;
using Appalachia.Core.Objects.Root;
using Sirenix.OdinInspector;

#endregion

namespace Appalachia.Spatial.Optimization
{
    [Serializable]
    public class BayesianOptimizationParameters : AppalachiaSimpleBase
    {
        [PropertyRange(1, 500)] public int iterations = 30;

        [PropertyRange(1, 500)] public int randomStartingPointsCount = 5;

        [PropertyRange(1, 10)] public int functionEvaluationsPerIterationCount = 1;

        [PropertyRange(500, 10000)]
        public int randomSearchPointCount = 1000;
    }
}
