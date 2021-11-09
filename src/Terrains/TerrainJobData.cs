#region

using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.Terrains
{
    [Serializable]
    public struct TerrainJobData : IEquatable<TerrainJobData>
    {
        public readonly Allocator allocator;

        public readonly int hashCode;
        public readonly int resolution;
        public readonly float3 scale;
        public readonly float3 size;
        public readonly float3 terrainPosition;

        public TerrainJobData(Terrain terrain, Allocator allocator)
        {
            var terrainData = terrain.terrainData;

            hashCode = terrain.GetHashCode();
            terrainPosition = terrain.GetPosition();
            resolution = terrainData.heightmapResolution;
            scale = terrainData.heightmapScale;
            size = terrainData.size;
            this.allocator = allocator;
        }

        [DebuggerStepThrough] public bool Equals(TerrainJobData other)
        {
            return (hashCode == other.hashCode) &&
                   terrainPosition.Equals(other.terrainPosition) &&
                   (resolution == other.resolution) &&
                   scale.Equals(other.scale) &&
                   size.Equals(other.size) &&
                   (allocator == other.allocator);
        }

        [Pure]
        public bool IsPositionValidForTerrain(float3 worldPos)
        {
            var terrainMin = terrainPosition;
            var terrainMax = terrainMin + size;

            if ((worldPos.x >= terrainMin.x) &&
                (worldPos.x < terrainMax.x) &&
                (worldPos.z >= terrainMin.z) &&
                (worldPos.z < terrainMax.z))
            {
                return true;
            }

            return false;
        }

        [DebuggerStepThrough] public override bool Equals(object obj)
        {
            return obj is TerrainJobData other && Equals(other);
        }

        [DebuggerStepThrough] public override int GetHashCode()
        {
            unchecked
            {
                var h = this.hashCode;
                h = (h * 397) ^ terrainPosition.GetHashCode();
                h = (h * 397) ^ resolution;
                h = (h * 397) ^ scale.GetHashCode();
                h = (h * 397) ^ size.GetHashCode();
                h = (h * 397) ^ (int) allocator;
                return h;
            }
        }

        [DebuggerStepThrough] public static bool operator ==(TerrainJobData left, TerrainJobData right)
        {
            return left.Equals(right);
        }

        [DebuggerStepThrough] public static bool operator !=(TerrainJobData left, TerrainJobData right)
        {
            return !left.Equals(right);
        }
    }
}
