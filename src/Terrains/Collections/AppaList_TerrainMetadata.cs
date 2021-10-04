#region

using System;
using Appalachia.Core.Collections;

#endregion

namespace Appalachia.Spatial.Terrains.Collections
{
    [Serializable]
    public sealed class AppaList_TerrainMetadata : AppaList<TerrainMetadata>
    {
        public AppaList_TerrainMetadata()
        {
        }

        public AppaList_TerrainMetadata(
            int capacity,
            float capacityIncreaseMultiplier = 2,
            bool noTracking = false) : base(capacity, capacityIncreaseMultiplier, noTracking)
        {
        }

        public AppaList_TerrainMetadata(AppaList<TerrainMetadata> list) : base(list)
        {
        }

        public AppaList_TerrainMetadata(TerrainMetadata[] values) : base(values)
        {
        }
    }
}
