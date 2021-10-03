#region

using System;
using Sirenix.OdinInspector;

#endregion

namespace Appalachia.Spatial.Optimization
{
    [Serializable]
    public class ParticleSwarmOptimizationParameters
    {
        [PropertyRange(1, 500)] public int maxIterations = 30;

        [PropertyRange(1, 500)] public int numberOfParticles = 5;

        [PropertyRange(1, 10)] public double localBestWeight = 2;

        [PropertyRange(1, 10)] public double globalBestWeight = 3;
    }
}
