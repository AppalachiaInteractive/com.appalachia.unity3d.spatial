#region

using System;
using Appalachia.Core.Extensions;
using Appalachia.Utility.Constants;
using Appalachia.Utility.Extensions;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.Voxels
{
    [Serializable]
    public struct VoxelFaceData : IEquatable<VoxelFaceData>
    {
        [SerializeField] public bool forward;
        [SerializeField] public bool back;
        [SerializeField] public bool left;
        [SerializeField] public bool right;
        [SerializeField] public bool up;
        [SerializeField] public bool down;

        [SerializeField] private float3 _normalRaw;
        [SerializeField] private float3 _worldNormalRaw;
        [SerializeField] private float3 _normal;
        [SerializeField] private float3 _worldNormal;

        [SerializeField] private float _area;
        [SerializeField] private float _worldArea;
        public bool isFace => forward || back || left || right || up || down;

        public float3 normal
        {
            get
            {
                if (_normal.Equals(default))
                {
                    RecalculateNormal();
                }

                return _normal;
            }
        }

        public float3 normalRaw => _normalRaw;

        public float3 worldNormalRaw => _worldNormalRaw;

        public float3 worldNormal => _worldNormal;

        public float area => _area;
        public float worldArea => _worldArea;

        public bool this[int i]
        {
            get
            {
                if ((i < 0) || (i > 5))
                {
                    throw new ArgumentOutOfRangeException(nameof(i), i, null);
                }

                return this[(VoxelFace) i];
            }
            set
            {
                if ((i < 0) || (i > 5))
                {
                    throw new ArgumentOutOfRangeException(nameof(i), i, null);
                }

                this[(VoxelFace) i] = value;
            }
        }

        public bool this[VoxelFace i]
        {
            get
            {
                switch (i)
                {
                    case VoxelFace.Forward:
                        return forward;
                    case VoxelFace.Back:
                        return back;
                    case VoxelFace.Left:
                        return left;
                    case VoxelFace.Right:
                        return right;
                    case VoxelFace.Up:
                        return up;
                    case VoxelFace.Down:
                        return down;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(i), i, null);
                }
            }
            set
            {
                switch (i)
                {
                    case VoxelFace.Forward:
                        forward = value;
                        break;
                    case VoxelFace.Back:
                        back = value;
                        break;
                    case VoxelFace.Left:
                        left = value;
                        break;
                    case VoxelFace.Right:
                        right = value;
                        break;
                    case VoxelFace.Up:
                        up = value;
                        break;
                    case VoxelFace.Down:
                        down = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(i), i, null);
                }
            }
        }

        [BurstCompile]
        public void RecalculateNormal()
        {
            if (!isFace)
            {
                _normalRaw = float3.zero;
                _normal = float3.zero;
            }

            _normalRaw = (forward ? float3c.forward : float3.zero) +
                         (back ? float3c.back : float3.zero) +
                         (left ? float3c.left : float3.zero) +
                         (right ? float3c.right : float3.zero) +
                         (up ? float3c.up : float3.zero) +
                         (down ? float3c.down : float3.zero);

            _normal = math.normalizesafe(_normalRaw);
        }

        [BurstCompile]
        public void UpdatePhysical(float4x4 localToWorld, float3 resolution, float3 worldResolution)
        {
            if (isFace)
            {
                _worldNormalRaw = localToWorld.MultiplyVector(normalRaw);
                _worldNormal = localToWorld.MultiplyVector(normal);
            }

            _area = (forward ? resolution.x * resolution.y : 0f) +
                    (back ? resolution.x * resolution.y : 0f) +
                    (left ? resolution.z * resolution.y : 0f) +
                    (right ? resolution.z * resolution.y : 0f) +
                    (up ? resolution.x * resolution.z : 0f) +
                    (down ? resolution.x * resolution.z : 0f);

            _worldArea = (forward ? worldResolution.x * worldResolution.y : 0f) +
                         (back ? worldResolution.x * worldResolution.y : 0f) +
                         (left ? worldResolution.z * worldResolution.y : 0f) +
                         (right ? worldResolution.z * worldResolution.y : 0f) +
                         (up ? worldResolution.x * worldResolution.z : 0f) +
                         (down ? worldResolution.x * worldResolution.z : 0f);
        }

        public static VoxelFaceData FullyExposed()
        {
            return new()
            {
                forward = true,
                back = true,
                left = true,
                right = true,
                up = true,
                down = true
            };
        }

#region IEquatable

        public bool Equals(VoxelFaceData other)
        {
            return (forward == other.forward) &&
                   (back == other.back) &&
                   (left == other.left) &&
                   (right == other.right) &&
                   (up == other.up) &&
                   (down == other.down) &&
                   _normal.Equals(other._normal);
        }

        public override bool Equals(object obj)
        {
            return obj is VoxelFaceData other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = forward.GetHashCode();
                hashCode = (hashCode * 397) ^ back.GetHashCode();
                hashCode = (hashCode * 397) ^ left.GetHashCode();
                hashCode = (hashCode * 397) ^ right.GetHashCode();
                hashCode = (hashCode * 397) ^ up.GetHashCode();
                hashCode = (hashCode * 397) ^ down.GetHashCode();
                hashCode = (hashCode * 397) ^ _normal.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(VoxelFaceData left, VoxelFaceData right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(VoxelFaceData left, VoxelFaceData right)
        {
            return !left.Equals(right);
        }

#endregion
    }
}
