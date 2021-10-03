#region

using System;
using Appalachia.Spatial.SpatialKeys;

#endregion

namespace Appalachia.Core.Collections.Implementations.Lists
{
    [Serializable]
    public sealed class AppaList_Matrix4x4Key : AppaList<Matrix4x4Key>
    {
        public AppaList_Matrix4x4Key()
        {
        }

        public AppaList_Matrix4x4Key(int capacity, float capacityIncreaseMultiplier = 2, bool noTracking = false) : base(
            capacity,
            capacityIncreaseMultiplier,
            noTracking
        )
        {
        }

        public AppaList_Matrix4x4Key(AppaList<Matrix4x4Key> list) : base(list)
        {
        }

        public AppaList_Matrix4x4Key(Matrix4x4Key[] values) : base(values)
        {
        }
    }
}
