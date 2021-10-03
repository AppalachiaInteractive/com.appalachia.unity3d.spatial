#region

using System;
using Appalachia.Spatial.SpatialKeys;

#endregion

namespace Appalachia.Core.Collections.Implementations.Lists
{
    [Serializable]
    public sealed class AppaList_Vector3Key : AppaList<Vector3Key>
    {
        public AppaList_Vector3Key()
        {
        }

        public AppaList_Vector3Key(int capacity, float capacityIncreaseMultiplier = 2, bool noTracking = false) : base(
            capacity,
            capacityIncreaseMultiplier,
            noTracking
        )
        {
        }

        public AppaList_Vector3Key(AppaList<Vector3Key> list) : base(list)
        {
        }

        public AppaList_Vector3Key(Vector3Key[] values) : base(values)
        {
        }
    }
}
