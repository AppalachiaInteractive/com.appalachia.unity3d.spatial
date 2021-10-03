#region

using System;
using Sirenix.OdinInspector;

#endregion

namespace Appalachia.Spatial.Optimization
{
    [Serializable]
    public class NelderMeadOptimizationParameters
    {
        [PropertyRange(1, 500)] public int maxRestarts = 15;

        [PropertyRange(.0001, .01)]
        public double noImprovementThreshold = 0.001;

        [PropertyRange(1, 50)] public int maxIterationsWithoutImprovement = 10;

        [PropertyRange(0, 100)] public int maxIterationsPrRestart;

        [PropertyRange(0, 100)] public int maxFunctionEvaluations;
    }
}
