#region

using System;
using Appalachia.Core.Objects.Root;
using Sirenix.OdinInspector;

#endregion

namespace Appalachia.Spatial.Optimization
{
    [Serializable]
    public class ParticleSwarmOptimizationParameters : AppalachiaSimpleBase
    {
        [PropertyRange(1, 500)] public int maxIterations = 30;

        [PropertyRange(1, 500)] public int numberOfParticles = 5;

        [PropertyRange(1, 10)] public double localBestWeight = 2;

        [PropertyRange(1, 10)] public double globalBestWeight = 3;
    }
}
