#region

using Unity.Profiling;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.Terrains.Utilities
{
    public static class TerrainUtilities
    {
        private const string _PRF_PFX = nameof(TerrainUtilities) + ".";

        private static readonly ProfilerMarker _PRF_GetTerrainAtPosition =
            new(_PRF_PFX + nameof(GetTerrainAtPosition));

        private static readonly ProfilerMarker _PRF_IsPositionValidForTerrain =
            new(_PRF_PFX + nameof(IsPositionValidForTerrain));

        private static readonly ProfilerMarker _PRF_GetWorldHeightAtPosition =
            new(_PRF_PFX + nameof(GetWorldHeightAtPosition));

        private static readonly ProfilerMarker _PRF_IsPositionAboveTerrain =
            new(_PRF_PFX + nameof(IsPositionAboveTerrain));

        private static readonly ProfilerMarker _PRF_IsPositionBelowTerrain =
            new(_PRF_PFX + nameof(IsPositionBelowTerrain));

        private static readonly ProfilerMarker _PRF_IsPositionGroundedOnTerrain =
            new(_PRF_PFX + nameof(IsPositionGroundedOnTerrain));

        private static readonly ProfilerMarker _PRF_GetInterpolatedTerrainPosition =
            new(_PRF_PFX + nameof(GetInterpolatedTerrainPosition));

        private static readonly ProfilerMarker _PRF_GetWorldTerrainBounds =
            new(_PRF_PFX + nameof(GetWorldTerrainBounds));

        public static Terrain GetTerrainAtPosition(this Terrain[] terrains, Vector3 worldPos)
        {
            using (_PRF_GetTerrainAtPosition.Auto())
            {
                for (var index = 0; index < terrains.Length; index++)
                {
                    var terrain = terrains[index];
                    if (IsPositionValidForTerrain(terrain, worldPos))
                    {
                        return terrain;
                    }
                }

                return null;
            }
        }

        public static bool IsPositionValidForTerrain(this Terrain terrain, Vector3 worldPos)
        {
            using (_PRF_IsPositionValidForTerrain.Auto())
            {
                var terrainData = terrain.terrainData;

                var terrainMin = terrain.GetPosition();
                var terrainMax = terrainMin + terrainData.size;

                if ((worldPos.x >= terrainMin.x) &&
                    (worldPos.x < terrainMax.x) &&
                    (worldPos.z >= terrainMin.z) &&
                    (worldPos.z < terrainMax.z))
                {
                    return true;
                }

                return false;
            }
        }

        public static float GetWorldHeightAtPosition(this Terrain[] terrains, Vector3 worldPos)
        {
            using (_PRF_GetWorldHeightAtPosition.Auto())
            {
                var terrain = GetTerrainAtPosition(terrains, worldPos);

                return terrain.GetWorldHeightAtPosition(worldPos);
            }
        }

        public static float GetWorldHeightAtPosition(this Terrain terrain, Vector3 worldPos)
        {
            using (_PRF_GetWorldHeightAtPosition.Auto())
            {
                if (terrain == null)
                {
                    return 0.0f;
                }

                var terrainMin = terrain.GetPosition();

                return GetWorldHeightAtPosition(terrain, terrainMin, worldPos);
            }
        }

        public static float GetWorldHeightAtPosition(
            this Terrain terrain,
            Vector3 terrainPosition,
            Vector3 worldPos)
        {
            using (_PRF_GetWorldHeightAtPosition.Auto())
            {
                if (terrain == null)
                {
                    return 0.0f;
                }

                var height = terrain.SampleHeight(worldPos) + terrainPosition.y;

                return height;
            }
        }

        public static bool IsPositionAboveTerrain(this Terrain[] terrains, Vector3 worldPos)
        {
            using (_PRF_IsPositionAboveTerrain.Auto())
            {
                var height = GetWorldHeightAtPosition(terrains, worldPos);

                return worldPos.y > height;
            }
        }

        public static bool IsPositionBelowTerrain(this Terrain[] terrains, Vector3 worldPos)
        {
            using (_PRF_IsPositionBelowTerrain.Auto())
            {
                var height = GetWorldHeightAtPosition(terrains, worldPos);

                return height > worldPos.y;
            }
        }

        public static bool IsPositionGroundedOnTerrain(
            this Terrain[] terrains,
            Vector3 worldPos,
            float closeness = 0.005f)
        {
            using (_PRF_IsPositionGroundedOnTerrain.Auto())
            {
                var height = GetWorldHeightAtPosition(terrains, worldPos);

                return Mathf.Abs(height - worldPos.y) < closeness;
            }
        }

        public static Vector2 GetInterpolatedTerrainPosition(this Terrain terrain, Vector3 worldPos)
        {
            using (_PRF_GetInterpolatedTerrainPosition.Auto())
            {
                var terrainData = terrain.terrainData;
                var terrainSpacePos = worldPos - terrain.transform.position;

                return new Vector2(
                    terrainSpacePos.x / terrainData.size.x,
                    terrainSpacePos.z / terrainData.size.z
                );
            }
        }

        public static Bounds GetWorldTerrainBounds(this Terrain terrain)
        {
            using (_PRF_GetWorldTerrainBounds.Auto())
            {
                var terrainBounds = terrain.terrainData.bounds;
                terrainBounds.center += terrain.GetPosition();

                return terrainBounds;
            }
        }
    }
}
