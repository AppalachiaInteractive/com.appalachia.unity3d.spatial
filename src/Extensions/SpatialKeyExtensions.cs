#region

using Appalachia.Spatial.SpatialKeys;
using Unity.Mathematics;
using UnityEngine;

#endregion

namespace Appalachia.Spatial.Extensions
{
    public static class SpatialKeyExtensions
    {
        public static Matrix4x4Key GetSpatialKey(this Matrix4x4 matrix, int groupingScale)
        {
            return new(matrix, groupingScale);
        }

        public static Matrix4x4Key GetSpatialKey(this float4x4 matrix, int groupingScale)
        {
            return new(matrix, groupingScale);
        }

        public static Vector4Key GetSpatialKey(this Vector4 vector, int groupingScale)
        {
            return new(vector, groupingScale);
        }

        public static Vector4Key GetSpatialKey(this float4 vector, int groupingScale)
        {
            return new(vector, groupingScale);
        }

        public static Vector3Key GetSpatialKey(this Vector3 vector, int groupingScale)
        {
            return new(vector, groupingScale);
        }

        public static Vector3Key GetSpatialKey(this float3 vector, int groupingScale)
        {
            return new(vector, groupingScale);
        }

        public static Vector2Key GetSpatialKey(this Vector2 vector, int groupingScale)
        {
            return new(vector, groupingScale);
        }

        public static Vector2Key GetSpatialKey(this float2 vector, int groupingScale)
        {
            return new(vector, groupingScale);
        }

        public static QuaternionKey GetSpatialKey(this Quaternion rotation, int groupingScale)
        {
            return new(rotation, groupingScale);
        }

        public static QuaternionKey GetSpatialKey(this quaternion rotation, int groupingScale)
        {
            return new(rotation, groupingScale);
        }
    }
}
