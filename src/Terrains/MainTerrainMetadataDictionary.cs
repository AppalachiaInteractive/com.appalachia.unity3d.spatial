using Appalachia.Core.Collections.Implementations.Lists;
using Appalachia.Core.Objects.Scriptables;
using Appalachia.Spatial.Terrains.Collections;

namespace Appalachia.Spatial.Terrains
{
    public class MainTerrainMetadataDictionary : SingletonAppalachiaObjectLookupCollection<int,
        TerrainMetadata, intList, TerrainMetadataList, TerrainMetadataLookup, TerrainMetadataDictionary,
        MainTerrainMetadataDictionary>
    {
#if UNITY_EDITOR
        [UnityEditor.MenuItem(
            PKG.Menu.Assets.Base + nameof(MainTerrainMetadataDictionary),
            priority = PKG.Menu.Assets.Priority
        )]
        public static void CreateAsset()
        {
            CreateNew<MainTerrainMetadataDictionary>();
        }
#endif
    }
}
