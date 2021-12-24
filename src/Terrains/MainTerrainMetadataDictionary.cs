using Appalachia.Core.Collections.Implementations.Lists;
using Appalachia.Core.Objects.Scriptables;
using Appalachia.Spatial.Terrains.Collections;

namespace Appalachia.Spatial.Terrains
{
    public class MainTerrainMetadataDictionary : SingletonAppalachiaObjectLookupCollection<int,
        TerrainMetadata, intList, TerrainMetadataList, TerrainMetadataLookup, TerrainMetadataDictionary,
        MainTerrainMetadataDictionary>
    {
    }
}
