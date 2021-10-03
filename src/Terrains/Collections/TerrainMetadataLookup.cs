#region

using System;
using Appalachia.Core.Collections.Implementations.Lists;
using Appalachia.Core.Terrains;
using UnityEngine;

#endregion

namespace Appalachia.Core.Collections.Implementations.Lookups
{
    [Serializable]
    public class TerrainMetadataLookup : AppaLookup<int, TerrainMetadata, AppaList_int, AppaList_TerrainMetadata>
    {
        protected override string GetDisplayTitle(int key, TerrainMetadata value)
        {
            return "Terrain: " + value.GetTerrain().name;
        }

        protected override string GetDisplaySubtitle(int key, TerrainMetadata value)
        {
            return value.terrainPosition.ToString();
        }

        protected override Color GetDisplayColor(int key, TerrainMetadata value)
        {
            return Color.white;
        }
    }
}
