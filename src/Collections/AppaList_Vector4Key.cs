#region

using System;
using Appalachia.Spatial.SpatialKeys;

#endregion

namespace Appalachia.Core.Collections.Implementations.Lists
{
    [Serializable]
    public sealed class AppaList_Vector4Key : AppaList<Vector4Key>
    {
        public AppaList_Vector4Key()
        {
        }

        public AppaList_Vector4Key(int capacity, float capacityIncreaseMultiplier = 2, bool noTracking = false) : base(
            capacity,
            capacityIncreaseMultiplier,
            noTracking
        )
        {
        }

        public AppaList_Vector4Key(AppaList<Vector4Key> list) : base(list)
        {
        }

        public AppaList_Vector4Key(Vector4Key[] values) : base(values)
        {
        }
    }
}
