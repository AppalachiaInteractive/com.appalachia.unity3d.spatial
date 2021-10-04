#region

using System;
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

        public readonly int hashCode;
        public readonly float3 terrainPosition;
        public readonly int resolution;
        public readonly float3 scale;
        public readonly float3 size;
        public readonly Allocator allocator;

        [Pure]
        public bool IsPositionValidForTerrain(float3 worldPos)
        {
            var terrainMin = terrainPosition;
            var terrainMax = terrainMin + size;

            if ((worldPos.x >= terrainMin.x) && (worldPos.x < terrainMax.x) && (worldPos.z >= terrainMin.z) && (worldPos.z < terrainMax.z))
            {
                return true;
            }

            return false;
        }

        public bool Equals(TerrainJobData other)
        {
            return (hashCode == other.hashCode) &&
                   terrainPosition.Equals(other.terrainPosition) &&
                   (resolution == other.resolution) &&
                   scale.Equals(other.scale) &&
                   size.Equals(other.size) &&
                   (allocator == other.allocator);
        }

        public override bool Equals(object obj)
        {
            return obj is TerrainJobData other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = this.hashCode;
                hashCode = (hashCode * 397) ^ terrainPosition.GetHashCode();
                hashCode = (hashCode * 397) ^ resolution;
                hashCode = (hashCode * 397) ^ scale.GetHashCode();
                hashCode = (hashCode * 397) ^ size.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) allocator;
                return hashCode;
            }
        }

        public static bool operator ==(TerrainJobData left, TerrainJobData right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TerrainJobData left, TerrainJobData right)
        {
            return !left.Equals(right);
        }
    }
}
