#region

using Appalachia.Core.Collections.Implementations.Lists;
using Appalachia.Core.Objects.Initialization;
using Appalachia.Core.Objects.Scriptables;
using Appalachia.Spatial.Terrains.Collections;
using Appalachia.Utility.Async;

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

        protected override async AppaTask Initialize(Initializer initializer)
        {
            using (_PRF_Initialize.Auto())
            {
                await base.Initialize(initializer);
            }
        }

        #region Profiling

        private const string _PRF_PFX = nameof(TerrainMetadataDictionary) + ".";

        #endregion
    }
}
