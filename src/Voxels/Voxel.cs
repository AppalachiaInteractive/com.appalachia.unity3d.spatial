#region

using System;
using Appalachia.Core.Extensions;
using Appalachia.Jobs.Types.Temporal;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.Voxels
{
    [Serializable]
    public struct Voxel : IEquatable<Voxel>
    {
        private const float VELOCITY_COMPONENT_LIMIT = 100f;

        /// <summary>
        ///     The position of this voxel in voxel space.
        /// </summary>
        [SerializeField]
        public float3 position;

        /// <summary>
        ///     The distance of this voxel to the center of mass in voxel space.
        /// </summary>
        [SerializeField]
        public float distanceToCenterOfMass;

        /// <summary>
        ///     The distance of this voxel to the center of mass in voxel space, normalized.
        /// </summary>
        [SerializeField]
        public float normalizedDistanceToCenterOfMass;

        /// <summary>
        ///     The surface area of this voxel in voxel space.
        /// </summary>
        [SerializeField]
        public float surfaceArea;

        /// <summary>
        ///     The volume of this voxel in voxel space.
        /// </summary>
        [SerializeField]
        public float volume;

        /// <summary>
        ///     The velocity of this voxel in voxel space.
        /// </summary>
        [SerializeField]
        public float3 velocity;

        /// <summary>
        ///     The acceleration of this voxel in voxel space.
        /// </summary>
        [SerializeField]
        public float3 acceleration;

        /// <summary>
        ///     The jerk of this voxel in voxel space.
        /// </summary>
        [SerializeField]
        public float3 jerk;

        /// <summary>
        ///     The position of this voxel in world space.
        /// </summary>
        [SerializeField]
        public float3_temporal worldPosition;

        /// <summary>
        ///     The surface area of this voxel in world space.
        /// </summary>
        [SerializeField]
        public float worldSurfaceArea;

        /// <summary>
        ///     The volume of this voxel in world space.
        /// </summary>
        [SerializeField]
        public float worldVolume;

        /// <summary>
        ///     The velocity of this voxel in world space.
        /// </summary>
        [SerializeField]
        public float3_temporal worldVelocity;

        /// <summary>
        ///     The acceleration of this voxel in world space.
        /// </summary>
        [SerializeField]
        public float3_temporal worldAcceleration;

        /// <summary>
        ///     The jerk of this voxel in world space.
        /// </summary>
        [SerializeField]
        public float3_temporal worldJerk;

        /// <summary>
        ///     The angle between the normal and the velocity.  Negative if pointing in the opposite direction. Positive if pointing in the same direction
        /// </summary>
        [SerializeField]
        public float normalVelocityCodirectionality;

        /// <summary>
        ///     The X, Y, and Z indices for accessing the relevant sample point for this voxel.
        /// </summary>
        [SerializeField]
        public int3 indices;

        /// <summary>
        ///     The face data and interpolated normal for this voxel.
        /// </summary>
        [SerializeField]
        public VoxelFaceData faceData;

        [BurstCompile]
        public void UpdatePhysical(float4x4 localToWorld, float4x4 worldToLocal, float3 resolution, float3 worldResolution, float deltaTime)
        {
            faceData.UpdatePhysical(localToWorld, resolution, worldResolution);

            volume = resolution.x * resolution.y * resolution.z;
            worldVolume = worldResolution.x * worldResolution.y * worldResolution.z;

            surfaceArea = faceData.area;
            worldSurfaceArea = faceData.worldArea;

            var pos = position;
            var newWorldPosition = localToWorld.MultiplyPoint3x4(pos).xyz;

            worldPosition.Update(newWorldPosition);

            if (!(deltaTime > 0f))
            {
                worldPosition.Update(newWorldPosition); // initial case
                return;
            }

            var movementDelta = worldPosition.delta;

            var velocityUnclamped = movementDelta * (1f / deltaTime);
            var velocityClamped = float3.zero;
            velocityClamped.x = math.clamp(velocityUnclamped.x, -VELOCITY_COMPONENT_LIMIT, VELOCITY_COMPONENT_LIMIT);
            velocityClamped.y = math.clamp(velocityUnclamped.y, -VELOCITY_COMPONENT_LIMIT, VELOCITY_COMPONENT_LIMIT);
            velocityClamped.z = math.clamp(velocityUnclamped.z, -VELOCITY_COMPONENT_LIMIT, VELOCITY_COMPONENT_LIMIT);

            worldVelocity.Update(velocityClamped);

            var accelerationDelta = worldVelocity.delta;
            worldAcceleration.Update(accelerationDelta * (1f / deltaTime));

            var jerkDelta = worldAcceleration.delta;
            worldJerk.Update(jerkDelta * (1f / deltaTime));

            velocity = worldToLocal.MultiplyVector(worldVelocity.value);
            acceleration = worldToLocal.MultiplyVector(worldAcceleration.value);
            jerk = worldToLocal.MultiplyVector(worldJerk.value);

            var worldVelocityDirection = math.normalize(worldVelocity.value);

            normalVelocityCodirectionality = math.clamp(math.dot(faceData.worldNormal, worldVelocityDirection), -1f, 1f);
        }

#region IEquatable

        public bool Equals(Voxel other)
        {
            return position.Equals(other.position) && indices.Equals(other.indices) && faceData.Equals(other.faceData);
        }

        public override bool Equals(object obj)
        {
            return obj is Voxel other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = position.GetHashCode();
                hashCode = (hashCode * 397) ^ indices.GetHashCode();
                hashCode = (hashCode * 397) ^ faceData.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(Voxel left, Voxel right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Voxel left, Voxel right)
        {
            return !left.Equals(right);
        }

#endregion
    }
}
