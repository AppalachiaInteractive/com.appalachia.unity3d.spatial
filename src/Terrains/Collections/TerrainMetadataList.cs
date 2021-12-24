#region

using System;
using Appalachia.Core.Collections;

#endregion

namespace Appalachia.Spatial.Terrains.Collections
{
    [Serializable]
    public sealed class TerrainMetadataList : AppaList<TerrainMetadata>
    {
        public TerrainMetadataList()
        {
        }

        public TerrainMetadataList(
            int capacity,
            float capacityIncreaseMultiplier = 2,
            bool noTracking = false) : base(capacity, capacityIncreaseMultiplier, noTracking)
        {
        }

        public TerrainMetadataList(AppaList<TerrainMetadata> list) : base(list)
        {
        }

        public TerrainMetadataList(TerrainMetadata[] values) : base(values)
        {
        }
    }
}
