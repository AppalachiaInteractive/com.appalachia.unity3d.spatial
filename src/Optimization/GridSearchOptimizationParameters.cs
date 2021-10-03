#region

using System;
using Sirenix.OdinInspector;

#endregion

namespace Appalachia.Spatial.Optimization
{
    [Serializable]
    public class GridSearchOptimizationParameters
    {
        [PropertyRange(1, 1000)] public int iterations = 150;
    }
}
