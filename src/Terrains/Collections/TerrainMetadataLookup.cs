#region

using System;
using System.Collections.Generic;
using Appalachia.Core.Collections;
using Appalachia.Core.Collections.Implementations.Lists;
using Appalachia.Utility.Strings;
using Unity.Profiling;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.Terrains.Collections
{
    [Serializable]
    public class TerrainMetadataLookup : AppaLookup<int, TerrainMetadata, intList, TerrainMetadataList>
    {
        #region Constants and Static Readonly

        private const string TERRAIN_FORMAT_TITLE = "Terrain: {0}";
        private const string TERRAIN_MISSING_TITLE = "Terrain: MISSING";

        #endregion

        #region Fields and Autoproperties

        private Dictionary<int, string> _displaySubtitles;

        private Dictionary<int, string> _displayTitles;

        #endregion

        /// <inheritdoc />
        protected override Color GetDisplayColor(int key, TerrainMetadata value)
        {
            return Color.white;
        }

        /// <inheritdoc />
        protected override string GetDisplaySubtitle(int key, TerrainMetadata value)
        {
            using (_PRF_GetDisplaySubtitle.Auto())
            {
                if (value == null)
                {
                    return string.Empty;
                }

                _displaySubtitles ??= new Dictionary<int, string>();
                var hashCode = value.GetHashCode();

                if (_displaySubtitles.TryGetValue(hashCode, out var result)) return result;

                var displaySubtitle = value.terrainPosition.ToString();

                _displaySubtitles.Add(hashCode, displaySubtitle);

                return displaySubtitle;
            }
        }

        /// <inheritdoc />
        protected override string GetDisplayTitle(int key, TerrainMetadata value)
        {
            using (_PRF_GetDisplayTitle.Auto())
            {
                var terrain = value.GetTerrain();

                if ((value == null) || (terrain == null))
                {
                    return TERRAIN_MISSING_TITLE;
                }

                _displayTitles ??= new Dictionary<int, string>();
                var hashCode = value.GetHashCode();

                if (_displayTitles.TryGetValue(hashCode, out var result)) return result;

                var displayTitle = ZString.Format(TERRAIN_FORMAT_TITLE, terrain.name);

                _displayTitles.Add(hashCode, displayTitle);

                return displayTitle;
            }
        }

        #region Profiling

        private const string _PRF_PFX = nameof(TerrainMetadataLookup) + ".";

        private static readonly ProfilerMarker _PRF_GetDisplaySubtitle =
            new ProfilerMarker(_PRF_PFX + nameof(GetDisplaySubtitle));

        private static readonly ProfilerMarker _PRF_GetDisplayTitle =
            new ProfilerMarker(_PRF_PFX + nameof(GetDisplayTitle));

        #endregion
    }
}
