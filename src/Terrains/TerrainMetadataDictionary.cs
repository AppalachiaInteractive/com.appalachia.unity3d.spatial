#region

using Appalachia.Core.Collections.Implementations.Lists;
using Appalachia.Core.Objects.Scriptables;
using Appalachia.Spatial.Terrains.Collections;

#endregion

namespace Appalachia.Spatial.Terrains
{
    public sealed class TerrainMetadataDictionary : AppalachiaObjectLookupCollection<int, TerrainMetadata,
        intList, TerrainMetadataList, TerrainMetadataLookup, TerrainMetadataDictionary>
    {
        /// <inheritdoc />
        public override bool HasDefault => false;

        /// <inheritdoc />
        protected override int GetUniqueKeyFromValue(TerrainMetadata value)
        {
            return value.Data.GetHashCode();
        }
    }
}
