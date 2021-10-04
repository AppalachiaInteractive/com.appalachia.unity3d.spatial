#region

using System;
using Appalachia.Core.Collections;
using Appalachia.Spatial.SpatialKeys;

#endregion

namespace Appalachia.Spatial.Collections
{
    [Serializable]
    public sealed class AppaList_Vector2Key : AppaList<Vector2Key>
    {
        public AppaList_Vector2Key()
        {
        }

        public AppaList_Vector2Key(
            int capacity,
            float capacityIncreaseMultiplier = 2,
            bool noTracking = false) : base(capacity, capacityIncreaseMultiplier, noTracking)
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
