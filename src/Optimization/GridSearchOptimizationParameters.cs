#region

using System;
using Appalachia.Core.Objects.Root;
using Sirenix.OdinInspector;

#endregion

namespace Appalachia.Spatial.Optimization
{
    [Serializable]
    public class GridSearchOptimizationParameters : AppalachiaSimpleBase
    {
        [PropertyRange(1, 1000)] public int iterations = 150;
    }
}
