#region

using System;
using Appalachia.Spatial.SpatialKeys;

#endregion

namespace Appalachia.Core.Collections.Implementations.Lists
{
    [Serializable]
    public sealed class AppaList_Vector2Key : AppaList<Vector2Key>
    {
        public AppaList_Vector2Key()
        {
        }

        public AppaList_Vector2Key(int capacity, float capacityIncreaseMultiplier = 2, bool noTracking = false) : base(
            capacity,
            capacityIncreaseMultiplier,
            noTracking
        )
        {
        }

        public AppaList_Vector2Key(AppaList<Vector2Key> list) : base(list)
        {
        }

        public AppaList_Vector2Key(Vector2Key[] values) : base(values)
        {
        }
    }
}
