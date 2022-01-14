#region

using Appalachia.Core.Collections.Implementations.Lists;
using Appalachia.Core.Objects.Scriptables;
using Appalachia.Spatial.Terrains.Collections;
using Unity.Profiling;

#endregion

namespace Appalachia.Spatial.Terrains
{
    public sealed class TerrainMetadataDictionary : AppalachiaObjectLookupCollection<int, TerrainMetadata,
        intList, TerrainMetadataList, TerrainMetadataLookup, TerrainMetadataDictionary>
    {
        public override bool HasDefault => false;

        protected override int GetUniqueKeyFromValue(TerrainMetadata value)
        {
            return value.Data.GetHashCode();
        }

        #region Profiling

        

        #endregion
    }
}
