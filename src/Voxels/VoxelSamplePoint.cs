#region

using System;
using Appalachia.Jobs.Burstable;
using Unity.Mathematics;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.Voxels
{
    [Serializable]
    public struct VoxelSamplePoint : IEquatable<VoxelSamplePoint>
    {
        [SerializeField] public float3 position;
        [SerializeField] public bool populated;
        [SerializeField] public int index;
        [SerializeField] public float3 time;

        public BoundsBurst ToBounds(float3 resolution)
        {
            return new BoundsBurst(position, resolution);
        }

#region IEquatable

        public bool Equals(VoxelSamplePoint other)
        {
            return position.Equals(other.position) && (populated == other.populated) && (index == other.index) && time.Equals(other.time);
        }

        public override bool Equals(object obj)
        {
            return obj is VoxelSamplePoint other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = position.GetHashCode();
                hashCode = (hashCode * 397) ^ populated.GetHashCode();
                hashCode = (hashCode * 397) ^ index;
                hashCode = (hashCode * 397) ^ time.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(VoxelSamplePoint left, VoxelSamplePoint right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(VoxelSamplePoint left, VoxelSamplePoint right)
        {
            return !left.Equals(right);
        }

#endregion
    }
}
